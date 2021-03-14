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
    [SerializeField] float fadeToClearTime;

    [SerializeField] PhotonView view;

    private void Awake()
    {
        image.DOColor(Color.black, 0);
        image.DOColor(Color.white, fadeToWhiteTime).SetDelay(holdBlackTime);
        image.DOColor(Color.clear, fadeToClearTime).SetDelay(holdBlackTime + fadeToWhiteTime);
        Invoke("PlayMusic", musicDelay);
    }

    void PlayMusic()
    {
        AudioManager.PlayMusic(MainMenuMusic.MenuMusic);
        railShooter.OnShoot -= SkipOpening;
    }

    private void OnEnable()
    {
        railShooter.OnShoot += SkipOpening;
    }

    private void OnDisable()
    {
        railShooter.OnShoot -= SkipOpening;
    }

    private void SkipOpening(Ray obj, Vector2 pos)
    {
        image.DOComplete();
        image.color = Color.clear;
        railShooter.OnShoot -= SkipOpening;
    }
}
