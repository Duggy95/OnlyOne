using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    MeshRenderer meshRenderer; // 폭탄의 색을 바꾸기 위한 메쉬렌더러
    Vector3 thisScale; // 기본 크기를 저장할 변수

    float bombPower = 1000f;  // 폭탄 파워
    float changeTime = 1f; // 색이 바뀌는 시간 변수
    bool isExp = false;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        thisScale = transform.localScale;  // 기본 크기 저장
    }

    private void OnEnable()
    {
        // 폭탄 생성한 후 시간에 따라 크기와 색이 변하는 함수 호출
        StartCoroutine(BomEffect());
    }

    IEnumerator BomEffect()
    {
        yield return new WaitForSeconds(2.5f); // 2초 뒤부터 이펙트 시작

        while (!isExp) // 터지지않는 동안에 무한루프
        {
            meshRenderer.material.color = Color.red; // 색을 빨강으로 바꿈
            transform.localScale += thisScale * (0.02f); // 함수가 호출될 때마다 스케일을 0.05배만큼 추가
            bombPower++; // 파워에 증가
            yield return new WaitForSeconds(changeTime);  // 짧아진 시간만큼 지연
            changeTime *= 0.8f; // 깜빡이는 타임이 점점 짧아지도록

            meshRenderer.material.color = Color.black; // 다시 검정으로 바꿈
            transform.localScale += thisScale * (0.02f);
            bombPower++;
            yield return new WaitForSeconds(changeTime);
            changeTime *= 0.8f;

            // 현재 스케일의 x값이 기본 스케일 x값의 1.5배가 되면 폭발
            if (transform.localScale.x > thisScale.x * 1.5)
            {
                ExpBomb(transform.position);
                Destroy(gameObject);  // 게임오브젝트 삭제
                isExp = true;
                Debug.Log("폭탄이 터졌다");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("PLAYER"))
        {
            ExpBomb(transform.position);
            Destroy(gameObject);  // 게임오브젝트 삭제
            isExp = true;  // 코루틴 반복문 종료를 위해 bool값 변경
            Debug.Log("폭탄이 터졌다");
        }
    }

    void ExpBomb(Vector3 pos)  
    {
        // 오버랩스페어 메소드는 가상의 콜라이더를 생성
        // 오버랩스페어(위치, 반경, 검출레이어)
        // 해당위치의 반경만큼 스페어 콜라이더 생성
        // 검출된 레이어에 해당하는 오브젝트만 추출
        // 따라서 지금 위치에서 지금 스케일 x 값의 2배만큼 반경 지정, 플레이어 레이어 탐색
        Collider[] colls = Physics.OverlapSphere(pos, transform.localScale.x * 3f, 1 << 10);
        // 추출된 오브젝트 전부 
        foreach (var coll in colls)
        {
            var coll_Rb = coll.GetComponent<Rigidbody>();
            // AddExplosionForce(횡폭발력, 위치, 반경, 종폭발력)
            // 따라서 현재 위치에서 탐색한 반경만큼 횡으로는 폭탄파워, 종으로는  반경만큼 힘 전달
            coll_Rb.AddExplosionForce(bombPower, pos, transform.localScale.x * 3f, transform.localScale.x);
            Debug.Log("날려보낸다");
        }
    }
}