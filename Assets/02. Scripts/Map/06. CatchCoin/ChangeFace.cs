using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeFace : MonoBehaviour
{
    float oriX;
    float oriY;
    float oriZ;

    void OnEnable()
    {
        StartCoroutine(Change());
    }

    private void Awake()
    {
        oriX = transform.localScale.x;
        oriY = transform.localScale.y;
        oriZ = transform.localScale.z;
    }

    // 코인의 얼굴 변화를 위한 함수
    IEnumerator Change()
    {
        while (GameManager.instance.isGameover == false)
        {
            if (GameManager.instance.isGameover == true)
                yield break;

            yield return new WaitForSeconds(0.5f);
            transform.localScale = new Vector3(oriX, oriY / 2, oriZ);

            yield return new WaitForSeconds(0.5f);
            transform.localScale = new Vector3(oriX, oriY, oriZ);
        }
    }
}
