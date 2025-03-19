using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ARGameManager : MonoBehaviourPunCallbacks
{
    // Reference to all 10 card buttons (set these in the Inspector).
    public Button[] cardButtons;

    void Start()
    {
        // Get the current player's card selection list from the GameManager.
        List<string> myCards = PhotonNetwork.IsMasterClient ?
            GameManager.Instance.player1Cards :
            GameManager.Instance.player2Cards;

        Debug.Log("My selected cards: " + string.Join(", ", myCards.ToArray()));

        // Loop through all card buttons and only activate those that match a selected card.
        foreach (Button btn in cardButtons)
        {
            // Assume each button has a CardButton component.
            CardNameButton cardBtn = btn.GetComponent<CardNameButton>();

            if (cardBtn != null)
            {
                if (myCards.Contains(cardBtn.cardName))
                {
                    // Show the button if its card is in the player's selection.
                    btn.gameObject.SetActive(true);
                }
                else
                {
                    // Hide it if it isn’t selected.
                    btn.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("Button " + btn.name + " is missing a CardButton component.");
                btn.gameObject.SetActive(false);
            }
        }
    }
}
