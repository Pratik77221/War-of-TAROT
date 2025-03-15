using System.Collections.Generic;
using UnityEngine;
using Vuforia;  // Import Vuforia namespace
using TMPro;    // Import TextMeshPro namespace

public class CardSequence : MonoBehaviour
{
    public TextMeshProUGUI sequenceText; // Reference to the TextMeshPro UI element that will display the card names
    private List<ObserverBehaviour> detectedCards = new List<ObserverBehaviour>(); // List of detected cards

    private void OnEnable()
    {
        // Subscribe to Vuforia Observer status change events
        foreach (var observer in FindObjectsOfType<ObserverBehaviour>())
        {
            if (observer != null)
            {
                observer.OnTargetStatusChanged += OnTargetStatusChanged;
            }
        }
    }

    // This is called when an image target's status changes (detected or lost)
    private void OnTargetStatusChanged(ObserverBehaviour observer, TargetStatus status)
    {
        if (status.Status == Status.TRACKED)
        {
            // Add the detected card to the list if it's not already in the list
            if (!detectedCards.Contains(observer))
            {
                detectedCards.Add(observer);
            }
        }
        else
        {
            // Remove the card from the list if it's no longer detected
            if (detectedCards.Contains(observer))
            {
                detectedCards.Remove(observer);
            }
        }

        // Sort the detected cards based on their X position (left to right)
        detectedCards.Sort((card1, card2) => card1.transform.position.x.CompareTo(card2.transform.position.x));

        // Prepare the sequence string
        string cardSequence = "Detected Cards: \n";
        foreach (var card in detectedCards)
        {
            cardSequence += card.name + "\n"; // Add the name of the card to the sequence
        }

        // Update the TextMeshPro text with the sequence
        sequenceText.text = cardSequence;
    }

    // Unsubscribe from events when the script is disabled or destroyed
    private void OnDisable()
    {
        // Unsubscribe from the OnTargetStatusChanged event
        foreach (var observer in FindObjectsOfType<ObserverBehaviour>())
        {
            if (observer != null)
            {
                observer.OnTargetStatusChanged -= OnTargetStatusChanged;
            }
        }
    }
}
