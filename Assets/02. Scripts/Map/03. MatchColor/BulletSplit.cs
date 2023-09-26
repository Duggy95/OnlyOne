using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSplit : MonoBehaviour
{
    public GameObject splitBullet;

    float speed = 300;

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("GROUND"))
        {
            // 8�������� ���ư�����
            List<Vector3> dir = new List<Vector3>
            {
                other.transform.forward,
                other.transform.forward - other.transform.right,
                other.transform.right,
                other.transform.right - (-other.transform.forward),
                (-other.transform.forward),
                (-other.transform.forward) - (-other.transform.right),
                (-other.transform.right),
                (-other.transform.right) - other.transform.forward
            };

            for (int i = 0; i < dir.Count; i++)
            {
                // �Ѿ��� �ش� ť�� ���� �����Ͽ� 8�������� 
                GameObject _splitBullet = Instantiate(splitBullet, other.transform.position, Quaternion.identity);
                _splitBullet.transform.position = transform.position + new Vector3(0, other.transform.localScale.y + 1f, 0);
                Rigidbody _splitBullet_Rb = _splitBullet.GetComponent<Rigidbody>();

                _splitBullet_Rb.AddForce(dir[i].normalized * speed);

                Destroy(_splitBullet.gameObject, 3f);
            }

            dir.Clear();

            Destroy(gameObject);
        }
    }
}
