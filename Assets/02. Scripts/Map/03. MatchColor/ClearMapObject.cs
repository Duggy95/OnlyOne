using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ClearMapObject : MonoBehaviourPun
{
    // 라운드 끝날 때마다 맵 초기화
/*    private void OnEnable()
    {
        RandomColorGround.clearMap += this.Destroy;
    }

    private void OnDisable()
    {
        RandomColorGround.clearMap -= this.Destroy;
    }

    public void Destroy()
    {
        Destroy(gameObject);
        Debug.Log("초기화");
    }*/
}
