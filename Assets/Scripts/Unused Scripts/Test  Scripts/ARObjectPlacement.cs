using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARObjectPlacement : MonoBehaviour
{
    // The 3D object prefab you want to instantiate on tap
    public GameObject objectToPlace;

    // Reference to the ARRaycastManager (usually attached to the AR Session Origin)
    public ARRaycastManager arRaycastManager;

    // List to store raycast hits
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        // Check if there is a touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // When touch begins, perform a raycast to detect AR planes
            if (touch.phase == TouchPhase.Began)
            {
                if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    // Get the pose of the first hit point (ground plane)
                    Pose hitPose = hits[0].pose;

                    // Instantiate the object at the hit position and rotation
                    Instantiate(objectToPlace, hitPose.position, hitPose.rotation);
                }
            }
        }
    }
}
