using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private const int BoardSize = 30;
    private const int SpawnStartIndex = 29;
    private const int RandomSpawnCandyTypeCount = 6;
    private const float NeighborTileDistance = 1.2f;
    private const float MatchNeighborDistance = 1.3f;
    private const float SelectionDistanceThreshold = 0.5f;
    private const int ShuffleRetryLimit = 30;

    // Candy prefabs and board slot anchors.
    public GameObject Green_Candy;
    public GameObject Blue_Candy;
    public GameObject Orange_Candy;
    public GameObject Red_Candy;
    public GameObject Purple_Candy;
    public GameObject Yellow_Candy;
    public GameObject[] tiles = new GameObject[BoardSize];
    public GameObject[] candy_tiles;
    private char[] check_tiles = new char[BoardSize];
    private int[] visited_tiles = new int[BoardSize];
    private int[] visited_final_tiles = new int[BoardSize];
    public GameObject[] Candy_Obj = new GameObject[7];
    public GameObject[] Heart_Obj = new GameObject[3];
    public AudioSource cat_bgm;
    public AudioSource button_bgm;
    public GameObject UI_manager;

    // Board state:
    // check_tiles = candy id per slot, visited_tiles = temp flood-fill state,
    // visited_final_tiles = slots that are currently empty.
    private char[] Candy_Obj_name = { 'g', 'b', 'y', 'r', 'p', 'o', 't' };
    List<List<int>> near_tiles = new List<List<int>>();
    private int cutline = 6;
    private int click_1_candy_num = 0;
    private int click_2_candy_num = 0;
    private bool Check_destory = false;
    public Text score_text;
    public Text score_gain_text;
    public Text combo_text;
    int score_num = 0;
    int wrong_move_num = 3;
    bool finish_check = true;
    bool isResolvingMove = false;
    bool isGameOver = false;
    [SerializeField] private float swapMoveSpeed = 1.2f;
    [SerializeField] private float fallMoveSpeed = 2.0f;
    [SerializeField] private float scorePopupDuration = 0.6f;
    [SerializeField] private float scorePopupRise = 26f;
    [SerializeField] private float comboPopupDuration = 1.0f;
    [SerializeField] private Color[] comboTierColors = new Color[]
    {
        new Color(0f, 0f, 0f),
        new Color(1f, 0.62f, 0.12f),
        new Color(0.2f, 0.85f, 0.25f),
        new Color(0.75f, 0.35f, 1f),
        new Color(1f, 0.2f, 0.2f)
    };
    private Coroutine scorePopupRoutine;
    private Coroutine comboPopupRoutine;
    private Vector3 scorePopupStartPos;
    private Color scorePopupBaseColor;
    private Color comboBaseColor = Color.white;
    private int comboCount = 0;

    // Build initial map and cache popup UI defaults.
    void Start()
    {
        Candy_Obj[0] = Green_Candy;
        Candy_Obj[1] = Blue_Candy;
        Candy_Obj[2] = Yellow_Candy;
        Candy_Obj[3] = Red_Candy;
        Candy_Obj[4] = Purple_Candy;
        Candy_Obj[5] = Orange_Candy;
        FindNearTiles();
        CreateFirst();

        if (score_gain_text != null)
        {
            scorePopupStartPos = score_gain_text.rectTransform.localPosition;
            scorePopupBaseColor = score_gain_text.color;
            score_gain_text.gameObject.SetActive(false);
        }

        if (combo_text != null)
        {
            comboBaseColor = combo_text.color;
            combo_text.gameObject.SetActive(false);
        }
    }

    private bool TryGetPrefabForCode(char candyCode, out GameObject prefab)
    {
        prefab = null;
        switch (candyCode)
        {
            case 'b':
                prefab = Blue_Candy;
                return true;
            case 'g':
                prefab = Green_Candy;
                return true;
            case 'o':
                prefab = Orange_Candy;
                return true;
            case 'p':
                prefab = Purple_Candy;
                return true;
            case 'r':
                prefab = Red_Candy;
                return true;
            case 'y':
                prefab = Yellow_Candy;
                return true;
            default:
                return false;
        }
    }

    private void SpawnCandyAt(int tileIndex, GameObject prefab, char candyCode)
    {
        Vector3 spawnPos = tiles[tileIndex].transform.position;
        candy_tiles[tileIndex] = Instantiate(prefab, spawnPos, Quaternion.identity);
        check_tiles[tileIndex] = candyCode;
        visited_final_tiles[tileIndex] = 0;
    }

    private bool TrySpawnCandyAt(int tileIndex, char candyCode)
    {
        if (!TryGetPrefabForCode(candyCode, out GameObject prefab))
            return false;

        SpawnCandyAt(tileIndex, prefab, candyCode);
        return true;
    }

    private bool IsOccupiedMatchTile(int index)
    {
        return visited_final_tiles[index] == 0 && check_tiles[index] != '0' && check_tiles[index] != 't';
    }

    private bool SwapCreatesMatch(int firstIndex, int secondIndex)
    {
        char firstCode = check_tiles[firstIndex];
        char secondCode = check_tiles[secondIndex];

        check_tiles[firstIndex] = secondCode;
        check_tiles[secondIndex] = firstCode;

        int[] visitedBackup = (int[])visited_tiles.Clone();
        int[] visitedFinalBackup = (int[])visited_final_tiles.Clone();
        bool destroyFlagBackup = Check_destory;

        CheckSameCandy(firstIndex);
        bool hasMatch = Check_destory;
        if (!hasMatch)
            CheckSameCandy(secondIndex);
        hasMatch = hasMatch || Check_destory;

        visited_tiles = visitedBackup;
        visited_final_tiles = visitedFinalBackup;
        Check_destory = destroyFlagBackup;

        check_tiles[firstIndex] = firstCode;
        check_tiles[secondIndex] = secondCode;
        return hasMatch;
    }

    private bool HasAnyPossibleMove()
    {
        for (int i = 0; i < BoardSize; i++)
        {
            if (!IsOccupiedMatchTile(i))
                continue;

            for (int j = 0; j < near_tiles[i].Count; j++)
            {
                int neighborIndex = near_tiles[i][j];
                if (neighborIndex <= i || !IsOccupiedMatchTile(neighborIndex))
                    continue;

                if (SwapCreatesMatch(i, neighborIndex))
                    return true;
            }
        }

        return false;
    }

    private void ReshufflePlayableTiles()
    {
        List<int> tileIndices = new List<int>();
        List<char> candyCodes = new List<char>();

        for (int i = 0; i < BoardSize; i++)
        {
            if (!IsOccupiedMatchTile(i))
                continue;

            tileIndices.Add(i);
            candyCodes.Add(check_tiles[i]);
        }

        for (int i = candyCodes.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            char temp = candyCodes[i];
            candyCodes[i] = candyCodes[randomIndex];
            candyCodes[randomIndex] = temp;
        }

        for (int i = 0; i < tileIndices.Count; i++)
        {
            int tileIndex = tileIndices[i];
            if (candy_tiles[tileIndex] != null)
                Destroy(candy_tiles[tileIndex]);

            TrySpawnCandyAt(tileIndex, candyCodes[i]);
        }
    }

    private void EnsurePlayableBoardOrShuffle()
    {
        if (HasAnyPossibleMove())
            return;

        for (int attempt = 0; attempt < ShuffleRetryLimit; attempt++)
        {
            ReshufflePlayableTiles();
            if (HasAnyPossibleMove())
                return;
        }

        Debug.LogWarning("Board shuffle retry limit reached without finding a playable move.");
    }

    private void AddScore(int amount)
    {
        score_num += amount;
        ShowScoreGainPopup(amount);
    }

    private void ShowScoreGainPopup(int amount)
    {
        if (score_gain_text == null || amount == 0)
            return;

        if (scorePopupRoutine != null)
            StopCoroutine(scorePopupRoutine);

        scorePopupRoutine = StartCoroutine(ScoreGainPopupRoutine(amount));
    }

    private IEnumerator ScoreGainPopupRoutine(int amount)
    {
        // Float-up + fade-out popup near total score text.
        score_gain_text.text = amount > 0 ? $"+{amount}" : amount.ToString();
        score_gain_text.rectTransform.localPosition = scorePopupStartPos;
        score_gain_text.color = scorePopupBaseColor;
        score_gain_text.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < scorePopupDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scorePopupDuration);

            score_gain_text.rectTransform.localPosition = scorePopupStartPos + Vector3.up * (scorePopupRise * t);

            Color c = scorePopupBaseColor;
            c.a = 1f - t;
            score_gain_text.color = c;

            yield return null;
        }

        score_gain_text.rectTransform.localPosition = scorePopupStartPos;
        score_gain_text.color = scorePopupBaseColor;
        score_gain_text.gameObject.SetActive(false);
        scorePopupRoutine = null;
    }

    private void OnComboSuccess()
    {
        // Increase combo on every successful match resolution.
        comboCount += 1;
        bool rewardedHeart = false;

        if (comboCount % 10 == 0)
        {
            // Reward one life every 10 combo chain.
            rewardedHeart = RechargeHeart();
        }

        ShowComboPopup(comboCount, rewardedHeart);
    }

    private void ResetCombo()
    {
        comboCount = 0;
        if (comboPopupRoutine != null)
        {
            StopCoroutine(comboPopupRoutine);
            comboPopupRoutine = null;
        }

        if (combo_text != null)
        {
            combo_text.gameObject.SetActive(false);
        }
    }

    private bool RechargeHeart()
    {
        if (wrong_move_num >= Heart_Obj.Length)
            return false;

        if (Heart_Obj[wrong_move_num] != null)
            Heart_Obj[wrong_move_num].SetActive(true);

        wrong_move_num += 1;
        return true;
    }

    private void ShowComboPopup(int combo, bool rewardedHeart)
    {
        if (combo_text == null)
            return;

        combo_text.text = rewardedHeart ? $"{combo} COMBO! +HEART" : $"{combo} COMBO!";
        combo_text.color = GetComboTierColor(combo);
        combo_text.gameObject.SetActive(true);

        if (comboPopupRoutine != null)
            StopCoroutine(comboPopupRoutine);

        comboPopupRoutine = StartCoroutine(HideComboPopupRoutine());
    }

    private IEnumerator HideComboPopupRoutine()
    {
        yield return new WaitForSeconds(comboPopupDuration);
        if (combo_text != null)
        {
            combo_text.color = comboBaseColor;
            combo_text.gameObject.SetActive(false);
        }
        comboPopupRoutine = null;
    }

    private Color GetComboTierColor(int combo)
    {
        // 0~9: tier 0, 10~19: tier 1, ... capped at last configured color.
        if (comboTierColors == null || comboTierColors.Length == 0)
            return comboBaseColor;

        int tier = combo / 10;
        if (tier <= 0)
            return comboTierColors[0];

        int colorIndex = Mathf.Min(tier, comboTierColors.Length - 1);
        return comboTierColors[colorIndex];
    }

    private void CreateFirst()
    {
        // Hardcoded initial board layout.
        candy_tiles = new GameObject[BoardSize];
        string mapSetting = "oyrgpobyrbpopgrggprpygbyogooyb";
        if (mapSetting.Length != BoardSize)
        {
            Debug.LogError($"Invalid map setting length. Expected {BoardSize}, got {mapSetting.Length}.");
            return;
        }

        for (int i = 0; i < BoardSize; i++)
        {
            visited_tiles[i] = 0;
            visited_final_tiles[i] = 0;
            check_tiles[i] = '0';
            TrySpawnCandyAt(i, mapSetting[i]);
        }

        EnsurePlayableBoardOrShuffle();
    }

    private int CheckGroupMap()
    {
        // Scan board after cascades. If another match exists, keep resolving.
        int new_candy = 0;
        for (int i = 0; i < BoardSize; i++)
        {
            if (visited_final_tiles[i] == 0)
            {
                CheckSameCandy(i);
                if (Check_destory == true)
                {
                    StartCoroutine(FillCandy());
                    new_candy = 0;
                    return new_candy;
                }
            }
            else
            {
                new_candy += 1;
            }
        }
        return new_candy;
    }

    private IEnumerator MoveCandy(int current_num, int next_num)
    {
        // Physically move one candy, then commit board arrays.
        if (candy_tiles[current_num] != null)
        {
            Vector2 pos1 = tiles[current_num].transform.position;
            Vector2 pos2 = tiles[next_num].transform.position;

            check_tiles[next_num] = check_tiles[current_num];
            check_tiles[current_num] = '0';
            visited_final_tiles[next_num] = 0;
            visited_final_tiles[current_num] = 1;

            while (Vector3.Distance(candy_tiles[current_num].transform.position, pos2) > 0.01f)
            {
                float step = Mathf.Max(0.01f, fallMoveSpeed) * Time.deltaTime;
                candy_tiles[current_num].transform.position = Vector3.MoveTowards(candy_tiles[current_num].transform.position, pos2, step);
                yield return null;
            }

            candy_tiles[next_num] = candy_tiles[current_num];
            candy_tiles[current_num] = null;
        }
    }

    private int FindNearEmptyTilesSmall(int num)
    {
        int small = 100;
        for (int j = 0; j < near_tiles[num].Count; j++)
        {
            if (small > near_tiles[num][j])
            {
                if (visited_final_tiles[near_tiles[num][j]] == 1)
                {
                    small = near_tiles[num][j];
                }
            }
        }
        return small;
    }

    private int candy_up_next_level(int candyNum)
    {
        // Hex-grid vertical index mapping for this board topology.
        if (candyNum == 0 || candyNum == 25)
        {
            candyNum = candyNum + 4;
        }
        else if (candyNum == 1 || candyNum == 2 || candyNum == 21 || candyNum == 22)
        {
            candyNum = candyNum + 6;
        }
        else if(candyNum == 20)
        {
            candyNum = candyNum + 10;
        }
        else
        {
            candyNum = candyNum + 7;
        }

        return candyNum;
    }

    private void FindNearTop()
    {
        ResetGroup();
        for (int i = 0; i < BoardSize; i++)
        {
            if (check_tiles[i] == 't')
            {
                int check = 0;
                for (int j = 0; j < near_tiles[i].Count; j++)
                {
                    if (visited_final_tiles[near_tiles[i][j]] == 1)
                    {
                        check = 1;
                    }
                }
                if (check == 1)
                {
                    candy_tiles[i].GetComponent<Top_count>().count += 1;
                    if (candy_tiles[i].GetComponent<Top_count>().count == 2)
                    {
                        AddScore(480);
                        visited_tiles[i] = 1;
                    }
                }
            }
        }
        for (int i = 0; i < BoardSize; i++)
        {
            if (visited_tiles[i] == 1)
                visited_final_tiles[i] = 1;
        }
    }

    private void DestroyCandy(List<int> destory_candy)
    {
        // Remove all flagged slots, then grant score once per batch.
        int destroyCount = 0;
        for (int i = 0; i < BoardSize; i++)
        {
            if (visited_final_tiles[i] == 1)
            {
                destroyCount += 1;
                destory_candy.Add(i);
                Destroy(candy_tiles[i]);
                candy_tiles[i] = null;
                check_tiles[i] = '0';
            }
        }
        if (destroyCount > 0)
            AddScore(destroyCount * 20);
        cat_bgm.Play();
    }

    private IEnumerator FillCandy()
    {
        // Resolve one full cycle: destroy -> fall -> side flow -> spawn -> recheck.
        List<int> destory_candy = new List<int>();
        DestroyCandy(destory_candy);
        for (int i = 0; i < destory_candy.Count; i++)
        {
            int candyNum = destory_candy[i];
            if (visited_final_tiles[candyNum] == 1)
            {
                int nextCandyNum = candy_up_next_level(candyNum);
                while (nextCandyNum < BoardSize)
                {
                    if (visited_final_tiles[nextCandyNum] == 0)
                    {
                        yield return StartCoroutine(MoveCandy(nextCandyNum, candyNum));
                        candyNum = candy_up_next_level(candyNum);
                    }
                    nextCandyNum = candy_up_next_level(nextCandyNum);
                }
            }
        }
        int[] check_edge = { 20, 23, 24, 26, 27, 28, 29 };
        for (int i = 0; i < 7; i++)
        {
            while (true)
            {
                int next_tile = FindNearEmptyTilesSmall(check_edge[i]);
                if (next_tile < check_edge[i])
                {
                    yield return StartCoroutine(MoveCandy(check_edge[i], next_tile));
                    check_edge[i] = next_tile;
                }
                else
                {
                    break;
                }
            }
        }

        int new_candy = CheckGroupMap();

        if (new_candy > 0)
        {
            List<int> new_candies = new List<int>();

            for (int i = 0; i < new_candy; i++)
            {
                int start_candy = SpawnStartIndex;
                int new_candy_num = Random.Range(0, RandomSpawnCandyTypeCount);
                SpawnCandyAt(start_candy, Candy_Obj[new_candy_num], Candy_Obj_name[new_candy_num]);
                new_candies.Add(start_candy);
                for (int j = 0; j < (i + 1); j++)
                {
                    int next_tile = FindNearEmptyTilesSmall(new_candies[j]);
                    if (next_tile < new_candies[j])
                    {
                        yield return StartCoroutine(MoveCandy(new_candies[j], next_tile));
                        new_candies[j] = next_tile;
                    }
                }
            }

            new_candy = CheckGroupMap();
        }

        EnsurePlayableBoardOrShuffle();
    }

    private void CheckUpDown(int candyNum)
    {
        Queue<int> q = new Queue<int>();
        q.Enqueue(candyNum);
        visited_tiles[candyNum] = 1;

        while (q.Count > 0)
        {
            candyNum = q.Dequeue();
            int check_num_1;
            int check_num_2;

            if (candyNum == 0 || candyNum == 29)
            {
                check_num_1 = candyNum + 4;
                check_num_2 = candyNum - 4;
            }
            else if (candyNum == 1 || candyNum == 2 || candyNum == 27 || candyNum == 28)
            {
                check_num_1 = candyNum + 6;
                check_num_2 = candyNum - 6;
            }
            else if (candyNum == 7 || candyNum == 8)
            {
                check_num_1 = candyNum + 7;
                check_num_2 = candyNum - 6;
            }
            else if (candyNum == 21 || candyNum == 22)
            {
                check_num_1 = candyNum + 6;
                check_num_2 = candyNum - 7;
            }
            else if (candyNum == 4)
            {
                check_num_1 = candyNum + 7;
                check_num_2 = candyNum - 4;
            }
            else if (candyNum == 25)
            {
                check_num_1 = candyNum + 4;
                check_num_2 = candyNum - 7;
            }
            else
            {
                check_num_1 = candyNum + 7;
                check_num_2 = candyNum - 7;
            }

            if (CheckNearSameCandy(candyNum, check_num_1) == 1)
            {
                q.Enqueue(check_num_1);
                visited_tiles[check_num_1] = 1;
            }

            if (CheckNearSameCandy(candyNum, check_num_2) == 1)
            {
                q.Enqueue(check_num_2);
                visited_tiles[check_num_2] = 1;
            }
        }
    }

    private void CheckLeftDia(int candyNum)
    {
        Queue<int> q = new Queue<int>();
        q.Enqueue(candyNum);
        visited_tiles[candyNum] = 1;

        while (q.Count > 0)
        {
            candyNum = q.Dequeue();
            int check_num_1;
            int check_num_2;

            if (candyNum == 0 || candyNum == 29)
            {
                check_num_1 = candyNum + 1;
                check_num_2 = candyNum - 1;
            }
            else if (candyNum == 1 || candyNum == 2)
            {
                check_num_1 = candyNum + 2;
                check_num_2 = candyNum - 1;
            }
            else if (candyNum == 3 || candyNum == 4 || candyNum == 5 || candyNum == 27)
            {
                check_num_1 = candyNum + 3;
                check_num_2 = candyNum - 2;
            }
            else if (candyNum == 24 || candyNum == 25 || candyNum == 26)
            {
                check_num_1 = candyNum + 2;
                check_num_2 = candyNum - 3;
            }
            else if (candyNum == 28)
            {
                check_num_1 = candyNum + 1;
                check_num_2 = candyNum - 2;
            }
            else
            {
                check_num_1 = candyNum + 3;
                check_num_2 = candyNum - 3;
            }

            if (CheckNearSameCandy(candyNum, check_num_1) == 1)
            {
                q.Enqueue(check_num_1);
                visited_tiles[check_num_1] = 1;
            }

            if (CheckNearSameCandy(candyNum, check_num_2) == 1)
            {
                q.Enqueue(check_num_2);
                visited_tiles[check_num_2] = 1;
            }
        }
    }

    private void CheckGroup()
    {
        int check_num = 0;
        for (int i = 0; i < BoardSize; i++)
        {
            if (visited_tiles[i] == 1)
                check_num += 1;
        }
        if (check_num >= 3)
        {
            Check_destory = true;
            SaveDestroyCandy();
        }
    }

    private int CheckNearSameCandy(int candy_num, int check_num)
    {
        // Returns 1 only when neighbor is valid, connected, unvisited, and same type.
        if (0 <= check_num && check_num < BoardSize)
        {
            if (visited_final_tiles[check_num] == 0)
            {
                Vector2 pos1 = tiles[candy_num].transform.position;
                Vector2 pos2 = tiles[check_num].transform.position;
                if (Vector3.Distance(pos1, pos2) < MatchNeighborDistance)
                {
                    if (visited_tiles[check_num] == 0)
                    {
                        if (check_tiles[candy_num] == check_tiles[check_num])
                        {
                            return 1;
                        }
                    }
                }
            }
        }
        return 0;
    }

    private void CheckRightDia(int candyNum)
    {
        Queue<int> q = new Queue<int>();
        q.Enqueue(candyNum);
        visited_tiles[candyNum] = 1;

        while (q.Count > 0)
        {
            candyNum = q.Dequeue();
            int check_num_1;
            int check_num_2;

            if (candyNum == 0)
            {
                check_num_1 = candyNum + 2;
                check_num_2 = -1;
            }
            else if (candyNum == 1 || candyNum == 2)
            {
                check_num_1 = candyNum + 3;
                check_num_2 = candyNum - 2;
            }
            else if (candyNum == 4 || candyNum == 5)
            {
                check_num_1 = candyNum + 4;
                check_num_2 = candyNum - 3;
            }
            else if (candyNum == 24 || candyNum == 25)
            {
                check_num_1 = candyNum + 3;
                check_num_2 = candyNum - 4;
            }
            else if (candyNum == 27 || candyNum == 28)
            {
                check_num_1 = candyNum + 2;
                check_num_2 = candyNum - 3;
            }
            else if (candyNum == 29)
            {
                check_num_1 = -1;
                check_num_2 = candyNum - 2;
            }
            else
            {
                check_num_1 = candyNum + 4;
                check_num_2 = candyNum - 4;
            }

            if (CheckNearSameCandy(candyNum, check_num_1) == 1)
            {
                q.Enqueue(check_num_1);
                visited_tiles[check_num_1] = 1;
            }

            if (CheckNearSameCandy(candyNum, check_num_2) == 1)
            {
                q.Enqueue(check_num_2);
                visited_tiles[check_num_2] = 1;
            }
        }
    }

    private void ResetGroup()
    {
        for (int i = 0; i < BoardSize; i++)
        {
            visited_tiles[i] = 0;
        }
    }

    private void SaveDestroyCandy()
    {
        for (int i = 0; i < BoardSize; i++)
        {
            if (visited_tiles[i] == 1)
                visited_final_tiles[i] = 1;
        }
    }

    private void CheckFourGroup()
    {
        int check_num = 0;
        for (int i = 0; i < BoardSize; i++)
        {
            if (visited_tiles[i] == 1)
            {
                int countNearTile = 0;
                for (int j = 0; j < near_tiles[i].Count; j++)
                {
                    if (visited_tiles[near_tiles[i][j]] == visited_tiles[i])
                    {
                        countNearTile++;
                    }
                }
                if (countNearTile >= 2)
                    check_num += 1;
                else
                    visited_tiles[i] = 0;
            }
        }
        if (check_num >= 4)
        {
            Check_destory = true;
            SaveDestroyCandy();
        }
    }

    private void CheckFindFourGroup(int candyNum)
    {
        Queue<int> q = new Queue<int>();
        q.Enqueue(candyNum);
        visited_tiles[candyNum] = 1;

        while (q.Count > 0)
        {
            candyNum = q.Dequeue();
            for (int j = 0; j < near_tiles[candyNum].Count; j++)
            {
                if (check_tiles[near_tiles[candyNum][j]] == check_tiles[candyNum])
                {
                    if (CheckNearSameCandy(candyNum, near_tiles[candyNum][j]) == 1)
                    {
                        q.Enqueue(near_tiles[candyNum][j]);
                        visited_tiles[near_tiles[candyNum][j]] = 1;
                    }
                }
            }
        }
    }

    private void CheckSameCandy(int candyNum)
    {
        Check_destory = false;
        if (check_tiles[candyNum] != 't' && check_tiles[candyNum] != '0')
        {
            CheckRightDia(candyNum);
            CheckGroup();
            ResetGroup();
            CheckLeftDia(candyNum);
            CheckGroup();
            ResetGroup();
            CheckUpDown(candyNum);
            CheckGroup();
            ResetGroup();
        }
    }

    private IEnumerator ChangeCandy(int click_1_candy_num, int click_2_candy_num)
    {
        // Swap animation and state swap for two adjacent tiles.
        Vector2 pos1 = tiles[click_1_candy_num].transform.position;
        Vector2 pos2 = tiles[click_2_candy_num].transform.position;
        if (Vector3.Distance(pos1, pos2) < 1)
        {
            while (Vector3.Distance(candy_tiles[click_1_candy_num].transform.position, pos2) > 0.01f)
            {
                float step = Mathf.Max(0.01f, swapMoveSpeed) * Time.deltaTime;
                candy_tiles[click_1_candy_num].transform.position = Vector3.MoveTowards(candy_tiles[click_1_candy_num].transform.position, pos2, step);
                candy_tiles[click_2_candy_num].transform.position = Vector3.MoveTowards(candy_tiles[click_2_candy_num].transform.position, pos1, step);
                yield return null;
            }

            char change = check_tiles[click_1_candy_num];
            check_tiles[click_1_candy_num] = check_tiles[click_2_candy_num];
            check_tiles[click_2_candy_num] = change;

            GameObject _obj = candy_tiles[click_1_candy_num];
            candy_tiles[click_1_candy_num] = candy_tiles[click_2_candy_num];
            candy_tiles[click_2_candy_num] = _obj;
        }
    }

    private IEnumerator CheckCandy(int click_1_candy_num, int click_2_candy_num)
    {
        // Player move pipeline: swap -> validate -> resolve or rollback.
        yield return StartCoroutine(ChangeCandy(click_1_candy_num, click_2_candy_num));

        CheckSameCandy(click_1_candy_num);
        CheckSameCandy(click_2_candy_num);

        if (Check_destory == true)
        {
            OnComboSuccess();
            yield return StartCoroutine(FillCandy());
            finish_check = true;
        }
        else
        {
            ResetCombo();
            yield return StartCoroutine(ChangeCandy(click_2_candy_num, click_1_candy_num));
            wrong_move_num = Mathf.Max(0, wrong_move_num - 1);
            if (wrong_move_num < Heart_Obj.Length && Heart_Obj[wrong_move_num] != null)
                Heart_Obj[wrong_move_num].SetActive(false);
            button_bgm.Play();
            finish_check = true;
        }

        isResolvingMove = false;
    }

    private int FindNearTilesToPos(int num, Vector3 pos)
    {
        float small_dis = 100;
        int small_tile_num = 0;
        for (int i = 0; i < near_tiles[num].Count; i++)
        {
            if (Vector3.Distance(pos, tiles[near_tiles[num][i]].transform.position) < small_dis)
            {
                small_dis = Vector3.Distance(pos, tiles[near_tiles[num][i]].transform.position);
                small_tile_num = near_tiles[num][i];
            }
        }
        return small_tile_num;
    }

    private int FindTileNum(Vector3 vector)
    {
        int check_num = 0;
        float check_distance = 1000;
        for (int i = 0; i < BoardSize; i++)
        {
            if (check_distance > Vector3.Distance(vector, tiles[i].transform.position))
            {
                check_distance = Vector3.Distance(vector, tiles[i].transform.position);
                check_num = i;
            }
        }
        return check_num;
    }

    private void FindNearTiles()
    {
        // Precompute adjacent slots once for pointer and match logic.
        for (int i = 0; i < BoardSize; i++)
        {
            near_tiles.Add(new List<int>());
            for (int j = 0; j < BoardSize; j++)
            {
                if (i != j && Vector3.Distance(tiles[i].transform.position, tiles[j].transform.position) < NeighborTileDistance)
                {
                    near_tiles[i].Add(j);
                }
            }
        }
    }

    private bool TryGetPointerBegan(out Vector2 screenPos)
    {
        screenPos = Vector2.zero;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            screenPos = Input.GetTouch(0).position;
            return true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            screenPos = Input.mousePosition;
            return true;
        }

        return false;
    }

    private bool TryGetPointerEnded(out Vector2 screenPos)
    {
        screenPos = Vector2.zero;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            screenPos = Input.GetTouch(0).position;
            return true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            screenPos = Input.mousePosition;
            return true;
        }

        return false;
    }
    void Update()
    {
        // Stop all input after game-over.
        if (isGameOver)
        {
            score_text.text = score_num.ToString();
            return;
        }

        Vector2 pointerScreenPos;
        if (TryGetPointerBegan(out pointerScreenPos))
        {
            // First tile selection on touch/mouse down.
            Vector2 pos = Camera.main.ScreenToWorldPoint(pointerScreenPos);
            if (finish_check == true)
            {
                click_1_candy_num = FindTileNum(pos);
                if (Vector3.Distance(tiles[click_1_candy_num].transform.position, pos) > SelectionDistanceThreshold)
                    click_1_candy_num = -1;
            }
        }

        if (TryGetPointerEnded(out pointerScreenPos))
        {
            // Second tile selection on touch/mouse up.
            Vector2 pos = Camera.main.ScreenToWorldPoint(pointerScreenPos);
            if (finish_check == true)
            {
                if (click_1_candy_num == -1)
                    return;

                click_2_candy_num = FindNearTilesToPos(click_1_candy_num, pos);
                if (click_2_candy_num != click_1_candy_num && click_1_candy_num != -1)
                {
                    finish_check = false;
                    isResolvingMove = true;
                    StartCoroutine(CheckCandy(click_1_candy_num, click_2_candy_num));
                }
            }
        }
        if (wrong_move_num == 0 && !isGameOver)
        {
            // Enter game-over once when lives are exhausted.
            finish_check = false;
            isResolvingMove = false;
            isGameOver = true;
            UI_manager.GetComponent<UI_manager>().score = score_num;
            UI_manager.GetComponent<UI_manager>().OnresultImage();
        }
        score_text.text = score_num.ToString();
        if (!isResolvingMove)
        {
            // Unlock input only when board has no pending empty slots.
            int check_last = 0;
            for (int i = 0; i < BoardSize; i++)
            {
                if (visited_final_tiles[i] == 1)
                {
                    check_last = 1;
                    finish_check = false;
                }
            }
            if (check_last == 0)
                finish_check = true;
        }
    }
}



