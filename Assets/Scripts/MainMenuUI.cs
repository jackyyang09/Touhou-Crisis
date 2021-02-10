﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System;

public class MainMenuUI : MonoBehaviourPunCallbacks
{
    [SerializeField]
    public const string playerNamePrefKey = "PlayerName";

    [SerializeField] Launcher launcher;

    [SerializeField] Ease easeType = Ease.Linear;
    [SerializeField] float uiMoveSpeed = 0.15f;

    [SerializeField] OptimizedCanvas title;
    [SerializeField] OptimizedCanvas controlPanel;
    [SerializeField] RectTransform settingsPanel;
    [SerializeField] OptimizedCanvas lobbyScreen;

    [SerializeField] RectTransform singleplayerButton;
    [SerializeField] RectTransform multiplayerButton;
    [SerializeField] UnityEngine.UI.Image multiplayerMask;
    [SerializeField] TMPro.TextMeshProUGUI remotePlayer1Name;
    [SerializeField] TMPro.TextMeshProUGUI player2Name;

    Coroutine settingsRoutine = null;

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

    public void JoinOrCreateGame()
    {
        RectTransform rect = controlPanel.transform as RectTransform;
        rect.DOAnchorPosX(1000, uiMoveSpeed).SetEase(easeType);
        Invoke("DelayedConnect", uiMoveSpeed);
    }

    public void QuickPlay()
    {
        RectTransform rect = controlPanel.transform as RectTransform;
        rect.DOAnchorPosX(1000, uiMoveSpeed).SetEase(easeType);
        Invoke("DelayedQuickplay", uiMoveSpeed);
    }

    public void DelayedConnect()
    {
        title.Hide();
        controlPanel.Hide();
        launcher.Connect();
    }

    public void DelayedQuickplay()
    {
        title.Hide();
        controlPanel.Hide();
        launcher.QuickPlay();
    }

    public void OpenSettings()
    {
        if (settingsRoutine == null)
        {
            settingsRoutine = StartCoroutine(ShowSettingsRoutine());
        }
    }

    public void HideSettings()
    {
        if (settingsRoutine == null)
        {
            settingsRoutine = StartCoroutine(HideSettingsRoutine());
        }
    }

    IEnumerator ShowSettingsRoutine()
    {
        (controlPanel.transform as RectTransform).DOAnchorPosX(1000, uiMoveSpeed).SetEase(easeType);

        yield return new WaitForSeconds(uiMoveSpeed);

        settingsPanel.DOAnchorPosX(411, uiMoveSpeed).SetEase(easeType);

        settingsRoutine = null;
    }

    IEnumerator HideSettingsRoutine()
    {
        settingsPanel.DOAnchorPosX(1300, uiMoveSpeed).SetEase(easeType);

        yield return new WaitForSeconds(uiMoveSpeed);

        (controlPanel.transform as RectTransform).DOAnchorPosX(0, uiMoveSpeed).SetEase(easeType);

        settingsRoutine = null;
    }

    public void EnterLobby()
    {
        singleplayerButton.DOAnchorPosX(-444, uiMoveSpeed).SetEase(easeType);
        multiplayerButton.DOAnchorPosX(436, uiMoveSpeed).SetEase(easeType);
    }

    public void ReturnToMainScreen()
    {
        if (settingsRoutine == null)
        {
            settingsRoutine = StartCoroutine(LeaveLobby());
        }
    }

    IEnumerator LeaveLobby()
    {
        singleplayerButton.DOAnchorPosX(-1360, uiMoveSpeed).SetEase(easeType);
        multiplayerButton.DOAnchorPosX(1360, uiMoveSpeed).SetEase(easeType);

        yield return new WaitForSeconds(uiMoveSpeed);

        launcher.Disconnect();
        lobbyScreen.Hide();
        (controlPanel.transform as RectTransform).DOAnchorPosX(0, uiMoveSpeed).SetEase(easeType);
        title.Show();

        settingsRoutine = null;
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            OnPlayerTwoJoin(PhotonNetwork.LocalPlayer.NickName);
        }
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            OnPlayerTwoJoin(other.NickName);
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            OnPlayerTwoLeave();
        }
    }

    public void OnPlayerTwoJoin(string name)
    {
        (player2Name.transform as RectTransform).DOAnchorPosX(700, 0);
        (player2Name.transform as RectTransform).DOAnchorPosX(-15, uiMoveSpeed).SetEase(easeType);
        player2Name.text = name;

        (remotePlayer1Name.transform as RectTransform).DOAnchorPosX(-700, 0);
        (remotePlayer1Name.transform as RectTransform).DOAnchorPosX(16, uiMoveSpeed).SetEase(easeType);
        remotePlayer1Name.text = PhotonNetwork.MasterClient.NickName;
        multiplayerMask.enabled = false;
    }

    public void OnPlayerTwoLeave()
    {
        (player2Name.transform as RectTransform).DOAnchorPosX(-15, 0);
        (player2Name.transform as RectTransform).DOAnchorPosX(700, uiMoveSpeed).SetEase(easeType);

        (remotePlayer1Name.transform as RectTransform).DOAnchorPosX(16, 0);
        (remotePlayer1Name.transform as RectTransform).DOAnchorPosX(-700, uiMoveSpeed).SetEase(easeType);
        multiplayerMask.enabled = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}