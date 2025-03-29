using Photon.Pun;
using UnityEngine;
using System.Collections;

/// <summary>
/// Parent the spawnned character  to the env so they follow the movement and remain inside
/// </summary>
public class NetworkedCharacterParenting : MonoBehaviourPun
{
    private int spawnIndex = -1;
    private ARGameManager gameManager;
    private float timeout = 10f;

    void Start()
    {
        if (photonView.InstantiationData != null && photonView.InstantiationData.Length > 0)
        {
            spawnIndex = (int)photonView.InstantiationData[0];
            StartCoroutine(ParentToEnvironment());
        }
    }

    IEnumerator ParentToEnvironment()
    {
        while (timeout > 0)
        {
            gameManager = FindObjectOfType<ARGameManager>();
            if (gameManager != null && gameManager.spawnPoints != null) // Now accessible
            {
                if (spawnIndex < gameManager.spawnPoints.spawnPoints.Length)
                {
                    Transform spawnPoint = gameManager.spawnPoints.spawnPoints[spawnIndex];
                    transform.SetParent(spawnPoint);
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                    Debug.Log($"Parented character to spawn point {spawnIndex}");
                    yield break;
                }
            }

            timeout -= Time.deltaTime;
            yield return null;
        }
        Debug.LogError($"Failed to parent character {name} to spawn point {spawnIndex}");
    }
}