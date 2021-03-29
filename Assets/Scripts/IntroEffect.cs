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

    [SerializeField] float musicDelay = 0.5f;
    [SerializeField] float holdBlackTime = 1;
    [SerializeField] float fadeToWhiteTime = 0.5f;
    [SerializeField] float holdWhiteTime = 2;
    [SerializeField] float fadeToClearTime;

    [SerializeField] OptimizedCanvas titleCanvas = null;
    [SerializeField] OptimizedCanvas menuCanvas = null;

    [SerializeField] TMPro.TextMeshProUGUI shootToStart = null;
    [SerializeField] float textFlashTime = 1;

    private void Awake()
    {
        image.DOColor(Color.black, 0);
        image.DOColor(Color.clear, fadeToClearTime);
        Invoke("UnsubscribeSkipOpening", fadeToClearTime);

        StartCoroutine(FlashShootToStart());
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
        railShooter.OnShoot -= SkipOpening;
        railShooter.OnShoot += ShootToStart;
    }

    private void SkipOpening(Ray obj, Vector2 pos)
    {
        if (IsInvoking("UnsubscribeSkipOpening")) CancelInvoke("UnsubscribeSkipOpening");
        image.DOComplete();
        image.color = Color.clear;
        railShooter.OnShoot -= SkipOpening;
        railShooter.OnShoot += ShootToStart;
    }

    void ShootToStart(Ray obj, Vector2 pos)
    {
        railShooter.OnShoot -= ShootToStart;
        AudioManager.PlaySound(MainMenuSounds.MenuButton);

        StartCoroutine(StartTransition());
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
}