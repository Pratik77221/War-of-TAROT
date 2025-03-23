using UnityEngine;
using Photon.Pun;

public class SpawnPoint : MonoBehaviourPunCallbacks
{
    public GameObject characterPrefab; // Reference to the character prefab
    public Transform spawnPoint; // Reference to the spawn point (child of cloud anchor)

    public void SpawnCharacter()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // Spawn the character at the spawn point's position
            PhotonNetwork.Instantiate(characterPrefab.name, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("Character Spawned at: " + spawnPoint.position);
        }
    }
}