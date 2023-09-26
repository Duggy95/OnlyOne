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
        // 시야각(원주) 시작점의 좌표를 계산(주어진 각도의 1/2지점부터)
        Vector3 fromAnglePos = fov.CirclePoint(-fov.viewAngle * 0.5f);

        Handles.color = Color.white;
        // (원점좌표, 노멀벡터, 원의 반지름)
        Handles.DrawWireDisc(fov.transform.position, Vector3.up, fov.viewRange);

        Handles.color = new Color(1, 1, 1, 0.2f);
        // (원점좌표, 노멀벡터, 부채꼴 시작 각도, 부채꼴 각도, 부채꼴 반지름)
        Handles.DrawSolidArc(fov.transform.position, Vector3.up, fromAnglePos, fov.viewAngle, fov.viewRange);

        // (원점좌표 + 방향, 텍스트)
        Handles.Label(fov.transform.position + (fov.transform.forward * 2f), fov.viewAngle.ToString());
    }
}
