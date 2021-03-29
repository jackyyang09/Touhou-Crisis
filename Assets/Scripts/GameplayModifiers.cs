using UnityEngine;
using UnityEngine.SceneManagement;
using Lean.Localization;
using TMPro;

public class GameplayModifiers : MonoBehaviour
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

    [SerializeField] Photon.Pun.PhotonView photonView = null;

    [SerializeField] string mainMenuScene = "MainMenu";

    public const string MAX_LIVES_TOKEN = "Tokens/MaxLives";
    [SerializeField] LiveCounts startingLives = LiveCounts.Five;
    [SerializeField] LeanLocalizedTextMeshProUGUI startingLivesDescription = null;
    public LiveCounts StartingLives { get { return startingLives; } }

    public const string UFO_SPAWN_TOKEN = "Tokens/UFOSpawnRate";
    [SerializeField] UFOSpawnRates ufoSpawnRate = UFOSpawnRates.Normal;
    public UFOSpawnRates UFOSpawnRate { get { return ufoSpawnRate; } }
    [SerializeField] LeanLocalizedTextMeshProUGUI ufoSpawnRateDescription = null;

    public const string BOSS_MOVE_SPEED_TOKEN = "Tokens/BossMoveSpeed";
    [SerializeField] BossMoveSpeeds bossMoveSpeed = BossMoveSpeeds.Normal;
    public BossMoveSpeeds BossMoveSpeed { get { return bossMoveSpeed; } }
    [SerializeField] TextMeshProUGUI bossMoveSpeedDescription = null;

    public const string BOSS_ACTION_SPEED_TOKEN = "Tokens/BossActionSpeed";
    [SerializeField] BossActionSpeeds bossActionSpeed = BossActionSpeeds.Normal;
    public BossActionSpeeds BossActionSpeed { get { return bossActionSpeed; } }
    [SerializeField] TextMeshProUGUI bossActionSpeedDescription = null;

    bool initialized = false;

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
    public void SyncCycleStartingLives()
    {
        photonView.RPC("CycleStartingLives", Photon.Pun.RpcTarget.All);
    }

    [Photon.Pun.PunRPC]
    public void CycleStartingLives()
    {
        JSAM.AudioManager.PlaySound(MainMenuSounds.MenuButton);

        bool isEnglish = LeanLocalization.CurrentLanguage.Equals("English");

        startingLives = (LiveCounts)Mathf.Repeat((int)startingLives + 1, (int)LiveCounts.Length);
        switch (startingLives)
        {
            case LiveCounts.One:
            case LiveCounts.Two:
            case LiveCounts.Three:
            case LiveCounts.Four:
            case LiveCounts.Five:
                startingLivesDescription.enabled = true;
                LeanLocalization.SetToken(MAX_LIVES_TOKEN, ((int)startingLives + 1).ToString());
                break;
            case LiveCounts.Infinite:
                startingLivesDescription.enabled = false;
                if (isEnglish)
                {
                    LeanLocalization.SetToken(MAX_LIVES_TOKEN, startingLives.ToString());
                }
                else
                {
                    LeanLocalization.SetToken(MAX_LIVES_TOKEN, "∞");
                }

                if (LeanLocalization.CurrentLanguage == "English")
                {
                    startingLivesDescription.GetComponent<TextMeshProUGUI>().text = "Player death is disabled";
                }
                else if (LeanLocalization.CurrentLanguage == "Japanese")
                {
                    startingLivesDescription.GetComponent<TextMeshProUGUI>().text = "Player death is disabled";
                }
                break;
        }
    }

    public void SyncCycleUFOSpawnRate()
    {
        photonView.RPC("CycleUFOSpawnRate", Photon.Pun.RpcTarget.All);
    }

    [Photon.Pun.PunRPC]
    public void CycleUFOSpawnRate()
    {
        JSAM.AudioManager.PlaySound(MainMenuSounds.MenuButton);

        ufoSpawnRate = (UFOSpawnRates)Mathf.Repeat((int)ufoSpawnRate + 1, (int)UFOSpawnRates.Length);
        bool isEnglish = LeanLocalization.CurrentLanguage.Equals("English");
        
        if (isEnglish)
        {
            LeanLocalization.SetToken(UFO_SPAWN_TOKEN, ufoSpawnRate.ToString());
        }
        else
        {
            switch (ufoSpawnRate)
            {
                case UFOSpawnRates.Disabled:
                    LeanLocalization.SetToken(UFO_SPAWN_TOKEN, "オフ");
                    break;
                case UFOSpawnRates.Low:
                    LeanLocalization.SetToken(UFO_SPAWN_TOKEN, "低");
                    break;
                case UFOSpawnRates.Normal:
                    LeanLocalization.SetToken(UFO_SPAWN_TOKEN, "中");
                    break;
                case UFOSpawnRates.High:
                    LeanLocalization.SetToken(UFO_SPAWN_TOKEN, "高");
                    break;
            }
        }

        if (ufoSpawnRate == UFOSpawnRates.Disabled)
        {
            ufoSpawnRateDescription.enabled = false;
            if (isEnglish)
            {
                ufoSpawnRateDescription.GetComponent<TextMeshProUGUI>().text = "Enemy UFOs will not spawn";
            }
            else
            {
                ufoSpawnRateDescription.GetComponent<TextMeshProUGUI>().text = "Enemy UFOs will not spawn";
            }
        }
        else
        {
            switch (ufoSpawnRate)
            {
                case UFOSpawnRates.Low:
                    if (isEnglish)
                    {
                        ufoSpawnRateDescription.GetComponent<TextMeshProUGUI>().text = "Enemy UFOs will rarely spawn";
                    }
                    else
                    {
                        ufoSpawnRateDescription.GetComponent<TextMeshProUGUI>().text = "珍しく出る";
                    }
                    break;
                case UFOSpawnRates.Normal:
                    if (isEnglish)
                    {
                        ufoSpawnRateDescription.GetComponent<TextMeshProUGUI>().text = "Enemy UFOs will spawn normally";
                    }
                    else
                    {
                        ufoSpawnRateDescription.GetComponent<TextMeshProUGUI>().text = "まあまあ出る";
                    }
                    break;
                case UFOSpawnRates.High:
                    if (isEnglish)
                    {
                        ufoSpawnRateDescription.GetComponent<TextMeshProUGUI>().text = "Enemy UFOs will spawn often";
                    }
                    else
                    {
                        ufoSpawnRateDescription.GetComponent<TextMeshProUGUI>().text = "頻繁に出る";
                    }
                    break;
            }
        }
    }

    public void SyncCycleBossMoveSpeed()
    {
        photonView.RPC("CycleBossMoveSpeed", Photon.Pun.RpcTarget.All);
    }

    [Photon.Pun.PunRPC]
    public void CycleBossMoveSpeed()
    {
        JSAM.AudioManager.PlaySound(MainMenuSounds.MenuButton);

        bossMoveSpeed = (BossMoveSpeeds)Mathf.Repeat((int)bossMoveSpeed + 1, (int)BossMoveSpeeds.Length);
        bool isEnglish = LeanLocalization.CurrentLanguage.Equals("English");

        if (isEnglish)
        {
            LeanLocalization.SetToken(BOSS_MOVE_SPEED_TOKEN, ufoSpawnRate.ToString());
        }
        else
        {
            switch (bossMoveSpeed)
            {
                case BossMoveSpeeds.Slow:
                    LeanLocalization.SetToken(BOSS_MOVE_SPEED_TOKEN, "低");
                    break;
                case BossMoveSpeeds.Normal:
                    LeanLocalization.SetToken(BOSS_MOVE_SPEED_TOKEN, "中");
                    break;
                case BossMoveSpeeds.Fast:
                    LeanLocalization.SetToken(BOSS_MOVE_SPEED_TOKEN, "高");
                    break;
            }
        }

        switch (bossMoveSpeed)
        {
            case BossMoveSpeeds.Slow:
                if (isEnglish)
                    bossMoveSpeedDescription.text = "Boss moves slower and less often";
                else
                    bossMoveSpeedDescription.text = "通常より移動が遅い・頻度が低い";
                break;
            case BossMoveSpeeds.Normal:
                if (isEnglish)
                    bossMoveSpeedDescription.text = "Boss moves at the intended pace";
                else
                    bossMoveSpeedDescription.text = "通常ペース";
                break;
            case BossMoveSpeeds.Fast:
                if (isEnglish)
                    bossMoveSpeedDescription.text = "Boss moves very fast";
                else
                    bossMoveSpeedDescription.text = "とてつもない速さで動く";
                break;
        }
    }

    public void SyncCycleBossActionSpeed()
    {
        photonView.RPC("CycleBossActionSpeed", Photon.Pun.RpcTarget.All);
    }

    [Photon.Pun.PunRPC]
    public void CycleBossActionSpeed()
    {
        JSAM.AudioManager.PlaySound(MainMenuSounds.MenuButton);

        bossActionSpeed = (BossActionSpeeds)Mathf.Repeat((int)bossActionSpeed + 1, (int)BossActionSpeeds.Length);
        LeanLocalization.SetToken(BOSS_ACTION_SPEED_TOKEN, bossActionSpeed.ToString());
        bool isEnglish = LeanLocalization.CurrentLanguage.Equals("English");

        if (isEnglish)
        {
            LeanLocalization.SetToken(BOSS_ACTION_SPEED_TOKEN, ufoSpawnRate.ToString());
        }
        else
        {
            switch (bossActionSpeed)
            {
                case BossActionSpeeds.Slow:
                    LeanLocalization.SetToken(BOSS_ACTION_SPEED_TOKEN, "低");
                    break;
                case BossActionSpeeds.Normal:
                    LeanLocalization.SetToken(BOSS_ACTION_SPEED_TOKEN, "中");
                    break;
                case BossActionSpeeds.Fast:
                    LeanLocalization.SetToken(BOSS_ACTION_SPEED_TOKEN, "高");
                    break;
            }
        }

        switch (bossActionSpeed)
        {
            case BossActionSpeeds.Slow:
                if (isEnglish)
                    bossActionSpeedDescription.text = "Boss attacks less often";
                else
                    bossActionSpeedDescription.text = "通常より頻度が低い";
                break;
            case BossActionSpeeds.Normal:
                if (isEnglish)
                    bossActionSpeedDescription.text = "Boss attacks as intended";
                else
                    bossActionSpeedDescription.text = "通常ペース";
                break;
            case BossActionSpeeds.Fast:
                if (isEnglish)
                    bossActionSpeedDescription.text = "Boss attacks very often";
                else
                    bossActionSpeedDescription.text = "通常より頻度が高い";
                break;
        }
    }

    /// <summary>
    /// Called remotely
    /// </summary>
    /// <param name="lives"></param>
    /// <param name="spawnRate"></param>
    /// <param name="actionSpeed"></param>
    /// <param name="moveSpeed"></param>
    public void ApplyAllProperties(LiveCounts lives, UFOSpawnRates spawnRate, BossActionSpeeds actionSpeed, BossMoveSpeeds moveSpeed)
    {
        startingLives = lives;
        ufoSpawnRate = spawnRate;
        bossActionSpeed = actionSpeed;
        bossMoveSpeed = moveSpeed;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        if (!initialized)
        {
            initialized = true;
        }
        else if (scene.name.Equals(mainMenuScene))
        {
            Destroy(gameObject);
        }
    }
}