using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(NavMeshAgent))]
public class BabyCoinMoveAgent : MonoBehaviourPun, IPunObservable
{
    NavMeshAgent agent;
    Transform babyCoinTr;

    public List<Transform> wayPoints;
    public int nextIdx;
    public Vector3 farPos;
    public float _damping = 10f; // 수신된 좌표로 이동할 때 사용할 감도

    Vector3 receivePos;
    Quaternion receiveRot;
    int receiveNextIdx;
    readonly float moveSpeed = 7f;
    readonly float runSpeed = 15f;
    float damping = 5f;
    bool isCollision = false;
    bool moving;

    public bool MOVING
    {
        get { return moving; }
        set
        {
            moving = value;
            if (moving)
            {
                agent.speed = moveSpeed;
                damping = 5f;
                MoveWayPoint();
            }
        }
    }

    Vector3 runTarget;
    public Vector3 RUNTARGET
    {
        get { return runTarget; }
        set
        {
            runTarget = value;
            agent.speed = runSpeed;
            damping = 20f;
            RunTarget(runTarget);
        }
    }

    public float SPEED
    {
        get { return agent.velocity.magnitude; }
    }

    void RunTarget(Vector3 pos)
    {
        if (agent.isPathStale)
            return;

        // 플레이어와 순찰지점[0]과의 거리와 포지션값 저장
        float farDis = Vector3.Distance(pos, wayPoints[0].position);
        farPos = wayPoints[0].position;
        int farPoint = 0; // nextIdx 확인용

        for (int i = 0; i < wayPoints.Count; i++)
        {
            // 플레이어와 순찰지점[i]의 거리 저장
            float dis = Vector3.Distance(pos, wayPoints[i].position);
            // 이전 값보다 작으면
            if (farDis < dis)
            {
                // 현재값 저장
                farDis = dis;
                farPos = wayPoints[i].position;
                farPoint = i;
            }
        }
        nextIdx = farPoint;
        agent.isStopped = false;
        MoveWayPoint();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // 송신
        {
            stream.SendNext(babyCoinTr.position); // 1
            stream.SendNext(babyCoinTr.rotation); // 2
            //stream.SendNext(nextIdx);
        }
        else // 수신 리딩부분
        {
            // 보낸 순서대로 수신
            receivePos = (Vector3)stream.ReceiveNext();
            receiveRot = (Quaternion)stream.ReceiveNext();
            //receiveNextIdx = (int)stream.ReceiveNext();
        }
    }

    void Start()
    {
        babyCoinTr = GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;
        agent.updateRotation = false;
    }

    private void OnEnable()
    {
        StartCoroutine(FindWayPoint());
    }

    IEnumerator FindWayPoint()
    {
        yield return new WaitForSeconds(1f);

        var group = transform.parent.Find("WayPoints_Baby");
        Debug.Log(group);

        if (group != null)
        {
            group.GetComponentsInChildren<Transform>(wayPoints);

            wayPoints.RemoveAt(0);

            nextIdx = Random.Range(0, wayPoints.Count);
        }

        Debug.Log(wayPoints.Count);
        moving = true;
    }

    void MoveWayPoint()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (agent.isPathStale)
            return;

        if (wayPoints.Count == 0)
            return;

        if (PhotonNetwork.IsMasterClient)
        {
            isCollision = false;
            agent.destination = wayPoints[nextIdx].position;
            agent.isStopped = false;
        }
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (GameManager.instance.isGameover == true)
            {
                Destroy(gameObject, 2);
            }

            if (!agent.isStopped)
            {
                Quaternion rot = Quaternion.LookRotation(agent.desiredVelocity);
                babyCoinTr.rotation = Quaternion.Slerp(babyCoinTr.rotation, rot, Time.deltaTime * damping);
            }

            if (!moving)
                return;

            if (isCollision == false)
            {
                // 속도가 0.2 이상인데 목적지와의 거리가 0.5 이하일 때
                if (agent.velocity.sqrMagnitude >= 0.2f * 0.2f && agent.remainingDistance <= 0.5f)
                {
                    // 속도 = 0, 다음 목적지 설정
                    agent.velocity = Vector3.zero;
                    nextIdx = Random.Range(0, wayPoints.Count);

                    MoveWayPoint();
                }
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, receivePos, Time.deltaTime * _damping);
            transform.rotation = Quaternion.Slerp(transform.rotation, receiveRot, Time.deltaTime * _damping);
        }
    }

    // 다른 에이전트들이랑 부딪혔을 시 새 목적지 설정
    private void OnCollisionEnter(Collision collision)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (collision.gameObject.CompareTag("MOMCOIN"))
        {
            nextIdx = Random.Range(0, wayPoints.Count);
            isCollision = true;
        }

        else if (collision.gameObject.CompareTag("BABYCOIN"))
        {
            nextIdx = Random.Range(0, wayPoints.Count);
            isCollision = true;
        }
    }
}
