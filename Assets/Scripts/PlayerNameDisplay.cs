using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerNameDisplay : MonoBehaviourPunCallbacks // Changed base class
{
    [Header("Text References")]
    public TextMeshPro player1Text;
    public TextMeshPro player2Text;

    void Start()
    {
        UpdatePlayerNames();
    }

    void UpdatePlayerNames()
    {
        // Get all players from Photon network
        Player[] players = PhotonNetwork.PlayerList;

        // Reset both texts first
        player1Text.text = "Waiting for Player 1...";
        player2Text.text = "Waiting for Player 2...";

        // Update names from LobbyManager's Photon network data
        foreach (Player player in players)
        {
            if (player.IsMasterClient)
            {
                player1Text.text = player.NickName;
            }
            else
            {
                player2Text.text = player.NickName;
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        UpdatePlayerNames();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        UpdatePlayerNames();
    }
}