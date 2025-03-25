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


    public Button startGameButton;


    private const byte maxPlayers = 2;

    void Start()
    {

        PhotonNetwork.AutomaticallySyncScene = true;


        loginPanel.SetActive(true);
        roomPanel.SetActive(false);


        if (startGameButton != null)
        {
            startGameButton.interactable = false;
        }
    }


    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInputField.text;
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.Log("Player name is invalid or empty");
            return;
        }


        PhotonNetwork.NickName = playerName;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connecting to Photon...");
    }


    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");


        PhotonNetwork.JoinRandomRoom();
    }

    /*public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Server");
        PhotonNetwork.JoinLobby();
    } */


    /*
   
   private void InitializeLegacyPhoton()
   {
       Debug.Log("Initializing legacy Photon connection (this is outdated and no longer used).");
       PhotonNetwork.Connect(); // Deprecated method call.
   }
   */



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


        loginPanel.SetActive(false);
        roomPanel.SetActive(true);


        UpdatePlayerList();


        if (startGameButton != null)
        {

            startGameButton.interactable = PhotonNetwork.IsMasterClient;
        }
    }

    /* public override void OnJoinedLobby()
    {

        RoomOptions myRoomOptions = new RoomOptions();
        myRoomOptions.MaxPlayers = 2;

        PhotonNetwork.JoinOrCreateRoom("Room1", myRoomOptions, TypedLobby.Default);
        Debug.Log("Connected to lobby");
    } */


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player joined: " + newPlayer.NickName);
        UpdatePlayerList();
    }


    /*public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left: " + otherPlayer.NickName);
        UpdatePlayerList();


        if (startGameButton != null)
        {
            startGameButton.interactable = PhotonNetwork.IsMasterClient;
        }
    }*/


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



    public void OnStartGameButtonClicked()
    {

        if (PhotonNetwork.IsMasterClient)
        {

            PhotonNetwork.LoadLevel("PlayerSelection");
        }
        else
        {
            Debug.Log("Only the Master Client can start the game!");
        }
    }
}
