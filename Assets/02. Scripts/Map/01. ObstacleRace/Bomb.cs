using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    MeshRenderer meshRenderer; // ��ź�� ���� �ٲٱ� ���� �޽�������
    Vector3 thisScale; // �⺻ ũ�⸦ ������ ����

    float bombPower = 1000f;  // ��ź �Ŀ�
    float changeTime = 1f; // ���� �ٲ�� �ð� ����
    bool isExp = false;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        thisScale = transform.localScale;  // �⺻ ũ�� ����
    }

    private void OnEnable()
    {
        // ��ź ������ �� �ð��� ���� ũ��� ���� ���ϴ� �Լ� ȣ��
        StartCoroutine(BomEffect());
    }

    IEnumerator BomEffect()
    {
        yield return new WaitForSeconds(2.5f); // 2�� �ں��� ����Ʈ ����

        while (!isExp) // �������ʴ� ���ȿ� ���ѷ���
        {
            meshRenderer.material.color = Color.red; // ���� �������� �ٲ�
            transform.localScale += thisScale * (0.02f); // �Լ��� ȣ��� ������ �������� 0.05�踸ŭ �߰�
            bombPower++; // �Ŀ��� ����
            yield return new WaitForSeconds(changeTime);  // ª���� �ð���ŭ ����
            changeTime *= 0.8f; // �����̴� Ÿ���� ���� ª��������

            meshRenderer.material.color = Color.black; // �ٽ� �������� �ٲ�
            transform.localScale += thisScale * (0.02f);
            bombPower++;
            yield return new WaitForSeconds(changeTime);
            changeTime *= 0.8f;

            // ���� �������� x���� �⺻ ������ x���� 1.5�谡 �Ǹ� ����
            if (transform.localScale.x > thisScale.x * 1.5)
            {
                ExpBomb(transform.position);
                Destroy(gameObject);  // ���ӿ�����Ʈ ����
                isExp = true;
                Debug.Log("��ź�� ������");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("PLAYER"))
        {
            ExpBomb(transform.position);
            Destroy(gameObject);  // ���ӿ�����Ʈ ����
            isExp = true;  // �ڷ�ƾ �ݺ��� ���Ḧ ���� bool�� ����
            Debug.Log("��ź�� ������");
        }
    }

    void ExpBomb(Vector3 pos)  
    {
        // ����������� �޼ҵ�� ������ �ݶ��̴��� ����
        // �����������(��ġ, �ݰ�, ���ⷹ�̾�)
        // �ش���ġ�� �ݰ游ŭ ����� �ݶ��̴� ����
        // ����� ���̾ �ش��ϴ� ������Ʈ�� ����
        // ���� ���� ��ġ���� ���� ������ x ���� 2�踸ŭ �ݰ� ����, �÷��̾� ���̾� Ž��
        Collider[] colls = Physics.OverlapSphere(pos, transform.localScale.x * 3f, 1 << 10);
        // ����� ������Ʈ ���� 
        foreach (var coll in colls)
        {
            var coll_Rb = coll.GetComponent<Rigidbody>();
            // AddExplosionForce(Ⱦ���߷�, ��ġ, �ݰ�, �����߷�)
            // ���� ���� ��ġ���� Ž���� �ݰ游ŭ Ⱦ���δ� ��ź�Ŀ�, �����δ�  �ݰ游ŭ �� ����
            coll_Rb.AddExplosionForce(bombPower, pos, transform.localScale.x * 3f, transform.localScale.x);
            Debug.Log("����������");
        }
    }
}