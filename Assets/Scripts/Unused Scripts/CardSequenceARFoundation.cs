using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class CardSequenceAR : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager; // Reference to ARTrackedImageManager
    public TextMeshProUGUI sequenceText; // Reference to TextMeshPro UI element for displaying sequence

    private List<ARTrackedImage> detectedCards = new List<ARTrackedImage>(); // List of detected images

    void OnEnable()
    {
        // Subscribe to the tracked image event
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        // Unsubscribe from the event when the script is disabled
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    // This method is called when the tracked images change (detected, updated, or removed)
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Process newly detected images
        foreach (var addedImage in eventArgs.added)
        {
            // Add the newly detected image to the list
            detectedCards.Add(addedImage);
        }

        // Process updated images (in case they are still being tracked)
        foreach (var updatedImage in eventArgs.updated)
        {
            if (!detectedCards.Contains(updatedImage))
            {
                detectedCards.Add(updatedImage);
            }
        }

        // Process removed images
        foreach (var removedImage in eventArgs.removed)
        {
            detectedCards.Remove(removedImage);
        }

        // Sort the detected cards based on their X and Z position in world space (detect their real-world sequence)
        detectedCards.Sort((card1, card2) =>
        {
            // We compare both the X and Z coordinates to determine the real-world sequence
            if (card1.transform.position.x != card2.transform.position.x)
            {
                return card1.transform.position.x.CompareTo(card2.transform.position.x); // Left to right based on X
            }
            else
            {
                return card1.transform.position.z.CompareTo(card2.transform.position.z); // Depth comparison if X is same
            }
        });

        // Prepare the sequence string to display the names of the detected cards
        string cardSequence = "Detected Cards: \n";
        foreach (var card in detectedCards)
        {
            cardSequence += card.referenceImage.name + "\n"; // Add the name of the card to the sequence
        }

        // Update the TextMeshPro UI element with the new sequence
        sequenceText.text = cardSequence;
    }
}
