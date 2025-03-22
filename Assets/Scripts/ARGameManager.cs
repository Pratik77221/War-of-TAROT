
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class ARGameManager : MonoBehaviourPunCallbacks
{
    public ARRaycastManager arRaycastManager;
    public GameObject environmentPrefab; // Must have EnvironmentSpawnPoints attached

    private GameObject environmentInstance;
    private Dictionary<string, GameObject> cardPrefabDict = new Dictionary<string, GameObject>();

    private bool environmentPlaced = false;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    [System.Serializable]
    public struct CardPrefabMapping
    {
        public string cardName;
        public GameObject prefab;
    }
    public List<CardPrefabMapping> cardPrefabMappings;

    void Start()
    {
        // Build a dictionary for quick prefab lookups.
        foreach (var mapping in cardPrefabMappings)
        {
            if (!cardPrefabDict.ContainsKey(mapping.cardName))
            {
                cardPrefabDict.Add(mapping.cardName, mapping.prefab);
            }
        }

        // Log out the cards for debugging.
        Debug.Log($"[MasterClient? {PhotonNetwork.IsMasterClient}] Player1Cards: {string.Join(", ", GameManager.Instance.player1Cards)}");
        Debug.Log($"[MasterClient? {PhotonNetwork.IsMasterClient}] Player2Cards: {string.Join(", ", GameManager.Instance.player2Cards)}");
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
                    PlaceEnvironment(hitPose.position, hitPose.rotation);
                }
            }
        }
    }

    bool IsPointerOverUI(Touch touch)
    {
        var eventData = new PointerEventData(EventSystem.current) { position = touch.position };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    void PlaceEnvironment(Vector3 position, Quaternion rotation)
    {
        if (environmentPlaced) return;

        // Master Client places the environment for everyone.
        environmentInstance = PhotonNetwork.Instantiate(environmentPrefab.name, position, rotation);
        environmentPlaced = true;

        Debug.Log($"[MasterClient] Environment placed at: {position}, rotation: {rotation}");

        // Retrieve the spawn points from the newly instantiated environment.
        var envSpawn = environmentInstance.GetComponent<EnvironmentSpawnPoints>();
        if (envSpawn != null && envSpawn.spawnPoints != null && envSpawn.spawnPoints.Length >= 10)
        {
            Debug.Log($"[MasterClient] Found {envSpawn.spawnPoints.Length} spawn points on environment prefab.");

            // Master Client spawns both players' characters.
            SpawnPlayerCharacters(envSpawn.spawnPoints, GameManager.Instance.player1Cards, 0, "Player1");
            SpawnPlayerCharacters(envSpawn.spawnPoints, GameManager.Instance.player2Cards, 5, "Player2");
        }
        else
        {
            Debug.LogError("[MasterClient] Environment prefab missing 'EnvironmentSpawnPoints' or not enough spawn points!");
        }
    }

    // Spawns up to 5 characters for the given player, starting at 'startIndex' in the spawnPoints array.
    void SpawnPlayerCharacters(Transform[] spawnPoints, List<string> cardList, int startIndex, string playerLabel)
    {
        // Each player has 5 spawn points allocated
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

                // Network Instantiate so all players see it.
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
}

