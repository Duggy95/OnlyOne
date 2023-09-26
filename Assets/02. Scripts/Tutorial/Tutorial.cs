using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    public Image tutorialImg;
    public Canvas mainCanvas;
    public Canvas tutorialCanvas;
    public Camera mainCam;
    public Camera tutorialCam;
    public GameObject player;

    AudioSource audioSource;
    AudioClip clickSound;

    void OnEnable()
    {
        mainCam.gameObject.SetActive(false);
        tutorialCam.gameObject.SetActive(true);
        mainCanvas.gameObject.SetActive(false);
        tutorialCanvas.gameObject.SetActive(true);

        audioSource = GetComponent<AudioSource>();
        clickSound = SoundManager.instance.UIClickClip;
        player.GetComponent<Rigidbody>().useGravity = true;
        player.transform.position = new Vector3(0, transform.position.y + 5, 0);
    }

    public void OnClickTutorial()
    {
        audioSource.PlayOneShot(clickSound, 1f);
        tutorialImg.gameObject.SetActive(true);
    }

    public void EscTutorial()
    {
        audioSource.PlayOneShot(clickSound, 1f);
        tutorialImg.gameObject.SetActive(false);
    }

    public void ExitTutorial()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        mainCam.gameObject.SetActive(true);
        tutorialCam.gameObject.SetActive(false);
        mainCanvas.gameObject.SetActive(true);
        tutorialCanvas.gameObject.SetActive(false);

        player.GetComponent<Rigidbody>().useGravity = false;
        player.SetActive(false);
        gameObject.SetActive(false);
    }
}
