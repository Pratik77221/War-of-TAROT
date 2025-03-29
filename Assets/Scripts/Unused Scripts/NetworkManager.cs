using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Not used/ Test Script
/// </summary>

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public Text infoText;

 
    void Start()
    {
      
        PhotonNetwork.GameVersion = "v01";
     
        PhotonNetwork.ConnectUsingSettings();
    }

  
    void Update()
    {
        if (!PhotonNetwork.InRoom)
        {
            infoText.text = PhotonNetwork.NetworkClientState.ToString();
        }
        else
        {
            infoText.text = "Connected to " + PhotonNetwork.CurrentRoom.Name + " and " + PhotonNetwork.CurrentRoom.PlayerCount + " Players Online";
        }
    }



    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {

        RoomOptions myRoomOptions = new RoomOptions();
        myRoomOptions.MaxPlayers = 2;

        PhotonNetwork.JoinOrCreateRoom("Room1", myRoomOptions, TypedLobby.Default);
        Debug.Log("Connected to lobby");
    }
}
