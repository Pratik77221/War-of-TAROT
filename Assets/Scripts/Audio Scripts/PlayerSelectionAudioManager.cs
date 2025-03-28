using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerSelectionAudioManager : MonoBehaviour
{
    // Start is called before the first frame update
    
    private AudioSource audioSource; // Now using the existing AudioSource
    public float fadeDuration = 2f;  // Duration of fade-out

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>(); // Get the existing AudioSource

        if (audioSource == null)
        {
            Debug.LogError("AudioSource not found on " + gameObject.name);
            return;
        }

        DontDestroyOnLoad(gameObject); // Keeps the music playing across scenes
        SceneManager.sceneUnloaded += OnSceneUnloaded; // Listen for scene changes
    }

    private void OnSceneUnloaded(Scene current)
    {
        StartCoroutine(FadeOutAudio(fadeDuration));
    }

    private IEnumerator FadeOutAudio(float duration)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // Reset for future scenes
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded; // Clean up event subscription
    }

}
