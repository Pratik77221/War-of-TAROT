using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class SpawnOnImage : MonoBehaviour
{
    public GameObject spawnedPrefab;
    private ARTrackedImageManager trackedImageManager;

    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            SpawnOrUpdate(trackedImage);
        }
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            SpawnOrUpdate(trackedImage);
        }
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            if (spawnedObjects.ContainsKey(trackedImage.trackableId.ToString()))
            {
                Destroy(spawnedObjects[trackedImage.trackableId.ToString()]);
                spawnedObjects.Remove(trackedImage.trackableId.ToString());
            }
        }
    }

    private void SpawnOrUpdate(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            string imageID = trackedImage.trackableId.ToString();
            if (!spawnedObjects.ContainsKey(imageID))
            {
                GameObject newObject = Instantiate(spawnedPrefab, trackedImage.transform.position, trackedImage.transform.rotation);
                newObject.transform.parent = trackedImage.transform;
                spawnedObjects.Add(imageID, newObject);
            }
            else
            {
                // Update position/rotation if needed
                GameObject existingObject = spawnedObjects[imageID];
                existingObject.transform.position = trackedImage.transform.position;
                existingObject.transform.rotation = trackedImage.transform.rotation;
            }
        }
    }
}
