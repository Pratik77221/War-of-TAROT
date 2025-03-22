using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Google.XR.ARCoreExtensions;
using UnityEngine.EventSystems;
using System.Collections;

public class ARGameManager : MonoBehaviourPunCallbacks
{
    [Header("AR Components")]
    public ARRaycastManager arRaycastManager;    
    public ARAnchorManager anchorManager;        

    [Header("Environment Prefab")]
    public GameObject environmentPrefab; 

    private GameObject environmentInstance;
    private bool environmentPlaced = false; 

    
    private string hostedAnchorId = null;
    private int environmentViewId = -1;

    // For plane raycasting
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    //[System.Serializable]
    //public struct CardPrefabMapping
    //{
    //    public string cardName;
    //    public GameObject prefab;
    //}
    //public List<CardPrefabMapping> cardPrefabMappings;

    
    // private Dictionary<string, GameObject> cardPrefabDict = new Dictionary<string, GameObject>();

    void Start()
    {
        
        /*
        foreach (var mapping in cardPrefabMappings)
        {
            if (!cardPrefabDict.ContainsKey(mapping.cardName))
            {
                cardPrefabDict.Add(mapping.cardName, mapping.prefab);
            }
        }
        */

        Debug.Log($"[MasterClient? {PhotonNetwork.IsMasterClient}] Player1Cards: {string.Join(", ", GameManager.Instance.player1Cards)}");
        Debug.Log($"[MasterClient? {PhotonNetwork.IsMasterClient}] Player2Cards: {string.Join(", ", GameManager.Instance.player2Cards)}");

        // Check references
        if (arRaycastManager == null)
            Debug.LogError("[ARGameManager] ARRaycastManager not assigned!");
        if (anchorManager == null)
            Debug.LogError("[ARGameManager] ARAnchorManager not assigned (needed for Cloud Anchors)!");
        if (environmentPrefab == null)
            Debug.LogError("[ARGameManager] environmentPrefab not assigned!");
    }

    void Update()
    {
        // Only the Master Client can place the environment, and only if it isn't placed yet.
        if (PhotonNetwork.IsMasterClient && !environmentPlaced && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began && !IsPointerOverUI(touch))
            {
                if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    Debug.Log($"[MasterClient] Plane hit at {hitPose.position}, hosting Cloud Anchor...");
                    HostAnchor(hitPose);
                }
            }
        }
        else if (!PhotonNetwork.IsMasterClient && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Non-master client taps do nothing
            Debug.Log("[Non-MasterClient] Tap detected, ignoring. Only Master can host environment.");
        }
    }

    bool IsPointerOverUI(Touch touch)
    {
        var eventData = new PointerEventData(EventSystem.current) { position = touch.position };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    //========================================================
    //        HOST ANCHOR (Master Client)
    //========================================================

    void HostAnchor(Pose pose)
    {
        ARAnchor localAnchor = anchorManager.AddAnchor(pose);
        if (localAnchor == null)
        {
            Debug.LogError("[MasterClient] Failed to create local anchor.");
            return;
        }

        environmentPlaced = true; // No more placements
        Debug.Log("[MasterClient] Local anchor created. Now hosting Cloud Anchor...");

        ARCloudAnchor cloudAnchor = anchorManager.HostCloudAnchor(localAnchor, 1);
        StartCoroutine(CheckHostingProgress(cloudAnchor, localAnchor));
    }

    IEnumerator CheckHostingProgress(ARCloudAnchor cloudAnchor, ARAnchor localAnchor)
    {
        Debug.Log("[MasterClient] Hosting in progress...");

        while (cloudAnchor.cloudAnchorState == CloudAnchorState.TaskInProgress)
        {
            yield return null;
        }

        if (cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
        {
            hostedAnchorId = cloudAnchor.cloudAnchorId;
            Debug.Log("[MasterClient] Hosted Cloud Anchor successfully! ID: " + hostedAnchorId);

            // Network-instantiate the environment
            environmentInstance = PhotonNetwork.Instantiate(environmentPrefab.name,
                localAnchor.transform.position,
                localAnchor.transform.rotation);

            // Parent it to the local anchor
            environmentInstance.transform.SetParent(localAnchor.transform, true);

            // Get the environment's PhotonView ID
            PhotonView envView = environmentInstance.GetComponent<PhotonView>();
            environmentViewId = envView.ViewID;

            Debug.Log($"[MasterClient] Environment placed & parented. ViewID={environmentViewId}.");

            // If environment has EnvironmentSpawnPoints, you could spawn characters here,
            // but we've commented that out.

            /*
            var envSpawn = environmentInstance.GetComponent<EnvironmentSpawnPoints>();
            if (envSpawn != null && envSpawn.spawnPoints != null && envSpawn.spawnPoints.Length >= 10)
            {
                Debug.Log($"[MasterClient] Found {envSpawn.spawnPoints.Length} spawn points. Spawning characters...");
                // Player1 => [0..4], Player2 => [5..9]
                SpawnPlayerCharacters(envSpawn.spawnPoints, GameManager.Instance.player1Cards, 0, "Player1");
                SpawnPlayerCharacters(envSpawn.spawnPoints, GameManager.Instance.player2Cards, 5, "Player2");
            }
            else
            {
                Debug.LogError("[MasterClient] Environment prefab missing 'EnvironmentSpawnPoints' or <10 spawn points!");
            }
            */

            // Finally, send the anchor ID & environment ViewID to non-master
            photonView.RPC("RPC_ReceiveCloudAnchorID", RpcTarget.Others, hostedAnchorId, environmentViewId);
        }
        else
        {
            Debug.LogError("[MasterClient] Failed to host Cloud Anchor: " + cloudAnchor.cloudAnchorState);
        }
    }

    //========================================================
    //       RPC: Second Device Resolves Anchor & Reparents
    //========================================================

    [PunRPC]
    public void RPC_ReceiveCloudAnchorID(string anchorId, int envViewId)
    {
        Debug.Log($"[Non-MasterClient] RPC_ReceiveCloudAnchorID anchorId={anchorId}, envViewId={envViewId}");
        hostedAnchorId = anchorId;
        environmentViewId = envViewId;
        ResolveCloudAnchor(anchorId);
    }

    void ResolveCloudAnchor(string anchorId)
    {
        Debug.Log("[Non-MasterClient] Attempting to resolve anchor ID: " + anchorId);

        if (anchorManager == null)
        {
            Debug.LogError("[Non-MasterClient] anchorManager is null. Cannot resolve.");
            return;
        }

        ARCloudAnchor resolvedAnchor = anchorManager.ResolveCloudAnchorId(anchorId);
        if (resolvedAnchor == null)
        {
            Debug.LogError("[Non-MasterClient] Failed to create resolve request for ID: " + anchorId);
            return;
        }

        StartCoroutine(CheckResolvingProgress(resolvedAnchor));
    }

    IEnumerator CheckResolvingProgress(ARCloudAnchor resolvedAnchor)
    {
        Debug.Log("[Non-MasterClient] Resolving in progress...");

        while (resolvedAnchor.cloudAnchorState == CloudAnchorState.TaskInProgress)
        {
            yield return null;
        }

        if (resolvedAnchor.cloudAnchorState == CloudAnchorState.Success)
        {
            Debug.Log("[Non-MasterClient] Successfully resolved Cloud Anchor!");

            if (environmentViewId == -1)
            {
                Debug.LogError("[Non-MasterClient] environmentViewId is invalid, can't find environment object.");
                yield break;
            }

            // Find the environment object by ViewID
            PhotonView envView = PhotonView.Find(environmentViewId);
            if (envView == null)
            {
                Debug.LogError("[Non-MasterClient] Could not find environment with ViewID=" + environmentViewId);
                yield break;
            }

            GameObject envObj = envView.gameObject;
            envObj.transform.SetParent(resolvedAnchor.transform, true);

            envObj.transform.localPosition = Vector3.zero;
            envObj.transform.localRotation = Quaternion.identity;

            Debug.Log("[Non-MasterClient] Environment re-parented to resolved anchor. Both see same real-world location.");

            // Characters would be spawned here if we wanted them on the second device.
            /*
            var envSpawn = envObj.GetComponent<EnvironmentSpawnPoints>();
            if (envSpawn != null && envSpawn.spawnPoints != null && envSpawn.spawnPoints.Length >= 10)
            {
                // Example: Spawn characters here if needed
                // SpawnPlayerCharacters(envSpawn.spawnPoints, GameManager.Instance.player1Cards, 0, "Player1");
                // SpawnPlayerCharacters(envSpawn.spawnPoints, GameManager.Instance.player2Cards, 5, "Player2");
            }
            */
        }
        else
        {
            Debug.LogError("[Non-MasterClient] Failed to resolve Cloud Anchor: " + resolvedAnchor.cloudAnchorState);
        }
    }

    //========================================================
    //      Character-Spawning Logic (Commented Out)
    //========================================================

    /*
    void SpawnPlayerCharacters(Transform[] spawnPoints, List<string> cardList, int startIndex, string playerLabel)
    {
        int maxCount = 5;
        int spawnCount = Mathf.Min(cardList.Count, maxCount);

        Debug.Log($"[MasterClient] Spawning {spawnCount} character(s) for {playerLabel} at spawnIndex range [{startIndex}..{startIndex + spawnCount - 1}]");

        for (int i = 0; i < spawnCount; i++)
        {
            string cardName = cardList[i];
            if (cardPrefabDict.ContainsKey(cardName))
            {
                int spawnIndex = startIndex + i;
                Transform sp = spawnPoints[spawnIndex];

                // Network Instantiate so all players see it
                GameObject spawnedObject = PhotonNetwork.Instantiate(
                    cardPrefabDict[cardName].name,
                    sp.position,
                    sp.rotation
                );

                Debug.Log($"[MasterClient] Spawned '{cardName}' for {playerLabel} at spawnIndex={spawnIndex} " +
                          $"(Pos: {sp.position}, Rot: {sp.rotation}). Object: {spawnedObject.name}");
            }
            else
            {
                Debug.LogWarning($"[MasterClient] No prefab mapped for card: {cardName}");
            }
        }
    }
    */
}
