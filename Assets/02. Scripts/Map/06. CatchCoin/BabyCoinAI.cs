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
        MOVE,  // 이동
        RUN    // 도망
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

    float dist; // 플레이어들과의 거리를 비교할 변수
    /*float receiveDist;
    int dest = 0;  // 가까운 플레이어의 값을 저장할 변수
    int receiveDest = 0;*/
    int count = 0;
    // int viewId;
    string receiveState;

    void Awake()
    {
        babyCoinTr = GetComponent<Transform>();
        moveAgent = GetComponent<BabyCoinMoveAgent>();   // 무브에이전트 스크립트 가져옴
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
                        dist = Vector3.Distance(babyCoinTr.transform.position, colls[0].gameObject.transform.position); // 첫번째를 기준으로 잡아주기 
                                                                                                                        //dest = 0; // 첫번째를 먼저 
                        desPlayer = colls[0].gameObject;

                        for (int i = 0; i < colls.Length; i++)
                        {
                            float distance = Vector3.Distance(babyCoinTr.transform.position, colls[i].gameObject.transform.position);

                            if (distance < dist) // 위에서 잡은 기준으로 거리 재기
                            {
                                desPlayer = colls[i].gameObject;
                                dist = distance;
                            }

                        }

                        // 거리가 도망갈 거리보다 짧다면
                        if (dist <= runDist)
                        {
                            // 플레이어가 반경 내에 있다면 도망
                            if (coinFOV.isFindPlayer())
                                state = State.RUN;
                            else // 아니라면 산책
                                state = State.MOVE;
                        }
                        else
                            state = State.MOVE;

                        Debug.Log("목표 플레이어 : " + desPlayer.GetComponent<PhotonView>().ViewID);
                    }

                    // 검출된 콜라이더가 없는 경우
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
                // 스테이트가 도망상태일 때
                case State.RUN:
                    smileFace.gameObject.SetActive(false);
                    cryingFace.gameObject.SetActive(true);
                    if (PhotonNetwork.IsMasterClient)
                        moveAgent.RUNTARGET = desPlayer.transform.position;
                    break;
                // 스테이트가 무브상태일 때
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
            spawnCoin = GetComponentInParent<SpawnCoin>();   // 스폰 설정 스크립트 가져옴
            Debug.Log(spawnCoin);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // 플레이어와 부딪히면 scoreUpdate 스크립트의 CoinCount 함수와
        // spawnCoin 스크립트의 DestroyCoin 함수 호출
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