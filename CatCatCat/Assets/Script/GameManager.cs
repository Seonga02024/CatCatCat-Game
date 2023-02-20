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
    public GameObject[] tiles = new GameObject[30]; // 육각형 위치
    public GameObject[] candy_tiles; // 육각형 위치에 따른 캔디 오브젝트 
    private char[] check_tiles = new char[30]; // 육각형 위치에 따른 값들
    private int[] visited_tiles = new int[30];
    private int[] visited_final_tiles = new int[30]; // 육각형 위치에 따른 현재 캔디 존재 상태
    public GameObject[] Candy_Obj = new GameObject[7];
    public GameObject[] Heart_Obj = new GameObject[3];
    public AudioSource cat_bgm;
    public AudioSource button_bgm;
    public GameObject UI_manager;
    private char[] Candy_Obj_name = { 'g', 'b', 'y', 'r', 'p', 'o', 't' };
    List<List<int>> near_tiles = new List<List<int>>(); // 각 타일의 주변 타일 저장
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
        CreateFrist(); // 처음 맵 세팅
    }

    private void CreateFrist()
    {
        candy_tiles = new GameObject[30];
        string MapSetting = "oyrgpobyrbpopgrggprpygbyogooyb"; // 처음 맵 구성 p b y r g
        //string MapSetting = "ptbtbbgprbptbbrggprpygbytgtttb"; // 처음 맵 구성
        //string MapSetting = "ttttptbyrbptpgrggprpygbytgtttb"; // 처음 맵 구성

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
        // 그룹되는지 다시 맵 확인하기 
        for (int i = 0; i < 30; i++)
        {
            if (visited_final_tiles[i] == 0) // 채워진 공간 확인
            {
                CheckSameCandy(i);
                if (Check_destory == true)
                {
                    StartCoroutine(FillCandy());
                    new_candy = 0;
                    return new_candy;
                }
            }
            else // 비어진 공간 확인
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

    private int FindNearEmptyTilesSmall(int num) // 근처 타일 중에 비어진 타일 찾아서 해당 번호 반환
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
        if (candyNum == 0 || candyNum == 25) // 일일이 예외 처리 말고 더 효율적으로 수정 필요
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

    private void FindNearTop() // 근처 팽이 찾아서 값 조정
    {
        ResetGroup();
        for (int i = 0; i < 30; i++)
        {
            if (check_tiles[i] == 't')
            {
                int check = 0;
                for (int j = 0; j < near_tiles[i].Count; j++)
                {
                    if (visited_final_tiles[near_tiles[i][j]] == 1) // 주변에 없어질 캔디가 있으면
                    {
                        check = 1;
                    }
                }
                if (check == 1)
                {
                    candy_tiles[i].GetComponent<Top_count>().count += 1; // 팽이값 올리고
                    if (candy_tiles[i].GetComponent<Top_count>().count == 2) // 만약 2면 삭제
                    {
                        score_num += 480;
                        visited_tiles[i] = 1;
                    }
                }
            }
        }
        // 삭제 팽이 옆 팽이 영향 안 받도록 따로
        for (int i = 0; i < 30; i++)
        {
            if (visited_tiles[i] == 1)
                visited_final_tiles[i] = 1;
        }
    }

    private void DestoryCandy(List<int> destory_candy) // 캔디 없애기
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
        List<int> destory_candy = new List<int>(); // 삭제된 캔디 작은 -> 큰 순 저장
        // 캔디 없애고 없어진 공간 저장
        DestoryCandy(destory_candy);

        // 위에서 캔디 내려오기
        for (int i = 0; i < destory_candy.Count; i++)
        {
            int candyNum = destory_candy[i]; // 삭제된 캔디
            if (visited_final_tiles[candyNum] == 1)
            {
                int nextCandyNum = candy_up_next_level(candyNum); // 삭제된 캔디 위 캔디
                while (nextCandyNum < 30)
                {
                    if (visited_final_tiles[nextCandyNum] == 0) // 위의 있는 캔디 찾음
                    {
                        yield return StartCoroutine(MoveCandy(nextCandyNum, candyNum));
                        candyNum = candy_up_next_level(candyNum);
                    }
                    nextCandyNum = candy_up_next_level(nextCandyNum);
                }
            }
        }

        // 끝 쪽에서 캔디 내려오기
        int[] check_edge = { 20, 23, 24, 26, 27, 28, 29 };
        for (int i = 0; i < 7; i++)
        {
            while (true)
            {
                int next_tile = FindNearEmptyTilesSmall(check_edge[i]);
                if (next_tile < check_edge[i]) // 성립이 안될 시 끝까지 내려갔다는 것을 의미
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
            Debug.Log("생성해야하는 캔디 갯수 : " + new_candy);

            for (int i = 0; i < new_candy; i++)
            {
                // 새로운 캔디 생성
                int start_candy = 29; // 출발하는 곳
                int new_candy_num = Random.Range(0, 6);
                // 새로운 캔디 생성 시, 오브젝트 / 오브젝트 값 / 오브젝트 존재 여부 배열에 값 넣어주기
                candy_tiles[start_candy] = Instantiate(Candy_Obj[new_candy_num], new Vector3(tiles[start_candy].transform.position.x, tiles[start_candy].transform.position.y), Quaternion.identity) as GameObject;
                check_tiles[start_candy] = Candy_Obj_name[new_candy_num];
                visited_final_tiles[start_candy] = 0;
                new_candies.Add(start_candy);
                // 캔디 내려오게 하기
                for (int j = 0; j < (i + 1); j++)
                {
                    int next_tile = FindNearEmptyTilesSmall(new_candies[j]);
                    if (next_tile < new_candies[j]) // 성립이 안될 시 끝까지 내려갔다는 것을 의미
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

            // 위아래 확인
            int check_num_1;
            int check_num_2;

            if (candyNum == 0 || candyNum == 29) // 일일이 예외 처리 말고 더 효율적으로 수정 필요
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
        // 왼쪽 대각선 확인
        Queue<int> q = new Queue<int>();
        q.Enqueue(candyNum);
        visited_tiles[candyNum] = 1;

        while (q.Count > 0)
        {
            candyNum = q.Dequeue();

            // 위아래 확인
            int check_num_1;
            int check_num_2;

            if (candyNum == 0 || candyNum == 29) // 일일이 예외 처리 말고 더 효율적으로 수정 필요
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
        if (check_num >= 3) // 그룹이 있을 경우
        {
            Check_destory = true;
            SaveDestoryCandy();
        }
    }

    private int Check_Near_Same_Candy(int candy_num, int check_num)
    {
        if (0 <= check_num && check_num < 30) // 범위가 넘지 않고
        {
            if (visited_final_tiles[check_num] == 0) // 캔디가 있을경우
            {
                Vector2 pos1 = tiles[candy_num].transform.position;
                Vector2 pos2 = tiles[check_num].transform.position;
                if (Vector3.Distance(pos1, pos2) < 1.3f) // 주변 캔디 판별
                {
                    if (visited_tiles[check_num] == 0) // 들리지 않은 곳
                    {
                        if (check_tiles[candy_num] == check_tiles[check_num]) // 같은 값을 가질 때
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
        // 오른쪽 대각선 확인
        Queue<int> q = new Queue<int>();
        q.Enqueue(candyNum);
        visited_tiles[candyNum] = 1;

        while (q.Count > 0)
        {
            candyNum = q.Dequeue();

            // 위아래 확인
            int check_num_1;
            int check_num_2;

            if (candyNum == 0) // 일일이 예외 처리 말고 더 효율적으로 수정 필요
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

    private void SaveDestoryCandy() // 없앨 캔디 저장
    {
        for (int i = 0; i < 30; i++)
        {
            if (visited_tiles[i] == 1)
                visited_final_tiles[i] = 1;
        }
    }

    private void CheckFourGroup()
    {
        // 4개 이상 타일 그룹에 부합할려면 내 주변에 2개 이상 같은 타일 존재 필요
        int check_num = 0;
        for (int i = 0; i < 30; i++)
        {
            if (visited_tiles[i] == 1)
            {
                int countNearTile = 0;
                for (int j = 0; j < near_tiles[i].Count; j++) // 주변에서 같은 타일
                {
                    if (visited_tiles[near_tiles[i][j]] == visited_tiles[i])
                    {
                        countNearTile++;
                    }
                }
                if (countNearTile >= 2) // 주변에서 같은 타일이 2개 이상 인접한 경우
                    check_num += 1; // 4개 그룹 일원 조건으로 부합
                else
                    visited_tiles[i] = 0;
            }
        }
        if (check_num >= 4) // 그룹이 있을 경우
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
            for (int j = 0; j < near_tiles[candyNum].Count; j++) // 주변에서 같은 타일 찾기
            {
                if (check_tiles[near_tiles[candyNum][j]] == check_tiles[candyNum]) // 같다면
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
            // 타일을 확인하는데 BFS 활용
            // 4개 이상 타일 확인
            //CheckFindFourGroup(candyNum);
            //CheckFourGroup();
            //ResetGroup();
            // 오른쪽 대각선 확인
            CheckRightDia(candyNum);
            CheckGroup();
            ResetGroup();
            // 왼쪽 대각선 확인
            CheckLeftDia(candyNum);
            CheckGroup();
            ResetGroup();
            // 위아래 확인
            CheckUpDown(candyNum);
            CheckGroup();
            ResetGroup();
        }
    }

    private IEnumerator ChangeCandy(int click_1_candy_num, int click_2_candy_num)
    {
        Vector2 pos1 = tiles[click_1_candy_num].transform.position;
        Vector2 pos2 = tiles[click_2_candy_num].transform.position;
        if (Vector3.Distance(pos1, pos2) < 1) // 주변 캔디인지 판별
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
            // 캔디 없애고 없어진 공간 저장
            // 없어진 공간 채우기 + 확인
            yield return StartCoroutine(FillCandy());
            finish_check = true;
        }
        else
        {
            // 없을 경우
            // 다시 위치 원위치
            yield return StartCoroutine(ChangeCandy(click_2_candy_num, click_1_candy_num));
            wrong_move_num -= 1;
            Heart_Obj[wrong_move_num].SetActive(false);
            button_bgm.Play();
            finish_check = true;
        }
    }
    
    private int FindNearTilesToPos(int num, Vector3 pos) // 두번째 타일을 찾는데 사용
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

    private void FindNearTiles() // 근처 타일 번호 찾아서 저장
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
                    click_1_candy_num = FindTileNum(pos); // 바꿀 첫번째 캔디 정하기
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
                    click_2_candy_num = FindNearTilesToPos(click_1_candy_num, pos); // 바꿀 두번째 캔디 정하기
                                                                                    // 바꿀 캔디들이 선택이 되었다면
                    if (click_2_candy_num != click_1_candy_num && click_1_candy_num != -1) // 같은 위치 캔디 방지 처리
                    {
                        Debug.Log("선택된 캔디 : " + click_1_candy_num + " " + click_2_candy_num);
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
