using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Login UI")]
    public InputField playerNameInputField;
    public GameObject loginPanel;

    [Header("Room UI")]
    public GameObject roomPanel;
    public Text playerListText;

    // Add a reference to the Start button (optional, only if you need to enable/disable it)
    public Button startGameButton;

    // Maximum number of players per room (set to 2 for your requirement)
    private const byte maxPlayers = 2;

    void Start()
    {
        // Enable Photon to sync scene loading
        PhotonNetwork.AutomaticallySyncScene = true;

        // Initially, show the login panel and hide the room panel
        loginPanel.SetActive(true);
        roomPanel.SetActive(false);

        // Optionally, disable the start button until 2 players have joined
        if (startGameButton != null)
        {
            startGameButton.interactable = false;
        }
    }

    // Called when the login button is clicked
    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInputField.text;
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.Log("Player name is invalid or empty");
            return;
        }

        // Set the player's nickname and connect to Photon
        PhotonNetwork.NickName = playerName;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connecting to Photon...");
    }

    // Called when successfully connected to the Photon Master Server
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");

        // Try to join any random room that is not full
        PhotonNetwork.JoinRandomRoom();
    }

    // Called when joining a random room fails (e.g., no available room exists)
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Join random room failed: " + message + ". Creating a new room.");

        string roomName = "Room" + Random.Range(1000, 9999);
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = maxPlayers };
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    // Called when the client has joined a room
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Player count (OnJoinedRoom): " + PhotonNetwork.PlayerList.Length);

        // Hide the login panel and show the room panel
        loginPanel.SetActive(false);
        roomPanel.SetActive(true);

        // Update the player list UI
        UpdatePlayerList();

        // If the Start button is assigned, decide if it should be enabled
        if (startGameButton != null)
        {
            // Enable it only if this client is the MasterClient (or if you want both players to be able to start)
            startGameButton.interactable = PhotonNetwork.IsMasterClient;
        }
    }

    // Called when a new player enters the room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player joined: " + newPlayer.NickName);
        UpdatePlayerList();
    }

    // Called when a player leaves the room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left: " + otherPlayer.NickName);
        UpdatePlayerList();

        // If you only want the Start button for the MasterClient
        if (startGameButton != null)
        {
            startGameButton.interactable = PhotonNetwork.IsMasterClient;
        }
    }

    // Updates the player list text UI to display all current players in the room
    private void UpdatePlayerList()
    {
        if (playerListText == null)
        {
            Debug.LogError("playerListText is not assigned in the Inspector!");
            return;
        }

        string players = "";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            players += player.NickName + "\n";
        }
        playerListText.text = players;
        Debug.Log("Player list updated: \n" + players);
    }

    // Called when the "Leave Lobby" button is clicked
    public void OnLeaveLobbyButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    // Called when the local player has left the room
    public override void OnLeftRoom()
    {
        Debug.Log("Left room, returning to login page");

        // Show the login panel and hide the room panel
        loginPanel.SetActive(true);
        roomPanel.SetActive(false);
    }

    // Called when the "Start Game" button is clicked
    public void OnStartGameButtonClicked()
    {
        // Only allow the MasterClient to load the AR scene (or remove this check if you want anyone to start)
        if (PhotonNetwork.IsMasterClient)
        {
            // Make sure "ARScene" is in your Build Settings
            PhotonNetwork.LoadLevel("Vuforia");
        }
        else
        {
            Debug.Log("Only the Master Client can start the game!");
        }
    }
}
