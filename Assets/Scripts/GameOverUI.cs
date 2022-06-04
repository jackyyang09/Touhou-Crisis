using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using static Facade;

public class GameOverUI : MonoBehaviour, IReloadable
{
    [SerializeField] OptimizedCanvas gameOverScreen = null;

    [SerializeField] OptimizedCanvas gameOverTitle = null;
    [SerializeField] TextMeshProUGUI gameOverTitleText = null;

    [SerializeField] TextMeshProUGUI timeText = null;
    [SerializeField] OptimizedCanvas timeCanvas = null;

    [SerializeField] OptimizedCanvas scoreCanvas = null;
    [SerializeField] TextMeshProUGUI[] scoreText = null;

    [SerializeField] OptimizedCanvas accuracyCanvas = null;
    [SerializeField] TextMeshProUGUI[] accuracyText = null;
    [SerializeField] TextMeshProUGUI[] accuracyBonusText = null;

    [SerializeField] OptimizedCanvas damageCanvas = null;
    [SerializeField] TextMeshProUGUI[] damageText = null;
    [SerializeField] TextMeshProUGUI[] damagePenaltyText = null;

    [SerializeField] TextMeshProUGUI[] winnerText = null;

    [SerializeField] UnityEngine.UI.Image player1Portrait = null;
    [SerializeField] Sprite[] reimuSprites;
    [SerializeField] UnityEngine.UI.Image player2Portrait = null;
    [SerializeField] Sprite[] marisaSprites;

    [SerializeField] OptimizedCanvas shootToContinue = null;

    [SerializeField] OptimizedCanvas gameOverButtons = null;
    [SerializeField] RematchNotification rematchNotification = null;
    [SerializeField] OptimizedCanvas retryNotif = null;
    [SerializeField] Lean.Localization.LeanLocalizedTextMeshProUGUI retryNotifLocalized = null;
    [SerializeField] GameObject retryButton = null;
    [SerializeField] GameObject retryBlock = null;

    [SerializeField] RailShooterLogic railShooter = null;

    bool offline = false;
    bool gameOverTriggered = false;

    public static System.Action OnGameOver;

    public void Reinitialize()
    {
        StopAllCoroutines();

        gameOverTriggered = false;

        gameOverScreen.Hide();

        gameOverTitle.Hide();
        player1Portrait.enabled = false;
        player2Portrait.enabled = false;

        timeCanvas.Hide();
        accuracyCanvas.Hide();
        damageCanvas.Hide();
        scoreCanvas.Hide();

        retryNotif.Hide();
        retryButton.SetActive(true);
        retryBlock.SetActive(false);
        shootToContinue.Hide();

        gameOverButtons.Hide();
    }

    // Start is called before the first frame update
    void Start()
    {
        Reinitialize();
        SoftSceneReloader.Instance.AddNewReloadable(this);
        offline = Photon.Pun.PhotonNetwork.OfflineMode;
    }

    private void OnEnable()
    {
        GameManager.OnReceiveRematchRequest += OnReceiveRematchRequest;
        GameManager.OnReloadScene += OnReloadScene;
    }

    private void OnDisable()
    {
        GameManager.OnReceiveRematchRequest -= OnReceiveRematchRequest;
        GameManager.OnReloadScene -= OnReloadScene;
    }

    private void OnDestroy()
    {
        if (SoftSceneReloader.Instance != null)
        {
            SoftSceneReloader.Instance.RemoveReloadable(this);
        }
    }

    public void RunGameOverSequence(bool bossDefeated)
    {
        if (gameOverTriggered) return;

        bool steamAchieved = false;
        if (!offline)
        {
            if (SteamManager.Initialized)
            {
#if UNITY_STANDALONE
                if (Steamworks.SteamUserStats.GetAchievement("ACHIEVE_1", out bool unlocked))
                {
                    if (!unlocked)
                    {
                        Steamworks.SteamUserStats.SetAchievement("ACHIEVE_1");
                        steamAchieved = true;
                    }
                }
#endif
            }
        }
        
        if (bossDefeated)
        {
            player1Portrait.sprite = modifiers.HostIsReimu ? reimuSprites[0] : marisaSprites[0];
            player2Portrait.sprite = !modifiers.HostIsReimu ? reimuSprites[0] : marisaSprites[0];
            gameOverTitleText.text = "BOSS CLEAR";

            string achieveText = "ACHIEVE_2";
            if (!modifiers.HostIsReimu && Photon.Pun.PhotonNetwork.IsMasterClient) achieveText = "ACHIEVE_3";
            else if (modifiers.HostIsReimu && !Photon.Pun.PhotonNetwork.IsMasterClient) achieveText = "ACHIEVE_3";
            if (SteamManager.Initialized)
            {
#if UNITY_STANDALONE
                if (Steamworks.SteamUserStats.GetAchievement(achieveText, out bool unlocked))
                {
                    if (!unlocked)
                    {
                        Steamworks.SteamUserStats.SetAchievement(achieveText);
                        steamAchieved = true;
                    }
                }
#endif
            }
        }
        else
        {
            player1Portrait.sprite = modifiers.HostIsReimu ? reimuSprites[1] : marisaSprites[1];
            player2Portrait.sprite = !modifiers.HostIsReimu ? reimuSprites[1] : marisaSprites[1];
            gameOverTitleText.text = "GAME OVER";
        }

        if (offline) StartCoroutine(ShowGameOverScreen1P());
        else StartCoroutine(ShowGameOverScreen2P(bossDefeated));

#if UNITY_STANDALONE
        if (steamAchieved) Steamworks.SteamUserStats.StoreStats();
#endif

        gameOverTriggered = true;

        OnGameOver?.Invoke();
    }

    bool skipResults = false;

    IEnumerator ShowGameOverScreen1P()
    {
        var localPlayer = PlayerManager.Instance.LocalPlayer;

        gameOverScreen.Show();

        yield return new WaitForSecondsRealtime(0.25f);

        gameOverTitle.Show();
        player1Portrait.enabled = true;
        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);

        yield return new WaitForSecondsRealtime(0.8f);

        //UpdateTimeCounter();
        timeCanvas.Show();
        timeText.text = HelperMethods.TimeToString(GameManager.Instance.GameTimeElapsed);
        if (timeText.text.Length > 8)
        {
            timeText.text = timeText.text.Remove(8);
        }
        timeCanvas.Show();
        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);

        railShooter.OnShoot += SkipResults;
        skipResults = false;

        if (!skipResults)
        {
            yield return new WaitForSecondsRealtime(0.15f);
            JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);
        }

        int accuracyBonus = 0;
        if (localPlayer.AccuracyCounter.ShotsFired > 0)
        {
            float accuracy = localPlayer.AccuracyCounter.Accuracy;
            if (accuracy > 0)
            {
                accuracyText[0].text = (accuracy * 100).ToString("#.#") + "%";
            }
            else
            {
                accuracyText[0].text = "0%";
            }
            accuracyBonus = (int)(localPlayer.AccuracyCounter.Accuracy * ScoreSystem.MAX_ACCURACY_BONUS);
        }
        else
        {
            accuracyText[0].text = "-";
        }
        accuracyBonusText[0].text = "+" + accuracyBonus;

        accuracyCanvas.Show();

        if (!skipResults)
        {
            yield return new WaitForSecondsRealtime(0.15f);
            JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);
        }

        int damageTaken = localPlayer.DamageCounter.DamageTaken; 
        damageText[0].text = damageTaken.ToString();
        float damagePenalty = damageTaken * ScoreSystem.DAMAGE_TAKEN_PENALTY;
        if (damagePenalty == 0) damagePenaltyText[0].text = "-0";
        else damagePenaltyText[0].text = ((int)damagePenalty).ToString();
        damageCanvas.Show();

        if (!skipResults)
        {
            yield return new WaitForSecondsRealtime(0.15f);
            JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);
        }

        int myScore = PlayerManager.Instance.LocalPlayer.ScoreSystem.CurrentScore;

        scoreText[0].text = myScore.ToString();

        scoreCanvas.Show();

        float timer = 2;
        while (!skipResults && timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        float startScore = myScore;
        float finalScore = myScore + accuracyBonus + damagePenalty;

        float changeTime = 1;
        timer = 0;
        skipResults = false;

        changeTime = 1;
        timer = 0;

        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.ScoreBeeps);
        while ((timer < changeTime) && !skipResults)
        {
            accuracyBonusText[0].text = "+" + (int)Mathf.Lerp(accuracyBonus, 0, timer / changeTime);
            if (damagePenalty == 0) damagePenaltyText[0].text = "-0";
            else damagePenaltyText[0].text = ((int)Mathf.Lerp(damagePenalty, 0, timer / changeTime)).ToString();
            scoreText[0].text = ((int)Mathf.Lerp(startScore, finalScore, timer / changeTime)).ToString();

            timer += Time.deltaTime;
            yield return null;
        }
        JSAM.AudioManager.StopSoundIfPlaying(TouhouCrisisSounds.ScoreBeeps);

        accuracyBonusText[0].text = "+0";
        damagePenaltyText[0].text = "-0";
        scoreText[0].text = finalScore.ToString();

        railShooter.OnShoot -= SkipResults;
        railShooter.OnShoot += Show;

        yield return new WaitForSecondsRealtime(0.15f);

        shootToContinue.Show();
        shootToContinue.PlayAnimation();
    }

    IEnumerator ShowGameOverScreen2P(bool bossDefeated)
    {
        bool isHost = Photon.Pun.PhotonNetwork.IsMasterClient;
        int hostId = 0;
        int clientId = 1;

        PlayerBehaviour hostPlayer, clientPlayer;
        if (isHost)
        {
            hostPlayer = PlayerManager.Instance.LocalPlayer;
            clientPlayer = PlayerManager.Instance.OtherPlayer.GetComponent<PlayerBehaviour>();
        }
        else
        {
            hostPlayer = PlayerManager.Instance.OtherPlayer.GetComponent<PlayerBehaviour>();
            clientPlayer = PlayerManager.Instance.LocalPlayer;
        }

        gameOverScreen.Show();

        yield return new WaitForSecondsRealtime(0.25f);

        gameOverTitle.Show();
        player1Portrait.enabled = true;
        player2Portrait.enabled = true;
        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);

        railShooter.OnShoot += SkipResults;

        skipResults = false;

        if (!skipResults)
        {
            yield return new WaitForSecondsRealtime(0.8f);
            JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);
        }

        //UpdateTimeCounter();
        timeCanvas.Show();
        timeText.text = HelperMethods.TimeToString(GameManager.Instance.GameTimeElapsed);
        if (timeText.text.Length > 8)
        {
            timeText.text = timeText.text.Remove(8);
        }
        timeCanvas.Show();

        if (!skipResults)
        {
            yield return new WaitForSecondsRealtime(0.15f);
            JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);
        }

        winnerText[0].enabled = false;
        winnerText[1].enabled = false;

        int[] accuracyBonus = new int[2];
        if (hostPlayer.AccuracyCounter.ShotsFired > 0)
        {
            float accuracy = hostPlayer.AccuracyCounter.Accuracy;
            accuracyBonus[hostId] = (int)(accuracy * ScoreSystem.MAX_ACCURACY_BONUS);

            if (accuracy > 0) accuracyText[hostId].text = (accuracy * 100).ToString("#.#") + "%";
            else accuracyText[hostId].text = "0%";
        }
        else
        {
            accuracyText[hostId].text = "-";
        }
        accuracyBonusText[hostId].text = "+" + accuracyBonus[hostId];

        if (clientPlayer.AccuracyCounter.ShotsFired > 0)
        {
            float accuracy = clientPlayer.AccuracyCounter.Accuracy;
            accuracyBonus[clientId] = (int)(accuracy * ScoreSystem.MAX_ACCURACY_BONUS);

            if (accuracy > 0) accuracyText[clientId].text = (accuracy * 100).ToString("#.#") + "%";
            else accuracyText[clientId].text = "0%";
        }
        else
        {
            accuracyText[clientId].text = "-";
        }
        accuracyBonusText[clientId].text = "+" + accuracyBonus[clientId];

        accuracyCanvas.Show();

        if (!skipResults)
        {
            yield return new WaitForSecondsRealtime(0.15f);
            JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);
        }

        float[] damagePenalties = new float[2];
        int damageTaken = hostPlayer.DamageCounter.DamageTaken;
        damageText[hostId].text = damageTaken.ToString();
        damagePenalties[hostId] = damageTaken * ScoreSystem.DAMAGE_TAKEN_PENALTY;
        if (damagePenalties[hostId] == 0) damagePenaltyText[hostId].text = "-0";
        else damagePenaltyText[hostId].text = ((int)damagePenalties[hostId]).ToString();

        damageTaken = clientPlayer.DamageCounter.DamageTaken;
        damageText[clientId].text = damageTaken.ToString();
        damagePenalties[clientId] = damageTaken * ScoreSystem.DAMAGE_TAKEN_PENALTY;
        if (damagePenalties[clientId] == 0) damagePenaltyText[clientId].text = "-0";
        else damagePenaltyText[clientId].text = ((int)damagePenalties[clientId]).ToString();

        damageCanvas.Show();

        if (!skipResults)
        {
            yield return new WaitForSecondsRealtime(0.15f);
            JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);
        }

        float[] scores = new float[2];
        scores[hostId] = hostPlayer.ScoreSystem.CurrentScore;
        scores[clientId] = clientPlayer.ScoreSystem.CurrentScore;

        scoreText[hostId].text = scores[hostId].ToString();
        scoreText[clientId].text = scores[clientId].ToString();

        scoreCanvas.Show();

        float[] beginScores = new float[2];
        beginScores[hostId] = scores[hostId];
        beginScores[clientId] = scores[clientId];

        float[] finalScores = new float[2];
        finalScores[hostId] = scores[hostId] + accuracyBonus[hostId] + damagePenalties[hostId];
        finalScores[clientId] = scores[clientId] + accuracyBonus[clientId] + damagePenalties[clientId];

        float changeTime = 2;
        float timer = 0;

        skipResults = false;

        while ((timer < changeTime) && !skipResults)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        changeTime = 1;
        timer = 0;

        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.ScoreBeeps);
        while ((timer < changeTime) && !skipResults)
        {
            for (int i = 0; i < 2; i++)
            {
                accuracyBonusText[i].text = "+" + (int)Mathf.Lerp(accuracyBonus[i], 0, timer / changeTime);
                if (damagePenalties[i] == 0) damagePenaltyText[i].text = "-0";
                else damagePenaltyText[i].text = ((int)Mathf.Lerp(damagePenalties[i], 0, timer / changeTime)).ToString();
                scoreText[i].text = ((int)Mathf.Lerp(beginScores[hostId], finalScores[i], timer / changeTime)).ToString();
            }
            timer += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < 2; i++)
        {
            accuracyBonusText[i].text = "+0";
            damagePenaltyText[i].text = "-0";
            scoreText[i].text = finalScores[i].ToString();
        }
        JSAM.AudioManager.StopSoundIfPlaying(TouhouCrisisSounds.ScoreBeeps);

        if (finalScores[hostId] > finalScores[clientId]) // Player 1 Wins
        {
            if (bossDefeated)
            {
                player1Portrait.sprite = modifiers.HostIsReimu ? reimuSprites[2] : marisaSprites[2];
            }

            var rect = winnerText[hostId].transform as RectTransform;
            float targetX = rect.anchoredPosition.x;

            rect.DOKill();
            rect.anchoredPosition = new Vector2(2000, rect.anchoredPosition.y);
            winnerText[hostId].enabled = true;
            rect.DOAnchorPosX(0, 0.1f);

            winnerText[clientId].enabled = false;
        }
        else if (finalScores[hostId] < finalScores[clientId]) // Player 2 Wins
        {
            if (bossDefeated)
            {
                player2Portrait.sprite = !modifiers.HostIsReimu ? reimuSprites[2] : marisaSprites[2];
            }

            var rect = winnerText[clientId].transform as RectTransform;
            float targetX = rect.anchoredPosition.x;

            rect.DOKill();
            rect.anchoredPosition = new Vector2(-2000, rect.anchoredPosition.y);
            winnerText[clientId].enabled = true;
            rect.DOAnchorPosX(0, 0.1f);

            winnerText[hostId].enabled = false;
        }

        yield return new WaitForSecondsRealtime(0.15f);

        railShooter.OnShoot -= SkipResults;
        railShooter.OnShoot += Show;

        shootToContinue.Show();
        shootToContinue.PlayAnimation();
    }

    public void SkipResults(Ray arg1, Vector2 arg2) => skipResults = true;

    private void Show(Ray arg1, Vector2 arg2)
    {
        railShooter.OnShoot -= Show;
        gameOverScreen.Hide();
        gameOverButtons.Show();
    }

    public void RequestRematch()
    {
        retryButton.SetActive(false);
        retryBlock.SetActive(true);
        GameManager.Instance.SyncRequestRematch();
    }

    private void OnReceiveRematchRequest()
    {
        if (!retryBlock.activeSelf)
        {
            JSAM.AudioManager.PlaySound(TouhouCrisisSounds.PlayerJoin);
            retryNotifLocalized.UpdateLocalization();
            retryNotif.Show();
            rematchNotification.OnReceiveRematchRequest();
        }
    }

    private void OnReloadScene()
    {
        gameOverButtons.Hide();
    }
}