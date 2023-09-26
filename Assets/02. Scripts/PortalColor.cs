using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Photon.Pun;
using Photon.Realtime;
using System.Net.NetworkInformation;

public class PortalColor : MonoBehaviourPun
{
    MeshRenderer meshRenderer;
    PhotonView pv;

    // 현재 색상
    float numR = 1f;
    float numG = 1f;
    float numB = 1f;
    float numA = 0f;

    float nextR;
    float nextG;
    float nextB;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        meshRenderer.material.color = new Color(numR, numG, numB, numA);
        transform.localScale = 
            new Vector3(transform.localScale.x / 2, transform.localScale.y / 2, transform.localScale.z / 2);
    }

    private void OnEnable()
    {
        StartCoroutine(ChangeColor());
    }

    IEnumerator ChangeColor()
    {
        if(!PhotonNetwork.IsMasterClient)
            yield break;

        float count = 40;  // 다음 색상이 되기까지 계산을 반복할 횟수
        WaitForSeconds ws = new WaitForSeconds(count / 1000); // 1번 반복할 때마다 지연할 시간

        yield return new WaitForSeconds(0.3f);

        while (GameManager.instance.isGameover == false)
        {
            if (GameManager.instance == null && GameManager.instance.isGameover == true)
                yield break;
            // 다음 색상 지정
            // pv.RPC("RandomColor", RpcTarget.All);
            nextR = Random.Range(0f, 1f);
            nextG = Random.Range(0f, 1f);
            nextB = Random.Range(0f, 1f);
            float nextA = 1f;
            yield return new WaitForSeconds(0.1f);

            // 다음 색상까지 반복해서 빼줄 값 계산
            float minR = nextR / count;
            float minB = nextB / count;
            float minG = nextG / count;
            float minA = nextA / count;

            float minX = transform.localScale.x / count;
            float minY = transform.localScale.y / count;
            float minZ = transform.localScale.z / count;
            // 서서히 다음색상이 되도록 반복하여 계산
            for (int i = 0; i < count; i++)
            {
                yield return ws;
                /*numR -= minR;
                numB -= minB;
                numG -= minG;
                numA += minA;
                meshRenderer.material.color = new Color(numR, numG, numB, numA);
                transform.localScale += new Vector3(minX, minY, minZ);*/
                pv.RPC("Change", RpcTarget.All, minR, minB, minG, minA, minX, minY, minZ);
            }

            yield return new WaitForSeconds(0.5f);

            // 기존 색상으로 서서히 다시 되돌려줌
            for (int i = 0; i < count; i++)
            {
                yield return ws;
                /*numR += minR;
                numB += minB;
                numG += minG;
                numA -= minA;
                meshRenderer.material.color = new Color(numR, numG, numB, numA);
                transform.localScale -= new Vector3(minX, minY, minZ);*/
                pv.RPC("Origin", RpcTarget.All, minR, minB, minG, minA, minX, minY, minZ);
            }
        }
    }

    [PunRPC]
    void Change(float r, float b, float g, float a, float x, float y, float z)
    {
        numR -= r;
        numB -= b;
        numG -= g;
        numA += a;
        meshRenderer.material.color = new Color(numR, numG, numB, numA);
        transform.localScale += new Vector3(x, y, z);
    }

    [PunRPC]
    void Origin(float r, float b, float g, float a, float x, float y, float z)
    {
        numR += r;
        numB += b;
        numG += g;
        numA -= a;
        meshRenderer.material.color = new Color(numR, numG, numB, numA);
        transform.localScale -= new Vector3(x, y, z);
    }
}
