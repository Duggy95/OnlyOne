using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CoinFOV))]

public class FOVEditor : Editor
{
    private void OnSceneGUI()
    {
        CoinFOV fov = (CoinFOV)target;
        // �þ߰�(����) �������� ��ǥ�� ���(�־��� ������ 1/2��������)
        Vector3 fromAnglePos = fov.CirclePoint(-fov.viewAngle * 0.5f);

        Handles.color = Color.white;
        // (������ǥ, ��ֺ���, ���� ������)
        Handles.DrawWireDisc(fov.transform.position, Vector3.up, fov.viewRange);

        Handles.color = new Color(1, 1, 1, 0.2f);
        // (������ǥ, ��ֺ���, ��ä�� ���� ����, ��ä�� ����, ��ä�� ������)
        Handles.DrawSolidArc(fov.transform.position, Vector3.up, fromAnglePos, fov.viewAngle, fov.viewRange);

        // (������ǥ + ����, �ؽ�Ʈ)
        Handles.Label(fov.transform.position + (fov.transform.forward * 2f), fov.viewAngle.ToString());
    }
}
