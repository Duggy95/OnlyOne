using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MazeMake : MonoBehaviourPun
{
    // public Cell cellPrefabs;  // �̷� �� ĭ�� ���� cell ������
    public GameObject portal;  // ��Ż
    public GameObject player;  // �÷��̾�
    public GameObject endGround;  // ��¼�
    public GameObject pushzonePrefabs;  // ������
    public GameObject removeFog;  // �Ȱ�ȿ�� ���� �浹ü
    public GameObject coinPrefabs;  // ���� ������
    public Text coinCountTxt;  // ���� ���� �ؽ�Ʈ
    public GameObject maze_One;
    public GameObject maze_Two;
    //public Image fogEff;

    GameObject[,] cellMap;  // �̷θ� ä�� cell ������ �迭
    List<GameObject> cellHistoryList;
    List<GameObject> coins = new List<GameObject>();   // ���� ����Ʈ
    List<Transform> cellPos = new List<Transform>();
    List<GameObject> pushzonePos = new List<GameObject>();
    Transform coinRandomPos;  // ������ ������ ���� ��ġ
    Vector3 randomPos;  // ��Ż�� �÷��̾ ���� ���� ��ġ���� ������ ����
    Vector3 coinDistance = new Vector3(0, 5, 0);  // ���� �� �÷��̾�� �ٴڰ��� �Ÿ�
    Vector3 pushzoneDistance = new Vector3(0, 1.5f, 0);  // Ǫ���� ������Ʈ�� �� �ٴڰ��� �Ÿ�
    PhotonView pv;
    AudioSource audioSource;

    int width;   // �̷� ĭ ��
    int height;  // �̷� �� ��
    int size = 10;  // cell �ϳ��� ����
    int coinMax = 20;  // �ִ� ���� ���� ����
    int coinCount = 0;  // ���� ���� ��
    int coinCountMax = 15;  //  ��ƾ� �ϴ� �� ���� ��
    int pushzoneMax = 25;  // Ǫ���� �� ����
    // float startDelay = 2;
    // ���� �� ���� ������Ʈ�� ��ġ�� ������ ����
    float x;
    float y;
    float z;

    private void Awake()
    {
        x = transform.position.x;
        y = transform.position.y;
        z = transform.position.z;
        coinCountTxt = GameObject.Find("CoinCountText").GetComponent<Text>();
        coinCountTxt.text = "Coin : " + coinCount + " / " + coinCountMax;  // coinCountTxt ������Ʈ
        pv = GetComponent<PhotonView>();
        audioSource = GetComponent<AudioSource>();
        player = GameManager.instance.player;

        //fogEff = GameObject.Find("FogImg").GetComponent<Image>();
    }

    void Start()
    {
        RenderSettings.fog = true;
        //fogEff.gameObject.SetActive(false);
        coinCountTxt.gameObject.SetActive(false);   // ���� ���� �ؽ�Ʈ ��Ȱ��ȭ
        portal.transform.position = new Vector3(x, y, z + 10 * size + 20);  // ��Ż��ġ
        removeFog.transform.position = new Vector3(x, y, z + (10 * size));   // �浹ü ��ġ

        if (!PhotonNetwork.IsMasterClient)
            return;

        if (gameObject.name == "Maze1")
        {
            width = 10;  // 10 * 10 ������� ����
            height = 10;
            transform.position = new Vector3(x - ((width * size) / 2) + (size / 2f), y, z);  // �̷� ���� ������

            BatckCells();  // �� ��ġ �Լ�
            MakeMaze(cellMap[0, 0]);  // �̷� ���� �Լ�

            // �̷��� �Ա��� �ⱸ ���� ��ġ�� ����
            cellMap[Random.Range(0, Mathf.FloorToInt(width / 2)), 0].gameObject.GetComponent<Cell>().isBackWall = false;
            cellMap[Random.Range(Mathf.FloorToInt(width / 2), width), 0].gameObject.GetComponent<Cell>().isBackWall = false;
            cellMap[Random.Range(0, Mathf.FloorToInt(width / 2)), height - 1].gameObject.GetComponent<Cell>().isForwardWall = false;
            cellMap[Random.Range(Mathf.FloorToInt(width / 2), width), height - 1].gameObject.GetComponent<Cell>().isForwardWall = false;
        }

        else if (gameObject.name == "Maze2")
        {
            width = 15;  // 15 * 15 ������� ����
            height = 15;
            transform.position = new Vector3(x - ((width * size) / 2) + (size / 2f), y, z + (height * size));

            BatckCells();
            MakeMaze(cellMap[0, 0]);  // �̷� ���� �Լ�
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // ���ӿ�����Ʈ�� �̸��� Maze2�̰� ���� ������ �ִ� �������� ���� �� ���� ���� �Լ� ȣ��
        if (gameObject.name == "Maze2" && coins.Count < coinMax)
            CoinSetting();

        // ���ӿ�����Ʈ�� �̸��� Maze2�̰� Ǫ������ ������ �ִ밳������ ���� �� Ǫ���� ���� �Լ� ȣ��
        if (gameObject.name == "Maze2" && pushzonePos.Count < coinMax)
            PushZoneSetting();
    }

    public void FogAttive()
    {
        //fogEff.gameObject.SetActive(false);
        coinCountTxt.gameObject.SetActive(true);   // ���� ���� �ؽ�Ʈ ��Ȱ��ȭ
    }

    public void RandomPos() // ��Ż�� Ÿ�� �̷�2�� ���� ���������� �̵�
    {
        randomPos = cellPos[Random.Range(0, cellPos.Count)].transform.position + coinDistance;
        player.transform.position = randomPos;
    }

    void BatckCells()
    {
        cellMap = new GameObject[width, height];
        cellHistoryList = new List<GameObject>();

        // �̷� 1 ����
        if (gameObject.name == "Maze1")
        {
            for (int i = 0; i < width; i++)   // ĭ ���� �� ����ŭ ����
            {
                for (int j = 0; j < height; j++)
                {
                    GameObject _cell = PhotonNetwork.Instantiate("Two/Cell", transform.position + new Vector3(i * size, 0, j * size), transform.rotation);
                    int id = _cell.GetComponent<PhotonView>().ViewID;
                    pv.RPC("CellSet", RpcTarget.All, id, 1);
                    //_cell.transform.parent = transform;
                    Cell cell = _cell.GetComponent<Cell>();
                    cell.index = new Vector2Int(i, j);  // cell�� �ּҸ� 2���� int������ ��Ÿ��
                    _cell.name = "cell" + i + "_" + j;   // �� cell�� �̸��� n-1ĭ, n-1��
                                                         // ���� ��ǥ�� ���� ũ�⸸ŭ �̵��ϸ� ��ġ
                                                         //_cell.transform.localPosition = new Vector3(i * size, 0, j * size);

                    cellMap[i, j] = _cell;  // cell�� ������ �迭�� ������ �� cell ����
                }
            }
        }

        // �̷� 2 ����
        else if (gameObject.name == "Maze2")
        {
            for (int i = 0; i < width; i++)   // ĭ ���� �� ����ŭ ����
            {
                for (int j = 0; j < height; j++)
                {
                    GameObject _cell = PhotonNetwork.Instantiate("Two/Cell", transform.position + new Vector3(i * size, 0, j * size), transform.rotation);
                    int id = _cell.GetComponent<PhotonView>().ViewID;
                    pv.RPC("CellSet", RpcTarget.All, id, 2);
                    //_cell.transform.parent = transform;
                    Cell cell = _cell.GetComponent<Cell>();
                    cell.index = new Vector2Int(i, j);  // cell�� �ּҸ� 2���� int������ ��Ÿ��
                    _cell.name = "cell" + i + "_" + j;   // �� cell�� �̸��� n-1ĭ, n-1��
                                                         // ���� ��ǥ�� ���� ũ�⸸ŭ �̵��ϸ� ��ġ

                    cellMap[i, j] = _cell;  // cell�� ������ �迭�� ������ �� cell ����
                    //cellPos.Add(_cell.gameObject.transform); // cell�� ��ġ�� ����Ʈ�� �߰�
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
                    cellPos.Add(cells[i].gameObject.transform); // cell�� ��ġ�� ����Ʈ�� �߰�
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
        for (int i = coins.Count; i < coinMax; i++)   // ������ ����Ʈ�� ������ coinMax �̸��� �� ������ �ݺ�
        {
            coinRandomPos = cellPos[Random.Range(0, cellPos.Count)]; // ������ �������� ����Ʈ �� ���� �ϳ� ����
            GameObject _coin = PhotonNetwork.Instantiate("Two/MazeCoin", coinRandomPos.position + coinDistance, Quaternion.identity);  // ���������� �ν��Ͻ�ȭ 
            int id = _coin.GetComponent<PhotonView>().ViewID;

            _coin.transform.up = transform.forward;
            _coin.transform.parent = transform;
            //_coin.transform.position = coinRandomPos.position + coinDistance; // ������ ������ ��ġ�� ���� ����� �� ��ġ�� �̵�

            for (int j = 0; j < coins.Count; j++)  // ���� ��ġ�� ���� ����Ʈ�� ũ�⸸ŭ �ݺ�
            {
                // ������ ������ ��ġ�� ���� ��ġ ����Ʈ�� ��� ��ġ�� ���Ͽ� ���� �� ������
                if (_coin.transform.position == coins[j].transform.position)
                {
                    PhotonNetwork.Destroy(_coin);  // ������ ���� ����
                    _coin = null;  // null�� ����
                    break;  // �ݺ��� Ż��
                }
            }
            if (_coin != null)  //  �ΰ��� �ƴϸ�
            {
                coins.Add(_coin);  // ������ ������ ����Ʈ�� �߰�
            }
        }
        Debug.Log("coin ������ ������ ����Ʈ�� ũ�� : " + coins.Count);
    }

    void PushZoneSetting()
    {
        for (int i = pushzonePos.Count; i < pushzoneMax; i++)
        {
            coinRandomPos = cellPos[Random.Range(0, cellPos.Count)]; // ������ �������� ����Ʈ �� ���� �ϳ���
            GameObject _jumpZone = PhotonNetwork.Instantiate("Two/PushZoneSphere", coinRandomPos.transform.position + pushzoneDistance, transform.rotation);  // ������������ �ν��Ͻ�ȭ 
            _jumpZone.transform.parent = transform;
            // _jumpZone.transform.position = coinRandomPos.transform.position + pushzoneDistance; // ������ �������� ��ġ�� ���� ����� �� ��ġ�� �̵�

            for (int j = 0; j < pushzonePos.Count; j++)  // ������ ��ġ�� ���� ����Ʈ�� ũ�⸸ŭ �ݺ�
            {
                // ������ �������� ��ġ�� ������ ��ġ ����Ʈ�� ��� ��ġ�� ���Ͽ� ���� �� ������
                if (_jumpZone.transform.position == pushzonePos[j].transform.position)
                {
                    PhotonNetwork.Destroy(_jumpZone);  // ������ ������ ����
                    _jumpZone = null;  // null�� ����
                    break;  // �ݺ��� Ż��
                }
            }
            if (_jumpZone != null)  //  �ΰ��� �ƴϸ�
            {
                pushzonePos.Add(_jumpZone);  // ������ �������� ����Ʈ�� �߰�
            }
            Debug.Log("pushZone ������ ������ ����Ʈ�� ũ�� : " + pushzonePos.Count);
        }
    }

    // �������� �÷��̾�� ������ �ٸ� ��ġ�� �̵���Ű�� �Լ�
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

        coinCount += count;   // ���� coinCount�� coin �߰�
        coinCountTxt.text = "Coin : " + coinCount + " / " + coinCountMax;  // coinCountTxt ������Ʈ

        int id = coin.GetComponent<PhotonView>().ViewID;
        pv.RPC("CoinTxtRPC", RpcTarget.MasterClient, count, id);

        if (coinCount >= coinCountMax)  // ���� coinCount�� coinCountMax �̻��̸� �÷��̾��� ��ġ�� ����������� �̵�
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
        Debug.Log("coin ������ ������ ����Ʈ�� ũ�� : " + coins.Count);
    }

}
