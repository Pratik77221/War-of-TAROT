using Photon.Pun;
using Photon.Realtime;  // For PhotonNetwork.CurrentRoom
using UnityEngine;
using Vuforia;
using ExitGames.Client.Photon; // For Hashtable

[System.Serializable]
public class CardEntry
{
    [Tooltip("Vuforia ObserverBehaviour (the ImageTarget).")]
    public ObserverBehaviour observer;

    [Tooltip("Prefab name in Resources (e.g. 'Model1'). Leave empty if no model is needed.")]
    public string prefabName;

    [HideInInspector] public bool spawned;
    [HideInInspector] public GameObject spawnedModel;
}

public class ImageTargetHandler : MonoBehaviour
{
    [Tooltip("Add one entry per ImageTarget. No local child objects!")]
    public CardEntry[] cardEntries;

    private void Start()
    {
        foreach (var entry in cardEntries)
        {
            if (entry.observer != null)
            {
                // Disable the default Vuforia event handler (prevents local-only spawning)
                var defaultHandler = entry.observer.GetComponent<DefaultObserverEventHandler>();
                if (defaultHandler != null)
                {
                    defaultHandler.enabled = false;
                }

                // Subscribe to tracking status changes
                var currentEntry = entry;
                currentEntry.observer.OnTargetStatusChanged += (obs, status) =>
                {
                    OnTargetStatusChanged(currentEntry, obs, status);
                };
            }
            else
            {
                Debug.LogError("ObserverBehaviour missing in one of the CardEntry elements.");
            }
        }
    }

    private void OnTargetStatusChanged(CardEntry entry, ObserverBehaviour observer, TargetStatus status)
    {
        // "Found" if TRACKED or EXTENDED_TRACKED
        if (status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED)
        {
            OnCardFound(entry);
        }
        else
        {
            OnCardLost(entry);
        }
    }

    private void OnCardFound(CardEntry entry)
    {
        // Already spawned locally? Skip.
        if (entry.spawned) return;
        // No prefab assigned? Skip.
        if (string.IsNullOrEmpty(entry.prefabName)) return;

        // Build a unique key (both for the room property and local object name)
        string uniqueKey = entry.observer.gameObject.name + "_" + entry.prefabName;

        // Locally, check if the object is already in the scene
        if (GameObject.Find(uniqueKey) != null)
        {
            // It's already there (spawned by someone else and synced to us)
            entry.spawned = true;
            return;
        }

        // 1) Check if room property is already set (someone else claimed it)
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(uniqueKey))
        {
            // Already claimed by another player, so skip spawning
            entry.spawned = true;
            return;
        }

        // 2) Attempt to set a custom property to claim this card
        //    We'll use "expectedProps" to ensure it's currently unset (null).
        Hashtable newProps = new Hashtable();
        newProps[uniqueKey] = PhotonNetwork.LocalPlayer.UserId;  // store who owns it

        Hashtable expectedProps = new Hashtable();
        expectedProps[uniqueKey] = null; // we expect no one has claimed it

        // If this fails, someone else claimed it at the same time
        bool success = PhotonNetwork.CurrentRoom.SetCustomProperties(newProps, expectedProps);
        if (!success)
        {
            // Another device claimed it in the same moment
            entry.spawned = true;
            return;
        }

        // 3) If we reach here, we successfully claimed the card first.
        //    Spawn the networked model and own it.
        Vector3 spawnPos = entry.observer.transform.position;
        Quaternion spawnRot = entry.observer.transform.rotation;

        GameObject netModel = PhotonNetwork.Instantiate(entry.prefabName, spawnPos, spawnRot);
        netModel.name = uniqueKey;

        // Parent to the target so it remains anchored
        netModel.transform.SetParent(entry.observer.transform, false);

        entry.spawnedModel = netModel;
        entry.spawned = true;

        // Request ownership so we can move/scale it
        PhotonView pv = netModel.GetComponent<PhotonView>();
        if (pv != null)
        {
            pv.RequestOwnership();
        }

        Debug.Log($"[CLAIMED] '{entry.prefabName}' for '{entry.observer.gameObject.name}' by {PhotonNetwork.LocalPlayer.NickName}");
    }

    private void OnCardLost(CardEntry entry)
    {
        Debug.Log($"Target lost: {entry.observer.gameObject.name}");
        // Optionally destroy/hide the model if tracking is lost:
        // if (entry.spawnedModel != null)
        // {
        //     PhotonNetwork.Destroy(entry.spawnedModel);
        //     entry.spawnedModel = null;
        //     entry.spawned = false;
        // }
    }
}
