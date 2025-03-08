using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public Text infoText;

    // Start is called before the first frame update
    void Start()
    {
        // Set your game version
        PhotonNetwork.GameVersion = "v01";
        // Connect using the settings (no parameter needed)
        PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
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



    //ince we're inheriting from MonoBehaviourPunCallbacks, you should override the callback methods using the override keyword.
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
