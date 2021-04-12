using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Localization;
using TMPro;

public class GameplayModifiersUI : MonoBehaviour
{
    public const string MAX_LIVES_TOKEN = "Tokens/MaxLives";
    public const string UFO_SPAWN_TOKEN = "Tokens/UFOSpawnRate";
    public const string BOSS_MOVE_SPEED_TOKEN = "Tokens/BossMoveSpeed";
    public const string BOSS_ACTION_SPEED_TOKEN = "Tokens/BossActionSpeed";

    [SerializeField] LeanLocalizedTextMeshProUGUI startingLivesDescription = null;
    [SerializeField] LeanLocalizedTextMeshProUGUI ufoSpawnRateDescription = null;
    [SerializeField] TextMeshProUGUI bossMoveSpeedDescription = null;
    [SerializeField] TextMeshProUGUI bossActionSpeedDescription = null;

    private void OnEnable()
    {
        GameplayModifiers.OnStartingLivesChanged += UpdateStartingLivesToken;
        GameplayModifiers.OnUFOSpawnRateChanged += UpdateUFOSpawnRateToken;
        GameplayModifiers.OnBossMoveSpeedChanged += UpdateBossMoveSpeedToken;
        GameplayModifiers.OnBossActionSpeedChanged += UpdateBossActionSpeedToken;
        GameplayModifiers.Instance.ForceRefreshProperties();
    }

    private void OnDisable()
    {
        GameplayModifiers.OnStartingLivesChanged -= UpdateStartingLivesToken;
        GameplayModifiers.OnUFOSpawnRateChanged -= UpdateUFOSpawnRateToken;
        GameplayModifiers.OnBossMoveSpeedChanged -= UpdateBossMoveSpeedToken;
        GameplayModifiers.OnBossActionSpeedChanged -= UpdateBossActionSpeedToken;
    }

    public void CycleStartingLives()
    {
        GameplayModifiers.Instance.SyncCycleStartingLives();
    }

    public void UpdateStartingLivesToken(GameplayModifiers.LiveCounts startingLives)
    {
        bool isEnglish = LeanLocalization.CurrentLanguage.Equals("English");

        switch (startingLives)
        {
            case GameplayModifiers.LiveCounts.One:
            case GameplayModifiers.LiveCounts.Two:
            case GameplayModifiers.LiveCounts.Three:
            case GameplayModifiers.LiveCounts.Four:
            case GameplayModifiers.LiveCounts.Five:
                startingLivesDescription.enabled = true;
                LeanLocalization.SetToken(MAX_LIVES_TOKEN, ((int)startingLives + 1).ToString());
                break;
            case GameplayModifiers.LiveCounts.Infinite:
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
                    startingLivesDescription.GetComponent<TextMeshProUGUI>().text = "プレイヤーが不死身になります";
                }
                break;
        }
    }

    public void CycleUFOSpawnRate()
    {
        GameplayModifiers.Instance.SyncCycleUFOSpawnRate();
    }

    public void UpdateUFOSpawnRateToken(GameplayModifiers.UFOSpawnRates ufoSpawnRate)
    {
        bool isEnglish = LeanLocalization.CurrentLanguage.Equals("English");

        if (isEnglish)
        {
            LeanLocalization.SetToken(UFO_SPAWN_TOKEN, ufoSpawnRate.ToString());
        }
        else
        {
            switch (ufoSpawnRate)
            {
                case GameplayModifiers.UFOSpawnRates.Disabled:
                    LeanLocalization.SetToken(UFO_SPAWN_TOKEN, "オフ");
                    break;
                case GameplayModifiers.UFOSpawnRates.Low:
                    LeanLocalization.SetToken(UFO_SPAWN_TOKEN, "低");
                    break;
                case GameplayModifiers.UFOSpawnRates.Normal:
                    LeanLocalization.SetToken(UFO_SPAWN_TOKEN, "中");
                    break;
                case GameplayModifiers.UFOSpawnRates.High:
                    LeanLocalization.SetToken(UFO_SPAWN_TOKEN, "高");
                    break;
            }
        }

        if (ufoSpawnRate == GameplayModifiers.UFOSpawnRates.Disabled)
        {
            ufoSpawnRateDescription.enabled = false;
            if (isEnglish)
            {
                ufoSpawnRateDescription.GetComponent<TextMeshProUGUI>().text = "Enemy UFOs will not spawn";
            }
            else
            {
                ufoSpawnRateDescription.GetComponent<TextMeshProUGUI>().text = "UFOは発生しない";
            }
        }
        else
        {
            switch (ufoSpawnRate)
            {
                case GameplayModifiers.UFOSpawnRates.Low:
                    if (isEnglish)
                    {
                        ufoSpawnRateDescription.GetComponent<TextMeshProUGUI>().text = "Enemy UFOs will rarely spawn";
                    }
                    else
                    {
                        ufoSpawnRateDescription.GetComponent<TextMeshProUGUI>().text = "珍しく出る";
                    }
                    break;
                case GameplayModifiers.UFOSpawnRates.Normal:
                    if (isEnglish)
                    {
                        ufoSpawnRateDescription.GetComponent<TextMeshProUGUI>().text = "Enemy UFOs will spawn normally";
                    }
                    else
                    {
                        ufoSpawnRateDescription.GetComponent<TextMeshProUGUI>().text = "まあまあ出る";
                    }
                    break;
                case GameplayModifiers.UFOSpawnRates.High:
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

    public void CycleBossMoveSpeed()
    {
        GameplayModifiers.Instance.SyncCycleBossMoveSpeed();
    }

    public void UpdateBossMoveSpeedToken(GameplayModifiers.BossMoveSpeeds bossMoveSpeed)
    {
        bool isEnglish = LeanLocalization.CurrentLanguage.Equals("English");

        if (isEnglish)
        {
            LeanLocalization.SetToken(BOSS_MOVE_SPEED_TOKEN, bossMoveSpeed.ToString());
        }
        else
        {
            switch (bossMoveSpeed)
            {
                case GameplayModifiers.BossMoveSpeeds.Slow:
                    LeanLocalization.SetToken(BOSS_MOVE_SPEED_TOKEN, "低");
                    break;
                case GameplayModifiers.BossMoveSpeeds.Normal:
                    LeanLocalization.SetToken(BOSS_MOVE_SPEED_TOKEN, "中");
                    break;
                case GameplayModifiers.BossMoveSpeeds.Fast:
                    LeanLocalization.SetToken(BOSS_MOVE_SPEED_TOKEN, "高");
                    break;
            }
        }

        switch (bossMoveSpeed)
        {
            case GameplayModifiers.BossMoveSpeeds.Slow:
                if (isEnglish)
                    bossMoveSpeedDescription.text = "Boss moves slower and less often";
                else
                    bossMoveSpeedDescription.text = "通常より移動が遅い・頻度が低い";
                break;
            case GameplayModifiers.BossMoveSpeeds.Normal:
                if (isEnglish)
                    bossMoveSpeedDescription.text = "Boss moves at the intended pace";
                else
                    bossMoveSpeedDescription.text = "通常ペース";
                break;
            case GameplayModifiers.BossMoveSpeeds.Fast:
                if (isEnglish)
                    bossMoveSpeedDescription.text = "Boss moves very fast";
                else
                    bossMoveSpeedDescription.text = "とてつもない速さで動く";
                break;
        }
    }

    public void CycleBossActionSpeed()
    {
        GameplayModifiers.Instance.SyncCycleBossActionSpeed();
    }

    public void UpdateBossActionSpeedToken(GameplayModifiers.BossActionSpeeds bossActionSpeed)
    {
        bool isEnglish = LeanLocalization.CurrentLanguage.Equals("English");

        if (isEnglish)
        {
            LeanLocalization.SetToken(BOSS_ACTION_SPEED_TOKEN, bossActionSpeed.ToString());
        }
        else
        {
            switch (bossActionSpeed)
            {
                case GameplayModifiers.BossActionSpeeds.Slow:
                    LeanLocalization.SetToken(BOSS_ACTION_SPEED_TOKEN, "低");
                    break;
                case GameplayModifiers.BossActionSpeeds.Normal:
                    LeanLocalization.SetToken(BOSS_ACTION_SPEED_TOKEN, "中");
                    break;
                case GameplayModifiers.BossActionSpeeds.Fast:
                    LeanLocalization.SetToken(BOSS_ACTION_SPEED_TOKEN, "高");
                    break;
            }
        }

        switch (bossActionSpeed)
        {
            case GameplayModifiers.BossActionSpeeds.Slow:
                if (isEnglish)
                    bossActionSpeedDescription.text = "Boss attacks less often";
                else
                    bossActionSpeedDescription.text = "通常より頻度が低い";
                break;
            case GameplayModifiers.BossActionSpeeds.Normal:
                if (isEnglish)
                    bossActionSpeedDescription.text = "Boss attacks as intended";
                else
                    bossActionSpeedDescription.text = "通常ペース";
                break;
            case GameplayModifiers.BossActionSpeeds.Fast:
                if (isEnglish)
                    bossActionSpeedDescription.text = "Boss attacks very often";
                else
                    bossActionSpeedDescription.text = "通常より頻度が高い";
                break;
        }
    }

}
