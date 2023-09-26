using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(NavMeshAgent))]
public class MomCoinMoveAgent : MonoBehaviourPun, IPunObservable
{
    NavMeshAgent agent;
    Transform momCoinTr;

    public List<Transform> wayPoints;
    public int nextIdx;
    public float _damping = 10f; // 수신된 좌표로 이동할 때 사용할 감도

    Vector3 receivePos;
    Quaternion receiveRot;
    // int receiveNextIdx;
    readonly float patrolSpeed = 10f;
    readonly float traceSpeed = 20f;
    float damping = 3f;
    bool isCollision = false;
    bool patrolling;

    public bool PATROLLING
    {
        get { return patrolling; }
        set
        {
            patrolling = value;

            if (patrolling)
            {
                agent.speed = patrolSpeed;
                damping = 3f;
                MoveWayPoint();
            }
        }
    }

    Vector3 traceTarget;
    public Vector3 TRACETARGET
    {
        get { return traceTarget; }
        set
        {
            traceTarget = value;
            agent.speed = traceSpeed;
            damping = 10f;
            TraceTarget(traceTarget);
        }
    }

    public float SPEED
    {
        get { return agent.velocity.magnitude; }
    }

    void TraceTarget(Vector3 pos)
    {
        if (agent.isPathStale)
            return;

        agent.SetDestination(pos);
        agent.isStopped = false;
    }

    public void Stop()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        patrolling = false;
    }

    void Start()
    {
        momCoinTr = GetComponent<Transform>();
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

        var group = transform.parent.Find("WayPoints_Mom");
        Debug.Log(group);

        if (group != null)
        {
            group.GetComponentsInChildren<Transform>(wayPoints);

            wayPoints.RemoveAt(0);

            nextIdx = Random.Range(0, wayPoints.Count);
        }

        Debug.Log(wayPoints.Count);
        patrolling = true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // 송신
        {
            stream.SendNext(momCoinTr.position); // 1
            stream.SendNext(momCoinTr.rotation);
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
                momCoinTr.rotation = Quaternion.Slerp(momCoinTr.rotation, rot, Time.deltaTime * damping);
            }

            if (!patrolling)
                return;

            if (isCollision == false)
            {
                if (agent.velocity.sqrMagnitude >= (0.2f * 0.2f) && agent.remainingDistance <= 1f)
                {
                    // 속도를 0으로 
                    Stop();
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

    void MoveWayPoint()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (wayPoints.Count == 0)
            return;

        if (agent.isPathStale)
            return;

        if (PhotonNetwork.IsMasterClient)
        {
            agent.destination = wayPoints[nextIdx].position;
            agent.isStopped = false;
            isCollision = false;
        }
    }

    // 서로 부딪히는 경우 새로운 순찰지점 적용
    private void OnCollisionEnter(Collision collision)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (collision.gameObject.CompareTag("MOMCOIN"))
        {
            isCollision = true;
            nextIdx = Random.Range(0, wayPoints.Count);
        }

        else if (collision.gameObject.CompareTag("BABYCOIN"))
        {
            isCollision = true;
            nextIdx = Random.Range(0, wayPoints.Count);
        }
    }
}
