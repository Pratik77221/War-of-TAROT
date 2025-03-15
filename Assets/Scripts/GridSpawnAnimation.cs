using Photon.Pun.Demo.SlotRacer.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpawnAnimation : MonoBehaviour
{
    public Transform Grid;  // 棋盘
    public float dropHeight = 2.0f; // 初始高度（从天而降）
    public float targetZ = 0.5f; // 离玩家一臂的距离（调整到适合的距离）
    public float dropSpeed = 2.0f;
    public float resetDistance = 0.1f;

    private Vector3 lastCameraPosition;
    private bool isDropping = false;
    bool gridLocked = false;

    void Start()
    {
        lastCameraPosition = Camera.main.transform.position;
        // 初始隐藏棋盘
        PlaceGridInAbove();
    }
    void Update()
    {
        if (!Grid.gameObject.activeSelf) return;
        if (gridLocked) return;

        Camera cam = Camera.main;
        Vector3 camPosition = cam.transform.position;

        // 计算水平位移 (忽略Y轴，只考虑X/Z移动)
        float horizontalDistance = Vector2.Distance(
            new Vector2(camPosition.x, camPosition.z),
            new Vector2(lastCameraPosition.x, lastCameraPosition.z)
        );

        if (horizontalDistance > resetDistance)
        {
            PlaceGridInAbove();
            lastCameraPosition = camPosition;
        }

        if (isDropping)
        {
            // 下降动画
            Grid.position = Vector3.Lerp(Grid.position, new Vector3(Grid.position.x, 0, Grid.position.z), Time.deltaTime * dropSpeed);
            if (Grid.position.y < 0.01f) isDropping = false;
        }

        PlaceGridInFront();

    }



    public void PlaceGrid(Vector3 planePosition)
    {
        // 确保棋盘从天而降
        Grid.position = new Vector3(planePosition.x, dropHeight, planePosition.z);
        Grid.gameObject.SetActive(true);
        isDropping = true;
    }

    void PlaceGridInAbove()
    {
        Camera mainCamera = Camera.main;
        Vector3 forward = mainCamera.transform.forward;  // 获取手机的前方向
        Vector3 armDistance = mainCamera.transform.position + forward * targetZ; // 一臂距离（0.5米）

        PlaceGrid(armDistance);
    }

    void PlaceGridInFront()
    {
        Camera mainCamera = Camera.main;
        Vector3 forward = mainCamera.transform.forward;  // 获取手机的前方向
        Vector3 armDistance = mainCamera.transform.position + forward * targetZ; // 一臂距离（0.5米）

        Grid.position = new Vector3(armDistance.x, Grid.position.y, armDistance.z);
    }

    void ConfirmGridPlacement()
    {
        gridLocked = true;
    }
}
