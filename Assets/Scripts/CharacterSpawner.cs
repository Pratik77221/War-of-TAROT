using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharacterSpawner : MonoBehaviourPun
{
    [System.Serializable]
    public struct CardPrefabMapping
    {
        public string cardName;   // e.g. "Warrior", "Mage"
        public GameObject prefab; // The character prefab
    }

    public List<CardPrefabMapping> cardMappings;

    private Dictionary<string, GameObject> cardPrefabDict = new Dictionary<string, GameObject>();

    void Start()
    {
        // Build dictionary for quick lookups
        foreach (var mapping in cardMappings)
        {
            if (!cardPrefabDict.ContainsKey(mapping.cardName))
            {
                cardPrefabDict.Add(mapping.cardName, mapping.prefab);
            }
        }
    }

    /// <summary>
    /// Called by ARGameManager once the environment is fully resolved on the second device.
    /// We spawn characters for both players at spawn indices [0..4] and [5..9].
    /// </summary>
    public void SpawnCharacters(GameObject environmentObj)
    {
        Debug.Log("[CharacterSpawner] SpawnCharacters called. Environment object: " + environmentObj.name);

        // Attempt to get the EnvironmentSpawnPoints
        EnvironmentSpawnPoints envSpawn = environmentObj.GetComponent<EnvironmentSpawnPoints>();
        if (envSpawn == null || envSpawn.spawnPoints == null || envSpawn.spawnPoints.Length < 10)
        {
            Debug.LogError("[CharacterSpawner] Environment missing spawn points or <10 spawn points!");
            return;
        }

        // Player1 => [0..4], Player2 => [5..9]
        Debug.Log("[CharacterSpawner] Spawning for Player1 in [0..4], Player2 in [5..9]");

        // Spawn for Player1
        DoSpawn(envSpawn.spawnPoints, GameManager.Instance.player1Cards, 0, "Player1");
        // Spawn for Player2
        DoSpawn(envSpawn.spawnPoints, GameManager.Instance.player2Cards, 5, "Player2");
    }

    private void DoSpawn(Transform[] spawnPoints, List<string> cardList, int startIndex, string playerLabel)
    {
        int maxCount = 5;
        int spawnCount = Mathf.Min(cardList.Count, maxCount);

        Debug.Log($"[CharacterSpawner] Spawning {spawnCount} character(s) for {playerLabel} at indices [{startIndex}..{startIndex + spawnCount - 1}]");

        for (int i = 0; i < spawnCount; i++)
        {
            string cardName = cardList[i];
            if (cardPrefabDict.ContainsKey(cardName))
            {
                int spawnIndex = startIndex + i;
                Transform sp = spawnPoints[spawnIndex];

                // Network instantiate so all players see it
                GameObject spawnedObj = PhotonNetwork.Instantiate(
                    cardPrefabDict[cardName].name,
                    sp.position,
                    sp.rotation
                );

                Debug.Log($"[CharacterSpawner] Spawned '{cardName}' for {playerLabel} at spawnIndex={spawnIndex} (Pos: {sp.position}, Rot: {sp.rotation}).");
            }
            else
            {
                Debug.LogWarning($"[CharacterSpawner] No prefab mapped for card: {cardName}");
            }
        }
    }
}
