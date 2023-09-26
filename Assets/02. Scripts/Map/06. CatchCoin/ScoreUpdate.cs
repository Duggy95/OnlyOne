using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;  // 씬 관련 작업 시 꼭 필요
using System.Linq;

public class ScoreUpdate : MonoBehaviourPun, IPunObservable
{
    public int score = 0;
    public int receiveScore = 0;
    public Text coinCountText;

    GameObject winner;
    PlayerCtrl playerCtrl;
    PhotonView pv;
    AudioSource audioSource;

    Scene scene;
    int curScene;
    int count;

    private void Awake()
    {
        scene = SceneManager.GetActiveScene();  // 씬 정보 불러오기
        curScene = scene.buildIndex;  // 현재 씬 정보 저장

        if (curScene != 7)
            return;

        playerCtrl = gameObject.GetComponent<PlayerCtrl>();
        pv = GetComponent<PhotonView>();
        audioSource = GetComponent<AudioSource>();
        coinCountText = GameObject.Find("CoinCountText").GetComponent<Text>();
    }

    private void Update()
    {
        if (curScene != 7)
            return;

        if (pv.IsMine)
            coinCountText.text = "Coin : " + score;

        else
            score = receiveScore;

        // 3분 이상이 되면 게임 종료 함수 호출
        // 게임 종료 시 플레이어들의 스코어 중 제일 높은 플레이어 Win
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (count == 0 && UIManager.instance.playTimeM >= 3)
        {
            pv.RPC("Res", RpcTarget.All);
            Debug.Log("게임 종료");
        }
    }

    [PunRPC]
    void Res()
    {
        count++;
        GameObject[] players = GameObject.FindGameObjectsWithTag("PLAYER");
        List<int> scores = new List<int>(); 

        Debug.Log("결과 정리" + players.Length);

        for (int i = 0; i < players.Length; i++)
        {
            int otherScore = players[i].GetComponent<ScoreUpdate>().score;
            scores.Add(otherScore);
        }

        int maxScore = scores[0];
        for(int i = 0; i < scores.Count; i++)
        {
            if (scores[i] > maxScore)
            {
                maxScore = scores[i];
            }
        }

        if (maxScore == score)
            playerCtrl.isWin = true;

        /*if(scores.Max() == score)
            playerCtrl.isWin = true;*/

        GameManager.instance.EndGame();
    }

    public void PlusScore()
    {
        audioSource.PlayOneShot(SoundManager.instance.scoreClip, 0.5f);

        score++;
    }

    public void MinScore()
    {
        audioSource.PlayOneShot(SoundManager.instance.hitClip, 0.5f);

        if (score > 3)
            score -= 3;

        else
            score = 0;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // 송신
        {
            stream.SendNext(score); // 1
        }
        else // 수신 리딩부분
        {
            // 보낸 순서대로 수신
            receiveScore = (int)stream.ReceiveNext();
        }
    }
}
