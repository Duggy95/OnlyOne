using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManager : MonoBehaviour
{
    public static SoundManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindAnyObjectByType<SoundManager>();
            }
            return m_instance;
        }
    }

    static SoundManager m_instance;

    public AudioClip BGM;
    public AudioClip gameWinClip;
    public AudioClip gameLoseClip;
    //public AudioClip gameStartClip;
    public AudioClip UIClickClip;
    public AudioClip jumpClip;
    public AudioClip hitClip;
    public AudioClip scoreClip;
    
    AudioSource audioSource;

    private void Awake()
    {
        if (instance != this) // 싱글톤된 게 자신이 아니라면 삭제
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        audioSource.clip = BGM;
        audioSource.mute = false;
        audioSource.loop = true;
        audioSource.volume = 0.5f;
        audioSource.Play();

        if (GameManager.instance != null && GameManager.instance.isGameover)
        {
            audioSource.Stop();
        }
    }
}
