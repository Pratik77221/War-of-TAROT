
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Google.XR.ARCoreExtensions;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using System;

public class ARGameManager : MonoBehaviourPunCallbacks
{
    public GameObject objectToPlace;
    public ARRaycastManager raycastManager;
    public ARAnchorManager anchorManager;

    public EnvironmentSpawnPoints spawnPoints { get; private set; }
    private bool environmentPlaced = false;
    private string hostedAnchorId;
    private GameObject environmentInstance;
    private bool charactersSpawned = false;
    private bool masterReady = false;
    private bool clientReady = false;


    public static event Action<GameObject> OnCharacterTapped;

    void Update()
    {
        // Handle environment placement
        if (!environmentPlaced && !charactersSpawned && photonView.IsMine)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    TryHostAnchor(Input.GetTouch(0).position);
                }
                else
                {
                    photonView.RPC("RPC_RequestAnchorID", RpcTarget.MasterClient);
                }
            }
        }

        if (environmentPlaced && charactersSpawned)
        {
            HandleCharacterTap();
        }
    }

    void TryHostAnchor(Vector2 screenPosition)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            HostAnchor(hitPose);
        }
    }

    void HostAnchor(Pose pose)
    {
        ARAnchor localAnchor = anchorManager.AddAnchor(pose);
        if (localAnchor == null) return;

        ARCloudAnchor cloudAnchor = anchorManager.HostCloudAnchor(localAnchor, 1);
        StartCoroutine(CheckHostingProgress(cloudAnchor));
    }

    IEnumerator CheckHostingProgress(ARCloudAnchor cloudAnchor)
    {
        while (cloudAnchor.cloudAnchorState == CloudAnchorState.TaskInProgress)
            yield return null;

        if (cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
        {
            hostedAnchorId = cloudAnchor.cloudAnchorId;
            photonView.RPC("RPC_ReceiveCloudAnchorID", RpcTarget.Others, hostedAnchorId);
            CreateLocalEnvironment(cloudAnchor.transform);
        }
    }

    [PunRPC]
    void RPC_RequestAnchorID()
    {
        if (!string.IsNullOrEmpty(hostedAnchorId))
            photonView.RPC("RPC_ReceiveCloudAnchorID", RpcTarget.Others, hostedAnchorId);
    }

    [PunRPC]
    void RPC_ReceiveCloudAnchorID(string anchorId)
    {
        if (!string.IsNullOrEmpty(anchorId))
            ResolveCloudAnchor(anchorId);
    }

    void ResolveCloudAnchor(string anchorId)
    {
        ARCloudAnchor resolvedAnchor = anchorManager.ResolveCloudAnchorId(anchorId);
        StartCoroutine(CheckResolvingProgress(resolvedAnchor));
    }

    IEnumerator CheckResolvingProgress(ARCloudAnchor resolvedAnchor)
    {
        while (resolvedAnchor.cloudAnchorState == CloudAnchorState.TaskInProgress)
            yield return null;

        if (resolvedAnchor.cloudAnchorState == CloudAnchorState.Success)
            CreateLocalEnvironment(resolvedAnchor.transform);
    }

    void CreateLocalEnvironment(Transform anchorTransform)
    {
        if (environmentPlaced) return;
        environmentPlaced = true;

        environmentInstance = Instantiate(objectToPlace,
            anchorTransform.position,
            anchorTransform.rotation);

        spawnPoints = environmentInstance.GetComponent<EnvironmentSpawnPoints>();

        if (PhotonNetwork.IsMasterClient)
        {
            masterReady = true;
            photonView.RPC("RPC_EnvironmentReady", RpcTarget.Others);
        }
        else
        {
            photonView.RPC("RPC_EnvironmentReady", RpcTarget.MasterClient);
        }

        CheckBothReady();
    }

    [PunRPC]
    void RPC_EnvironmentReady()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            clientReady = true;
        }
        else
        {
            masterReady = true;
        }
        CheckBothReady();
    }

    void CheckBothReady()
    {
        if (masterReady && clientReady)
        {
            photonView.RPC("RPC_SpawnAllCharacters", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void RPC_SpawnAllCharacters()
    {
        if (charactersSpawned || spawnPoints == null) return;
        charactersSpawned = true;

        if (PhotonNetwork.IsMasterClient)
        {
            SpawnPlayerCharacters(GameManager.Instance.player1Cards, 0, 0);
            SpawnPlayerCharacters(GameManager.Instance.player2Cards, 5, 1);
        }
    }

    void SpawnPlayerCharacters(List<string> characterPrefabNames, int startIndex, int playerIndex)
    {
        Player targetPlayer = playerIndex == 0 ?
            PhotonNetwork.LocalPlayer :
            PhotonNetwork.PlayerListOthers[0];

        for (int i = 0; i < 5; i++)
        {
            if (i >= characterPrefabNames.Count) break;

            int spawnIndex = startIndex + i;
            object[] instantiationData = new object[] { spawnIndex };

            GameObject character = PhotonNetwork.Instantiate(
                characterPrefabNames[i],
                Vector3.zero,
                Quaternion.identity,
                data: instantiationData
            );

            PhotonView pv = character.GetComponent<PhotonView>();
            if (pv != null)
            {
                pv.TransferOwnership(targetPlayer.ActorNumber);
            }
        }
    }

    /*private void HandleCharacterTap()
    {
        if (!charactersSpawned) return;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                PhotonView pv = hit.collider.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    Debug.Log($"{PhotonNetwork.NickName} tapped: {hit.collider.gameObject.name}");
                    OnCharacterTapped?.Invoke(hit.collider.gameObject);
                }
            }
        }
    }*/

    private void HandleCharacterTap()
    {
        if (!charactersSpawned) return;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            int characterLayer = LayerMask.GetMask("Characters");
            float maxDistance = 100f;

            if (Physics.Raycast(ray, out hit, maxDistance, characterLayer))
            {
                PhotonView pv = hit.collider.GetComponentInParent<PhotonView>();

                if (pv != null)
                {
                    if (pv.IsMine)
                    {
                        Debug.Log($"{PhotonNetwork.NickName} tapped: {hit.collider.gameObject.name}");
                        OnCharacterTapped?.Invoke(hit.collider.gameObject);
                    }
                }
                else
                {
                    Debug.LogWarning("Tapped object has no PhotonView component");
                }
            }
        }
    }
}
