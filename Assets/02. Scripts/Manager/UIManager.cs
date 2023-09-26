using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // ui 관련한 작업 시 꼭 필요 
using Photon.Pun;
using Photon.Realtime;

public class UIManager : MonoBehaviourPunCallbacks
{
    // UIManager 싱글턴
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

    public GameObject[] walls;  // 게임 시작 전 플레이어 이동을 제한할 벽 오브젝트 배열
    public GameObject gameMenu; // 게임 진행 중 esc키를 누르면 나오는 메뉴
    public Text warningTxt;  // 벽에 막혔을 때 출력되는 경고 텍스트
    public Text winTxt;   // 게임에서 이겼을 때 출력되는 텍스트
    public Text loseTxt;  // 게임에서 졌을 때 출력되는 텍스트
    public Text ruleTxt;  // 게임 시작 시 3초 동안 출력되는 규칙 텍스트 
    public Text startTimeCounrTxt;  // 남은 게임 시작 시간을 알려주는 텍스트
    public Text gameStartTxt;  // 게임이 시작됨을 알리는 텍스트
    public Text playTimeTxt;   // 플레이 타임 텍스트

    public bool isGameStart = false;   // 게임이 시작되었는지를 체크해줄 변수
    public int playTimeM = 0;  // 분

    AudioSource audioSource;
    AudioClip clickSound;

    float startTime = 5f;  // 게임 시작까지 남은 시간 변수
    float time = 0f; // 시간
    int playTimeS;  // 초

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        clickSound = SoundManager.instance.UIClickClip;

        // 시작시간을 알려주는 코루틴 함수 호출
        StartCoroutine(GameStartCount());
    }

    void Start()
    {
        gameMenu.SetActive(false);   // 게임메뉴 비활성화
        winTxt.gameObject.SetActive(false);
        loseTxt.gameObject.SetActive(false);
        ruleTxt.gameObject.SetActive(true);  // 룰 텍스트 활성화
        gameStartTxt.gameObject.SetActive(false);  // 게임 스타트 텍스트 비활성화
        startTimeCounrTxt.text = "게임 시작 - " + (int)startTime + "초 전";  // 게임시작시간 카운트 텍스트 초기화
    }

    void Update()
    {
        Scene scene = SceneManager.GetActiveScene();  // 씬 정보 불러오기
        int curScene = scene.buildIndex;  // 현재 씬 정보 저장
        if (curScene > 1) // 현재 씬의 번호가 0보다 크면
        {
            if (Input.GetKeyDown(KeyCode.Escape)) // esc키를 눌렀을 때
            {
                gameMenu.SetActive(true); // 게임 메뉴 활성화
            }
        }

        if (isGameStart && !GameManager.instance.isGameover)
        {
            time += Time.deltaTime;

            playTimeS = (int)time % 60;  // 60으로 나눈 나머지는 초
            playTimeM = (int)time / 60;  // 몫은 분

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
        // 게임 시작이 되지않는 동안
        while (!isGameStart)
        {
            if (startTime > 0)  // 남은 시간이 0 초과면
            {
                yield return new WaitForSeconds(1f);  // 1초 뒤

                startTime--;  // 남은 시간 -1
                startTimeCounrTxt.text = "게임 시작 - " + (int)startTime + "초 전";  // 남은 시간 텍스트 업데이트 
            }

            if (startTime <= 0)   // 남은 시간이 0 이하라면
            {
                ruleTxt.gameObject.SetActive(false);   //  룰텍스트 비활성화 
                gameStartTxt.gameObject.SetActive(true);   // 게임 스타트 텍스트 활성화
                for (int i = 0; i < walls.Length; i++)
                {
                    walls[i].SetActive(false);   // 게임진행을 가로막던 벽 비활성화
                }
                isGameStart = true;   //  게임시작

                yield return new WaitForSeconds(1f);   // 1초 뒤

                gameStartTxt.gameObject.SetActive(false);  // 게임 시작 텍스트 비활성화

                yield break;  // 코루틴 탈출
            }
        }
    }

    public void ToGame()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        gameMenu.SetActive(false);
        Debug.Log("게임으로 되돌아가기");
    }

    public void ToStartScene()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        PhotonNetwork.LeaveRoom();
        Debug.Log("게임 시작 화면으로");
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
        // PhotonNetwork.LoadLevel(0);
    }
}
