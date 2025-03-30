using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI Components")]
    public TextMeshProUGUI tutorialText; // Text component for displaying tutorial messages
    public GameObject tutorialPanel; // Panel to show or hide tutorial messages

    private void Awake()
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

    private void Start()
    {
        // Initially hide the tutorial panel
        HideTutorial();
    }

    // Add this method to find and bind UI components in the current scene
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find UI components in the new scene
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            // Try to find tutorial text and panel in each canvas
            TextMeshProUGUI[] texts = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.gameObject.name.Contains("Tips"))
                {
                    tutorialText = text;
                    break;
                }
            }

            Image[] images = canvas.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                if (img.gameObject.name.Contains("TutorialPanel"))
                {
                    tutorialPanel = img.gameObject;
                    break;
                }
            }
        }

        // Log warning if components are not found
        if (tutorialText == null)
            Debug.LogWarning("TutorialManager: Could not find TutorialText in scene " + scene.name);
        if (tutorialPanel == null)
            Debug.LogWarning("TutorialManager: Could not find TutorialPanel in scene " + scene.name);
    }

    public void ShowTemporaryMessage(string message, float duration)
    {
        if (tutorialText == null || tutorialPanel == null)
        {
            Debug.LogError("TutorialManager: UI components not found!");
            return;
        }
        StartCoroutine(ShowMessageRoutine(message, duration));
    }

    private IEnumerator ShowMessageRoutine(string message, float duration)
    {
        // Coroutine to display a message and then fade it out
        tutorialText.text = message;
        tutorialPanel.SetActive(true);
        yield return StartCoroutine(FadeOutRoutine(duration));
        tutorialPanel.SetActive(false);
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        // Coroutine to fade out the tutorial panel over a specified duration
        CanvasGroup canvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
        }

        float startAlpha = canvasGroup.alpha;
        float rate = 1.0f / duration;
        float progress = 0.0f;

        while (progress < 1.0f)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, progress);
            progress += rate * Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0;
    }

    public void ShowPersistentMessage(string message)
    {
        if (tutorialText == null || tutorialPanel == null)
        {
            Debug.LogError("TutorialManager: UI components not found!");
            return;
        }
        tutorialText.text = message;
        tutorialPanel.SetActive(true);
    }

    public void HideTutorial()
    {
        // Hide the tutorial panel
        tutorialPanel.SetActive(false);
    }
}