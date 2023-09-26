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
        PATROL,  // 순찰
        TRACE,   // 추적
        ATTACK,  // 공격
        DELAY,
    }

    public State state = State.PATROL;  // 순찰을 기본 상태로

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
    float attackDist = 10f;  // 공격가능 거리
    float delay = 5f;  // 지연 시간
    float attackSpeed = 500f;
    float attackPower = 5f;
    float dist = 100; // 플레이어들과의 거리를 비교할 변수
    /*float receiveDist;
    int dest;  // 가까운 플레이어의 값을 저장할 변수
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
        StartCoroutine(CheckState());  // 상태체크함수
        StartCoroutine(Action());
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // 송신
        {
            stream.SendNext(state.ToString()); // 1
            //stream.SendNext(dest);
            // stream.SendNext(dist);
        }
        else // 수신 리딩부분
        {
            // 보낸 순서대로 수신
            receiveState = (string)stream.ReceiveNext();
            //receiveDest = (int)stream.ReceiveNext();
            // receiveDist = (float)stream.ReceiveNext();
        }
    }

    IEnumerator CheckState()
    {
        yield return new WaitForSeconds(2f);
        // 게임오버가 아닌 동안
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
                        dist = Vector3.Distance(momCoinTr.transform.position, colls[0].gameObject.transform.position); // 첫번째를 기준으로 잡아주기 
                                                                                                                       //dest = 0; // 첫번째를 먼저 
                        desPlayer = colls[0].gameObject;

                        for (int i = 0; i < colls.Length; i++)
                        {
                            float distance = Vector3.Distance(momCoinTr.transform.position, colls[i].gameObject.transform.position);

                            if (distance < dist) // 위에서 잡은 기준으로 거리 재기
                            {
                                desPlayer = colls[i].gameObject;
                                dist = distance;
                            }

                        }


                        // 딜레이상태가 아니고 공격상태가 아닌 경우
                        if (isDelay == false && isAttack == false)
                        {
                            // 플레이어가 범위내에 있으며 벽에 가리지 않은 경우
                            if (!coinFOV.isHidePlayer(desPlayer))
                            {
                                // 거리가 사정거리보다 짧으면 어택 아니면 추적
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
                            // 그 외엔 순찰
                            else
                                state = State.PATROL;
                        }
                        Debug.Log("목표 플레이어 : " + desPlayer.GetComponent<PhotonView>().ViewID);
                    }

                    // 검출된 콜라이더가 없는 경우
                    else
                    {
                        state = State.PATROL;
                    }
                }

                else
                {
                    // 문자열로 받은 정보를 타입 변환 하여 상태 변화
                    state = (State)Enum.Parse(typeof(State), receiveState);
                    //dest = receiveDest;
                    // dist = receiveDist;
                }

            }
            Debug.Log("mom : " + state);

            //Debug.Log("목표 플레이어 ID : " + viewId);
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
                // 상태가 추적이면 moveagent의 tracetarget을 플레이어의 포지션으로
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

                // 순찰 상태라면 moveagent의 patrolling을 true로
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

                // 공격 상태라면 moveagent의 stop함수 호출, attack함수 호출
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

        // 방향은 플레이어를 바라보는 방향
        // 그 방향으로 attackSpeed 값의 속도만큼 이동

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
        Debug.Log("공격준비");
    }

    [PunRPC]
    void AttackRPC()
    {
        momCoinRb.AddForce(momCoinTr.forward * attackSpeed, ForceMode.Impulse);
        Debug.Log("발사");
    }

    [PunRPC]
    void VeloZero()
    {
        momCoinRb.velocity = Vector3.zero;
        Debug.Log("공격상태 해제");
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
        // 플레이어가 부딪혔다면 상태를 delay로
        if (isAttack = true && collision.gameObject.CompareTag("PLAYER") && collision.gameObject.GetComponent<PhotonView>().IsMine)
        {
            scoreUpdate.MinScore();
            Debug.Log("MinScore");

            // 부딪힌 플레이어의 리지드바디 가져오기
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 dir = collision.contacts[0].normal;
            // 부딪힌 노말 벡터 방향으로 플레이어 밀어냄
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

        // 5초 정지
        yield return new WaitForSeconds(delay - 1);
        isDelay = false;
    }
}
