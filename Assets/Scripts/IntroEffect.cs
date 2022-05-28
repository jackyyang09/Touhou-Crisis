using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using JSAM;
using Photon.Pun;

public class IntroEffect : MonoBehaviour
{
    [SerializeField] RailShooterLogic railShooter;

    [SerializeField] UnityEngine.UI.Image image;

    [SerializeField] float timeToAttract = 5;

    [SerializeField] float holdBlackTime = 1;
    [SerializeField] float fadeToWhiteTime = 0.5f;
    [SerializeField] float holdWhiteTime = 2;
    [SerializeField] float fadeToClearTime;

    [SerializeField] OptimizedCanvas titleCanvas = null;
    [SerializeField] OptimizedCanvas menuCanvas = null;

    [SerializeField] AttractModeUI attractUI;

    [SerializeField] TMPro.TextMeshProUGUI shootToStart = null;
    [SerializeField] float textFlashTime = 1;

    Coroutine fadeRoutine;

    bool nowAttracting = false;
    bool openingSkipped = false;

    private IEnumerator Start()
    {
        fadeRoutine = StartCoroutine(FadeIn(null));

        yield return new WaitForSeconds(holdBlackTime);

        AudioManager.PlaySound(MainMenuSounds.TitleVO);

        yield return new WaitForSeconds(fadeToClearTime);

        if (!openingSkipped)
        {
            UnsubscribeSkipOpening();
            Invoke(nameof(StartAttracting), timeToAttract);
            StartCoroutine(FlashShootToStart());
        }
    }

    IEnumerator FlashShootToStart()
    {
        while (titleCanvas.IsVisible)
        {
            shootToStart.enabled = !shootToStart.enabled;
            yield return new WaitForSeconds(textFlashTime);
        }
    }

    private void OnEnable()
    {
        railShooter.OnShoot += SkipOpening;
    }

    private void OnDisable()
    {
        railShooter.OnShoot -= SkipOpening;
    }

    void UnsubscribeSkipOpening()
    {
        openingSkipped = true;
        railShooter.OnShoot -= SkipOpening;
        railShooter.OnShoot += ShootToStart;
    }

    private void SkipOpening(Ray obj, Vector2 pos)
    {
        if (IsInvoking(nameof(UnsubscribeSkipOpening))) CancelInvoke(nameof(UnsubscribeSkipOpening));
        if (IsInvoking(nameof(StartAttracting))) CancelInvoke(nameof(StartAttracting));
        Invoke(nameof(StartAttracting), timeToAttract);

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        image.DOComplete();
        image.color = Color.clear;

        if (!openingSkipped) UnsubscribeSkipOpening();
        StartCoroutine(FlashShootToStart());
    }

    void ShootToStart(Ray obj, Vector2 pos)
    {
        if (attractUI) attractUI.OptimizedCanvas.Hide();
        if (nowAttracting)
        {
            OnAttractionEnded();
        }
        else
        {
            if (IsInvoking(nameof(StartAttracting))) CancelInvoke(nameof(StartAttracting));
            StopAllCoroutines();
            image.DOKill();
            image.color = Color.clear;
            railShooter.OnShoot -= ShootToStart;
            AudioManager.PlaySound(MainMenuSounds.MenuButton);

            StartCoroutine(StartTransition());
        }
    }

    IEnumerator StartTransition()
    {
        image.DOColor(Color.white, fadeToWhiteTime);

        yield return new WaitForSeconds(fadeToWhiteTime + holdWhiteTime);

        image.DOColor(Color.clear, fadeToClearTime);

        AudioManager.PlayMusic(MainMenuMusic.MenuMusic);
        titleCanvas.Hide();
        menuCanvas.Show();
    }


    IEnumerator FadeIn(System.Action onEnd)
    {
        image.DOKill();
        image.DOColor(Color.black, 0);
        yield return new WaitForSeconds(holdBlackTime);
        image.DOColor(Color.clear, fadeToClearTime);
        yield return new WaitForSeconds(fadeToClearTime);
        onEnd?.Invoke();
    }

    IEnumerator FadeOut(System.Action onEnd)
    {
        image.DOKill();
        image.DOColor(Color.black, fadeToClearTime);
        yield return new WaitForSeconds(fadeToClearTime);
        onEnd?.Invoke();
    }

    void StartAttracting()
    {
#if UNITY_STANDALONE
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOut(() =>
        {
            attractUI.OptimizedCanvas.ShowDelayed(holdBlackTime);
            StartCoroutine(FadeIn(null));
            nowAttracting = true;
        }));
#endif
    }

    public void OnAttractionEnded()
    {
        nowAttracting = false;
        image.color = Color.black;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeIn(() =>
        {
            if (IsInvoking(nameof(StartAttracting))) CancelInvoke(nameof(StartAttracting));
            Invoke(nameof(StartAttracting), timeToAttract);
        }));

        AudioManager.PlaySound(MainMenuSounds.TitleVO);
    }
}