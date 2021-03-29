using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using System;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] Camera screenSpaceCamera = null;
    [SerializeField] Canvas overlayCanvas = null;

    [Header("Object References")]
    [SerializeField] PlayerBehaviour player = null;
    [SerializeField] ScoreSystem score = null;
    [SerializeField] Sakuya sakuya = null;
    [SerializeField] RailShooterLogic railShooterLogic = null;
    [SerializeField] RailShooterEffects railShooterEffects = null;
    [SerializeField] GameObject[] personalObjects = null;

    [Header("Ammo Count")]
    [SerializeField] TextMeshProUGUI ammoText = null;
    [SerializeField] OptimizedCanvas[] bulletImages = null;

    [Header("HUD Alerts")]
    [SerializeField] OptimizedCanvas actionImage = null;
    [SerializeField] OptimizedCanvas reloadImage = null;
    [SerializeField] Image waitImage = null;
    [SerializeField] TextMeshProUGUI timeText = null;
    [SerializeField] OptimizedCanvas coverTutorial = null;
    [SerializeField] TextMeshProUGUI coverTutorialText = null;

    [SerializeField] int shotsBeforeTutorial = 2;
    int shotsFiredInCover = 0;

    [Header("Combo System")]
    [SerializeField] PuckUI player1Puck = null;
    [SerializeField] PuckUI player2Puck = null;

    [Header("Damage Effects")]
    [SerializeField] Image slashDamageImage = null;
    [SerializeField] Image bulletDamageImage = null;

    [Header("Score System")]
    [SerializeField] TextMeshProUGUI scoreText = null;

    [Header("Lives Display")]
    [SerializeField] Image[] livesImages = null;
    [SerializeField] Image infiniteLifeImage = null;

    [Header("Enemy UI")]
    [SerializeField] Image enemyHealthBar = null;
    [SerializeField] Image[] enemySpellcards = null;

    [Header("Game Over")]
    [SerializeField] OptimizedCanvas gameOverScreen = null;
    [SerializeField] TextMeshProUGUI gameOverText = null;
    [SerializeField] TextMeshProUGUI gameOverTimeText = null;
    [SerializeField] OptimizedCanvas gameOverTimeCanvas = null;
    [SerializeField] OptimizedCanvas p1ScoreCanvas = null;
    [SerializeField] TextMeshProUGUI p1ScoreText = null;
    [SerializeField] OptimizedCanvas p2ScoreCanvas = null;
    [SerializeField] TextMeshProUGUI p2ScoreText = null;
    [SerializeField] OptimizedCanvas gameOverButtonCanvas = null;
    [SerializeField] Button retryButton = null;
    [SerializeField] Canvas reimuPortrait = null;
    [SerializeField] Canvas marisaPortrait = null;

    LoadingScreen loadingScreen = null;

    private void Awake()
    {
        if (!player.PhotonView.IsMine)
        {
            for (int i = 0; i < personalObjects.Length; i++)
            {
                Destroy(personalObjects[i]);
            }
            Destroy(gameObject);
        }

        sakuya = FindObjectOfType<Sakuya>();
        loadingScreen = FindObjectOfType<LoadingScreen>();
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.SetParent(null, false);

        var cameraData = Camera.main.GetUniversalAdditionalCameraData();
        cameraData.cameraStack.Add(screenSpaceCamera);

        if (Photon.Pun.PhotonNetwork.IsMasterClient)
        {
            player2Puck.enabled = false;
        }
        else
        {
            player1Puck.enabled = false;
        }

        // Initialize lives
        UpdateLivesDisplay(DamageType.Bullet);
    }

    private void OnEnable()
    {
        if (coverTutorial != null)
        {
            railShooterLogic.OnShoot += ShowCoverTutorial;
            player.OnExitCover += HideCoverTutorial;

            KeyCode newCoverKey = (KeyCode)PlayerPrefs.GetInt(PauseMenu.CoverInputKey);
            if (Lean.Localization.LeanLocalization.CurrentLanguage.Equals("English"))
            {
                coverTutorialText.text =
                "PRESS THE PEDAL <" + newCoverKey.ToString() + ">\n" +
                "TO RETURN FIRE!";
            }
            else
            {
                coverTutorialText.text =
                "ペダル <" + newCoverKey.ToString() + ">\n" +
                "を踏んで撃ち返せ！";
            }
        }
        
        player.OnBulletFired += SpawnMuzzleFlash;
        player.OnReload += HideReloadGraphic;
        player.OnReload += UpdateAmmoCount;
        player.OnTakeDamage += FadeDamageEffect;
        player.OnTakeDamage += UpdateLivesDisplay;
        player.OnFireNoAmmo += ShowReloadGraphic;
        player.OnEnterTransit += FlashWaitGraphic;
        player.OnExitTransit += FlashActionGraphic;
        player.OnPlayerDeath += LoseSequence;

        score.OnScoreChanged += UpdateScore;

        if (sakuya != null)
        {
            sakuya.OnShot += UpdateBossHealth;
            sakuya.OnChangePhase += RefillBossHealth;
            sakuya.OnBossDefeat += WinSequence;
            sakuya.OnBossDefeat += HideCoverTutorial;
        }
    }

    private void OnDisable()
    {
        if (coverTutorial != null)
        {
            railShooterLogic.OnShoot -= ShowCoverTutorial;
            player.OnExitCover -= HideCoverTutorial;
        }

        player.OnBulletFired -= SpawnMuzzleFlash;
        player.OnReload -= HideReloadGraphic;
        player.OnReload -= UpdateAmmoCount;
        player.OnTakeDamage -= FadeDamageEffect;
        player.OnTakeDamage -= UpdateLivesDisplay;
        player.OnFireNoAmmo -= ShowReloadGraphic;
        player.OnEnterTransit -= FlashWaitGraphic;
        player.OnExitTransit -= FlashActionGraphic;
        player.OnPlayerDeath -= LoseSequence;

        score.OnScoreChanged -= UpdateScore;

        if (sakuya != null)
        {
            sakuya.OnShot -= UpdateBossHealth;
            sakuya.OnChangePhase -= RefillBossHealth;
            sakuya.OnBossDefeat -= WinSequence;
            sakuya.OnBossDefeat -= HideCoverTutorial;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimeCounter();
    }

    private void ShowCoverTutorial(Ray arg1, Vector2 arg2)
    {
        if (!player.InCover || coverTutorial.IsVisible || !player.CanPlay) return;

        shotsFiredInCover++;
        if (shotsFiredInCover >= shotsBeforeTutorial)
        {
            coverTutorial.Show();
        }
    }

    void HideCoverTutorial()
    {
        shotsFiredInCover = 0;
        if (coverTutorial.IsVisible)
        {
            coverTutorial.Hide();
        }
    }

    void SpawnMuzzleFlash(bool missed, Vector2 hitPosition)
    {
        // Get muzzle flash prefab
        GameObject muzzleFlash = gameObject; // Temp allocation
        if (missed)
        {
            muzzleFlash = player.ActiveWeapon.missFlashPrefab;
        }
        else
        {
            muzzleFlash = player.ActiveWeapon.hitFlashPrefab;
        }

        //Spawn bullet on the canvas
        var bullet = Instantiate(muzzleFlash, overlayCanvas.transform).transform as RectTransform;

#if UNITY_ANDROID && !UNITY_EDITOR
        bullet.position = Input.touches[Input.touchCount - 1].position;
#else
        bullet.position = Input.mousePosition;
#endif

        UpdateAmmoCount();
        //ExpendAmmo();
    }

    #region Deprecated Ammo Code
    void ExpendAmmo()
    {
        bulletImages[player.CurrentAmmo].Hide();
    }

    void ReloadAmmo()
    {
        RectTransform parentRect = bulletImages[0].transform.parent as RectTransform;
        parentRect.DOComplete();
        parentRect.DOAnchorPosY(parentRect.anchoredPosition.y, 0.2f);
        parentRect.anchoredPosition -= new Vector2(0, 150);
        for (int i = 0; i < bulletImages.Length; i++)
        {
            bulletImages[i].Show();
        }
        UpdateAmmoCount();
    }
    #endregion

    void UpdateAmmoCount()
    {
        ammoText.text = player.CurrentAmmo.ToString();
    }

    void ShowReloadGraphic() => reloadImage.Show();

    void HideReloadGraphic() => reloadImage.Hide();

    void FlashActionGraphic()
    {
        StopFlashWaitGraphic();
        actionImage.Show();
        Invoke("HideActionGraphic", 0.5f);
    }
    void HideActionGraphic() => actionImage.Hide();

    void FlashWaitGraphic()
    {
        StartCoroutine("FlashWaitRoutine");
    }

    IEnumerator FlashWaitRoutine()
    {
        waitImage.enabled = true;
        while (waitImage.enabled)
        {
            waitImage.DOFade(1, 0);
            waitImage.DOFade(0, 0.25f).SetDelay(0.5f);
            yield return new WaitForSeconds(0.75f);
        }
    }

    void StopFlashWaitGraphic() => waitImage.enabled = false;

    float cachedTime = 0;
    void UpdateTimeCounter()
    {
        if (cachedTime > GameManager.Instance.GameTimeElapsed) return;

        cachedTime = GameManager.Instance.GameTimeElapsed;
        timeText.text = HelperMethods.TimeToString(cachedTime);
        if (timeText.text.Length > 8)
        {
            timeText.text = timeText.text.Remove(8);
        }
    }

    void FadeDamageEffect(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Slash:
                slashDamageImage.DOColor(Color.white, 0);
                slashDamageImage.DOColor(Color.clear, 0.5f).SetDelay(1);
                break;
            case DamageType.Bullet:
                bulletDamageImage.DOColor(Color.white, 0);
                bulletDamageImage.DOColor(Color.clear, 0.5f).SetDelay(1);
                break;
        }
    }

    void UpdateScore(int newScore)
    {
        scoreText.text = newScore.ToString();
    }

    void UpdateLivesDisplay(DamageType type)
    {
        if (player.BuddhaMode)
        {
            for (int i = 0; i < livesImages.Length; i++)
            {
                livesImages[i].enabled = false;
            }
            infiniteLifeImage.enabled = true;
        }
        else
        {
            int i = 0;
            for (; i < player.CurrentLives; i++)
            {
                livesImages[i].enabled = true;
            }
            for (; i < livesImages.Length; i++)
            {
                livesImages[i].enabled = false;
            }
        }
    }

    void UpdateBossHealth()
    {
        enemyHealthBar.fillAmount = sakuya.HealthPercentage;

        if (enemyHealthBar.fillAmount <= 0)
        {
            enemySpellcards[enemySpellcards.Length - sakuya.CurrentPhase].enabled = false;
        }
    }

    void RefillBossHealth(int currentPhase)
    {
        enemyHealthBar.DOFillAmount(1, 0.5f);
    }

    void WinSequence()
    {
        PlayerManager.Instance.LocalPlayer.RemovePlayerControl();
        JSAM.AudioManager.PlayMusic(TouhouCrisisMusic.GameOverWin);
        gameOverText.text = "BOSS CLEAR";
        StartCoroutine(ShowGameOverScreen());
    }

    void LoseSequence()
    {
        PlayerManager.Instance.LocalPlayer.RemovePlayerControl();
        JSAM.AudioManager.PlayMusic(TouhouCrisisMusic.GameOverLose);
        gameOverText.text = "GAME OVER";
        StartCoroutine(ShowGameOverScreen());
    }

    IEnumerator ShowGameOverScreen()
    {
        bool isHost = Photon.Pun.PhotonNetwork.IsMasterClient;
        
        var otherPlayer = PlayerManager.Instance.OtherPlayer;

        railShooterEffects.SetInMenu(true);
        railShooterEffects.playSound = true;
        railShooterEffects.spawnMuzzleFlash = true;

        if (isHost)
        {
            p1ScoreText.text = scoreText.text;
            reimuPortrait.enabled = true;
            marisaPortrait.enabled = false;
        }
        else
        {
            p1ScoreText.text = otherPlayer.GetComponent<ScoreSystem>().CurrentScore.ToString();
            p2ScoreText.text = scoreText.text;
            reimuPortrait.enabled = false;
            marisaPortrait.enabled = true;
            UpdateTimeCounter();
        }

        if (otherPlayer != null)
        {
            if (isHost)
            {
                p2ScoreText.text = otherPlayer.GetComponent<ScoreSystem>().CurrentScore.ToString();
            }
            else
            {
                p1ScoreText.text = otherPlayer.GetComponent<ScoreSystem>().CurrentScore.ToString();
            }
        }

        gameOverScreen.Show();

        yield return new WaitForSeconds(0.25f);

        gameOverText.enabled = true;
        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);

        yield return new WaitForSeconds(0.8f);

        UpdateTimeCounter();
        cachedTime = GameManager.Instance.GameTimeElapsed;
        gameOverTimeText.text = HelperMethods.TimeToString(cachedTime);
        if (gameOverTimeText.text.Length > 8)
        {
            gameOverTimeText.text = gameOverTimeText.text.Remove(8);
        }
        gameOverTimeCanvas.Show();
        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);

        yield return new WaitForSeconds(0.15f);

        p1ScoreCanvas.Show();
        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);

        yield return new WaitForSeconds(0.15f);

        if (!isHost || otherPlayer != null)
        {
            p2ScoreCanvas.Show();
            JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);
            yield return new WaitForSeconds(0.15f);
        }

        retryButton.interactable = isHost;
        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);
        gameOverButtonCanvas.Show();
    }

    Coroutine delayRoutine = null;
    System.Action delayedFunction = null;
    public void ReloadLevel()
    {
        if (delayRoutine != null) return;

        delayedFunction += GameManager.Instance.ReloadScene;
        delayRoutine = StartCoroutine(DelayedTransition());
    }

    public void ReturnToTitle()
    {
        if (delayRoutine != null) return;

        delayedFunction += GameManager.Instance.LeaveRoom;
        delayRoutine = StartCoroutine(DelayedTransition());
    }

    IEnumerator DelayedTransition()
    {
        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.MenuButton);

        yield return StartCoroutine(loadingScreen.ShowRoutine());

        delayedFunction?.Invoke();
        delayedFunction = null;
    }
}