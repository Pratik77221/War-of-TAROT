using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem; // 新输入系统（如需使用）

public class ARRaycastHandler : MonoBehaviour
{
    [Header("ARComponent")]
    public ARRaycastManager arRaycastManager; // 请在 Inspector 中绑定 AR Session Origin 上的 ARRaycastManager
    public ARPlaneManager arPlaneManager;        // 绑定 AR Session Origin 上的 ARPlaneManager

    [Header("InteractionEffect")]
    public GameObject placementIndicatorPrefab;        // 平面指示器预制件
    public ParticleSystem hitEffect;             // 命中时的粒子特效
    public AudioClip successSound;               // 命中音效
    public GameObject objectToPlace;             // 可放置的物体预制件

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool isPlaneDetected = false;  // 标记平面是否已被检测
    private GameObject placementIndicator;
    private Touch touch1, touch2; 
    private GameObject selectedObject;

    private void Start()
    {
        UIManager.Instance.ShowMessage("Please scan the image and click on the screen interaction!", 5f);
        arPlaneManager.planesChanged += OnPlanesChanged;

        // 实例化 placementIndicator 并初始化为隐藏
        if (placementIndicatorPrefab != null)
        {
            placementIndicator = Instantiate(placementIndicatorPrefab);
            placementIndicator.SetActive(false);
        }
    }

    void Update()
    {
        // 使用新输入系统检测触摸
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
            ProcessRaycast(touchPos);
        }
        // 编辑器中使用鼠标进行调试
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            ProcessRaycast(mousePos);
        }

        if (Input.touchCount == 2)
        {
            touch1 = Input.GetTouch(0);
            touch2 = Input.GetTouch(1);
            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;
            Vector2 prevPos2 = touch2.position - touch2.deltaPosition;
            float prevDistance = (prevPos1 - prevPos2).magnitude;
            float currentDistance = (touch1.position - touch2.position).magnitude;
            float scaleFactor = currentDistance / prevDistance;
            if (selectedObject != null)
            {
                selectedObject.transform.localScale *= scaleFactor; // 缩放
            }
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // 当首次检测到平面时更新 UI
        if (args.added.Count > 0 && !isPlaneDetected)
        {
            isPlaneDetected = true;
            UIManager.Instance.ShowMessage("Plane detected! You can now place objects.", 5f);

            // 隐藏已生成的平面
            foreach (var plane in arPlaneManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
        }
    }

    void ProcessRaycast(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        // 执行 AR 射线检测，只检测已识别的平面
        if (arRaycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            // 可选：将 placementIndicator 移动到检测到的平面上
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(true);
                placementIndicator.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            }

            //  播放命中特效和音效
            if (hitEffect != null)
            {
                float distance = Vector3.Distance(Camera.main.transform.position, hitPose.position);
                var mainModule = hitEffect.main;
                mainModule.startSize = Mathf.Clamp(0.1f * distance, 0.5f, 2f);
                ParticleSystem effect = Instantiate(hitEffect, hitPose.position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration); // 播放完毕后自动销毁
            }

            if (successSound != null)
            {
                AudioSource.PlayClipAtPoint(successSound, hitPose.position);
            }

            // 放置物体（可扩展为触发动画等）
            if (objectToPlace != null)
            {
                Instantiate(objectToPlace, hitPose.position, Quaternion.identity);
            }

            // 震动反馈 
            if (Application.platform == RuntimePlatform.Android)
                Handheld.Vibrate(); // 安卓短震动

            // 移动选中的物体
            if (Physics.Raycast(ray, out hit))
            {
                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                {
                    selectedObject = hit.collider.gameObject;
                    selectedObject.transform.position = hitPose.position; // 实时更新位置
                }
                else
                {
                    selectedObject = null;
                }
            }

            // 显示触摸反馈
            UIManager.Instance.ShowMessage("The object has been placed! Location: " + hitPose.position.ToString("F2"));
        }
        else
        {
            // 如果没有检测到平面，可以隐藏 placementIndicator 并显示提示
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(false);
            }
            UIManager.Instance.ShowMessage("No plane detected, please move the device.");
        }
    }
    private void OnDestroy()
    {
        // 取消事件监听
        arPlaneManager.planesChanged -= OnPlanesChanged;
    }
}
