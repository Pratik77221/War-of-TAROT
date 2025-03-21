using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // 引入新输入系统

public class RaycastManager : MonoBehaviour
{
    void Update()
    {
        // 检测触摸输入（安卓设备）
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
            ProcessRaycast(touchPos);
        }
        // 编辑器中使用鼠标调试
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            ProcessRaycast(mousePos);
        }
    }

    void ProcessRaycast(Vector2 screenPos)
    {
        // 从主摄像机发射射线
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 1.0f); // 调试可视化
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("Object Detected: " + hit.collider.gameObject.name);
            // 示例：改变被点击物体的颜色
            Renderer rend = hit.collider.gameObject.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = Color.red;
            }
            // 调用交互逻辑（例如触发动画、声音等）
            HandleInteraction(hit.collider.gameObject);
        }
    }

    void HandleInteraction(GameObject target)
    {
        // 判断目标是否有特定标签（例如"Interactable"），以执行定制逻辑
        if (target.CompareTag("Interactable"))
        {
            // 触发目标上的动画（确保目标上有Animator组件和相应动画）
            Animator animator = target.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("OnInteract");
            }
            // 播放交互声音（确保设置了AudioSource组件和音频剪辑）
            AudioSource audio = target.GetComponent<AudioSource>();
            if (audio != null)
            {
                audio.Play();
            }
            // 可以调用振动效果（安卓支持）
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
            // 其他交互逻辑：比如更新UI、分数加成、打开面板等
            UIManager.Instance.ShowMessage("Object clicked: " + target.name);
        }
    }
}
