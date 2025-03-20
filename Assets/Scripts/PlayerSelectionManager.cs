using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using Photon.Pun;  // Photon namespace for networking
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSelectionManager : MonoBehaviourPunCallbacks
{
    public ARTrackedImageManager trackedImageManager; // AR tracked images
    public TextMeshProUGUI sequenceText; // Single text box for both players
    public Button startGameButton; // Start game button

    // Define player identifiers
    private const string PLAYER1 = "Player1";
    private const string PLAYER2 = "Player2";
    private string currentPlayer;

    // A simple class to store scanned card info (name and position for sorting)
    [System.Serializable]
    public class ScannedCard
    {
        public string cardName;
        public Vector2 position; // We'll use x and z (stored as x,y)

        public ScannedCard(string cardName, Vector2 pos)
        {
            this.cardName = cardName;
            position = pos;
        }
    }

    // Lists that will be updated over the network via RPC
    private List<ScannedCard> scannedCardsPlayer1 = new List<ScannedCard>();
    private List<ScannedCard> scannedCardsPlayer2 = new List<ScannedCard>();

    void Start()
    {

        PhotonNetwork.AutomaticallySyncScene = true;

        // Determine player based on Photon master client status.
        currentPlayer = PhotonNetwork.IsMasterClient ? PLAYER1 : PLAYER2;

        // Initially, disable the start button.
        startGameButton.gameObject.SetActive(false);

        // Subscribe to tracked image events.
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    // Handle AR tracked images changes.
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // For added images, send an RPC to all clients.
        foreach (var addedImage in eventArgs.added)
        {
            photonView.RPC("RPC_AddCard", RpcTarget.All,
                addedImage.referenceImage.name,
                currentPlayer,
                addedImage.transform.position.x,
                addedImage.transform.position.z);
        }

        // For updated images, check if the card is not already registered and add if needed.
        foreach (var updatedImage in eventArgs.updated)
        {
            if (currentPlayer == PLAYER1)
            {
                bool exists = false;
                foreach (var card in scannedCardsPlayer1)
                {
                    if (card.cardName == updatedImage.referenceImage.name)
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    photonView.RPC("RPC_AddCard", RpcTarget.All,
                        updatedImage.referenceImage.name,
                        currentPlayer,
                        updatedImage.transform.position.x,
                        updatedImage.transform.position.z);
                }
            }
            else // For PLAYER2
            {
                bool exists = false;
                foreach (var card in scannedCardsPlayer2)
                {
                    if (card.cardName == updatedImage.referenceImage.name)
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    photonView.RPC("RPC_AddCard", RpcTarget.All,
                        updatedImage.referenceImage.name,
                        currentPlayer,
                        updatedImage.transform.position.x,
                        updatedImage.transform.position.z);
                }
            }
        }

        // For removed images, send an RPC to remove the card.
        foreach (var removedImage in eventArgs.removed)
        {
            photonView.RPC("RPC_RemoveCard", RpcTarget.All,
                removedImage.referenceImage.name,
                currentPlayer);
        }
    }

    // RPC to add a scanned card into the correct player's list.
    [PunRPC]
    void RPC_AddCard(string cardName, string player, float posX, float posZ)
    {
        Vector2 pos = new Vector2(posX, posZ);
        if (player == PLAYER1)
        {
            Debug.Log($"RPC_AddCard called for {cardName} from {player}");
            bool exists = false;
            foreach (var card in scannedCardsPlayer1)
            {
                if (card.cardName == cardName)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                scannedCardsPlayer1.Add(new ScannedCard(cardName, pos));
            }
        }
        else if (player == PLAYER2)
        {
            bool exists = false;
            foreach (var card in scannedCardsPlayer2)
            {
                if (card.cardName == cardName)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                scannedCardsPlayer2.Add(new ScannedCard(cardName, pos));
            }
        }
        UpdateSequenceText();
    }

    // RPC to remove a card from a player's list.
    [PunRPC]
    void RPC_RemoveCard(string cardName, string player)
    {
        if (player == PLAYER1)
        {
            scannedCardsPlayer1.RemoveAll(card => card.cardName == cardName);
        }
        else if (player == PLAYER2)
        {
            scannedCardsPlayer2.RemoveAll(card => card.cardName == cardName);
        }
        UpdateSequenceText();
    }

    // Sort cards based on world position (using x and z coordinates).
    void SortScannedCards(List<ScannedCard> cards)
    {
        cards.Sort((a, b) =>
        {
            if (a.position.x != b.position.x)
                return a.position.x.CompareTo(b.position.x);
            return a.position.y.CompareTo(b.position.y);
        });
    }

    // Combine both players' card lists into one string and update the shared text box.
    void UpdateSequenceText()
    {
        SortScannedCards(scannedCardsPlayer1);
        SortScannedCards(scannedCardsPlayer2);

        string text = "Player 1 Cards:\n";
        foreach (var card in scannedCardsPlayer1)
        {
            text += card.cardName + "\n";
        }
        text += "\nPlayer 2 Cards:\n";
        foreach (var card in scannedCardsPlayer2)
        {
            text += card.cardName + "\n";
        }
        sequenceText.text = text;

        // Enable the start button only if both players have scanned at least 5 cards.
        startGameButton.gameObject.SetActive(
            scannedCardsPlayer1.Count >= 5 && scannedCardsPlayer2.Count >= 5);

        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.player1Cards.Clear();
            foreach (var card in scannedCardsPlayer1)
            {
                GameManager.Instance.player1Cards.Add(card.cardName);
            }
        }
        else
        {
            GameManager.Instance.player2Cards.Clear();
            foreach (var card in scannedCardsPlayer2)
            {
                GameManager.Instance.player2Cards.Add(card.cardName);
            }
        }

        Debug.Log("Player 1 card count: " + scannedCardsPlayer1.Count);
        Debug.Log("Player 2 card count: " + scannedCardsPlayer2.Count);
    }

    // Only the master client (Player 1) can start the game.
    public void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {

            PhotonNetwork.LoadLevel("ARGame");
        }
        else
        {
            Debug.Log("Only the Master Client can start the game!");
        }
    }
}

