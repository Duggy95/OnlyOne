using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MazeMake : MonoBehaviourPun
{
    // public Cell cellPrefabs;  // 미로 한 칸을 만들 cell 프리팹
    public GameObject portal;  // 포탈
    public GameObject player;  // 플레이어
    public GameObject endGround;  // 결승선
    public GameObject pushzonePrefabs;  // 점프존
    public GameObject removeFog;  // 안개효과 지울 충돌체
    public GameObject coinPrefabs;  // 코인 프리팹
    public Text coinCountTxt;  // 코인 개수 텍스트
    public GameObject maze_One;
    public GameObject maze_Two;
    //public Image fogEff;

    GameObject[,] cellMap;  // 미로를 채울 cell 이차원 배열
    List<GameObject> cellHistoryList;
    List<GameObject> coins = new List<GameObject>();   // 코인 리스트
    List<Transform> cellPos = new List<Transform>();
    List<GameObject> pushzonePos = new List<GameObject>();
    Transform coinRandomPos;  // 생성된 코인의 랜덤 위치
    Vector3 randomPos;  // 포탈이 플레이어를 보낼 랜덤 위치값을 저장할 변수
    Vector3 coinDistance = new Vector3(0, 5, 0);  // 코인 및 플레이어와 바닥과의 거리
    Vector3 pushzoneDistance = new Vector3(0, 1.5f, 0);  // 푸쉬존 오브젝트와 맵 바닥과의 거리
    PhotonView pv;
    AudioSource audioSource;

    int width;   // 미로 칸 수
    int height;  // 미로 줄 수
    int size = 10;  // cell 하나의 길이
    int coinMax = 20;  // 최대 코인 생성 개수
    int coinCount = 0;  // 현재 코인 수
    int coinCountMax = 15;  //  모아야 하는 총 코인 수
    int pushzoneMax = 25;  // 푸쉬존 총 개수
    // float startDelay = 2;
    // 현재 맵 생성 오브젝트의 위치를 저장할 변수
    float x;
    float y;
    float z;

    private void Awake()
    {
        x = transform.position.x;
        y = transform.position.y;
        z = transform.position.z;
        coinCountTxt = GameObject.Find("CoinCountText").GetComponent<Text>();
        coinCountTxt.text = "Coin : " + coinCount + " / " + coinCountMax;  // coinCountTxt 업데이트
        pv = GetComponent<PhotonView>();
        audioSource = GetComponent<AudioSource>();
        player = GameManager.instance.player;

        //fogEff = GameObject.Find("FogImg").GetComponent<Image>();
    }

    void Start()
    {
        RenderSettings.fog = true;
        //fogEff.gameObject.SetActive(false);
        coinCountTxt.gameObject.SetActive(false);   // 코인 세는 텍스트 비활성화
        portal.transform.position = new Vector3(x, y, z + 10 * size + 20);  // 포탈위치
        removeFog.transform.position = new Vector3(x, y, z + (10 * size));   // 충돌체 위치

        if (!PhotonNetwork.IsMasterClient)
            return;

        if (gameObject.name == "Maze1")
        {
            width = 10;  // 10 * 10 사이즈로 제작
            height = 10;
            transform.position = new Vector3(x - ((width * size) / 2) + (size / 2f), y, z);  // 미로 생성 시작점

            BatckCells();  // 셀 배치 함수
            MakeMaze(cellMap[0, 0]);  // 미로 만들 함수

            // 미로의 입구와 출구 랜덤 위치에 생성
            cellMap[Random.Range(0, Mathf.FloorToInt(width / 2)), 0].gameObject.GetComponent<Cell>().isBackWall = false;
            cellMap[Random.Range(Mathf.FloorToInt(width / 2), width), 0].gameObject.GetComponent<Cell>().isBackWall = false;
            cellMap[Random.Range(0, Mathf.FloorToInt(width / 2)), height - 1].gameObject.GetComponent<Cell>().isForwardWall = false;
            cellMap[Random.Range(Mathf.FloorToInt(width / 2), width), height - 1].gameObject.GetComponent<Cell>().isForwardWall = false;
        }

        else if (gameObject.name == "Maze2")
        {
            width = 15;  // 15 * 15 사이즈로 제작
            height = 15;
            transform.position = new Vector3(x - ((width * size) / 2) + (size / 2f), y, z + (height * size));

            BatckCells();
            MakeMaze(cellMap[0, 0]);  // 미로 만들 함수
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // 게임오브젝트의 이름이 Maze2이고 코인 개수가 최대 개수보다 작을 때 코인 생성 함수 호출
        if (gameObject.name == "Maze2" && coins.Count < coinMax)
            CoinSetting();

        // 게임오브젝트의 이름이 Maze2이고 푸쉬존의 개수가 최대개수보다 작을 때 푸쉬존 생성 함수 호출
        if (gameObject.name == "Maze2" && pushzonePos.Count < coinMax)
            PushZoneSetting();
    }

    public void FogAttive()
    {
        //fogEff.gameObject.SetActive(false);
        coinCountTxt.gameObject.SetActive(true);   // 코인 세는 텍스트 비활성화
    }

    public void RandomPos() // 포탈을 타면 미로2의 랜덤 포지션으로 이동
    {
        randomPos = cellPos[Random.Range(0, cellPos.Count)].transform.position + coinDistance;
        player.transform.position = randomPos;
    }

    void BatckCells()
    {
        cellMap = new GameObject[width, height];
        cellHistoryList = new List<GameObject>();

        // 미로 1 생성
        if (gameObject.name == "Maze1")
        {
            for (int i = 0; i < width; i++)   // 칸 수와 줄 수만큼 생성
            {
                for (int j = 0; j < height; j++)
                {
                    GameObject _cell = PhotonNetwork.Instantiate("Two/Cell", transform.position + new Vector3(i * size, 0, j * size), transform.rotation);
                    int id = _cell.GetComponent<PhotonView>().ViewID;
                    pv.RPC("CellSet", RpcTarget.All, id, 1);
                    //_cell.transform.parent = transform;
                    Cell cell = _cell.GetComponent<Cell>();
                    cell.index = new Vector2Int(i, j);  // cell의 주소를 2개의 int값으로 나타냄
                    _cell.name = "cell" + i + "_" + j;   // 각 cell의 이름은 n-1칸, n-1줄
                                                         // 셀의 좌표에 따라 크기만큼 이동하며 배치
                                                         //_cell.transform.localPosition = new Vector3(i * size, 0, j * size);

                    cellMap[i, j] = _cell;  // cell의 이차원 배열에 생성된 각 cell 저장
                }
            }
        }

        // 미로 2 생성
        else if (gameObject.name == "Maze2")
        {
            for (int i = 0; i < width; i++)   // 칸 수와 줄 수만큼 생성
            {
                for (int j = 0; j < height; j++)
                {
                    GameObject _cell = PhotonNetwork.Instantiate("Two/Cell", transform.position + new Vector3(i * size, 0, j * size), transform.rotation);
                    int id = _cell.GetComponent<PhotonView>().ViewID;
                    pv.RPC("CellSet", RpcTarget.All, id, 2);
                    //_cell.transform.parent = transform;
                    Cell cell = _cell.GetComponent<Cell>();
                    cell.index = new Vector2Int(i, j);  // cell의 주소를 2개의 int값으로 나타냄
                    _cell.name = "cell" + i + "_" + j;   // 각 cell의 이름은 n-1칸, n-1줄
                                                         // 셀의 좌표에 따라 크기만큼 이동하며 배치

                    cellMap[i, j] = _cell;  // cell의 이차원 배열에 생성된 각 cell 저장
                    //cellPos.Add(_cell.gameObject.transform); // cell의 위치를 리스트에 추가
                }
            }
        }
    }

    [PunRPC]
    void CellSet(int id, int mazeName)
    {
        GameObject[] cells = GameObject.FindGameObjectsWithTag("CELL");

        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].GetComponent<PhotonView>().ViewID == id)
            {
                if (1 == mazeName)
                {
                    cells[i].transform.parent = maze_One.transform;
                }
                else if (2 == mazeName)
                {
                    cells[i].transform.parent = maze_Two.transform;
                    cellPos.Add(cells[i].gameObject.transform); // cell의 위치를 리스트에 추가
                }
            }
        }
    }

    void MakeMaze(GameObject startCell)
    {
        GameObject[] neighbor = GetNeighborCells(startCell);
        if (neighbor.Length > 0)
        {
            GameObject nextCell = neighbor[Random.Range(0, neighbor.Length)];
            ConnectCells(startCell, nextCell);
            cellHistoryList.Add(nextCell);
            MakeMaze(nextCell);
        }

        else
        {
            if (cellHistoryList.Count > 0)
            {
                GameObject lastCell = cellHistoryList[cellHistoryList.Count - 1];
                cellHistoryList.Remove(lastCell);
                MakeMaze(lastCell);
            }
        }
    }

    GameObject[] GetNeighborCells(GameObject cell)
    {
        List<GameObject> retCellList = new List<GameObject>();
        Vector2Int index = cell.GetComponent<Cell>().index;
        // forward
        if (index.y + 1 < height)
        {
            GameObject neighbor = cellMap[index.x, index.y + 1];
            if (neighbor.GetComponent<Cell>().CheckAllWall())
            {
                retCellList.Add(neighbor);
            }
        }
        // back
        if (index.y - 1 >= 0)
        {
            GameObject neighbor = cellMap[index.x, index.y - 1];
            if (neighbor.GetComponent<Cell>().CheckAllWall())
            {
                retCellList.Add(neighbor);
            }
        }
        // left
        if (index.x - 1 >= 0)
        {
            GameObject neighbor = cellMap[index.x - 1, index.y];
            if (neighbor.GetComponent<Cell>().CheckAllWall())
            {
                retCellList.Add(neighbor);
            }
        }
        // right
        if (index.x + 1 < height)
        {
            GameObject neighbor = cellMap[index.x + 1, index.y];
            if (neighbor.GetComponent<Cell>().CheckAllWall())
            {
                retCellList.Add(neighbor);
            }
        }

        return retCellList.ToArray();
    }

    //[PunRPC]
    void ConnectCells(GameObject c0, GameObject c1)
    {
        Vector2Int dir = c0.GetComponent<Cell>().index - c1.GetComponent<Cell>().index;

        // forward
        if (dir.y <= -1)
        {
            c0.GetComponent<Cell>().isForwardWall = false;
            c1.GetComponent<Cell>().isBackWall = false;
        }
        // back
        else if (dir.y >= 1)
        {
            c0.GetComponent<Cell>().isBackWall = false;
            c1.GetComponent<Cell>().isForwardWall = false;
        }
        // left
        else if (dir.x >= 1)
        {
            c0.GetComponent<Cell>().isLeftWall = false;
            c1.GetComponent<Cell>().isRightWall = false;
        }
        // right
        else if (dir.x <= -1)
        {
            c0.GetComponent<Cell>().isRightWall = false;
            c1.GetComponent<Cell>().isLeftWall = false;
        }
    }

    void CoinSetting()
    {
        for (int i = coins.Count; i < coinMax; i++)   // 코인의 리스트의 수부터 coinMax 미만이 될 때까지 반복
        {
            coinRandomPos = cellPos[Random.Range(0, cellPos.Count)]; // 생성된 셀프리팹 리스트 중 랜덤 하나 저장
            GameObject _coin = PhotonNetwork.Instantiate("Two/MazeCoin", coinRandomPos.position + coinDistance, Quaternion.identity);  // 코인프리팹 인스턴스화 
            int id = _coin.GetComponent<PhotonView>().ViewID;

            _coin.transform.up = transform.forward;
            _coin.transform.parent = transform;
            //_coin.transform.position = coinRandomPos.position + coinDistance; // 생성된 코인의 위치를 랜덤 저장된 셀 위치로 이동

            for (int j = 0; j < coins.Count; j++)  // 코인 위치를 담은 리스트의 크기만큼 반복
            {
                // 생성된 코인의 위치와 코인 위치 리스트에 담긴 위치를 비교하여 같은 게 있으면
                if (_coin.transform.position == coins[j].transform.position)
                {
                    PhotonNetwork.Destroy(_coin);  // 생성된 코인 삭제
                    _coin = null;  // null값 저장
                    break;  // 반복문 탈출
                }
            }
            if (_coin != null)  //  널값이 아니면
            {
                coins.Add(_coin);  // 생성된 코인을 리스트에 추가
            }
        }
        Debug.Log("coin 개수를 저장한 리스트의 크기 : " + coins.Count);
    }

    void PushZoneSetting()
    {
        for (int i = pushzonePos.Count; i < pushzoneMax; i++)
        {
            coinRandomPos = cellPos[Random.Range(0, cellPos.Count)]; // 생성된 셀프리팹 리스트 중 랜덤 하나에
            GameObject _jumpZone = PhotonNetwork.Instantiate("Two/PushZoneSphere", coinRandomPos.transform.position + pushzoneDistance, transform.rotation);  // 점프존프리팹 인스턴스화 
            _jumpZone.transform.parent = transform;
            // _jumpZone.transform.position = coinRandomPos.transform.position + pushzoneDistance; // 생성된 점프존의 위치를 랜덤 저장된 셀 위치로 이동

            for (int j = 0; j < pushzonePos.Count; j++)  // 점프존 위치를 담은 리스트의 크기만큼 반복
            {
                // 생성된 점프존의 위치와 점프존 위치 리스트에 담긴 위치를 비교하여 같은 게 있으면
                if (_jumpZone.transform.position == pushzonePos[j].transform.position)
                {
                    PhotonNetwork.Destroy(_jumpZone);  // 생성된 점프존 삭제
                    _jumpZone = null;  // null값 저장
                    break;  // 반복문 탈출
                }
            }
            if (_jumpZone != null)  //  널값이 아니면
            {
                pushzonePos.Add(_jumpZone);  // 생성된 점프존을 리스트에 추가
            }
            Debug.Log("pushZone 개수를 저장한 리스트의 크기 : " + pushzonePos.Count);
        }
    }

    // 점프존이 플레이어와 닿으면 다른 위치로 이동시키는 함수
    public void PushZoneMove(GameObject push)
    {
        audioSource.PlayOneShot(SoundManager.instance.jumpClip, 1f);
        int id = push.GetComponent<PhotonView>().ViewID;
        pv.RPC("MoveRPC", RpcTarget.MasterClient, id);
    }

    [PunRPC]
    void MoveRPC(int id)
    {
        for (int i = 0; i < pushzonePos.Count; i++)
        {
            if (pushzonePos[i].GetComponent<PhotonView>().ViewID == id)
            {
                PhotonNetwork.Destroy(pushzonePos[i]);
                PushZoneSetting();
                pushzonePos.Remove(pushzonePos[i]);
            }
        }
    }

    public void CoinCount(int count, GameObject coin)
    {
        audioSource.PlayOneShot(SoundManager.instance.scoreClip, 0.5f);

        coinCount += count;   // 현재 coinCount에 coin 추가
        coinCountTxt.text = "Coin : " + coinCount + " / " + coinCountMax;  // coinCountTxt 업데이트

        int id = coin.GetComponent<PhotonView>().ViewID;
        pv.RPC("CoinTxtRPC", RpcTarget.MasterClient, count, id);

        if (coinCount >= coinCountMax)  // 현재 coinCount가 coinCountMax 이상이면 플레이어의 위치를 결승지점으로 이동
        {
            player.transform.position = endGround.transform.position + new Vector3(Random.Range(-10, 10), 5, Random.Range(-10, -5));
        }
    }

    [PunRPC]
    void CoinTxtRPC(int count, int id)
    {
        for (int i = 0; i < coins.Count; i++)
        {
            if (coins[i].GetComponent<PhotonView>().ViewID == id)
            {
                PhotonNetwork.Destroy(coins[i]);
                CoinSetting();
                coins.Remove(coins[i]);
            }
        }
        Debug.Log("coin 개수를 저장한 리스트의 크기 : " + coins.Count);
    }

}
