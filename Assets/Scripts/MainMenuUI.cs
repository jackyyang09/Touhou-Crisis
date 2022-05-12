using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using JSAM;
using static Facade;

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

    [SerializeField] TMPro.TextMeshProUGUI versionLabel = null;

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

    [SerializeField] OptimizedCanvas player1ReimuPlate = null;
    [SerializeField] OptimizedCanvas player1MarisaPlate = null;
    RectTransform player1NameRect;
    float player1NameDest;
    [SerializeField] OptimizedCanvas player2MarisaPlate = null;
    [SerializeField] OptimizedCanvas player2ReimuPlate = null;
    RectTransform player2NameRect;
    float player2NameDest;

    [SerializeField] GameObject startGameClientBlock = null;

    [SerializeField] UnityEngine.UI.RawImage reimuBG = null;
    [SerializeField] UnityEngine.UI.RawImage marisaBG = null;

    [SerializeField] GameObject onlineCrosshair = null;

    Coroutine gameStartRoutine = null;

    string player2Name;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        versionLabel.text = "VERSION " + Application.version;

        yield return null;

        player1NameRect = player1ReimuPlate.transform as RectTransform;
        player2NameRect = player2MarisaPlate.transform as RectTransform;

        player1NameDest = player1NameRect.anchoredPosition.x;
        player2NameDest = player2NameRect.anchoredPosition.x;

        UpdateDiscordToMenu();
    }

    public override void OnEnable()
    {
        base.OnEnable();

        GameplayModifiers.OnHostPlayerChanged += SwapNameLabels;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        GameplayModifiers.OnHostPlayerChanged -= SwapNameLabels;
    }

    public void PlayButtonSound()
    {
        AudioManager.Instance.PlaySoundInternal(buttonShoot);
    }

    public void JoinOrCreateGame(float delay)
    {
        PlayButtonSound();
        onePlayerButton.SetActive(false);
        twoPlayerButtons.SetActive(true);
        Invoke(nameof(DelayedConnect), delay);
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

        if (modifiers.HostIsReimu) Lean.Localization.LeanLocalization.SetToken("Tokens/ReimuName", PhotonNetwork.MasterClient.NickName);
        else Lean.Localization.LeanLocalization.SetToken("Tokens/MarisaName", PhotonNetwork.MasterClient.NickName);

        player2ReimuPlate.Hide();
        player2MarisaPlate.Hide();
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
        photonView.RPC(nameof(EnterGameSetup), RpcTarget.All);
    }

    public void SyncGameplayModifiers()
    {
        photonView.RPC(nameof(SyncGameplayModifiersRPC), RpcTarget.All, new object[]
        {
            modifiers.StartingLives,
            modifiers.UFOSpawnRate,
            modifiers.BossActionSpeed,
            modifiers.BossMoveSpeed,
            modifiers.GameMode,
            modifiers.HostIsReimu
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
            (GameplayModifiers.GameModes)data[4],
            (bool)data[5]
            );

        modifiers.ForceRefreshProperties();
    }

    [PunRPC]
    void EnterGameSetup()
    {
        PlayButtonSound();
        lobbyScreen.Hide();
        gameSetup.ShowDelayed(0.1f);

        startGameClientBlock.SetActive(!PhotonNetwork.IsMasterClient);

        string character = "Marisa";
        if (modifiers.HostIsReimu && PhotonNetwork.IsMasterClient) character = "Reimu";
        else if (!modifiers.HostIsReimu && !PhotonNetwork.IsMasterClient) character = "Reimu";

        DiscordWrapper.Instance.UpdateActivity(
            state: "Game Setup",
            details: PhotonNetwork.OfflineMode ? "Offline Solo" : 
            (GameplayModifiers.Instance.GameMode == GameplayModifiers.GameModes.Coop ? "Online Co-Op" : "Online Versus"),
            largeImageKey: "sakuya",
            smallImageKey: character.ToLower() + "_discord",
            smallImageText: PhotonNetwork.LocalPlayer.NickName + " playing as " + character,
            partySize: PhotonNetwork.OfflineMode ? 1 : 2, 
            partyMax: PhotonNetwork.OfflineMode ? 1 : 2
        );
    }

    public void SyncEnterGame()
    {
        photonView.RPC(nameof(EnterGame), RpcTarget.All, new object[] 
        {
            modifiers.StartingLives,
            modifiers.UFOSpawnRate,
            modifiers.BossActionSpeed,
            modifiers.BossMoveSpeed,
            modifiers.GameMode,
            modifiers.HostIsReimu
        });
    }

    public void SyncReturnToLobby()
    {
        photonView.RPC(nameof(ReturnToLobby), RpcTarget.All);
    }

    [PunRPC]
    void ReturnToLobby()
    {
        PlayButtonSound();
        lobbyScreen.ShowDelayed(0.1f);
        gameSetup.Hide();

        UpdateDiscordWithRoomState(PhotonNetwork.CurrentRoom.PlayerCount);
    }

    [PunRPC]
    void EnterGame(object[] data)
    {
        if (gameStartRoutine != null) return;

        modifiers.ApplyAllProperties(
            (GameplayModifiers.LiveCounts)data[0],
            (GameplayModifiers.UFOSpawnRates)data[1],
            (GameplayModifiers.BossActionSpeeds)data[2],
            (GameplayModifiers.BossMoveSpeeds)data[3],
            (GameplayModifiers.GameModes)data[4],
            (bool)data[5]
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

        if (modifiers.HostIsReimu) Lean.Localization.LeanLocalization.SetToken("Tokens/ReimuName", PhotonNetwork.MasterClient.NickName);
        else Lean.Localization.LeanLocalization.SetToken("Tokens/MarisaName", PhotonNetwork.MasterClient.NickName);
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

            UpdateDiscordWithRoomState(1);
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

        UpdateDiscordToMenu();
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        if (modifiers.HostIsReimu)
        {
            player1ReimuPlate.SetActive(true);
            player1MarisaPlate.SetActive(false);
            Lean.Localization.LeanLocalization.SetToken("Tokens/ReimuName", PhotonNetwork.LocalPlayer.NickName);
        }
        else
        {
            player1ReimuPlate.SetActive(false);
            player1MarisaPlate.SetActive(true);
            Lean.Localization.LeanLocalization.SetToken("Tokens/MarisaName", PhotonNetwork.LocalPlayer.NickName);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            OnPlayerTwoLeave();
        }

        Lean.Localization.LeanLocalization.UpdateTranslations();
    }

    public void OnPlayerTwoJoin(string name)
    {
        player2Name = name;

        player2MarisaPlate.Show();

        player2NameRect.anchoredPosition = new Vector2(player2NameDest + 2000, 0);
        player2NameRect.DOAnchorPosX(player2NameDest, uiMoveSpeed).SetEase(easeType);

        if (modifiers.HostIsReimu) Lean.Localization.LeanLocalization.SetToken("Tokens/MarisaName", name);
        else Lean.Localization.LeanLocalization.SetToken("Tokens/ReimuName", name);

        if (!PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < 2; i++)
            {
                hostPrivilegeMask[i].Show();
            }
            reimuBG.DOFade(0, bgFadeTime);
            marisaBG.DOKill();
            marisaBG.DOFade(1, bgFadeTime);

            player1NameRect.anchoredPosition = new Vector2(player1NameDest - 2000, 0);
            player1NameRect.DOAnchorPosX(player1NameDest, uiMoveSpeed).SetEase(easeType);

            if (modifiers.HostIsReimu) Lean.Localization.LeanLocalization.SetToken("Tokens/ReimuName", PhotonNetwork.MasterClient.NickName);
            else Lean.Localization.LeanLocalization.SetToken("Tokens/MarisaName", PhotonNetwork.MasterClient.NickName);
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

        UpdateDiscordWithRoomState(2);
    }

    public void OnPlayerTwoLeave()
    {
        player2NameRect.DOComplete();
        player2NameRect.DOAnchorPosX(player2NameRect.anchoredPosition.x + 2000, uiMoveSpeed).SetEase(easeType);

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

        UpdateDiscordWithRoomState(1);
    }

    public void SwitchPlayers()
    {
        modifiers.SyncCycleHostPlayer();
    }

    public void SwapNameLabels(bool hostIsReimu)
    {
        player1NameRect.gameObject.SetActive(false);
        player2NameRect.gameObject.SetActive(false);

        if (hostIsReimu)
        {
            player1NameRect = player1ReimuPlate.transform as RectTransform;
            player2NameRect = player2MarisaPlate.transform as RectTransform;
            Lean.Localization.LeanLocalization.SetToken("Tokens/ReimuName", PhotonNetwork.MasterClient.NickName);
            Lean.Localization.LeanLocalization.SetToken("Tokens/MarisaName", player2Name);
        }
        else
        {
            player1NameRect = player1MarisaPlate.transform as RectTransform;
            player2NameRect = player2ReimuPlate.transform as RectTransform;
            Lean.Localization.LeanLocalization.SetToken("Tokens/ReimuName", player2Name);
            Lean.Localization.LeanLocalization.SetToken("Tokens/MarisaName", PhotonNetwork.MasterClient.NickName);
        }
        
        player1NameRect.gameObject.SetActive(true);
        player2NameRect.gameObject.SetActive(true);

        player1ReimuPlate.SetActive(hostIsReimu);
        player1MarisaPlate.SetActive(!hostIsReimu);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            player2MarisaPlate.SetActive(hostIsReimu);
            player2ReimuPlate.SetActive(!hostIsReimu);
        }
        else
        {
            player2ReimuPlate.Hide();
            player2MarisaPlate.Hide();
        }

        UpdateDiscordWithRoomState(PhotonNetwork.CurrentRoom.PlayerCount);
    }

    public void QuitGame()
    {
        PlayButtonSound();
        Application.Quit();
    }

    void UpdateDiscordToMenu()
    {
        DiscordWrapper.Instance.UpdateActivity(
            details: "In-Menu",
            largeImageKey: "sakuya"
        );
    }

    void UpdateDiscordWithRoomState(int playersConnected)
    {
        string character = "Marisa";
        if (modifiers.HostIsReimu && PhotonNetwork.IsMasterClient) character = "Reimu";
        else if (!modifiers.HostIsReimu && !PhotonNetwork.IsMasterClient) character = "Reimu";

        DiscordWrapper.Instance.UpdateActivity(
            details: playersConnected == 1 ? 
            (PhotonNetwork.OfflineMode ? "Offline Solo" : "Hosting Room w/ ID: " + PhotonNetwork.CurrentRoom.Name) : "Choosing Game Mode",
            state: "Waiting for Players",
            largeImageKey: "sakuya",
            smallImageKey: character.ToLower() + "_discord",
            smallImageText: PhotonNetwork.LocalPlayer.NickName + " playing as " + character,
            partySize: playersConnected,
            partyMax: PhotonNetwork.OfflineMode ? 1 : 2
            );
    }
}