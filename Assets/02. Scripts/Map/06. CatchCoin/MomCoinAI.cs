using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;
using static UnityEngine.EventSystems.EventTrigger;

public class MomCoinAI : MonoBehaviourPun, IPunObservable
{
    public enum State
    {
        PATROL,  // ����
        TRACE,   // ����
        ATTACK,  // ����
        DELAY,
    }

    public State state = State.PATROL;  // ������ �⺻ ���·�

    public ScoreUpdate scoreUpdate;
    public GameObject patrolFace;
    public GameObject traceFace;
    public GameObject smileFace;

    public bool isDelay = false;

    Transform momCoinTr;
    Rigidbody momCoinRb;
    MomCoinMoveAgent moveAgent;
    CoinFOV coinFOV;
    PhotonView pv;
    GameObject desPlayer;

    [Range(8f, 15f)]
    float attackDist = 10f;  // ���ݰ��� �Ÿ�
    float delay = 5f;  // ���� �ð�
    float attackSpeed = 500f;
    float attackPower = 5f;
    float dist = 100; // �÷��̾����� �Ÿ��� ���� ����
    /*float receiveDist;
    int dest;  // ����� �÷��̾��� ���� ������ ����
    int receiveDest = 0;*/
    //int viewId;
    string receiveState;
    bool isAttack = false;

    void Awake()
    {
        momCoinTr = GetComponent<Transform>();
        momCoinRb = GetComponent<Rigidbody>();
        moveAgent = GetComponent<MomCoinMoveAgent>();
        coinFOV = GetComponent<CoinFOV>();
        pv = GetComponent<PhotonView>();
        scoreUpdate = GameManager.instance.player.GetComponent<ScoreUpdate>();
    }

    private void Start()
    {
        patrolFace.gameObject.SetActive(true);
        traceFace.gameObject.SetActive(false);
        smileFace.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        StartCoroutine(CheckState());  // ����üũ�Լ�
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

    IEnumerator CheckState()
    {
        yield return new WaitForSeconds(2f);
        // ���ӿ����� �ƴ� ����
        while (GameManager.instance.isGameover == false)
        {
            if (GameManager.instance.isGameover == true)
                yield break;

            if (UIManager.instance.isGameStart)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    Collider[] colls = Physics.OverlapSphere(momCoinTr.position, coinFOV.viewRange, 1 << 10);

                    if (colls.Length != 0)
                    {
                        dist = Vector3.Distance(momCoinTr.transform.position, colls[0].gameObject.transform.position); // ù��°�� �������� ����ֱ� 
                                                                                                                       //dest = 0; // ù��°�� ���� 
                        desPlayer = colls[0].gameObject;

                        for (int i = 0; i < colls.Length; i++)
                        {
                            float distance = Vector3.Distance(momCoinTr.transform.position, colls[i].gameObject.transform.position);

                            if (distance < dist) // ������ ���� �������� �Ÿ� ���
                            {
                                desPlayer = colls[i].gameObject;
                                dist = distance;
                            }

                        }


                        // �����̻��°� �ƴϰ� ���ݻ��°� �ƴ� ���
                        if (isDelay == false && isAttack == false)
                        {
                            // �÷��̾ �������� ������ ���� ������ ���� ���
                            if (!coinFOV.isHidePlayer(desPlayer))
                            {
                                // �Ÿ��� �����Ÿ����� ª���� ���� �ƴϸ� ����
                                if (dist <= attackDist)
                                {
                                    if (coinFOV.isFindPlayer())
                                        state = State.ATTACK;

                                    else
                                        state = State.TRACE;
                                }
                                else
                                    state = State.TRACE;
                            }
                            // �� �ܿ� ����
                            else
                                state = State.PATROL;
                        }
                        Debug.Log("��ǥ �÷��̾� : " + desPlayer.GetComponent<PhotonView>().ViewID);
                    }

                    // ����� �ݶ��̴��� ���� ���
                    else
                    {
                        state = State.PATROL;
                    }
                }

                else
                {
                    // ���ڿ��� ���� ������ Ÿ�� ��ȯ �Ͽ� ���� ��ȭ
                    state = (State)Enum.Parse(typeof(State), receiveState);
                    //dest = receiveDest;
                    // dist = receiveDist;
                }

            }
            Debug.Log("mom : " + state);

            //Debug.Log("��ǥ �÷��̾� ID : " + viewId);
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator Action()
    {
        yield return new WaitForSeconds(2f);

        while (GameManager.instance.isGameover == false)
        {
            if (GameManager.instance.isGameover == true)
                yield break;

            yield return new WaitForSeconds(0.3f);

            switch (state)
            {
                // ���°� �����̸� moveagent�� tracetarget�� �÷��̾��� ����������
                case State.TRACE:
                    if (isDelay == false)
                    {
                        patrolFace.gameObject.SetActive(false);
                        traceFace.gameObject.SetActive(true);
                        smileFace.gameObject.SetActive(false);
                        if (PhotonNetwork.IsMasterClient)
                            moveAgent.TRACETARGET = desPlayer.transform.position;
                    }
                    break;

                // ���� ���¶�� moveagent�� patrolling�� true��
                case State.PATROL:
                    if (isDelay == false)
                    {
                        patrolFace.gameObject.SetActive(true);
                        traceFace.gameObject.SetActive(false);
                        smileFace.gameObject.SetActive(false);
                        if (PhotonNetwork.IsMasterClient)
                            moveAgent.PATROLLING = true;
                    }
                    break;

                // ���� ���¶�� moveagent�� stop�Լ� ȣ��, attack�Լ� ȣ��
                case State.ATTACK:
                    if (isDelay == false && isAttack == false)
                    {
                        patrolFace.gameObject.SetActive(false);
                        traceFace.gameObject.SetActive(true);
                        smileFace.gameObject.SetActive(false);

                        if (PhotonNetwork.IsMasterClient)
                        {
                            moveAgent.TRACETARGET = desPlayer.transform.position;
                            StartCoroutine(Attack());
                        }
                    }
                    break;

                case State.DELAY:
                    patrolFace.gameObject.SetActive(false);
                    traceFace.gameObject.SetActive(false);
                    smileFace.gameObject.SetActive(true);
                    if (PhotonNetwork.IsMasterClient)
                        moveAgent.Stop();
                    break;
            }
        }
    }


    IEnumerator Attack()
    {
        //Vector3 dir = desPlayer.transform.position - momCoinTr.position;

        pv.RPC("Ready", RpcTarget.All);

        yield return new WaitForSeconds(0.5f);

        // ������ �÷��̾ �ٶ󺸴� ����
        // �� �������� attackSpeed ���� �ӵ���ŭ �̵�

        yield return new WaitForSeconds(0.5f);

        pv.RPC("AttackRPC", RpcTarget.All);

        yield return new WaitForSeconds(1.5f);

        pv.RPC("VeloZero", RpcTarget.All);

       /* yield return new WaitForSeconds(2f);

        pv.RPC("AttPossible", RpcTarget.All);*/
    }

    [PunRPC]
    void Ready()
    {
        isAttack = true;
        //momCoinTr.forward = dir;
        moveAgent.Stop();
        Debug.Log("�����غ�");
    }

    [PunRPC]
    void AttackRPC()
    {
        momCoinRb.AddForce(momCoinTr.forward * attackSpeed, ForceMode.Impulse);
        Debug.Log("�߻�");
    }

    [PunRPC]
    void VeloZero()
    {
        momCoinRb.velocity = Vector3.zero;
        Debug.Log("���ݻ��� ����");
        coinFOV.isHide = true;
        state = State.PATROL;
        isAttack = false;
    }

    /*[PunRPC]
    void AttPossible()
    {
    }*/

    void OnCollisionEnter(Collision collision)
    {
        // �÷��̾ �ε����ٸ� ���¸� delay��
        if (isAttack = true && collision.gameObject.CompareTag("PLAYER") && collision.gameObject.GetComponent<PhotonView>().IsMine)
        {
            scoreUpdate.MinScore();
            Debug.Log("MinScore");

            // �ε��� �÷��̾��� ������ٵ� ��������
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 dir = collision.contacts[0].normal;
            // �ε��� �븻 ���� �������� �÷��̾� �о
            playerRb.AddForce(-dir.normalized * attackPower, ForceMode.Impulse);
            isDelay = true;
            momCoinRb.velocity = Vector3.zero;

            StartCoroutine(Delay(playerRb));
        }
    }


    IEnumerator Delay(Rigidbody playerRb)
    {
        state = State.DELAY;

        yield return new WaitForSeconds(1f);
        playerRb.velocity = Vector3.zero;

        // 5�� ����
        yield return new WaitForSeconds(delay - 1);
        isDelay = false;
    }
}
