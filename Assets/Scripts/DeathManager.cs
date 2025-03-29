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
        PhotonNetwork.LoadLevel("Lobby");
    }
}
