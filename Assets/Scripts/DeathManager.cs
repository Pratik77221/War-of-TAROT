using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections.Generic;

public class DeathManager : MonoBehaviourPun
{
    public static DeathManager Instance;

    // Death counters for each player's characters.
    private int player1DeathCount = 0;
    private int player2DeathCount = 0;

    // Reference to the Game Over panel (assign via the Inspector)
    public GameObject gameOverPanel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optionally persist this manager across scenes.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Registers the death of a character. Only when 5 characters of a single player die will the game end.
    /// </summary>
    /// <param name="deadCharacterName">Name of the character that died.</param>
    public void RegisterDeath(string deadCharacterName)
    {
        // Clean the name (remove any "(Clone)" suffix)
        string cleanedName = deadCharacterName.Replace("(Clone)", "").Trim();

        // Check which player's card list contains this character.
        if (GameManager.Instance.player1Cards.Contains(cleanedName))
        {
            player1DeathCount++;
            Debug.Log("Player1 death count: " + player1DeathCount);
            if (player1DeathCount >= 5)
            {
                // Trigger game over via RPC to all clients.
                photonView.RPC("RPC_EndGame", RpcTarget.All);
            }
        }
        else if (GameManager.Instance.player2Cards.Contains(cleanedName))
        {
            player2DeathCount++;
            Debug.Log("Player2 death count: " + player2DeathCount);
            if (player2DeathCount >= 5)
            {
                // Trigger game over via RPC to all clients.
                photonView.RPC("RPC_EndGame", RpcTarget.All);
            }
        }
        else
        {
            Debug.Log("Character " + cleanedName + " not found in any player's card list.");
        }
    }

    [PunRPC]
    void RPC_EndGame()
    {
        // Show the Game Over panel on this client.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Called from the Game Over UI button to return to the lobby scene.
    /// </summary>
    public void OnReturnToLobbyButtonClicked()
    {
        PhotonNetwork.LoadLevel("Lobby");
    }
}
