using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;

public class DeathManager : MonoBehaviourPun
{
    public static DeathManager Instance;

    private int player1DeathCount = 0;
    private int player2DeathCount = 0;

    public GameObject gameOverPanel;
    public TMP_Text winnerText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // void Start()
    // {
    //     // Display game instructions when the game starts
    //     if (TutorialManager.Instance != null)
    //     {
    //         TutorialManager.Instance.ShowTemporaryMessage("Game started! Defeat all opponent's characters to win.", 3f);
    //     }
    // }

    /*
     public void RegisterDeath(string characterName)
    {
        // Simple death registration without Photon or card checks
        if (characterName.Contains("Player1"))
        {
            player1Deaths++;
        }
        else if (characterName.Contains("Player2"))
        {
            player2Deaths++;
        }
        
        UpdateDeathText();
        
        // Basic game over check
        if (player1Deaths >= 5 || player2Deaths >= 5)
        {
            EndGame();
        }
    }
    */

    /// <param name="deadCharacterName">Name of the character that died.</param>
    public void RegisterDeath(string deadCharacterName)
    {
        string cleanedName = deadCharacterName.Replace("(Clone)", "").Trim();

        if (GameManager.Instance.player1Cards.Contains(cleanedName))
        {
            player1DeathCount++;
            Debug.Log("Player1 death count: " + player1DeathCount);
            
            // Show death count notification
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.ShowTemporaryMessage("Player 1 lost a character! (" + player1DeathCount + "/5)", 2f);
            }
            
            if (player1DeathCount >= 5)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    string winningPlayerName = "";
                    foreach (var player in PhotonNetwork.PlayerList)
                    {
                        if (!player.IsMasterClient)
                        {
                            winningPlayerName = player.NickName;
                            break;
                        }
                    }
                    photonView.RPC("RPC_EndGame", RpcTarget.All, winningPlayerName);
                }
            }
        }
        else if (GameManager.Instance.player2Cards.Contains(cleanedName))
        {
            player2DeathCount++;
            Debug.Log("Player2 death count: " + player2DeathCount);
            
            // Show death count notification
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.ShowTemporaryMessage("Player 2 lost a character! (" + player2DeathCount + "/5)", 2f);
            }
            
            if (player2DeathCount >= 5)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    string winningPlayerName = PhotonNetwork.MasterClient.NickName;
                    photonView.RPC("RPC_EndGame", RpcTarget.All, winningPlayerName);
                }
            }
        }
        else
        {
            Debug.Log("Character " + cleanedName + " not found in any player's card list.");
        }
    }

    [PunRPC]
    void RPC_EndGame(string winningPlayerName)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (winnerText != null)
        {
            winnerText.text = winningPlayerName;
        }
    }

    public void OnReturnToLobbyButtonClicked()
    {
        // Show returning message
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowTemporaryMessage("Returning to lobby...", 1f);
        }
        
        PhotonNetwork.LoadLevel("Lobby");
    }
}
