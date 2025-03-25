using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class CharacterControllerValidator : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Automatically adjust invalid values")]
    public bool autoAdjust = true;

    [Header("Debug")]
    public bool showDebugVisuals = true;
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    private CharacterController controller;
    private float originalStepOffset;

    void OnValidate()
    {
        ValidateController();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            ValidateController();
        }
#endif
    }

    void ValidateController()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null) return;

        // Calculate scaled dimensions
        Vector3 lossyScale = transform.lossyScale;
        float scaledHeight = controller.height * lossyScale.y;
        float scaledRadius = controller.radius * Mathf.Max(lossyScale.x, lossyScale.z);

        // Calculate maximum allowed step offset
        float maxStepOffset = scaledHeight + (scaledRadius * 2);

        // Check step offset
        if (controller.stepOffset > maxStepOffset)
        {
            Debug.LogError($"Invalid Step Offset: {controller.stepOffset} > {maxStepOffset}", this);

            if (autoAdjust)
            {
                controller.stepOffset = maxStepOffset;
                Debug.Log($"Auto-adjusted Step Offset to {maxStepOffset}");
            }
        }

        // Store original value for visualization
        originalStepOffset = controller.stepOffset;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!showDebugVisuals || controller == null) return;

        Handles.color = controller.stepOffset <= originalStepOffset ? validColor : invalidColor;

        // Draw collision capsule
        Vector3 center = transform.position + controller.center;
        float height = controller.height * transform.lossyScale.y;
        float radius = controller.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);

        DrawCapsule(center, height, radius);
    }

    void DrawCapsule(Vector3 center, float height, float radius)
    {
        Vector3 top = center + Vector3.up * (height / 2 - radius);
        Vector3 bottom = center - Vector3.up * (height / 2 - radius);

        // Draw spheres
        Handles.SphereHandleCap(0, top, Quaternion.identity, radius * 2, EventType.Repaint);
        Handles.SphereHandleCap(0, bottom, Quaternion.identity, radius * 2, EventType.Repaint);

        // Draw cylinder
        Handles.DrawWireArc(top, Vector3.forward, Vector3.right, 360, radius);
        Handles.DrawWireArc(bottom, Vector3.forward, Vector3.right, 360, radius);
        Handles.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
        Handles.DrawLine(top - Vector3.right * radius, bottom - Vector3.right * radius);
        Handles.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
        Handles.DrawLine(top - Vector3.forward * radius, bottom - Vector3.forward * radius);
    }
#endif
}