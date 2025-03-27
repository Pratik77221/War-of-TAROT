using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Store the selected charaacters in runtime and across scene
/// </summary>
/// 
public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance;

    public List<string> player1Cards = new List<string>();
    public List<string> player2Cards = new List<string>();

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

    [PunRPC]
    public void RPC_SetPlayer2Cards(string[] cards)
    {
        Debug.Log("[MasterClient] RPC_SetPlayer2Cards called. Updating player2Cards in GameManager.");
        player2Cards.Clear();
        player2Cards.AddRange(cards);
    }

    // (Optional) Similar RPC for Player1 if needed
    [PunRPC]
    public void RPC_SetPlayer1Cards(string[] cards)
    {
        Debug.Log("[MasterClient] RPC_SetPlayer1Cards called. Updating player1Cards in GameManager.");
        player1Cards.Clear();
        player1Cards.AddRange(cards);
    }
}