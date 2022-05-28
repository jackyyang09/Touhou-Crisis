using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;

public class AttractModeUI : BaseGameUI
{
    [SerializeField] float textScrollTime;
    [SerializeField] float textScrollSpeed;
    [SerializeField] float fadeOutBuffer;

    [SerializeField] VideoClip[] clips;
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] TMPro.TextMeshProUGUI[] scrollingText;
    [SerializeField] IntroEffect introEffect;

    IEnumerator AttractRoutine()
    {
        videoPlayer.Play();

        int i = 0;
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

        if (videoPlayer.time >= videoPlayer.length - fadeOutBuffer)
        {
            optimizedCanvas.Hide();
            introEffect.OnAttractionEnded();
        }

        videoPlayer.playbackSpeed = Time.timeScale;
    }

    public override void ShowUI()
    {
        videoPlayer.clip = clips[0];
        videoPlayer.time = 0;
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