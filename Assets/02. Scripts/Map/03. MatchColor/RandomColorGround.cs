using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RandomColorGround : MonoBehaviourPun
{
    // delegate �Լ� �̺�Ʈ ������� ���� �ѹ��� �Լ� ȣ��
    /*public delegate void ClearMap();
    public static event ClearMap clearMap;*/

    public List<GameObject> _grounds = new List<GameObject>();
    public List<GameObject> grounds_m = new List<GameObject>();
    public List<GameObject> _answers = new List<GameObject>();
    public GameObject colorGroundPrefabs;   // ������ ���� ������ Ÿ�� ������
    public GameObject[] answer;   // ���� ������Ʈ �迭
    public GameObject bulletPrefab; // �Ѿ�
    public Color[] randomColor = { Color.blue, Color.green, Color.red, Color.yellow };  // ���� ���� ����

    PhotonView pv;
    PlayerCtrl pc;
    CameraCtrl cameraCtrl;
    new Transform transform;
    Vector3 positionZero;  // ���� �߾�
    Vector3 nextPlayerPos;  // ���� ���� ���� �� �÷��̾� ��ġ
    GameObject player;  // ��Ƽ�� �Ǹ� �÷��̾� �迭��??
    GameObject _answerClone;
    MeshRenderer _answerMesh;
    MeshRenderer _groundMesh;
    GameObject[] players;

    float x;   // ���� ������Ʈ�� x��
    float y;   // ���� ������Ʈ�� y��
    float z;   // ���� ������Ʈ�� z��
    float distanceX;    // ���� ������Ʈ�� ���� x�� �Ÿ�
    float distanceY;    // ���� ������Ʈ�� ���� y�� �Ÿ�
    float originMoveSpeed;
    float size = 3f;   // ������ ���� �����ϴ� Ÿ���� ������
    float fireSpeed = 50;  // �߻� ���ǵ�

    int width = 10;  // ������ ���� ��
    int height = 10;  // ������ ���� ��
    int count = 0;  // ���� ī��Ʈ
    int widthCount;  // ������ ���� ���� ��
    int heightCount;  // ������ ���� ���� ��
    int[] randomDelay = { 0, 1, 2, 3 }; // �߻� �� ������ �ð�
    int KeyCount = 0;

    bool nextRound = false;   // ���� ���� ���Կ���
    bool isFire = false;  // �߻� ���� ����
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
        // �� ���� �����̵�
        nextPlayerPos = new Vector3(Random.Range(x - 3, x + 3), Random.Range(y + 15, y + 20), Random.Range(z - 3, z + 3));
        originMoveSpeed = player.GetComponent<Movement>().moveSpeed;
        positionZero = new Vector3(((widthCount * size) / 2f) - (size / 2f), y + size, ((heightCount * size) / 2f) - (size / 2f));
        players = GameObject.FindGameObjectsWithTag("PLAYER");
    }

    private void Update()
    {
        // ������ ���ϰԵ� �÷��̾�� ����Ű �� �� ����
        // �ٸ� �÷��̾��� ������ ���Ѻ� �� �ִ�.
        if (myPlayerDie)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // ī�޶� ���ߴ� �÷��̾ KeyCount % players.Length ��° �÷��̾��
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

            for (int i = 0; i < widthCount; i++)   // ĭ ���� �� ����ŭ ����(�ּ� 4 * 4)
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
                    // �� ����
                    pv.RPC("MakeMap", RpcTarget.All, id, cubeR, cubeG, cubeB);
                }
            }
            if (widthCount > 5 && heightCount > 5)
                count++;

            pv.RPC("AddCube", RpcTarget.All);

            distanceX = widthCount / 2 + width * size;
            distanceY = heightCount / 2 + height * size;

            Debug.Log("_grounds ����Ʈ ũ�� :" + grounds_m.Count);

            yield return new WaitForSeconds(3f);
            Debug.Log("���� ������Ʈ ����");
            // 5�� �� ������ �� ������Ʈ ����
            // GameObject _answer = answer[Random.Range(0, answer.Length)];  // ǥ�� ������ ���� ����
            // Debug.Log("������ _answer�� �±� :" + _answer.tag);
            int _answer = Random.Range(0, answer.Length);
            Color _answerColor = randomColor[Random.Range(0, randomColor.Length)]; // �̸� ������ ���� ����
            float answerR = _answerColor.r;
            float answerG = _answerColor.g;
            float answerB = _answerColor.b;
            Debug.Log("������ _answer�� �� :" + _answerColor);
            // ǥ���� �� ��濡 ��ġ�ϱ� ���� ��ġ ����
            Vector3[] _answerPos = {
                    new Vector3((x - distanceX), y, z),    // ���� ���� ������ ��ġ 
                    new Vector3((x + distanceX), y, z),    // ���� ���� ������ ��ġ
                    new Vector3(x, y, z - distanceY),      // ���� ���� �Ʒ��� ��ġ
                    new Vector3(x, y, z + distanceY) };    // ���� ���� ���� ��ġ
            for (int i = 0; i < _answerPos.Length; i++)
            {
                if (answer[_answer].gameObject.tag == "SAME")
                    _answerClone = PhotonNetwork.Instantiate("Three/Same", _answerPos[i], transform.rotation); // ���� ������ ǥ�� ����

                else if (answer[_answer].gameObject.tag == "DIFFERENT")
                    _answerClone = PhotonNetwork.Instantiate("Three/Different", _answerPos[i], transform.rotation); // ���� ������ ǥ�� ����

                int id = _answerClone.GetComponent<PhotonView>().ViewID;
                Vector3 dir = positionZero - _answerClone.transform.position;
                string _tag = _answerClone.gameObject.tag;
                pv.RPC("AnswerSet", RpcTarget.All, id, _tag, dir, answerR, answerG, answerB);

                Transform firePos = _answerClone.transform.Find("FirePos").GetComponent<Transform>();
                isFire = true;

                StartCoroutine(Fire(firePos));
            }

            yield return new WaitForSeconds(10f);
            Debug.Log("���� ����");

            isFire = false;

            pv.RPC("ZeroSpeed", RpcTarget.All);

            // Ÿ�� ����Ʈ ũ�⸸ŭ �ݺ�
            for (int i = 0; i < grounds_m.Count; i++)
            {
                MeshRenderer _groundsmesh = grounds_m[i].GetComponent<MeshRenderer>();
                Color groundColor = _groundsmesh.material.color;

                if (answer[_answer].CompareTag("SAME"))  // ǥ���� �±װ� SAME�̸�
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
            Debug.Log("��Ƴ��� �÷��̾� �� : " + _player.Count);

            pv.RPC("DieCheck", RpcTarget.All);

            yield return new WaitForSeconds(0.5f);

            /*pv.RPC("NextPos", RpcTarget.All);

            Debug.Log("���� �������� ����");
            NextRound();*/

            if (_player.Count > 1)  // ��Ƽ ���� �� = player�� 2�� �̻��� ���� ��ĥ ��
            {
                pv.RPC("NextPos", RpcTarget.All);

                Debug.Log("���� �������� ����");
                NextRound();
            }
            else if (_player.Count <= 1)  // ��Ƽ ���� �� = player�� 1�� ���� ���� ��ĥ ��
            {
                pv.RPC("Res", RpcTarget.All);
                Debug.Log("�������� ����");
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
                // �� ����� �ٶ󺸵���
                answers[i].transform.LookAt(dir);
                // �ν��Ͻ��� ǥ���� �޽������� ��������
                _answerMesh = answers[i].GetComponent<MeshRenderer>();
                Color answerC = new Color(r, g, b);
                if (_answerMesh == null)
                    return;
                else    // �������� ������ ���� �ο�
                    _answerMesh.material.color = answerC;
            }
        }

    }

    [PunRPC]
    void ZeroSpeed()
    {
        if (!player.GetComponent<PhotonView>().IsMine)
            return;

        Debug.Log("�ӵ� 0");

        player.GetComponent<Movement>().moveSpeed = 0f;
    }

    IEnumerator Fire(Transform pos)
    {
        int fireCount = Random.Range(1, 4);
        yield return new WaitForSeconds(randomDelay[Random.Range(0, randomDelay.Length)]);   // ���� �ð����� ����

        for (int i = 0; i < fireCount; i++)
        {
            if (!isFire)
                break;
            Vector3 destination = grounds_m[Random.Range(0, grounds_m.Count)].transform.position;

            pv.RPC("FireRPC", RpcTarget.All, pos.position, destination);

            yield return new WaitForSeconds(randomDelay[Random.Range(1, randomDelay.Length)]);   // ���� �ð����� ����
        }
    }

    [PunRPC]
    void FireRPC(Vector3 pos, Vector3 destination)
    {
        // �淿 ����
        GameObject _bullet = Instantiate(bulletPrefab, pos, Quaternion.identity);
        Rigidbody _bullet_Rb = _bullet.GetComponent<Rigidbody>();
        // �淿�� ������ firePos���� ������ ť�긦 ���ϴ� ��������
        // Vector3 destination = _grounds[Random.Range(0, _grounds.Count)].transform.position;
        Vector3 fireDir = destination - _bullet.transform.position;
        // �淿�� y���� firePos�� z�࿡ ����
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

        Debug.Log("�׾�����");
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
        Debug.Log("��ġ��");
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