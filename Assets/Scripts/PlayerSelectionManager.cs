using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSelectionManager : MonoBehaviourPunCallbacks
{
    public ARTrackedImageManager trackedImageManager;
    public TextMeshProUGUI player1SequenceText;
    public TextMeshProUGUI player2SequenceText;
    public Button startGameButton;

    private const string PLAYER1 = "Player1";
    private const string PLAYER2 = "Player2";
    private string currentPlayer;

    [System.Serializable]
    public class ScannedCard
    {
        public string cardName;
        public Vector2 position;

        public ScannedCard(string cardName, Vector2 pos)
        {
            this.cardName = cardName;
            position = pos;
        }
    }

    private List<ScannedCard> scannedCardsPlayer1 = new List<ScannedCard>();
    private List<ScannedCard> scannedCardsPlayer2 = new List<ScannedCard>();

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        currentPlayer = PhotonNetwork.IsMasterClient ? PLAYER1 : PLAYER2;
        startGameButton.gameObject.SetActive(false);
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // For added images
        foreach (var addedImage in eventArgs.added)
        {
            photonView.RPC("RPC_AddCard", RpcTarget.All,
                addedImage.referenceImage.name,
                currentPlayer,
                addedImage.transform.position.x,
                addedImage.transform.position.z);
        }

        // For updated images
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
            else // PLAYER2
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

       /* // For removed images
        foreach (var removedImage in eventArgs.removed)
        {
            photonView.RPC("RPC_RemoveCard", RpcTarget.All,
                removedImage.referenceImage.name,
                currentPlayer);
        }*/
    }

    [PunRPC]
    void RPC_AddCard(string cardName, string player, float posX, float posZ)
    {
        Vector2 pos = new Vector2(posX, posZ);
        if (player == PLAYER1)
        {
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

    [PunRPC]
   /* void RPC_RemoveCard(string cardName, string player)
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
    }*/

    void SortScannedCards(List<ScannedCard> cards)
    {
        cards.Sort((a, b) =>
        {
            if (a.position.x != b.position.x)
                return a.position.x.CompareTo(b.position.x);
            return a.position.y.CompareTo(b.position.y);
        });
    }

    void UpdateSequenceText()
    {
        // Sort cards before updating text
        SortScannedCards(scannedCardsPlayer1);
        SortScannedCards(scannedCardsPlayer2);

        string textPlayer1 = "Player 1 Cards:\n";
        foreach (var card in scannedCardsPlayer1)
        {
            textPlayer1 += card.cardName + "\n";
        }
        player1SequenceText.text = textPlayer1;

        string textPlayer2 = "Player 2 Cards:\n";
        foreach (var card in scannedCardsPlayer2)
        {
            textPlayer2 += card.cardName + "\n";
        }
        player2SequenceText.text = textPlayer2;

        // Enable the start button only if both players have scanned at least 5 cards.
        startGameButton.gameObject.SetActive(
            scannedCardsPlayer1.Count >= 5 && scannedCardsPlayer2.Count >= 5);

        // Update local GameManager lists
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
            // Send Player2's card list to the Master Client via RPC.
            photonView.RPC("RPC_SyncPlayer2Cards", RpcTarget.MasterClient,
                GameManager.Instance.player2Cards.ToArray());
        }
    }

    [PunRPC]
    void RPC_SyncPlayer2Cards(string[] cardsFromP2)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.player2Cards.Clear();
            GameManager.Instance.player2Cards.AddRange(cardsFromP2);
        }
    }

    // Only the Master Client (Player1) can start the game.
    public void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("ARGame");
        }
    }
}
