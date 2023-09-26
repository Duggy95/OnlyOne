using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;

public class PlayerCtrl : MonoBehaviourPun
{
    Rigidbody rb;
    Vector3 respawnPos; // 플레이어의 리스폰 포지션을 저장할 변수
    PhotonView pv;
    AudioSource audioSource;

    //public Transform startGround;
    int count = 0;

    public bool isWin = false;  // 레이스게임에서 이겼는지 판단할 변수

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        UIManager.instance.warningTxt.gameObject.SetActive(false);  // WarningTxt 비활성화
        UIManager.instance.winTxt.gameObject.SetActive(false);  // WinTxt 비활성화
        UIManager.instance.loseTxt.gameObject.SetActive(false);  // LoseTxt 비활성화
        // 플레이어 시작 포지션 랜덤 지정
        //transform.position = startGround.position + new Vector3(Random.Range(-5, 5), 5, Random.Range(-2, 2));
        respawnPos = transform.position; // 초기 위치를 저장
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (count == 0 && GameManager.instance.isGameover == true)
        {
            count++;
            // 게임이 종료되면 1초 후 결과 창 띄움
            StartCoroutine(Result());
        }
    }

    // 게임 종료 1초 뒤 결과 활성화
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
            UIManager.instance.warningTxt.gameObject.SetActive(true);  // WarningTxt 활성화
        }
        else if (collision.gameObject.CompareTag("DESTROY"))
        {
            isWin = false;
        }
    }

    void OnCollisionExit(Collision collision) // 충돌을 벗어났을 때
    {
        if (!pv.IsMine)
            return;

        if (collision.gameObject.CompareTag("WARNING")) // 그 충돌오브젝트의 태그가 WARNING이면
        {
            UIManager.instance.warningTxt.gameObject.SetActive(false);  // WarningTxt 비활성화
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!pv.IsMine)
            return;

        if (other.gameObject.CompareTag("SAVEPOINT")) // 닿은 태그가 SAVEPOINT면
        {
            respawnPos = other.transform.position + new Vector3(Random.Range(-5, 5), 5, Random.Range(-2, 2));  // thisPos에 세이브포인트 위치를 저장
            GameManager.instance.playerSafe = true; // 플레이어 세이프 모드 
        }

        else if (other.gameObject.CompareTag("DEAD")) // 닿은 태그가 DAED면
        {
            transform.position = respawnPos;  // 현재 위치를 this.Pos로 옮김

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
        // 게임매니저의 EndGame 함수 호출
        isWin = true;
        GameManager.instance.EndGame();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!pv.IsMine)
            return;

        if (other.gameObject.CompareTag("SAVEPOINT"))
        {
            GameManager.instance.playerSafe = false;  // 플레이어 세이프 모드 해제
        }
    }
}
