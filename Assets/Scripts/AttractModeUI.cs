using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;

public class AttractModeUI : BaseGameUI
{
    [SerializeField] string videoSubDirectory;

    [SerializeField] float textScrollTime;
    [SerializeField] float textScrollSpeed;
    [SerializeField] float fadeOutBuffer;

    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] TMPro.TextMeshProUGUI[] scrollingText;
    [SerializeField] IntroEffect introEffect;
    [SerializeField] UnityEngine.UI.RawImage videoFeed;

    readonly List<string> SUPPORTED_FORMATS = new List<string> 
    { ".asf", ".avi", ".dv", ".m4v", ".mov", ".mp4", ".mpg", ".mpeg", ".ogv", ".vp8", ".webm", ".wmv" };

#if !UNITY_STANDALONE
    private void Awake()
    {
        Destroy(gameObject);
    }
#endif

    IEnumerator AttractRoutine()
    {
        var path = Path.Combine(Application.dataPath, videoSubDirectory);
        var directory = new DirectoryInfo(path);
        var videos = new List<FileInfo>(directory.GetFiles());

        int i = videos.Count - 1;
        for (; i > -1; i--)
        {
            if (!SUPPORTED_FORMATS.Contains(videos[i].Extension)) videos.RemoveAt(i);
        }

        string previousFile = videoPlayer.url;
        int attempts = 0;
        do
        {
            videoPlayer.url = videos[Random.Range(0, videos.Count)].FullName;
            attempts++;
        } while (previousFile == videoPlayer.url && attempts < videos.Count);

        videoPlayer.time = 0;
        videoPlayer.Play();

        i = 0;
        while (optimizedCanvas.IsVisible)
        {
            scrollingText[i].rectTransform.anchoredPosition = new Vector2(2000, 0);
            yield return new WaitForSeconds(textScrollTime);
            i = (int)Mathf.Repeat(i + 1, scrollingText.Length);
        }
    }

    private void Update()
    {
        for (int i = 0; i < scrollingText.Length; i++)
        {
            scrollingText[i].rectTransform.anchoredPosition -= Vector2.right * textScrollSpeed * Time.deltaTime;
        }

        if (videoPlayer.isPrepared)
        {
            if (videoPlayer.time >= videoPlayer.length - fadeOutBuffer)
            {
                optimizedCanvas.Hide();
                introEffect.OnAttractionEnded();
            }
        }

        videoPlayer.playbackSpeed = Time.timeScale;
    }

    public override void ShowUI()
    {
        for (int i = 0; i < scrollingText.Length; i++)
        {
            scrollingText[i].rectTransform.anchoredPosition = new Vector2(-2000, 0);
        }
        videoFeed.color = Color.clear;
        videoFeed.DOColor(Color.white, 0.5f);
        enabled = true;
        StartCoroutine(AttractRoutine());
    }

    public override void HideUI()
    {
        videoPlayer.time = 0;
        videoPlayer.Stop();
        enabled = false;
        StopAllCoroutines();
    }
}