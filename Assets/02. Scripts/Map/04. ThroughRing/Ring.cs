using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviourPun
{
    int score;

    // 일반 링은 1점, 골드링은 5점

    private void Start()
    {
        if (this.gameObject.tag == "RING")
            score = 1;

        else if (this.gameObject.tag == "GOLDRING")
            score = 5;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PLAYER") && other.GetComponent<PhotonView>().IsMine)
        {
            GameObject.FindGameObjectWithTag("MAKEMAP").GetComponent<MakeRingMap>().RingCount(score, gameObject);
            Debug.Log("함수호출");
            Debug.Log(GameObject.FindGameObjectWithTag("MAKEMAP"));
            Debug.Log("ring Score : " + score);
        }
    }
}
