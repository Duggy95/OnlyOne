using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;

public class PlayerCtrl : MonoBehaviourPun
{
    Rigidbody rb;
    Vector3 respawnPos; // �÷��̾��� ������ �������� ������ ����
    PhotonView pv;
    AudioSource audioSource;

    //public Transform startGround;
    int count = 0;

    public bool isWin = false;  // ���̽����ӿ��� �̰���� �Ǵ��� ����

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        UIManager.instance.warningTxt.gameObject.SetActive(false);  // WarningTxt ��Ȱ��ȭ
        UIManager.instance.winTxt.gameObject.SetActive(false);  // WinTxt ��Ȱ��ȭ
        UIManager.instance.loseTxt.gameObject.SetActive(false);  // LoseTxt ��Ȱ��ȭ
        // �÷��̾� ���� ������ ���� ����
        //transform.position = startGround.position + new Vector3(Random.Range(-5, 5), 5, Random.Range(-2, 2));
        respawnPos = transform.position; // �ʱ� ��ġ�� ����
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (count == 0 && GameManager.instance.isGameover == true)
        {
            count++;
            // ������ ����Ǹ� 1�� �� ��� â ���
            StartCoroutine(Result());
        }
    }

    // ���� ���� 1�� �� ��� Ȱ��ȭ
    IEnumerator Result()
    {
        yield return new WaitForSeconds(1.5f);

        pv.RPC("GameRes", RpcTarget.All);
    }

    [PunRPC]
    void GameRes()
    {
        if (!pv.IsMine)
            return;

        if (isWin)
        {
            UIManager.instance.winTxt.gameObject.SetActive(true);
            audioSource.PlayOneShot(SoundManager.instance.gameWinClip, 0.5f);

            Debug.Log("win" + isWin);
        }

        else
        {
            UIManager.instance.loseTxt.gameObject.SetActive(true);
            audioSource.PlayOneShot(SoundManager.instance.gameLoseClip, 0.5f);

            Debug.Log("win" + isWin);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!pv.IsMine)
            return;

        if (collision.gameObject.CompareTag("WARNING"))
        {
            UIManager.instance.warningTxt.gameObject.SetActive(true);  // WarningTxt Ȱ��ȭ
        }
        else if (collision.gameObject.CompareTag("DESTROY"))
        {
            isWin = false;
        }
    }

    void OnCollisionExit(Collision collision) // �浹�� ����� ��
    {
        if (!pv.IsMine)
            return;

        if (collision.gameObject.CompareTag("WARNING")) // �� �浹������Ʈ�� �±װ� WARNING�̸�
        {
            UIManager.instance.warningTxt.gameObject.SetActive(false);  // WarningTxt ��Ȱ��ȭ
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!pv.IsMine)
            return;

        if (other.gameObject.CompareTag("SAVEPOINT")) // ���� �±װ� SAVEPOINT��
        {
            respawnPos = other.transform.position + new Vector3(Random.Range(-5, 5), 5, Random.Range(-2, 2));  // thisPos�� ���̺�����Ʈ ��ġ�� ����
            GameManager.instance.playerSafe = true; // �÷��̾� ������ ��� 
        }

        else if (other.gameObject.CompareTag("DEAD")) // ���� �±װ� DAED��
        {
            transform.position = respawnPos;  // ���� ��ġ�� this.Pos�� �ű�

            rb.velocity = Vector3.zero;
        }

        else if (other.gameObject.CompareTag("ENDGAME"))
        {

            if (pv.IsMine)
            {
                pv.RPC("EndRPC", RpcTarget.All);
            }
        }

        else if (other.gameObject.CompareTag("REMOVEFOG"))
        {
            RenderSettings.fog = false;
        }
    }

    [PunRPC]
    void EndRPC()
    {
        // ���ӸŴ����� EndGame �Լ� ȣ��
        isWin = true;
        GameManager.instance.EndGame();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!pv.IsMine)
            return;

        if (other.gameObject.CompareTag("SAVEPOINT"))
        {
            GameManager.instance.playerSafe = false;  // �÷��̾� ������ ��� ����
        }
    }
}
