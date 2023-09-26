using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;

public class BabyCoinAI : MonoBehaviourPun, IPunObservable
{
    public enum State
    {
        MOVE,  // �̵�
        RUN    // ����
    }
    public State state = State.MOVE;

    public GameObject smileFace;
    public GameObject cryingFace;
    public SpawnCoin spawnCoin;
    public ScoreUpdate scoreUpdate;

    Transform babyCoinTr;
    BabyCoinMoveAgent moveAgent;
    CoinFOV coinFOV;
    PhotonView pv;
    GameObject desPlayer;

    [Range(15f, 30f)]
    float runDist = 20f;

    float dist; // �÷��̾����� �Ÿ��� ���� ����
    /*float receiveDist;
    int dest = 0;  // ����� �÷��̾��� ���� ������ ����
    int receiveDest = 0;*/
    int count = 0;
    // int viewId;
    string receiveState;

    void Awake()
    {
        babyCoinTr = GetComponent<Transform>();
        moveAgent = GetComponent<BabyCoinMoveAgent>();   // ���꿡����Ʈ ��ũ��Ʈ ������
        coinFOV = GetComponent<CoinFOV>();
        pv = GetComponent<PhotonView>();
        scoreUpdate = GameManager.instance.player.GetComponent<ScoreUpdate>();
    }

    private void Start()
    {
        smileFace.gameObject.SetActive(true);
        cryingFace.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        StartCoroutine(CheckState());
        StartCoroutine(Action());
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // �۽�
        {
            stream.SendNext(state.ToString()); // 1
            //stream.SendNext(dest);
            // stream.SendNext(dist);
        }
        else // ���� �����κ�
        {
            // ���� ������� ����
            receiveState = (string)stream.ReceiveNext();
            //receiveDest = (int)stream.ReceiveNext();
            // receiveDist = (float)stream.ReceiveNext();
        }
    }

    private void Update()
    {
        if (spawnCoin == null)
        {
            Spawn();
        }
    }

    IEnumerator CheckState()
    {
        yield return new WaitForSeconds(2);

        while (GameManager.instance.isGameover == false)
        {
            if (GameManager.instance.isGameover == true)
                yield break;

            if (UIManager.instance.isGameStart)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    Collider[] colls = Physics.OverlapSphere(babyCoinTr.position, coinFOV.viewRange, 1 << 10);

                    if (colls.Length != 0)
                    {
                        dist = Vector3.Distance(babyCoinTr.transform.position, colls[0].gameObject.transform.position); // ù��°�� �������� ����ֱ� 
                                                                                                                        //dest = 0; // ù��°�� ���� 
                        desPlayer = colls[0].gameObject;

                        for (int i = 0; i < colls.Length; i++)
                        {
                            float distance = Vector3.Distance(babyCoinTr.transform.position, colls[i].gameObject.transform.position);

                            if (distance < dist) // ������ ���� �������� �Ÿ� ���
                            {
                                desPlayer = colls[i].gameObject;
                                dist = distance;
                            }

                        }

                        // �Ÿ��� ������ �Ÿ����� ª�ٸ�
                        if (dist <= runDist)
                        {
                            // �÷��̾ �ݰ� ���� �ִٸ� ����
                            if (coinFOV.isFindPlayer())
                                state = State.RUN;
                            else // �ƴ϶�� ��å
                                state = State.MOVE;
                        }
                        else
                            state = State.MOVE;

                        Debug.Log("��ǥ �÷��̾� : " + desPlayer.GetComponent<PhotonView>().ViewID);
                    }

                    // ����� �ݶ��̴��� ���� ���
                    else
                    {
                        state = State.MOVE;
                    }
                }
                else
                {
                    state = (State)Enum.Parse(typeof(State), receiveState);
                    //dest = receiveDest;
                    //dist = receiveDist;
                }
            }
            Debug.Log("baby : " + state);
            yield return new WaitForSeconds(0.3f);
        }
    }


    IEnumerator Action()
    {
        yield return new WaitForSeconds(2);

        while (GameManager.instance.isGameover == false)
        {
            if (GameManager.instance.isGameover == true)
                yield break;

            yield return new WaitForSeconds(0.3f);

            switch (state)
            {
                // ������Ʈ�� ���������� ��
                case State.RUN:
                    smileFace.gameObject.SetActive(false);
                    cryingFace.gameObject.SetActive(true);
                    if (PhotonNetwork.IsMasterClient)
                        moveAgent.RUNTARGET = desPlayer.transform.position;
                    break;
                // ������Ʈ�� ��������� ��
                case State.MOVE:
                    smileFace.gameObject.SetActive(true);
                    cryingFace.gameObject.SetActive(false);
                    if (PhotonNetwork.IsMasterClient)
                        moveAgent.MOVING = true;
                    break;
            }
        }
    }

    void Spawn()
    {
        if (spawnCoin == null)
        {
            spawnCoin = GetComponentInParent<SpawnCoin>();   // ���� ���� ��ũ��Ʈ ������
            Debug.Log(spawnCoin);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // �÷��̾�� �ε����� scoreUpdate ��ũ��Ʈ�� CoinCount �Լ���
        // spawnCoin ��ũ��Ʈ�� DestroyCoin �Լ� ȣ��
        if (collision.gameObject.CompareTag("PLAYER") && count < 2 && collision.gameObject.GetComponent<PhotonView>().IsMine)
        {
            scoreUpdate.PlusScore();
            spawnCoin.ChangeSpawnPos(gameObject);
            count++;
            Debug.Log("PlusScore : " + "1");
        }
    }
}
/*&&
           collision.gameObject.GetComponent<PhotonView>().IsMine*/