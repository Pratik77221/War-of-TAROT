using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Photon.Pun;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ARNetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    private ARTrackedImageManager trackedImageManager;

    // Ensures each client only spawns its player once.
    private bool hasSpawnedPlayer = false;

    // Reference to the spawned player instance.
    private GameObject spawnedPlayer;

    // Names of the networked player prefabs (placed in Resources folder)
    public string player1PrefabName = "Player1"; // For Master Client
    public string player2PrefabName = "Player2"; // For other clients

    // Optional offset to adjust the spawn position relative to the tracked image.
    public Vector3 spawnOffset = new Vector3(0f, 0.1f, 0f); // Adjust this as needed

    private void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
        Debug.Log("ARNetworkPlayerSpawner Awake: ARTrackedImageManager assigned.");
    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        Debug.Log("ARNetworkPlayerSpawner OnEnable: Subscribed to trackedImagesChanged.");
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        Debug.Log("ARNetworkPlayerSpawner OnDisable: Unsubscribed from trackedImagesChanged.");
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Process added images
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            Debug.Log("Tracked image added: " + trackedImage.referenceImage.name);
            TrySpawnForImage(trackedImage);
        }

        // Update the spawned object's transform for updated tracking data.
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            Debug.Log("Tracked image updated: " + trackedImage.referenceImage.name + " state: " + trackedImage.trackingState);
            if (hasSpawnedPlayer && trackedImage.trackingState == TrackingState.Tracking)
            {
                if (spawnedPlayer != null)
                {
                    // Optionally use local position relative to the image target
                    spawnedPlayer.transform.position = trackedImage.transform.position + spawnOffset;
                    spawnedPlayer.transform.rotation = trackedImage.transform.rotation;
                    Debug.Log("Updated spawned player's transform with offset: " + spawnOffset);
                }
            }
        }

        // Optionally handle removed images if needed.
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            Debug.Log("Tracked image removed: " + trackedImage.referenceImage.name);
            // Optionally, you might choose to destroy the spawned object here.
        }
    }

    /// <summary>
    /// Attempts to spawn the networked player prefab when an image is tracked.
    /// </summary>
    /// <param name="trackedImage">The ARTrackedImage detected.</param>
    private void TrySpawnForImage(ARTrackedImage trackedImage)
    {
        if (!hasSpawnedPlayer && trackedImage.trackingState == TrackingState.Tracking)
        {
            Debug.Log("Attempting to spawn player for image: " + trackedImage.referenceImage.name);
            Vector3 spawnPos = trackedImage.transform.position + spawnOffset;
            Quaternion spawnRot = trackedImage.transform.rotation;

            // Choose prefab based on PhotonNetwork.IsMasterClient
            string prefabName = PhotonNetwork.IsMasterClient ? player1PrefabName : player2PrefabName;
            Debug.Log("Instantiating prefab: " + prefabName + " at " + spawnPos);

            // Instantiate the networked player prefab using PhotonNetwork.Instantiate
            spawnedPlayer = PhotonNetwork.Instantiate(prefabName, spawnPos, spawnRot);
            if (spawnedPlayer != null)
            {
                Debug.Log("Player spawned successfully: " + prefabName);
                // Optionally, set the spawned object's parent after instantiation
                spawnedPlayer.transform.SetParent(trackedImage.transform, true);
                hasSpawnedPlayer = true;
            }
            else
            {
                Debug.LogError("PhotonNetwork.Instantiate failed for prefab: " + prefabName);
            }
        }
        else if (hasSpawnedPlayer)
        {
            Debug.Log("Player already spawned; skipping spawn for image: " + trackedImage.referenceImage.name);
        }
        else
        {
            Debug.Log("Tracked image not in Tracking state; cannot spawn player.");
        }
    }
}
