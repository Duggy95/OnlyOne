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
    public float _damping = 10f; // ���ŵ� ��ǥ�� �̵��� �� ����� ����

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

        // �÷��̾�� ��������[0]���� �Ÿ��� �����ǰ� ����
        float farDis = Vector3.Distance(pos, wayPoints[0].position);
        farPos = wayPoints[0].position;
        int farPoint = 0; // nextIdx Ȯ�ο�

        for (int i = 0; i < wayPoints.Count; i++)
        {
            // �÷��̾�� ��������[i]�� �Ÿ� ����
            float dis = Vector3.Distance(pos, wayPoints[i].position);
            // ���� ������ ������
            if (farDis < dis)
            {
                // ���簪 ����
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
        if (stream.IsWriting) // �۽�
        {
            stream.SendNext(babyCoinTr.position); // 1
            stream.SendNext(babyCoinTr.rotation); // 2
            //stream.SendNext(nextIdx);
        }
        else // ���� �����κ�
        {
            // ���� ������� ����
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
                // �ӵ��� 0.2 �̻��ε� ���������� �Ÿ��� 0.5 ������ ��
                if (agent.velocity.sqrMagnitude >= 0.2f * 0.2f && agent.remainingDistance <= 0.5f)
                {
                    // �ӵ� = 0, ���� ������ ����
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

    // �ٸ� ������Ʈ���̶� �ε����� �� �� ������ ����
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
