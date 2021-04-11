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
    [SerializeField] public const string playerNamePrefKey = "PlayerName";

    [SerializeField] AudioFileSoundObject buttonShoot = null;
    [SerializeField] RailShooterLogic railShooter = null;

    [SerializeField] Launcher launcher = null;

    [SerializeField] Ease easeType = Ease.Linear;
    [SerializeField] float uiMoveSpeed = 0.15f;
    [SerializeField] float bgFadeTime = 0.5f;
    [SerializeField] float gameStartDelay = 0.25f;

    [SerializeField] OptimizedCanvas lobbyTopBar = null;
    [SerializeField] LoadingScreen loadingScreen = null;

    [SerializeField] OptimizedCanvas roomSetupScreen = null;
    [SerializeField] OptimizedCanvas lobbyScreen = null;
    [SerializeField] OptimizedCanvas gameSetup = null;

    [SerializeField] GameObject onePlayerButton = null;
    [SerializeField] GameObject twoPlayerCoopButton = null;
    [SerializeField] GameObject twoPlayerVersusButton = null;

    [SerializeField] OptimizedCanvas multiplayerMask = null;
    [SerializeField] TMPro.TextMeshProUGUI remotePlayer1Name = null;
    [SerializeField] TMPro.TextMeshProUGUI player2Name = null;
    [SerializeField] OptimizedCanvas hostPrivilegeMask = null;

    [SerializeField] GameObject startGameClientBlock = null;

    [SerializeField] UnityEngine.UI.RawImage reimuBG = null;
    [SerializeField] UnityEngine.UI.RawImage marisaBG = null;

    [SerializeField] GameObject gameplayModifiers = null;
    GameplayModifiers modifiers = null;

    [SerializeField] GameObject onlineCrosshair = null;

    Coroutine gameStartRoutine = null;

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

    public void PlayButtonSound()
    {
        AudioManager.instance.PlaySoundInternal(buttonShoot);
    }

    public void JoinOrCreateGame(float delay)
    {
        PlayButtonSound();
        onePlayerButton.SetActive(false);
        twoPlayerCoopButton.SetActive(true);
        // Enable me in 0.6
        //twoPlayerVersusButton.SetActive(true);
        Invoke("DelayedConnect", delay);
    }

    public void DelayedConnect()
    {
        launcher.Connect();
    }

    public void OfflinePlay()
    {
        PlayButtonSound();
        onePlayerButton.SetActive(true);
        twoPlayerCoopButton.SetActive(false);
        twoPlayerVersusButton.SetActive(false);
        
        launcher.EnterSinglePlayerMode();
    }

    public void LeaveLobby()
    {
        PlayButtonSound();
        launcher.Disconnect();
    }

    public void SyncEnterGameSetup()
    {
        photonView.RPC("EnterGameSetup", RpcTarget.All);
    }

    [PunRPC]
    void EnterGameSetup()
    {
        PlayButtonSound();
        lobbyScreen.Hide();
        gameSetup.ShowDelayed(0.1f);
        PhotonNetwork.Instantiate(onlineCrosshair.name, transform.position, Quaternion.identity);
    }

    public void SyncEnterGame()
    {
        photonView.RPC("EnterGame", RpcTarget.All, new object[] 
        {
            modifiers.StartingLives,
            modifiers.UFOSpawnRate,
            modifiers.BossActionSpeed,
            modifiers.BossMoveSpeed
        });
    }

    [PunRPC]
    void EnterGame(object[] data)
    {
        if (gameStartRoutine != null) return;

        modifiers = FindObjectOfType<GameplayModifiers>();
        modifiers.ApplyAllProperties(
            (GameplayModifiers.LiveCounts)data[0],
            (GameplayModifiers.UFOSpawnRates)data[1],
            (GameplayModifiers.BossActionSpeeds)data[2],
            (GameplayModifiers.BossMoveSpeeds)data[3]
            );

        PlayButtonSound();

        lobbyTopBar.Hide();

        gameStartRoutine = StartCoroutine(LoadGame());
    }

    IEnumerator LoadGame()
    {
        gameSetup.Hide();

        yield return new WaitForSeconds(uiMoveSpeed);

        railShooter.enabled = false;

        yield return StartCoroutine(loadingScreen.ShowRoutine());

        yield return new WaitForSeconds(gameStartDelay);

        launcher.Load2PlayerMode();
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

                startGameClientBlock.SetActive(true);
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            modifiers = PhotonNetwork.Instantiate(gameplayModifiers.name, Vector3.zero, Quaternion.identity)
            .GetComponentInChildren<GameplayModifiers>();
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
        startGameClientBlock.SetActive(false);

        roomSetupScreen.ShowDelayed(0.1f);
        var crosshairs = FindObjectsOfType<CrosshairOnline>();
        for (int i = 0; i < crosshairs.Length; i++)
        {
            Destroy(crosshairs[i].transform.parent.gameObject);
        }
        Destroy(modifiers);
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

        if (gameSetup.IsVisible)
        {
            gameSetup.Hide();
        }
        if (!lobbyScreen.IsVisible)
        {
            lobbyScreen.ShowDelayed(0.1f);
        }
    }

    public void QuitGame()
    {
        PlayButtonSound();
        Application.Quit();
    }
}