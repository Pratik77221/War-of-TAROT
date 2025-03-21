using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem; // 新输入系统

public class ImageRaycastHandler : MonoBehaviour
{
    [Header("AR Components")]
    public ARRaycastManager arRaycastManager;        // 请在 Inspector 中绑定 XR Origin 上的 ARRaycastManager
    public ARTrackedImageManager arTrackedImageManager; // 请在 Inspector 中绑定 XR Origin 上的 ARTrackedImageManager

    [Header("Interaction Effects")]
    public GameObject placementIndicatorPrefab;      // 用于指示检测到的目标位置的预制件
    public ParticleSystem hitEffect;                 // 命中时播放的粒子特效
    public AudioClip successSound;                   // 命中音效
    public GameObject objectToPlace;                 // 命中后要放置的物体预制件

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject placementIndicator;
    private Touch touch1, touch2;
    private GameObject selectedObject;

    private void Start()
    {
        // 检查 arTrackedImageManager 是否已在 Inspector 中绑定
        if (arTrackedImageManager == null)
        {
            Debug.LogError("ARTrackedImageManager is not assigned. Please assign it in the Inspector.");
            return;
        }

        // 订阅图片目标的事件
        arTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

        // 如果有预制的 placementIndicator，则实例化并初始隐藏
        if (placementIndicatorPrefab != null)
        {
            placementIndicator = Instantiate(placementIndicatorPrefab);
            placementIndicator.SetActive(false);
        }

        // 初始UI提示（通过 UIManager 显示）
        UIManager.Instance.ShowMessage("Please scan the image target and tap to interact!", 5f);
    }

    private void Update()
    {
        // 使用新输入系统检测触摸（安卓设备）
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

        // 双指缩放
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

    // 当图片目标检测状态发生变化时调用
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // 当检测到新图片目标时，显示提示和指示器
        foreach (var trackedImage in eventArgs.added)
        {
            UIManager.Instance.ShowMessage("Image target detected: " + trackedImage.referenceImage.name + "You can now place objects.", 3f);
            // 可选：将 placementIndicator 移动到图片目标位置
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(true);
                placementIndicator.transform.position = trackedImage.transform.position;
                placementIndicator.transform.rotation = trackedImage.transform.rotation;
            }
        }
        // 你可以根据需要处理 updated 和 removed 状态
    }

    void ProcessRaycast(Vector2 screenPos)
    {
        // 生成一条射线
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 1.0f); // 在 Scene 视图中可视化

        RaycastHit hit;
        // 使用 Physics.Raycast 检测触摸是否击中了带 Collider 的对象
        if (Physics.Raycast(ray, out hit))
        {
            // 判断该对象是否属于图片目标（假设图片目标预制件上带有 ARTrackedImage 组件，或者其父对象有）
            ARTrackedImage trackedImage = hit.collider.GetComponentInParent<ARTrackedImage>();
            if (trackedImage != null)
            {
                // 获取射线击中的位置作为交互点
                Pose hitPose = new Pose(hit.point, hit.collider.transform.rotation);

                // 更新 placementIndicator（如果设置了）
                if (placementIndicator != null)
                {
                    placementIndicator.SetActive(true);
                    placementIndicator.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                }

                // 播放粒子特效
                if (hitEffect != null)
                {
                    float distance = Vector3.Distance(Camera.main.transform.position, hitPose.position);
                    var mainModule = hitEffect.main;
                    mainModule.startSize = Mathf.Clamp(0.1f * distance, 0.5f, 2f);
                    ParticleSystem effect = Instantiate(hitEffect, hitPose.position, Quaternion.identity);
                    effect.Play();
                    Destroy(effect.gameObject, effect.main.duration); // 播放完毕后自动销毁
                }

                // 播放音效
                if (successSound != null)
                {
                    AudioSource.PlayClipAtPoint(successSound, hitPose.position);
                }

                // 放置物体（例如一个虚拟物品）
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

                // 更新 UI 状态
                UIManager.Instance.ShowMessage("Image target raycast hit: " + trackedImage.referenceImage.name);
                return;
            }
        }

        // 如果射线没有击中图片目标
        if (placementIndicator != null)
        {
            placementIndicator.SetActive(false);
        }
        UIManager.Instance.ShowMessage("No image target detected.");
    }

    private void OnDestroy()
    {
        if (arTrackedImageManager != null)
            arTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
}