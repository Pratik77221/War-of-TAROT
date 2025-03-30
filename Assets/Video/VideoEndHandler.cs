using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoEndHandler : MonoBehaviour
{
    public RawImage rawImage;
    public VideoPlayer videoPlayer;
    public Button skipButton;

    void Start()
    {
        rawImage.gameObject.SetActive(true);
        videoPlayer.loopPointReached += CheckVideoEnd;
        skipButton.onClick.AddListener(SkipVideo);
    }

    void CheckVideoEnd(VideoPlayer vp)
    {
        rawImage.gameObject.SetActive(false);
    }

    void SkipVideo()
    {
        videoPlayer.Stop();
        rawImage.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        videoPlayer.loopPointReached -= CheckVideoEnd;
        skipButton.onClick.RemoveListener(SkipVideo);
    }
}
