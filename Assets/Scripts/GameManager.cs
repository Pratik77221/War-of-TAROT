using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Lists to store the card names selected by each player.
    public List<string> player1Cards = new List<string>();
    public List<string> player2Cards = new List<string>();

    void Awake()
    {
        // Ensure this GameManager persists across scene loads.
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
}
