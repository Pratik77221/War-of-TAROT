using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class XR_Placement : MonoBehaviour
{
    
    [SerializeField] private GameObject prefab;
    private List<GameObject> spawnedPrefabs;
    private ARRaycastManager raycastManager;
    
    // Start is called before the first frame update
    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void ARRaycasting(Vector2 pos)
    {
        List<ARRaycastHit> hits = new();
        
        if(raycastManager.Raycast(pos, hits, trackableTypes:TrackableType.PlaneEstimated))
        {
            Pose pose = hits[0].pose;
            ARInstantiation(pose.position, pose.rotation);
        }
    }

    void ARInstantiation(Vector3 pos, Quaternion rot)
    {
        spawnedPrefabs.Add(item:Instantiate(prefab, pos, rot));
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(index:0);
            Vector2 touchPosition = touch.position;
            ARRaycasting(touchPosition);
        }
        
    }
}
