using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // ui ������ �۾� �� �� �ʿ� 
using Photon.Pun;
using Photon.Realtime;

public class UIManager : MonoBehaviourPunCallbacks
{
    // UIManager �̱���
    public static UIManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindAnyObjectByType<UIManager>();
            }
            return m_instance;
        }
    }

    static UIManager m_instance;

    public GameObject[] walls;  // ���� ���� �� �÷��̾� �̵��� ������ �� ������Ʈ �迭
    public GameObject gameMenu; // ���� ���� �� escŰ�� ������ ������ �޴�
    public Text warningTxt;  // ���� ������ �� ��µǴ� ��� �ؽ�Ʈ
    public Text winTxt;   // ���ӿ��� �̰��� �� ��µǴ� �ؽ�Ʈ
    public Text loseTxt;  // ���ӿ��� ���� �� ��µǴ� �ؽ�Ʈ
    public Text ruleTxt;  // ���� ���� �� 3�� ���� ��µǴ� ��Ģ �ؽ�Ʈ 
    public Text startTimeCounrTxt;  // ���� ���� ���� �ð��� �˷��ִ� �ؽ�Ʈ
    public Text gameStartTxt;  // ������ ���۵��� �˸��� �ؽ�Ʈ
    public Text playTimeTxt;   // �÷��� Ÿ�� �ؽ�Ʈ

    public bool isGameStart = false;   // ������ ���۵Ǿ������� üũ���� ����
    public int playTimeM = 0;  // ��

    AudioSource audioSource;
    AudioClip clickSound;

    float startTime = 5f;  // ���� ���۱��� ���� �ð� ����
    float time = 0f; // �ð�
    int playTimeS;  // ��

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        clickSound = SoundManager.instance.UIClickClip;

        // ���۽ð��� �˷��ִ� �ڷ�ƾ �Լ� ȣ��
        StartCoroutine(GameStartCount());
    }

    void Start()
    {
        gameMenu.SetActive(false);   // ���Ӹ޴� ��Ȱ��ȭ
        winTxt.gameObject.SetActive(false);
        loseTxt.gameObject.SetActive(false);
        ruleTxt.gameObject.SetActive(true);  // �� �ؽ�Ʈ Ȱ��ȭ
        gameStartTxt.gameObject.SetActive(false);  // ���� ��ŸƮ �ؽ�Ʈ ��Ȱ��ȭ
        startTimeCounrTxt.text = "���� ���� - " + (int)startTime + "�� ��";  // ���ӽ��۽ð� ī��Ʈ �ؽ�Ʈ �ʱ�ȭ
    }

    void Update()
    {
        Scene scene = SceneManager.GetActiveScene();  // �� ���� �ҷ�����
        int curScene = scene.buildIndex;  // ���� �� ���� ����
        if (curScene > 1) // ���� ���� ��ȣ�� 0���� ũ��
        {
            if (Input.GetKeyDown(KeyCode.Escape)) // escŰ�� ������ ��
            {
                gameMenu.SetActive(true); // ���� �޴� Ȱ��ȭ
            }
        }

        if (isGameStart && !GameManager.instance.isGameover)
        {
            time += Time.deltaTime;

            playTimeS = (int)time % 60;  // 60���� ���� �������� ��
            playTimeM = (int)time / 60;  // ���� ��

            if (playTimeS >= 0 && playTimeS < 10)
            {
                playTimeTxt.text = playTimeM + " : 0" + playTimeS;
            }

            else
            {
                playTimeTxt.text = playTimeM + " : " + playTimeS;
            }
        }
    }


    IEnumerator GameStartCount()
    {
        // ���� ������ �����ʴ� ����
        while (!isGameStart)
        {
            if (startTime > 0)  // ���� �ð��� 0 �ʰ���
            {
                yield return new WaitForSeconds(1f);  // 1�� ��

                startTime--;  // ���� �ð� -1
                startTimeCounrTxt.text = "���� ���� - " + (int)startTime + "�� ��";  // ���� �ð� �ؽ�Ʈ ������Ʈ 
            }

            if (startTime <= 0)   // ���� �ð��� 0 ���϶��
            {
                ruleTxt.gameObject.SetActive(false);   //  ���ؽ�Ʈ ��Ȱ��ȭ 
                gameStartTxt.gameObject.SetActive(true);   // ���� ��ŸƮ �ؽ�Ʈ Ȱ��ȭ
                for (int i = 0; i < walls.Length; i++)
                {
                    walls[i].SetActive(false);   // ���������� ���θ��� �� ��Ȱ��ȭ
                }
                isGameStart = true;   //  ���ӽ���

                yield return new WaitForSeconds(1f);   // 1�� ��

                gameStartTxt.gameObject.SetActive(false);  // ���� ���� �ؽ�Ʈ ��Ȱ��ȭ

                yield break;  // �ڷ�ƾ Ż��
            }
        }
    }

    public void ToGame()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        gameMenu.SetActive(false);
        Debug.Log("�������� �ǵ��ư���");
    }

    public void ToStartScene()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        PhotonNetwork.LeaveRoom();
        Debug.Log("���� ���� ȭ������");
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
        // PhotonNetwork.LoadLevel(0);
    }
}
