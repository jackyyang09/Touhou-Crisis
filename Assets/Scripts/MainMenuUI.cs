using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System;
using JSAM;

public class MainMenuUI : MonoBehaviourPunCallbacks
{
    [SerializeField]
    public const string playerNamePrefKey = "PlayerName";

    [SerializeField] AudioFileSoundObject buttonShoot;

    [SerializeField] Launcher launcher;

    [SerializeField] Ease easeType = Ease.Linear;
    [SerializeField] float uiMoveSpeed = 0.15f;
    [SerializeField] float bgFadeTime = 0.5f;

    [SerializeField] OptimizedCanvas title;
    [SerializeField] OptimizedCanvas controlPanel;
    [SerializeField] RectTransform settingsPanel;
    [SerializeField] OptimizedCanvas lobbyScreen;

    [SerializeField] RectTransform singleplayerButton;
    [SerializeField] RectTransform multiplayerButton;
    [SerializeField] OptimizedCanvas multiplayerMask;
    [SerializeField] TMPro.TextMeshProUGUI remotePlayer1Name;
    [SerializeField] TMPro.TextMeshProUGUI player2Name;
    [SerializeField] OptimizedCanvas hostPrivilegeMask;

    [SerializeField] UnityEngine.UI.RawImage reimuBG;
    [SerializeField] UnityEngine.UI.RawImage marisaBG;

    Coroutine settingsRoutine = null;

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

    public void PlayButtonSound()
    {
        AudioManager.instance.PlaySoundInternal(buttonShoot);
    }

    public void JoinOrCreateGame()
    {
        PlayButtonSound();
        RectTransform rect = controlPanel.transform as RectTransform;
        rect.DOAnchorPosX(1000, uiMoveSpeed).SetEase(easeType);
        Invoke("DelayedConnect", uiMoveSpeed);
    }

    //public void QuickPlay()
    //{
    //    PlayButtonSound();
    //    RectTransform rect = controlPanel.transform as RectTransform;
    //    rect.DOAnchorPosX(1000, uiMoveSpeed).SetEase(easeType);
    //    Invoke("DelayedQuickplay", uiMoveSpeed);
    //}

    public void DelayedConnect()
    {
        title.Hide();
        controlPanel.Hide();
        launcher.Connect();
    }

    //public void DelayedQuickplay()
    //{
    //    title.Hide();
    //    controlPanel.Hide();
    //    launcher.QuickPlay();
    //}

    public void OfflinePlay()
    {
        PlayButtonSound();
        RectTransform rect = controlPanel.transform as RectTransform;
        rect.DOAnchorPosX(1000, uiMoveSpeed).SetEase(easeType);
        title.Hide();
        controlPanel.Hide();
        launcher.EnterSinglePlayerMode();
    }

    public void OpenSettings()
    {
        if (settingsRoutine == null)
        {
            PlayButtonSound();
            settingsRoutine = StartCoroutine(ShowSettingsRoutine());
        }
    }

    public void HideSettings()
    {
        if (settingsRoutine == null)
        {
            PlayButtonSound();
            settingsRoutine = StartCoroutine(HideSettingsRoutine());
        }
    }

    IEnumerator ShowSettingsRoutine()
    {
        (controlPanel.transform as RectTransform).DOAnchorPosX(1400, uiMoveSpeed).SetEase(easeType);

        yield return new WaitForSeconds(uiMoveSpeed);

        settingsPanel.DOAnchorPosX(290, uiMoveSpeed).SetEase(easeType);

        settingsRoutine = null;
    }

    IEnumerator HideSettingsRoutine()
    {
        settingsPanel.DOAnchorPosX(1600, uiMoveSpeed).SetEase(easeType);

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
            PlayButtonSound();
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
            if (!PhotonNetwork.IsMasterClient)
            {
                hostPrivilegeMask.Show();
                multiplayerMask.Hide();
                reimuBG.DOFade(0, bgFadeTime);
                marisaBG.DOKill();
                marisaBG.DOFade(1, bgFadeTime);
            }
        }

    }
    public override void OnPlayerEnteredRoom(Player other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            AudioManager.PlaySound(MainMenuSounds.PlayerJoin);
            OnPlayerTwoJoin(other.NickName);
        }
    }

    public override void OnLeftRoom()
    {
        multiplayerMask.Show();
        reimuBG.DOFade(1, bgFadeTime);
        marisaBG.DOKill();
        marisaBG.DOFade(0, bgFadeTime);
        hostPrivilegeMask.Hide();
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
        multiplayerMask.Hide();
    }

    public void OnPlayerTwoLeave()
    {
        (player2Name.transform as RectTransform).DOAnchorPosX(-15, 0);
        (player2Name.transform as RectTransform).DOAnchorPosX(700, uiMoveSpeed).SetEase(easeType);

        (remotePlayer1Name.transform as RectTransform).DOAnchorPosX(16, 0);
        (remotePlayer1Name.transform as RectTransform).DOAnchorPosX(-700, uiMoveSpeed).SetEase(easeType);
        multiplayerMask.Show();
        hostPrivilegeMask.Hide();
        reimuBG.DOFade(1, bgFadeTime);
        marisaBG.DOKill();
        marisaBG.DOFade(0, bgFadeTime);
    }

    public void QuitGame()
    {
        PlayButtonSound();
        Application.Quit();
    }
}