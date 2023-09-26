using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;  // �� ���� �۾� �� �� �ʿ�
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
        scene = SceneManager.GetActiveScene();  // �� ���� �ҷ�����
        curScene = scene.buildIndex;  // ���� �� ���� ����

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

        // 3�� �̻��� �Ǹ� ���� ���� �Լ� ȣ��
        // ���� ���� �� �÷��̾���� ���ھ� �� ���� ���� �÷��̾� Win
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (count == 0 && UIManager.instance.playTimeM >= 3)
        {
            pv.RPC("Res", RpcTarget.All);
            Debug.Log("���� ����");
        }
    }

    [PunRPC]
    void Res()
    {
        count++;
        GameObject[] players = GameObject.FindGameObjectsWithTag("PLAYER");
        List<int> scores = new List<int>(); 

        Debug.Log("��� ����" + players.Length);

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
        if (stream.IsWriting) // �۽�
        {
            stream.SendNext(score); // 1
        }
        else // ���� �����κ�
        {
            // ���� ������� ����
            receiveScore = (int)stream.ReceiveNext();
        }
    }
}
