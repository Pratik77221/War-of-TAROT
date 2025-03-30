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

    void Start()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowPersistentMessage("Tap on a flat surface to place the game environment. \n Both players need to scan the same area.");
        }
        
    //     // Check for AR component availability
    //     if (raycastManager == null || anchorManager == null)
    //     {
    //         if (TutorialManager.Instance != null)
    //         {
    //             TutorialManager.Instance.ShowPersistentMessage("Error: AR components not available. Please restart the app or check device compatibility.");
    //         }
    //         Debug.LogError("AR components missing: " + (raycastManager == null ? "ARRaycastManager " : "") + (anchorManager == null ? "ARAnchorManager" : ""));
    //         return;
    //     }
        
    //     // Check if ARCore Extensions are available on device
    //     CheckARCoreAvailability();
    // }
    
    // private void CheckARCoreAvailability()
    // {
    //     // This method checks if the device supports ARCore Cloud Anchors
    //     try
    //     {
    //         if (!ARCoreExtensions.IsARCoreExtensionsEnabled())
    //         {
    //             if (TutorialManager.Instance != null)
    //             {
    //                 TutorialManager.Instance.ShowPersistentMessage("Warning: ARCore Extensions not available. Multiplayer functionality may be limited.");
    //             }
    //         }
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Debug.LogError("Error checking ARCore availability: " + ex.Message);
    //         if (TutorialManager.Instance != null)
    //         {
    //             TutorialManager.Instance.ShowPersistentMessage("Error initializing AR. Please ensure ARCore is installed and up to date.");
    //         }
    //     }
    }

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
        else
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.ShowTemporaryMessage("No surface detected. \n Please aim at a flat surface and try again.", 2f);
            }
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


    /*
   void CreateLocalEnvironment(Transform anchorTransform)
   {
       
       environmentInstance = Instantiate(objectToPlace, anchorTransform.position, anchorTransform.rotation);
       spawnPoints = environmentInstance.GetComponent<EnvironmentSpawnPoints>();
       HideARPlanes();
       photonView.RPC("RPC_SpawnAllCharacters", RpcTarget.AllBuffered);
   }
   */

    void CreateLocalEnvironment(Transform anchorTransform)
    {
        if (environmentPlaced) return;
        environmentPlaced = true;

        environmentInstance = Instantiate(objectToPlace,
            anchorTransform.position,
            anchorTransform.rotation);

        spawnPoints = environmentInstance.GetComponent<EnvironmentSpawnPoints>();

        HideARPlanes();

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

    // method to hide AR planes.
    void HideARPlanes()
    {
        ARPlaneManager planeManager = FindObjectOfType<ARPlaneManager>();
        if (planeManager != null)
        {
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
            planeManager.enabled = false;
        }
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
        
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowTemporaryMessage("Characters spawned! \n Tap on your character to control it.", 3f);
        }
    }


    /*
   void SpawnPlayerCharacters(List<string> characterPrefabNames, int startIndex, int playerIndex)
   {
       foreach (string prefabName in characterPrefabNames)
       {
           PhotonNetwork.Instantiate(prefabName, Vector3.zero, Quaternion.identity);
       }
   }
   */

    void SpawnPlayerCharacters(List<string> characterPrefabNames, int startIndex, int playerIndex)
    {
        Player targetPlayer = playerIndex == 0 ?
            PhotonNetwork.LocalPlayer :
            PhotonNetwork.PlayerListOthers[0];
            
        if (characterPrefabNames.Count == 0)
        {
            Debug.LogError("No character prefabs found for player " + (playerIndex + 1));
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.ShowTemporaryMessage("<color=red>Error: No characters found for Player " + (playerIndex + 1) + ". \n Please restart the game.</color>", 3f);
            }
            return;
        }

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
                        
                        if (TutorialManager.Instance != null)
                        {
                            TutorialManager.Instance.ShowTemporaryMessage($"{hit.collider.gameObject.name} activated! \n It will attack the nearest enemy.", 1.5f);
                        }
                    }
                    else
                    {
                        if (TutorialManager.Instance != null)
                        {
                            TutorialManager.Instance.ShowTemporaryMessage("You can only control your own characters!", 1.5f);
                        }
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
