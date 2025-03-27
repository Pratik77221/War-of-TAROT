using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// For spawning characters on the cards
/// </summary>

[RequireComponent(typeof(ARTrackedImageManager))]
public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabsToSpawn;

    private ARTrackedImageManager _arTrackedImageManager;
    private Dictionary<string, GameObject> _arObjects = new Dictionary<string, GameObject>();

    private void Awake()
    {
        _arTrackedImageManager = GetComponent<ARTrackedImageManager>();
        foreach (GameObject prefab in prefabsToSpawn)
        {
            GameObject newARObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            newARObject.name = prefab.name;
            newARObject.SetActive(false);
            if (!_arObjects.ContainsKey(newARObject.name))
            {
                _arObjects.Add(newARObject.name, newARObject);
            }
        }
    }

    private void OnEnable()
    {
        _arTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        _arTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            UpdateTrackedImage(trackedImage);
        }
        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateTrackedImage(trackedImage);
        }
        foreach (var trackedImage in eventArgs.removed)
        {
            if (_arObjects.ContainsKey(trackedImage.referenceImage.name))
            {
                _arObjects[trackedImage.referenceImage.name].SetActive(false);
            }
        }
    }

    private void UpdateTrackedImage(ARTrackedImage trackedImage)
    {
        if (!_arObjects.ContainsKey(trackedImage.referenceImage.name))
        {
            return;
        }
        GameObject arObject = _arObjects[trackedImage.referenceImage.name];
        if (trackedImage.trackingState == TrackingState.Limited || trackedImage.trackingState == TrackingState.None)
        {
            arObject.SetActive(false);
            return;
        }
        arObject.SetActive(true);
        arObject.transform.position = trackedImage.transform.position;
        arObject.transform.rotation = trackedImage.transform.rotation;
    }
}
