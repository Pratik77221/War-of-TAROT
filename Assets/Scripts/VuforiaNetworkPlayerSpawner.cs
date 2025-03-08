using UnityEngine;
using Vuforia;
using Photon.Pun;

public class VuforiaNetworkPlayerSpawner : MonoBehaviour
{
    private ObserverBehaviour observerBehaviour;
    private bool hasSpawnedPlayer = false;
    private GameObject spawnedPlayer;

    // Names of the networked player prefabs (from the Resources folder)
    public string player1PrefabName = "Player1"; // For Master Client
    public string player2PrefabName = "Player2"; // For non-master clients

    // Optional offset to adjust the spawn position relative to the image target.
    public Vector3 spawnOffset = new Vector3(0f, 0.1f, 0f);

    void Start()
    {
        observerBehaviour = GetComponent<ObserverBehaviour>();
        if (observerBehaviour != null)
        {
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
            Debug.Log("VuforiaNetworkPlayerSpawner: Registered OnTargetStatusChanged for " + observerBehaviour.TargetName);
        }
        else
        {
            Debug.LogError("VuforiaNetworkPlayerSpawner: No ObserverBehaviour found on this GameObject.");
        }
    }

    private void OnDestroy()
    {
        if (observerBehaviour != null)
        {
            observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }

    /// <summary>
    /// Called when the status of the image target changes.
    /// </summary>
    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        // Consider the target as detected if its status is TRACKED or EXTENDED_TRACKED.
        if (targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED)
        {
            Debug.Log("Image target detected: " + behaviour.TargetName);
            TrySpawnForImage();
        }
        else
        {
            Debug.Log("Image target lost: " + behaviour.TargetName);
        }
    }

    /// <summary>
    /// Spawns the networked player prefab on the image target if it hasn't been spawned already.
    /// </summary>
    private void TrySpawnForImage()
    {
        if (!hasSpawnedPlayer)
        {
            // Calculate the spawn position with an offset.
            Vector3 spawnPos = transform.position + spawnOffset;
            Quaternion spawnRot = transform.rotation;

            // Choose prefab based on PhotonNetwork.IsMasterClient.
            string prefabName = PhotonNetwork.IsMasterClient ? player1PrefabName : player2PrefabName;
            Debug.Log("Attempting to spawn prefab: " + prefabName + " at " + spawnPos);

            // Instantiate the networked prefab using PhotonNetwork.Instantiate.
            spawnedPlayer = PhotonNetwork.Instantiate(prefabName, spawnPos, spawnRot);
            if (spawnedPlayer != null)
            {
                // Parent the spawned object to the image target so it stays anchored.
                spawnedPlayer.transform.SetParent(transform, true);
                hasSpawnedPlayer = true;
                Debug.Log("Player spawned successfully: " + prefabName);
            }
            else
            {
                Debug.LogError("Failed to spawn prefab: " + prefabName);
            }
        }
        else
        {
            Debug.Log("Player already spawned on this image target.");
        }
    }
}
 