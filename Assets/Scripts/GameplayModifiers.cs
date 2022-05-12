using UnityEngine;
using UnityEngine.SceneManagement;
using Lean.Localization;
using TMPro;
using Photon.Pun;

public class GameplayModifiers : MonoBehaviourPun
{
    public enum LiveCounts
    {
        One,
        Two,
        Three,
        Four,
        Five,
        Infinite,
        Length
    }

    public enum UFOSpawnRates
    {
        Disabled,
        Low,
        Normal,
        High,
        Length
    }    

    public enum BossActionSpeeds
    {
        Slow,
        Normal,
        Fast,
        Length
    }

    public enum BossMoveSpeeds
    {
        Slow,
        Normal,
        Fast,
        Length
    }

    public enum GameModes
    {
        Versus,
        Coop
    }

    [SerializeField] string mainMenuScene = "MainMenu";

    [SerializeField] LiveCounts startingLives = LiveCounts.Five;
    public LiveCounts StartingLives { get { return startingLives; } }

    [SerializeField] UFOSpawnRates ufoSpawnRate = UFOSpawnRates.Normal;
    public UFOSpawnRates UFOSpawnRate { get { return ufoSpawnRate; } }

    [SerializeField] BossMoveSpeeds bossMoveSpeed = BossMoveSpeeds.Normal;
    public BossMoveSpeeds BossMoveSpeed { get { return bossMoveSpeed; } }

    [SerializeField] BossActionSpeeds bossActionSpeed = BossActionSpeeds.Normal;
    public BossActionSpeeds BossActionSpeed { get { return bossActionSpeed; } }

    [SerializeField] GameModes gameMode = GameModes.Versus;
    public GameModes GameMode { get { return gameMode; } }

    [SerializeField] bool hostIsReimu = true;
    public bool HostIsReimu { get { return hostIsReimu; } }

    bool initialized = false;

    static GameplayModifiers instance;
    public static GameplayModifiers Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameplayModifiers>();
            }
            return instance;
        }
    }

    public static System.Action<LiveCounts> OnStartingLivesChanged;
    public static System.Action<UFOSpawnRates> OnUFOSpawnRateChanged;
    public static System.Action<BossMoveSpeeds> OnBossMoveSpeedChanged;
    public static System.Action<BossActionSpeeds> OnBossActionSpeedChanged;
    public static System.Action<bool> OnHostPlayerChanged;

    // Start is called before the first frame update
    void Start()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void ForceRefreshProperties()
    {
        OnStartingLivesChanged?.Invoke(startingLives);
        OnUFOSpawnRateChanged?.Invoke(ufoSpawnRate);
        OnBossMoveSpeedChanged?.Invoke(bossMoveSpeed);
        OnBossActionSpeedChanged?.Invoke(bossActionSpeed);
        OnHostPlayerChanged?.Invoke(hostIsReimu);
    }

    public void SyncCycleStartingLives()
    {
        photonView.RPC(nameof(CycleStartingLives), RpcTarget.All);
    }

    [PunRPC]
    void CycleStartingLives()
    {
        JSAM.AudioManager.PlaySound(MainMenuSounds.MenuButton);

        startingLives = (LiveCounts)Mathf.Repeat((int)startingLives + 1, (int)LiveCounts.Length);

        OnStartingLivesChanged?.Invoke(startingLives);
    }

    public void SyncCycleUFOSpawnRate()
    {
        photonView.RPC(nameof(CycleUFOSpawnRate), RpcTarget.All);
    }

    [PunRPC]
    void CycleUFOSpawnRate()
    {
        JSAM.AudioManager.PlaySound(MainMenuSounds.MenuButton);

        ufoSpawnRate = (UFOSpawnRates)Mathf.Repeat((int)ufoSpawnRate + 1, (int)UFOSpawnRates.Length);

        OnUFOSpawnRateChanged?.Invoke(ufoSpawnRate);
    }

    public void SyncCycleBossMoveSpeed()
    {
        photonView.RPC(nameof(CycleBossMoveSpeed), RpcTarget.All);
    }

    [PunRPC]
    void CycleBossMoveSpeed()
    {
        JSAM.AudioManager.PlaySound(MainMenuSounds.MenuButton);

        bossMoveSpeed = (BossMoveSpeeds)Mathf.Repeat((int)bossMoveSpeed + 1, (int)BossMoveSpeeds.Length);

        OnBossMoveSpeedChanged?.Invoke(bossMoveSpeed);
    }

    public void SyncCycleBossActionSpeed()
    {
        photonView.RPC(nameof(CycleBossActionSpeed), RpcTarget.All);
    }

    [PunRPC]
    void CycleBossActionSpeed()
    {
        JSAM.AudioManager.PlaySound(MainMenuSounds.MenuButton);

        bossActionSpeed = (BossActionSpeeds)Mathf.Repeat((int)bossActionSpeed + 1, (int)BossActionSpeeds.Length);

        OnBossActionSpeedChanged?.Invoke(bossActionSpeed);
    }

    public void SetGameMode(GameModes mode)
    {
        //gameMode = mode;
        photonView.RPC(nameof(SyncGameMode), RpcTarget.All, mode);
    }

    [PunRPC]
    void SyncGameMode(object newGameMode)
    {
        gameMode = (GameModes)newGameMode;
    }

    public void SyncCycleHostPlayer()
    {
        photonView.RPC(nameof(CycleHostPlayer), RpcTarget.All, !hostIsReimu);
    }

    [PunRPC]
    public void CycleHostPlayer(object newFlag)
    {
        JSAM.AudioManager.PlaySound(MainMenuSounds.MenuButton);
        hostIsReimu = (bool)newFlag;
        OnHostPlayerChanged?.Invoke(hostIsReimu);
    }

    /// <summary>
    /// Called remotely
    /// </summary>
    /// <param name="lives"></param>
    /// <param name="spawnRate"></param>
    /// <param name="actionSpeed"></param>
    /// <param name="moveSpeed"></param>
    public void ApplyAllProperties(LiveCounts lives, UFOSpawnRates spawnRate, BossActionSpeeds actionSpeed, BossMoveSpeeds moveSpeed, GameModes gameMode, bool player)
    {
        startingLives = lives;
        ufoSpawnRate = spawnRate;
        bossActionSpeed = actionSpeed;
        bossMoveSpeed = moveSpeed;
        hostIsReimu = player;
    }

    public void ResetProperties()
    {
        startingLives = LiveCounts.Five;
        ufoSpawnRate = UFOSpawnRates.Normal;
        bossActionSpeed = BossActionSpeeds.Normal;
        bossMoveSpeed = BossMoveSpeeds.Normal;
        gameMode = GameModes.Versus;
        hostIsReimu = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        if (!initialized)
        {
            initialized = true;
        }
        else if (!scene.name.Equals(mainMenuScene))
        {
            GameManager.OnQuitToMenu += SelfDestruct;
        }
    }

    void OnDestroy()
    {
        GameManager.OnQuitToMenu -= SelfDestruct;
    }

    void SelfDestruct() => Destroy(gameObject);
}