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
    public Text player1Text;
    public Text player2Text;

    public Button startGameButton;

    
    public GameObject waitingImage;

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
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.ShowPersistentMessage("<color=red>Username cannot be empty!</color>");
            }
            return;
        }

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.HideTutorial();
        }

        PhotonNetwork.NickName = playerName;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connecting to Photon...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowTemporaryMessage("<color=green>Connected to server successfully!</color>", 2f);
        }

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

        // Show waiting image if Master Client is waiting for another player.
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length < maxPlayers)
        {
            if (waitingImage != null)
                waitingImage.SetActive(true);
        }
        else
        {
            if (waitingImage != null)
                waitingImage.SetActive(false);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player joined: " + newPlayer.NickName);
        UpdatePlayerList();

        // Hide waiting image once the room has the maximum players.
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.PlayerList.Length >= maxPlayers)
            {
                if (waitingImage != null)
                    waitingImage.SetActive(false);

                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.ShowTemporaryMessage("Room is full! Click Start to begin the game.", 2f);
                }
            }
        }

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ShowTemporaryMessage($"<color=green>{newPlayer.NickName} has joined the room!</color>", 2f);
        }
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
        // Reset texts
        if (player1Text != null)
            player1Text.text = "";
        if (player2Text != null)
            player2Text.text = "";

        // Assign names based on ownership:
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.IsMasterClient)
            {
                if (player1Text != null)
                    player1Text.text = player.NickName;
            }
            else
            {
                if (player2Text != null)
                    player2Text.text = player.NickName;
            }
        }

        Debug.Log("Player list updated: Player1 = " + player1Text.text + " | Player2 = " + player2Text.text);
    }

    public void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.ShowTemporaryMessage("Starting game...", 1f);
            }
            PhotonNetwork.LoadLevel("PlayerSelection");
        }
        else
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.ShowTemporaryMessage("<color=red>Only the host can start the game!</color>", 2f);
            }
        }
    }
}
