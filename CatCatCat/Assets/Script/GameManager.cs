using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject Green_Candy;
    public GameObject Blue_Candy;
    public GameObject Orange_Candy;
    public GameObject Red_Candy;
    public GameObject Purple_Candy;
    public GameObject Yellow_Candy;
    public GameObject[] tiles = new GameObject[30]; // ������ ��ġ
    public GameObject[] candy_tiles; // ������ ��ġ�� ���� ĵ�� ������Ʈ 
    private char[] check_tiles = new char[30]; // ������ ��ġ�� ���� ����
    private int[] visited_tiles = new int[30];
    private int[] visited_final_tiles = new int[30]; // ������ ��ġ�� ���� ���� ĵ�� ���� ����
    public GameObject[] Candy_Obj = new GameObject[7];
    public GameObject[] Heart_Obj = new GameObject[3];
    public AudioSource cat_bgm;
    public AudioSource button_bgm;
    public GameObject UI_manager;
    private char[] Candy_Obj_name = { 'g', 'b', 'y', 'r', 'p', 'o', 't' };
    List<List<int>> near_tiles = new List<List<int>>(); // �� Ÿ���� �ֺ� Ÿ�� ����
    private int cutline = 6;
    private int click_1_candy_num = 0;
    private int click_2_candy_num = 0;
    private bool Check_destory = false;
    public Text score_text;
    int score_num = 0;
    int wrong_move_num = 3;
    bool finish_check = true;

    // Start is called before the first frame update
    void Start()
    {
        Candy_Obj[0] = Green_Candy;
        Candy_Obj[1] = Blue_Candy;
        Candy_Obj[2] = Yellow_Candy;
        Candy_Obj[3] = Red_Candy;
        Candy_Obj[4] = Purple_Candy;
        Candy_Obj[5] = Orange_Candy;
        FindNearTiles();
        CreateFrist(); // ó�� �� ����
    }

    private void CreateFrist()
    {
        candy_tiles = new GameObject[30];
        string MapSetting = "oyrgpobyrbpopgrggprpygbyogooyb"; // ó�� �� ���� p b y r g
        //string MapSetting = "ptbtbbgprbptbbrggprpygbytgtttb"; // ó�� �� ����
        //string MapSetting = "ttttptbyrbptpgrggprpygbytgtttb"; // ó�� �� ����

        for (int i=0; i<30; i++)
        {
            visited_tiles[i] = 0;
            visited_final_tiles[i] = 0;
            if (MapSetting[i] == 'b')
            {
                candy_tiles[i] = Instantiate(Blue_Candy, new Vector3(tiles[i].transform.position.x, tiles[i].transform.position.y), Quaternion.identity) as GameObject;
                check_tiles[i] = 'b';
            }
            if (MapSetting[i] == 'g')
            {
                candy_tiles[i] = Instantiate(Green_Candy, new Vector3(tiles[i].transform.position.x, tiles[i].transform.position.y), Quaternion.identity) as GameObject;
                check_tiles[i] = 'g';
            }
            if (MapSetting[i] == 'o')
            {
                candy_tiles[i] = Instantiate(Orange_Candy, new Vector3(tiles[i].transform.position.x, tiles[i].transform.position.y), Quaternion.identity) as GameObject;
                check_tiles[i] = 'o';
            }
            if (MapSetting[i] == 'p')
            {
                candy_tiles[i] = Instantiate(Purple_Candy, new Vector3(tiles[i].transform.position.x, tiles[i].transform.position.y), Quaternion.identity) as GameObject;
                check_tiles[i] = 'p';
            }
            if (MapSetting[i] == 'r')
            {
                candy_tiles[i] = Instantiate(Red_Candy, new Vector3(tiles[i].transform.position.x, tiles[i].transform.position.y), Quaternion.identity) as GameObject;
                check_tiles[i] = 'r';
            }
            if (MapSetting[i] == 'y')
            {
                candy_tiles[i] = Instantiate(Yellow_Candy, new Vector3(tiles[i].transform.position.x, tiles[i].transform.position.y), Quaternion.identity) as GameObject;
                check_tiles[i] = 'y';
            }
        }
    }

    private int CheckGrounpMap()
    {
        int new_candy = 0;
        // �׷�Ǵ��� �ٽ� �� Ȯ���ϱ� 
        for (int i = 0; i < 30; i++)
        {
            if (visited_final_tiles[i] == 0) // ä���� ���� Ȯ��
            {
                CheckSameCandy(i);
                if (Check_destory == true)
                {
                    StartCoroutine(FillCandy());
                    new_candy = 0;
                    return new_candy;
                }
            }
            else // ����� ���� Ȯ��
            {
                new_candy += 1;
            }
        }
        return new_candy;
    }

    private IEnumerator MoveCandy(int current_num, int next_num)
    {
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
                candy_tiles[current_num].transform.position = Vector3.MoveTowards(candy_tiles[current_num].transform.position, pos2, 0.05f);
                yield return new WaitForSeconds(0.001f);
            }

            candy_tiles[next_num] = candy_tiles[current_num];
            candy_tiles[current_num] = null;
        }
    }

    private int FindNearEmptyTilesSmall(int num) // ��ó Ÿ�� �߿� ����� Ÿ�� ã�Ƽ� �ش� ��ȣ ��ȯ
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
        if (candyNum == 0 || candyNum == 25) // ������ ���� ó�� ���� �� ȿ�������� ���� �ʿ�
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

    private void FindNearTop() // ��ó ���� ã�Ƽ� �� ����
    {
        ResetGroup();
        for (int i = 0; i < 30; i++)
        {
            if (check_tiles[i] == 't')
            {
                int check = 0;
                for (int j = 0; j < near_tiles[i].Count; j++)
                {
                    if (visited_final_tiles[near_tiles[i][j]] == 1) // �ֺ��� ������ ĵ�� ������
                    {
                        check = 1;
                    }
                }
                if (check == 1)
                {
                    candy_tiles[i].GetComponent<Top_count>().count += 1; // ���̰� �ø���
                    if (candy_tiles[i].GetComponent<Top_count>().count == 2) // ���� 2�� ����
                    {
                        score_num += 480;
                        visited_tiles[i] = 1;
                    }
                }
            }
        }
        // ���� ���� �� ���� ���� �� �޵��� ����
        for (int i = 0; i < 30; i++)
        {
            if (visited_tiles[i] == 1)
                visited_final_tiles[i] = 1;
        }
    }

    private void DestoryCandy(List<int> destory_candy) // ĵ�� ���ֱ�
    {
        //List<int> destory_candy_bgm = new List<int>();
        //FindNearTop();
        for (int i = 0; i < 30; i++)
        {
            if (visited_final_tiles[i] == 1)
            {
                score_num += 20;
                destory_candy.Add(i);
                Destroy(candy_tiles[i]);
                candy_tiles[i] = null;
                //destory_candy_bgm.Add(check_tiles[i]);
                check_tiles[i] = '0';
            }
        }
        //destory_candy_bgm = destory_candy_bgm.Distinct().ToList();
        //PlayBGM(destory_candy_bgm);
        cat_bgm.Play();
    }

    private IEnumerator FillCandy()
    {
        List<int> destory_candy = new List<int>(); // ������ ĵ�� ���� -> ū �� ����
        // ĵ�� ���ְ� ������ ���� ����
        DestoryCandy(destory_candy);

        // ������ ĵ�� ��������
        for (int i = 0; i < destory_candy.Count; i++)
        {
            int candyNum = destory_candy[i]; // ������ ĵ��
            if (visited_final_tiles[candyNum] == 1)
            {
                int nextCandyNum = candy_up_next_level(candyNum); // ������ ĵ�� �� ĵ��
                while (nextCandyNum < 30)
                {
                    if (visited_final_tiles[nextCandyNum] == 0) // ���� �ִ� ĵ�� ã��
                    {
                        yield return StartCoroutine(MoveCandy(nextCandyNum, candyNum));
                        candyNum = candy_up_next_level(candyNum);
                    }
                    nextCandyNum = candy_up_next_level(nextCandyNum);
                }
            }
        }

        // �� �ʿ��� ĵ�� ��������
        int[] check_edge = { 20, 23, 24, 26, 27, 28, 29 };
        for (int i = 0; i < 7; i++)
        {
            while (true)
            {
                int next_tile = FindNearEmptyTilesSmall(check_edge[i]);
                if (next_tile < check_edge[i]) // ������ �ȵ� �� ������ �������ٴ� ���� �ǹ�
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

        int new_candy = CheckGrounpMap(); // map check

        if (new_candy > 0)
        {
            List<int> new_candies = new List<int>();
            Debug.Log("�����ؾ��ϴ� ĵ�� ���� : " + new_candy);

            for (int i = 0; i < new_candy; i++)
            {
                // ���ο� ĵ�� ����
                int start_candy = 29; // ����ϴ� ��
                int new_candy_num = Random.Range(0, 6);
                // ���ο� ĵ�� ���� ��, ������Ʈ / ������Ʈ �� / ������Ʈ ���� ���� �迭�� �� �־��ֱ�
                candy_tiles[start_candy] = Instantiate(Candy_Obj[new_candy_num], new Vector3(tiles[start_candy].transform.position.x, tiles[start_candy].transform.position.y), Quaternion.identity) as GameObject;
                check_tiles[start_candy] = Candy_Obj_name[new_candy_num];
                visited_final_tiles[start_candy] = 0;
                new_candies.Add(start_candy);
                // ĵ�� �������� �ϱ�
                for (int j = 0; j < (i + 1); j++)
                {
                    int next_tile = FindNearEmptyTilesSmall(new_candies[j]);
                    if (next_tile < new_candies[j]) // ������ �ȵ� �� ������ �������ٴ� ���� �ǹ�
                    {
                        yield return StartCoroutine(MoveCandy(new_candies[j], next_tile));
                        new_candies[j] = next_tile;
                    }
                }
            }
            
            new_candy = CheckGrounpMap(); // map check
            //finish_check = true;
            Debug.Log("finish");
        }
    }

    private void CheckUpDown(int candyNum)
    {
        Queue<int> q = new Queue<int>();
        q.Enqueue(candyNum);
        visited_tiles[candyNum] = 1;

        while (q.Count > 0)
        {
            candyNum = q.Dequeue();

            // ���Ʒ� Ȯ��
            int check_num_1;
            int check_num_2;

            if (candyNum == 0 || candyNum == 29) // ������ ���� ó�� ���� �� ȿ�������� ���� �ʿ�
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

            if (Check_Near_Same_Candy(candyNum, check_num_1) == 1)
            {
                q.Enqueue(check_num_1);
                visited_tiles[check_num_1] = 1;
            }

            if (Check_Near_Same_Candy(candyNum, check_num_2) == 1)
            {
                q.Enqueue(check_num_2);
                visited_tiles[check_num_2] = 1;
            }
        }
    }

    private void CheckLeftDia(int candyNum)
    {
        // ���� �밢�� Ȯ��
        Queue<int> q = new Queue<int>();
        q.Enqueue(candyNum);
        visited_tiles[candyNum] = 1;

        while (q.Count > 0)
        {
            candyNum = q.Dequeue();

            // ���Ʒ� Ȯ��
            int check_num_1;
            int check_num_2;

            if (candyNum == 0 || candyNum == 29) // ������ ���� ó�� ���� �� ȿ�������� ���� �ʿ�
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

            if (Check_Near_Same_Candy(candyNum, check_num_1) == 1)
            {
                q.Enqueue(check_num_1);
                visited_tiles[check_num_1] = 1;
            }

            if (Check_Near_Same_Candy(candyNum, check_num_2) == 1)
            {
                q.Enqueue(check_num_2);
                visited_tiles[check_num_2] = 1;
            }
        }
    }

    private void CheckGroup()
    {
        int check_num = 0;
        for (int i = 0; i < 30; i++)
        {
            if (visited_tiles[i] == 1)
                check_num += 1;
        }
        if (check_num >= 3) // �׷��� ���� ���
        {
            Check_destory = true;
            SaveDestoryCandy();
        }
    }

    private int Check_Near_Same_Candy(int candy_num, int check_num)
    {
        if (0 <= check_num && check_num < 30) // ������ ���� �ʰ�
        {
            if (visited_final_tiles[check_num] == 0) // ĵ�� �������
            {
                Vector2 pos1 = tiles[candy_num].transform.position;
                Vector2 pos2 = tiles[check_num].transform.position;
                if (Vector3.Distance(pos1, pos2) < 1.3f) // �ֺ� ĵ�� �Ǻ�
                {
                    if (visited_tiles[check_num] == 0) // �鸮�� ���� ��
                    {
                        if (check_tiles[candy_num] == check_tiles[check_num]) // ���� ���� ���� ��
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
        // ������ �밢�� Ȯ��
        Queue<int> q = new Queue<int>();
        q.Enqueue(candyNum);
        visited_tiles[candyNum] = 1;

        while (q.Count > 0)
        {
            candyNum = q.Dequeue();

            // ���Ʒ� Ȯ��
            int check_num_1;
            int check_num_2;

            if (candyNum == 0) // ������ ���� ó�� ���� �� ȿ�������� ���� �ʿ�
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

            if (Check_Near_Same_Candy(candyNum, check_num_1) == 1)
            {
                q.Enqueue(check_num_1);
                visited_tiles[check_num_1] = 1;
            }

            if (Check_Near_Same_Candy(candyNum, check_num_2) == 1)
            {
                q.Enqueue(check_num_2);
                visited_tiles[check_num_2] = 1;
            }
        }
    }

    private void ResetGroup()
    {
        for (int i = 0; i < 30; i++)
        {
            visited_tiles[i] = 0;
        }
    }

    private void SaveDestoryCandy() // ���� ĵ�� ����
    {
        for (int i = 0; i < 30; i++)
        {
            if (visited_tiles[i] == 1)
                visited_final_tiles[i] = 1;
        }
    }

    private void CheckFourGroup()
    {
        // 4�� �̻� Ÿ�� �׷쿡 �����ҷ��� �� �ֺ��� 2�� �̻� ���� Ÿ�� ���� �ʿ�
        int check_num = 0;
        for (int i = 0; i < 30; i++)
        {
            if (visited_tiles[i] == 1)
            {
                int countNearTile = 0;
                for (int j = 0; j < near_tiles[i].Count; j++) // �ֺ����� ���� Ÿ��
                {
                    if (visited_tiles[near_tiles[i][j]] == visited_tiles[i])
                    {
                        countNearTile++;
                    }
                }
                if (countNearTile >= 2) // �ֺ����� ���� Ÿ���� 2�� �̻� ������ ���
                    check_num += 1; // 4�� �׷� �Ͽ� �������� ����
                else
                    visited_tiles[i] = 0;
            }
        }
        if (check_num >= 4) // �׷��� ���� ���
        {
            Check_destory = true;
            SaveDestoryCandy();
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
            for (int j = 0; j < near_tiles[candyNum].Count; j++) // �ֺ����� ���� Ÿ�� ã��
            {
                if (check_tiles[near_tiles[candyNum][j]] == check_tiles[candyNum]) // ���ٸ�
                {
                    if (Check_Near_Same_Candy(candyNum, near_tiles[candyNum][j]) == 1)
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
            // Ÿ���� Ȯ���ϴµ� BFS Ȱ��
            // 4�� �̻� Ÿ�� Ȯ��
            //CheckFindFourGroup(candyNum);
            //CheckFourGroup();
            //ResetGroup();
            // ������ �밢�� Ȯ��
            CheckRightDia(candyNum);
            CheckGroup();
            ResetGroup();
            // ���� �밢�� Ȯ��
            CheckLeftDia(candyNum);
            CheckGroup();
            ResetGroup();
            // ���Ʒ� Ȯ��
            CheckUpDown(candyNum);
            CheckGroup();
            ResetGroup();
        }
    }

    private IEnumerator ChangeCandy(int click_1_candy_num, int click_2_candy_num)
    {
        Vector2 pos1 = tiles[click_1_candy_num].transform.position;
        Vector2 pos2 = tiles[click_2_candy_num].transform.position;
        if (Vector3.Distance(pos1, pos2) < 1) // �ֺ� ĵ������ �Ǻ�
        {
            while (Vector3.Distance(candy_tiles[click_1_candy_num].transform.position, pos2) > 0.01f)
            {
                candy_tiles[click_1_candy_num].transform.position = Vector3.MoveTowards(candy_tiles[click_1_candy_num].transform.position, pos2, 0.02f);
                candy_tiles[click_2_candy_num].transform.position = Vector3.MoveTowards(candy_tiles[click_2_candy_num].transform.position, pos1, 0.02f);
                yield return new WaitForSeconds(0.001f);
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
        yield return StartCoroutine(ChangeCandy(click_1_candy_num, click_2_candy_num));
        
        CheckSameCandy(click_1_candy_num);
        CheckSameCandy(click_2_candy_num);

        if (Check_destory == true)
        {
            // ĵ�� ���ְ� ������ ���� ����
            // ������ ���� ä��� + Ȯ��
            yield return StartCoroutine(FillCandy());
            finish_check = true;
        }
        else
        {
            // ���� ���
            // �ٽ� ��ġ ����ġ
            yield return StartCoroutine(ChangeCandy(click_2_candy_num, click_1_candy_num));
            wrong_move_num -= 1;
            Heart_Obj[wrong_move_num].SetActive(false);
            button_bgm.Play();
            finish_check = true;
        }
    }
    
    private int FindNearTilesToPos(int num, Vector3 pos) // �ι�° Ÿ���� ã�µ� ���
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
        for (int i = 0; i < 30; i++)
        {
            if (check_distance > Vector3.Distance(vector, tiles[i].transform.position))
            {
                check_distance = Vector3.Distance(vector, tiles[i].transform.position);
                check_num = i;
            }
        }
        return check_num;
    }

    private void FindNearTiles() // ��ó Ÿ�� ��ȣ ã�Ƽ� ����
    {
        for (int i = 0; i < 30; i++)
        {
            near_tiles.Add(new List<int>());
            for (int j = 0; j < 30; j++)
            {
                if (i != j && Vector3.Distance(tiles[i].transform.position, tiles[j].transform.position) < 1.2f)
                {
                    near_tiles[i].Add(j);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                //Debug.Log("Began");
                Touch touch = Input.GetTouch(0);
                Vector2 pos = Camera.main.ScreenToWorldPoint(touch.position);
                if (finish_check == true)
                {
                    click_1_candy_num = FindTileNum(pos); // �ٲ� ù��° ĵ�� ���ϱ�
                    if (Vector3.Distance(tiles[click_1_candy_num].transform.position, pos) > 0.5f)
                        click_1_candy_num = -1;
                }
            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                //Debug.Log("Ended");
                Touch touch = Input.GetTouch(0);
                Vector2 pos = Camera.main.ScreenToWorldPoint(touch.position);
                if (finish_check == true)
                {
                    click_2_candy_num = FindNearTilesToPos(click_1_candy_num, pos); // �ٲ� �ι�° ĵ�� ���ϱ�
                                                                                    // �ٲ� ĵ����� ������ �Ǿ��ٸ�
                    if (click_2_candy_num != click_1_candy_num && click_1_candy_num != -1) // ���� ��ġ ĵ�� ���� ó��
                    {
                        Debug.Log("���õ� ĵ�� : " + click_1_candy_num + " " + click_2_candy_num);
                        finish_check = false;
                        StartCoroutine(CheckCandy(click_1_candy_num, click_2_candy_num));
                    }
                }
            }
        }
        if(wrong_move_num == 0)
        {
            finish_check = false;
            UI_manager.GetComponent<UI_manager>().score = score_num;
            UI_manager.GetComponent<UI_manager>().OnresultImage();
        }
        score_text.text = score_num.ToString();
        int check_last = 0;
        for (int i = 0; i < 30; i++)
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
