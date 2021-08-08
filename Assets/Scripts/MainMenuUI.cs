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
    [SerializeField] GameObject twoPlayerButtons = null;
    [SerializeField] GameObject[] twoPlayerButtonListener = null;

    [SerializeField] OptimizedCanvas[] multiplayerMask = null;
    [SerializeField] OptimizedCanvas[] hostPrivilegeMask = null;

    [SerializeField] OptimizedCanvas player1NamePlate = null;
    [SerializeField] OptimizedCanvas player2NamePlate = null;

    [SerializeField] GameObject startGameClientBlock = null;

    [SerializeField] UnityEngine.UI.RawImage reimuBG = null;
    [SerializeField] UnityEngine.UI.RawImage marisaBG = null;

    [SerializeField] GameObject onlineCrosshair = null;

    Coroutine gameStartRoutine = null;

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

    public void PlayButtonSound()
    {
        AudioManager.Instance.PlaySoundInternal(buttonShoot);
    }

    public void JoinOrCreateGame(float delay)
    {
        PlayButtonSound();
        onePlayerButton.SetActive(false);
        twoPlayerButtons.SetActive(true);
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
        twoPlayerButtons.SetActive(false);
        
        launcher.EnterSinglePlayerMode();
    }

    public void CoOpPlay()
    {
        SyncEnterGameSetup();

        GameplayModifiers.Instance.SetGameMode(GameplayModifiers.GameModes.Coop);
    }

    public void VersusPlay()
    {
        SyncEnterGameSetup();

        GameplayModifiers.Instance.SetGameMode(GameplayModifiers.GameModes.Versus);
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


    public void SyncGameplayModifiers()
    {
        photonView.RPC("SyncGameplayModifiersRPC", RpcTarget.All, new object[]
        {
            GameplayModifiers.Instance.StartingLives,
            GameplayModifiers.Instance.UFOSpawnRate,
            GameplayModifiers.Instance.BossActionSpeed,
            GameplayModifiers.Instance.BossMoveSpeed,
            GameplayModifiers.Instance.GameMode
        });
    }

    [PunRPC]
    void SyncGameplayModifiersRPC(object[] data)
    {
        GameplayModifiers.Instance.ApplyAllProperties(
            (GameplayModifiers.LiveCounts)data[0],
            (GameplayModifiers.UFOSpawnRates)data[1],
            (GameplayModifiers.BossActionSpeeds)data[2],
            (GameplayModifiers.BossMoveSpeeds)data[3],
            (GameplayModifiers.GameModes)data[4]
            );

        GameplayModifiers.Instance.ForceRefreshProperties();
    }

    [PunRPC]
    void EnterGameSetup()
    {
        PlayButtonSound();
        lobbyScreen.Hide();
        gameSetup.ShowDelayed(0.1f);
    }

    public void SyncEnterGame()
    {
        photonView.RPC("EnterGame", RpcTarget.All, new object[] 
        {
            GameplayModifiers.Instance.StartingLives,
            GameplayModifiers.Instance.UFOSpawnRate,
            GameplayModifiers.Instance.BossActionSpeed,
            GameplayModifiers.Instance.BossMoveSpeed,
            GameplayModifiers.Instance.GameMode
        });
    }

    public void SyncReturnToLobby()
    {
        photonView.RPC("ReturnToLobby", RpcTarget.All);
    }

    [PunRPC]
    void ReturnToLobby()
    {
        PlayButtonSound();
        lobbyScreen.ShowDelayed(0.1f);
        gameSetup.Hide();
    }

    [PunRPC]
    void EnterGame(object[] data)
    {
        if (gameStartRoutine != null) return;

        GameplayModifiers.Instance.ApplyAllProperties(
            (GameplayModifiers.LiveCounts)data[0],
            (GameplayModifiers.UFOSpawnRates)data[1],
            (GameplayModifiers.BossActionSpeeds)data[2],
            (GameplayModifiers.BossMoveSpeeds)data[3],
            (GameplayModifiers.GameModes)data[4]
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
        PhotonNetwork.Instantiate(onlineCrosshair.name, transform.position, Quaternion.identity);

        Lean.Localization.LeanLocalization.SetToken("Tokens/Player1Name", PhotonNetwork.MasterClient.NickName);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            OnPlayerTwoJoin(PhotonNetwork.LocalPlayer.NickName);
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < 2; i++)
                {
                    multiplayerMask[i].Show();
                    twoPlayerButtonListener[i].SetActive(false);
                }
            }
            player2NamePlate.Hide();
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
        for (int i = 0; i < 2; i++)
        {
            multiplayerMask[i].Show();
            hostPrivilegeMask[i].Hide();
        }
        reimuBG.DOFade(1, bgFadeTime);
        marisaBG.DOKill();
        marisaBG.DOFade(0, bgFadeTime);
        startGameClientBlock.SetActive(false);

        roomSetupScreen.ShowDelayed(0.1f);
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
        player2NamePlate.Show();
        (player2NamePlate.transform as RectTransform).DOAnchorPosX(900, 0);
        (player2NamePlate.transform as RectTransform).DOAnchorPosX(79, uiMoveSpeed).SetEase(easeType);
        Lean.Localization.LeanLocalization.SetToken("Tokens/Player2Name", name);

        if (!PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < 2; i++)
            {
                hostPrivilegeMask[i].Show();
            }
            reimuBG.DOFade(0, bgFadeTime);
            marisaBG.DOKill();
            marisaBG.DOFade(1, bgFadeTime);

            startGameClientBlock.SetActive(true);

            (player1NamePlate.transform as RectTransform).DOAnchorPosX(-900, 0);
            (player1NamePlate.transform as RectTransform).DOAnchorPosX(-79, uiMoveSpeed).SetEase(easeType);

            Lean.Localization.LeanLocalization.SetToken("Tokens/Player1Name", PhotonNetwork.MasterClient.NickName);
        }
        else
        {
            SyncGameplayModifiers();
        }

        for (int i = 0; i < 2; i++)
        {
            multiplayerMask[i].Hide();
            twoPlayerButtonListener[i].SetActive(PhotonNetwork.IsMasterClient);
        }
    }

    public void OnPlayerTwoLeave()
    {
        (player2NamePlate.transform as RectTransform).DOAnchorPosX(79, 0);
        (player2NamePlate.transform as RectTransform).DOAnchorPosX(900, uiMoveSpeed).SetEase(easeType);

        //(player1NamePlate.transform as RectTransform).DOAnchorPosX(16, 0);
        //(player1NamePlate.transform as RectTransform).DOAnchorPosX(-700, uiMoveSpeed).SetEase(easeType);
        for (int i = 0; i < 2; i++)
        {
            multiplayerMask[i].Show();
            hostPrivilegeMask[i].Hide();
            twoPlayerButtonListener[i].SetActive(false);
        }
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