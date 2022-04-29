using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

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

    [SerializeField] UnityEngine.UI.Image reimuPortrait = null;
    [SerializeField] Sprite[] reimuSprites;
    [SerializeField] UnityEngine.UI.Image marisaPortrait = null;
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

    public static System.Action OnGameOver;

    public void Reinitialize()
    {
        gameOverScreen.Hide();

        gameOverTitle.Hide();
        reimuPortrait.enabled = false;
        marisaPortrait.enabled = false;

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
        if (bossDefeated)
        {
            reimuPortrait.sprite = reimuSprites[0];
            marisaPortrait.sprite = marisaSprites[0];
            if (Lean.Localization.LeanLocalization.CurrentLanguage.Equals("English"))
            {
                gameOverTitleText.text = "BOSS CLEAR";
            }
            else if (Lean.Localization.LeanLocalization.CurrentLanguage.Equals("Japanese"))
            {
                gameOverTitleText.text = "<REPLACE ME>";
            }
        }
        else
        {
            reimuPortrait.sprite = reimuSprites[1];
            marisaPortrait.sprite = marisaSprites[1];
            if (Lean.Localization.LeanLocalization.CurrentLanguage.Equals("English"))
            {
                gameOverTitleText.text = "GAME OVER";
            }
            else if (Lean.Localization.LeanLocalization.CurrentLanguage.Equals("Japanese"))
            {
                gameOverTitleText.text = "ゲームオーバー";
            }
        }

        if (offline) StartCoroutine(ShowGameOverScreen1P());
        else StartCoroutine(ShowGameOverScreen2P(bossDefeated));

        OnGameOver?.Invoke();
    }

    bool skipResults = false;

    IEnumerator ShowGameOverScreen1P()
    {
        var localPlayer = PlayerManager.Instance.LocalPlayer;

        gameOverScreen.Show();

        yield return new WaitForSecondsRealtime(0.25f);

        gameOverTitle.Show();
        reimuPortrait.enabled = true;
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
                accuracyText[0].text = (localPlayer.AccuracyCounter.Accuracy * 100).ToString("#.#") + "%";
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
        damagePenaltyText[0].text = ((int)damagePenalty).ToString();
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

        JSAM.AudioManager.PlaySoundLoop(TouhouCrisisSounds.ScoreBeeps);
        while ((timer < changeTime) && !skipResults)
        {
            if (!JSAM.AudioManager.IsSoundLooping(TouhouCrisisSounds.ScoreBeeps))
            {
                JSAM.AudioManager.PlaySoundLoop(TouhouCrisisSounds.ScoreBeeps);
            }

            accuracyBonusText[0].text = "+" + (int)Mathf.Lerp(accuracyBonus, 0, timer / changeTime);
            damagePenaltyText[0].text = ((int)Mathf.Lerp(damagePenalty, 0, timer / changeTime)).ToString();
            scoreText[0].text = ((int)Mathf.Lerp(startScore, finalScore, timer / changeTime)).ToString();

            timer += Time.deltaTime;
            yield return null;
        }
        JSAM.AudioManager.StopSoundIfLooping(TouhouCrisisSounds.ScoreBeeps);

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
        reimuPortrait.enabled = true;
        marisaPortrait.enabled = true;
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

            if (accuracy > 0)
            {
                accuracyText[hostId].text = (hostPlayer.AccuracyCounter.Accuracy * 100).ToString("#.#") + "%";
            }
            else
            {
                accuracyText[hostId].text = "0";
            }
            accuracyBonusText[hostId].text = "+" + accuracyBonus[hostId];
        }
        else
        {
            accuracyText[hostId].text = "-";
        }

        if (clientPlayer.AccuracyCounter.ShotsFired > 0)
        {
            float accuracy = clientPlayer.AccuracyCounter.Accuracy;
            accuracyBonus[clientId] = (int)(accuracy * ScoreSystem.MAX_ACCURACY_BONUS);

            accuracyText[clientId].text = (accuracy * 100).ToString("#.#") + "%";
            accuracyBonusText[clientId].text = "+" + accuracyBonus[clientId];
        }
        else
        {
            accuracyText[clientId].text = "-";
        }

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
        damagePenaltyText[hostId].text = ((int)damagePenalties[hostId]).ToString();

        damageTaken = clientPlayer.DamageCounter.DamageTaken;
        damageText[clientId].text = damageTaken.ToString();
        damagePenalties[clientId] = damageTaken * ScoreSystem.DAMAGE_TAKEN_PENALTY;
        damagePenaltyText[clientId].text = ((int)damagePenalties[clientId]).ToString();

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

        while ((timer < changeTime) && !skipResults)
        {
            if (!JSAM.AudioManager.IsSoundLooping(TouhouCrisisSounds.ScoreBeeps))
            {
                JSAM.AudioManager.PlaySoundLoop(TouhouCrisisSounds.ScoreBeeps);
            }

            for (int i = 0; i < 2; i++)
            {
                accuracyBonusText[i].text = "+" + (int)Mathf.Lerp(accuracyBonus[i], 0, timer / changeTime);
                damagePenaltyText[i].text = ((int)Mathf.Lerp(damagePenalties[i], 0, timer / changeTime)).ToString();
                scoreText[i].text = ((int)Mathf.Lerp(beginScores[hostId], finalScores[i], timer / changeTime)).ToString();
            }
            timer += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < 2; i++)
        {
            accuracyBonusText[i].enabled = false;
            damagePenaltyText[i].enabled = false;
            scoreText[i].text = finalScores[i].ToString();
        }
        JSAM.AudioManager.StopSoundIfLooping(TouhouCrisisSounds.ScoreBeeps);

        if (finalScores[hostId] > finalScores[clientId]) // Reimu Wins
        {
            if (bossDefeated) reimuPortrait.sprite = reimuSprites[2];

            var rect = winnerText[hostId].transform as RectTransform;
            float targetX = rect.anchoredPosition.x;

            rect.anchoredPosition = new Vector2(2000, rect.anchoredPosition.y);
            winnerText[hostId].enabled = true;
            rect.DOAnchorPosX(0, 0.1f);

            winnerText[clientId].enabled = false;
        }
        else if (finalScores[hostId] < finalScores[clientId]) // Marisa Wins
        {
            if (bossDefeated) marisaPortrait.sprite = marisaSprites[2];

            var rect = winnerText[clientId].transform as RectTransform;
            float targetX = rect.anchoredPosition.x;

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