using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Google.XR.ARCoreExtensions;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class SingleTapCloudAnchorNetworkedCube : MonoBehaviourPun
{
    [Header("AR Managers")]
    public ARAnchorManager anchorManager;
    public ARRaycastManager raycastManager;

    // The ID of the Cloud Anchor once hosted by the Master Client
    private string hostedAnchorId = null;

    // We only want the Master Client to host once
    private bool environmentPlaced = false;

    // The ViewID of the single networked cube. 
    // We use this so the second device can find the same PhotonView.
    private int netObjViewId = -1;

    void Start()
    {
        Debug.Log("[SingleTapCloudAnchorNetworkedCube] Start. IsMasterClient? " + PhotonNetwork.IsMasterClient);

        // Quick reference checks
        if (anchorManager == null)
            Debug.LogError("[SingleTapCloudAnchorNetworkedCube] ARAnchorManager not assigned!");
        if (raycastManager == null)
            Debug.LogError("[SingleTapCloudAnchorNetworkedCube] ARRaycastManager not assigned!");
    }

    void Update()
    {
        // Master Client can tap to host if not already done
        if (PhotonNetwork.IsMasterClient && !environmentPlaced && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log("[MasterClient] Tap: Attempting to host anchor via plane raycast.");
                TryHostAnchor(touch.position);
            }
        }
    }

    /// <summary>
    /// Raycast a plane and host an anchor if found (Master Client only).
    /// </summary>
    private void TryHostAnchor(Vector2 screenPos)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;
            Debug.Log("[MasterClient] Plane hit. Hosting anchor at pose: " + pose.position);
            HostAnchor(pose);
        }
        else
        {
            Debug.Log("[MasterClient] No plane detected at tap position.");
        }
    }

    private void HostAnchor(Pose pose)
    {
        ARAnchor localAnchor = anchorManager.AddAnchor(pose);
        if (localAnchor == null)
        {
            Debug.LogError("[MasterClient] Failed to create local anchor.");
            return;
        }

        environmentPlaced = true;
        Debug.Log("[MasterClient] Local anchor created. Hosting Cloud Anchor...");

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

            // Now we create one networked cube
            // Make sure "DemoCubePrefab" is in Resources and has a PhotonView.
            GameObject netObj = PhotonNetwork.Instantiate("Justice",
                localAnchor.transform.position,
                localAnchor.transform.rotation);

            // Parent the cube to the local anchor on the Master Client
            netObj.transform.SetParent(localAnchor.transform, true);

            // Store the cube's ViewID so the second device can find it
            PhotonView netObjView = netObj.GetComponent<PhotonView>();
            netObjViewId = netObjView.ViewID;

            Debug.Log("[MasterClient] Spawned networked cube. ViewID=" + netObjViewId);

            // Send anchor ID and netObjViewId to the other player(s)
            photonView.RPC("RPC_ReceiveCloudAnchorID", RpcTarget.Others, hostedAnchorId, netObjViewId);
        }
        else
        {
            Debug.LogError("[MasterClient] Failed to host Cloud Anchor: " + cloudAnchor.cloudAnchorState);
        }
    }

    //========================================================
    //          RPC to Send Anchor & Cube Info
    //========================================================

    [PunRPC]
    public void RPC_ReceiveCloudAnchorID(string anchorId, int viewId)
    {
        Debug.Log("[Non-MasterClient] RPC_ReceiveCloudAnchorID called. anchorId=" + anchorId + ", viewId=" + viewId);
        hostedAnchorId = anchorId;
        netObjViewId = viewId;
        ResolveCloudAnchor(anchorId);
    }

    //========================================================
    //          RESOLVING LOGIC (Non-Master)
    //========================================================

    private void ResolveCloudAnchor(string anchorId)
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

            // Reparent the existing networked cube to this resolved anchor
            // so that it appears in the correct real-world location.
            if (netObjViewId == -1)
            {
                Debug.LogError("[Non-MasterClient] netObjViewId is invalid, can't find the cube.");
                yield break;
            }

            PhotonView netObjView = PhotonView.Find(netObjViewId);
            if (netObjView == null)
            {
                Debug.LogError("[Non-MasterClient] Could not find networked object with ViewID=" + netObjViewId);
                yield break;
            }

            GameObject netObj = netObjView.gameObject;
            // Parent it to the resolved anchor
            netObj.transform.SetParent(resolvedAnchor.transform, true);

            // Optionally snap it exactly to anchor center
            netObj.transform.localPosition = Vector3.zero;
            netObj.transform.localRotation = Quaternion.identity;

            Debug.Log("[Non-MasterClient] Re-parented networked cube to resolved anchor. Now both see the same object.");
        }
        else
        {
            Debug.LogError("[Non-MasterClient] Failed to resolve Cloud Anchor: " + resolvedAnchor.cloudAnchorState);
        }
    }
}
