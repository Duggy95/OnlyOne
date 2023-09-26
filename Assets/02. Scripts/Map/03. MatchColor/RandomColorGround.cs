using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RandomColorGround : MonoBehaviourPun
{
    // delegate 함수 이벤트 등록으로 인해 한번에 함수 호출
    /*public delegate void ClearMap();
    public static event ClearMap clearMap;*/

    public List<GameObject> _grounds = new List<GameObject>();
    public List<GameObject> grounds_m = new List<GameObject>();
    public List<GameObject> _answers = new List<GameObject>();
    public GameObject colorGroundPrefabs;   // 생성될 맵을 구성할 타일 프리팹
    public GameObject[] answer;   // 조건 오브젝트 배열
    public GameObject bulletPrefab; // 총알
    public Color[] randomColor = { Color.blue, Color.green, Color.red, Color.yellow };  // 랜덤 색상 저장

    PhotonView pv;
    PlayerCtrl pc;
    CameraCtrl cameraCtrl;
    new Transform transform;
    Vector3 positionZero;  // 맵의 중앙
    Vector3 nextPlayerPos;  // 다음 라운드 시작 시 플레이어 위치
    GameObject player;  // 멀티가 되면 플레이어 배열로??
    GameObject _answerClone;
    MeshRenderer _answerMesh;
    MeshRenderer _groundMesh;
    GameObject[] players;

    float x;   // 조건 오브젝트의 x값
    float y;   // 조건 오브젝트의 y값
    float z;   // 조건 오브젝트의 z값
    float distanceX;    // 조건 오브젝트와 맵의 x축 거리
    float distanceY;    // 조건 오브젝트와 맵의 y축 거리
    float originMoveSpeed;
    float size = 3f;   // 생성될 맵을 구성하는 타일의 사이즈
    float fireSpeed = 50;  // 발사 스피드

    int width = 10;  // 생성될 맵의 행
    int height = 10;  // 생성될 맵의 열
    int count = 0;  // 라운드 카운트
    int widthCount;  // 생성될 맵의 행의 수
    int heightCount;  // 생성될 맵의 열의 수
    int[] randomDelay = { 0, 1, 2, 3 }; // 발사 후 딜레이 시간
    int KeyCount = 0;

    bool nextRound = false;   // 다음 라운드 진입여부
    bool isFire = false;  // 발사 가능 여부
    bool myPlayerDie = false;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        transform = GetComponent<Transform>();
        player = GameManager.instance.player;
        pc = player.GetComponent<PlayerCtrl>();
        cameraCtrl = Camera.main.GetComponent<CameraCtrl>();

        x = transform.position.x;
        y = transform.position.y;
        z = transform.position.z;

        Debug.Log(player.GetComponent<PhotonView>().ViewID);
    }

    private void OnEnable()
    {
        StartCoroutine(MakeRound());
    }

    void Start()
    {
        // 맵 생성 원점이동
        nextPlayerPos = new Vector3(Random.Range(x - 3, x + 3), Random.Range(y + 15, y + 20), Random.Range(z - 3, z + 3));
        originMoveSpeed = player.GetComponent<Movement>().moveSpeed;
        positionZero = new Vector3(((widthCount * size) / 2f) - (size / 2f), y + size, ((heightCount * size) / 2f) - (size / 2f));
        players = GameObject.FindGameObjectsWithTag("PLAYER");
    }

    private void Update()
    {
        // 게임을 못하게된 플레이어는 방향키 위 를 눌러
        // 다른 플레이어의 게임을 지켜볼 수 있다.
        if (myPlayerDie)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // 카메라가 비추는 플레이어를 KeyCount % players.Length 번째 플레이어로
                cameraCtrl.player = players[KeyCount % players.Length].transform;
                Debug.Log(players[KeyCount % players.Length].GetComponent<PhotonView>().ViewID);
                KeyCount++;
            }
        }
    }

    IEnumerator MakeRound()
    {
        if (!PhotonNetwork.IsMasterClient)
            yield break;

        if (GameManager.instance == null && GameManager.instance.isGameover == true)
            yield break;

        else if (UIManager.instance != null && !UIManager.instance.isGameStart)
            yield return new WaitForSeconds(4f);

        if (nextRound == false)
        {
            widthCount = width - count;
            heightCount = height - count;

            Vector3 pos = new Vector3(-((widthCount * size) / 2f) + (size / 2f), y, -((heightCount * size) / 2f) + (size / 2f));

            pv.RPC("Setting", RpcTarget.All, pos);
            nextRound = true;
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < widthCount; i++)   // 칸 수와 줄 수만큼 생성(최소 4 * 4)
            {
                for (int j = 0; j < heightCount; j++)
                {
                    Color cubeColor = randomColor[Random.Range(0, randomColor.Length)];
                    float cubeR = cubeColor.r;
                    float cubeG = cubeColor.g;
                    float cubeB = cubeColor.b;

                    GameObject _ground = PhotonNetwork.Instantiate("Three/Cube", transform.position + new Vector3(i * size, y, j * size), transform.rotation);
                    _ground.transform.parent = transform;
                    _ground.name = i + "_" + j;
                    int id = _ground.GetComponent<PhotonView>().ViewID;
                    grounds_m.Add(_ground);
                    // 맵 생성
                    pv.RPC("MakeMap", RpcTarget.All, id, cubeR, cubeG, cubeB);
                }
            }
            if (widthCount > 5 && heightCount > 5)
                count++;

            pv.RPC("AddCube", RpcTarget.All);

            distanceX = widthCount / 2 + width * size;
            distanceY = heightCount / 2 + height * size;

            Debug.Log("_grounds 리스트 크기 :" + grounds_m.Count);

            yield return new WaitForSeconds(3f);
            Debug.Log("조건 오브젝트 생성");
            // 5초 뒤 조건이 될 오브젝트 생성
            // GameObject _answer = answer[Random.Range(0, answer.Length)];  // 표본 프리팹 랜덤 지정
            // Debug.Log("생성된 _answer의 태그 :" + _answer.tag);
            int _answer = Random.Range(0, answer.Length);
            Color _answerColor = randomColor[Random.Range(0, randomColor.Length)]; // 미리 랜덤된 색상 저장
            float answerR = _answerColor.r;
            float answerG = _answerColor.g;
            float answerB = _answerColor.b;
            Debug.Log("생성된 _answer의 색 :" + _answerColor);
            // 표본을 맵 사방에 배치하기 위한 위치 저장
            Vector3[] _answerPos = {
                    new Vector3((x - distanceX), y, z),    // 원점 기준 좌측에 위치 
                    new Vector3((x + distanceX), y, z),    // 원점 기준 우측에 위치
                    new Vector3(x, y, z - distanceY),      // 원점 기준 아래에 위치
                    new Vector3(x, y, z + distanceY) };    // 원점 기준 위에 위치
            for (int i = 0; i < _answerPos.Length; i++)
            {
                if (answer[_answer].gameObject.tag == "SAME")
                    _answerClone = PhotonNetwork.Instantiate("Three/Same", _answerPos[i], transform.rotation); // 랜덤 지정된 표본 생성

                else if (answer[_answer].gameObject.tag == "DIFFERENT")
                    _answerClone = PhotonNetwork.Instantiate("Three/Different", _answerPos[i], transform.rotation); // 랜덤 지정된 표본 생성

                int id = _answerClone.GetComponent<PhotonView>().ViewID;
                Vector3 dir = positionZero - _answerClone.transform.position;
                string _tag = _answerClone.gameObject.tag;
                pv.RPC("AnswerSet", RpcTarget.All, id, _tag, dir, answerR, answerG, answerB);

                Transform firePos = _answerClone.transform.Find("FirePos").GetComponent<Transform>();
                isFire = true;

                StartCoroutine(Fire(firePos));
            }

            yield return new WaitForSeconds(10f);
            Debug.Log("라운드 종료");

            isFire = false;

            pv.RPC("ZeroSpeed", RpcTarget.All);

            // 타일 리스트 크기만큼 반복
            for (int i = 0; i < grounds_m.Count; i++)
            {
                MeshRenderer _groundsmesh = grounds_m[i].GetComponent<MeshRenderer>();
                Color groundColor = _groundsmesh.material.color;

                if (answer[_answer].CompareTag("SAME"))  // 표본의 태그가 SAME이면
                {
                    if (_answerColor != groundColor)
                    {
                        PhotonNetwork.Destroy(grounds_m[i].gameObject);
                        //grounds_m.RemoveAt(i);
                    }
                }
                else if (answer[_answer].CompareTag("DIFFERENT"))
                {
                    if (_answerColor == groundColor)
                    {
                        PhotonNetwork.Destroy(grounds_m[i].gameObject);
                        //grounds_m.RemoveAt(i);
                    }
                }
            }
            yield return new WaitForSeconds(5f);

            players = GameObject.FindGameObjectsWithTag("PLAYER");

            List<GameObject> _player = new List<GameObject>();
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].transform.position.y > transform.position.y - 0.5f)
                {
                    _player.Add(players[i]);
                }
            }
            Debug.Log("살아남은 플레이어 수 : " + _player.Count);

            pv.RPC("DieCheck", RpcTarget.All);

            yield return new WaitForSeconds(0.5f);

            /*pv.RPC("NextPos", RpcTarget.All);

            Debug.Log("다음 스테이지 시작");
            NextRound();*/

            if (_player.Count > 1)  // 멀티 구현 후 = player가 2명 이상일 경우로 고칠 것
            {
                pv.RPC("NextPos", RpcTarget.All);

                Debug.Log("다음 스테이지 시작");
                NextRound();
            }
            else if (_player.Count <= 1)  // 멀티 구현 후 = player가 1명 남은 경우로 고칠 것
            {
                pv.RPC("Res", RpcTarget.All);
                Debug.Log("스테이지 종료");
            }

            grounds_m.Clear();
        }
    }

    [PunRPC]
    void Setting(Vector3 pos)
    {
        transform.localPosition = pos;
        Debug.Log(transform.position);
    }

    [PunRPC]
    void MakeMap(int id, float r, float g, float b)
    {
        GameObject[] grounds = GameObject.FindGameObjectsWithTag("GROUND");
        for (int i = 0; i < grounds.Length; i++)
        {
            if (grounds[i].GetComponent<PhotonView>().ViewID == id)
            {
                Color cubeColor = new Color(r, g, b);
                _groundMesh = grounds[i].GetComponent<MeshRenderer>();
                _groundMesh.material.color = cubeColor;
            }
        }
    }

    [PunRPC]
    void AddCube()
    {
        for (int i = 0; i < grounds_m.Count; i++)
        {
            _grounds.Add(grounds_m[i]);
        }
    }

    [PunRPC]
    void AnswerSet(int id, string tag_a, Vector3 dir, float r, float g, float b)
    {
        GameObject[] answers = GameObject.FindGameObjectsWithTag(tag_a);
        for (int i = 0; i < answers.Length; i++)
        {
            if (answers[i].GetComponent<PhotonView>().ViewID == id)
            {
                _answers.Add(answers[i]);
                // 맵 가운데를 바라보도록
                answers[i].transform.LookAt(dir);
                // 인스턴스된 표본의 메쉬렌더러 가져오기
                _answerMesh = answers[i].GetComponent<MeshRenderer>();
                Color answerC = new Color(r, g, b);
                if (_answerMesh == null)
                    return;
                else    // 랜덤으로 정해진 색상 부여
                    _answerMesh.material.color = answerC;
            }
        }

    }

    [PunRPC]
    void ZeroSpeed()
    {
        if (!player.GetComponent<PhotonView>().IsMine)
            return;

        Debug.Log("속도 0");

        player.GetComponent<Movement>().moveSpeed = 0f;
    }

    IEnumerator Fire(Transform pos)
    {
        int fireCount = Random.Range(1, 4);
        yield return new WaitForSeconds(randomDelay[Random.Range(0, randomDelay.Length)]);   // 랜덤 시간동안 지연

        for (int i = 0; i < fireCount; i++)
        {
            if (!isFire)
                break;
            Vector3 destination = grounds_m[Random.Range(0, grounds_m.Count)].transform.position;

            pv.RPC("FireRPC", RpcTarget.All, pos.position, destination);

            yield return new WaitForSeconds(randomDelay[Random.Range(1, randomDelay.Length)]);   // 랜덤 시간동안 지연
        }
    }

    [PunRPC]
    void FireRPC(Vector3 pos, Vector3 destination)
    {
        // 뷸렛 생성
        GameObject _bullet = Instantiate(bulletPrefab, pos, Quaternion.identity);
        Rigidbody _bullet_Rb = _bullet.GetComponent<Rigidbody>();
        // 뷸렛의 방향은 firePos에서 랜덤한 큐브를 향하는 방향으로
        // Vector3 destination = _grounds[Random.Range(0, _grounds.Count)].transform.position;
        Vector3 fireDir = destination - _bullet.transform.position;
        // 뷸렛의 y축을 firePos의 z축에 맞춤
        _bullet.transform.up = fireDir;
        _bullet_Rb.AddForce(fireDir * fireSpeed);
        Destroy(_bullet, 3f);
    }

    void NextRound()
    {
        nextRound = false;

        for (int i = 0; i < grounds_m.Count; i++)
        {
            if (grounds_m[i] != null)
                PhotonNetwork.Destroy(grounds_m[i].gameObject);
        }

        for (int i = 0; i < _answers.Count; i++)
        {
            if (_answers[i] != null)
                PhotonNetwork.Destroy(_answers[i].gameObject);
        }

        StartCoroutine(MakeRound());
    }

    [PunRPC]
    void DieCheck()
    {
        /*if (!player.GetComponent<PhotonView>().IsMine)
            return;*/

        if (player.transform.position.y < transform.position.y - 0.5f)
            myPlayerDie = true;

        Debug.Log("죽었는지");
    }

    [PunRPC]
    void NextPos()
    {
        /*if (!player.GetComponent<PhotonView>().IsMine)
            return;*/
        if (myPlayerDie)
            return;

        nextPlayerPos = new Vector3(Random.Range(x - 3, x + 3), Random.Range(y + 15, y + 20), Random.Range(z - 3, z + 3));
        player.transform.position = nextPlayerPos;
        player.GetComponent<Movement>().moveSpeed = originMoveSpeed;
        Debug.Log("위치로");
    }

    [PunRPC]
    void Res()
    {
       /* if (!player.GetComponent<PhotonView>().IsMine)
            return;*/

        if (player.transform.position.y > transform.position.y - 0.5f)
            player.GetComponent<PlayerCtrl>().isWin = true;

        GameManager.instance.EndGame();
    }
}