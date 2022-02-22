using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using System;

public class PlayerUIManager : BasicSingleton<PlayerUIManager>, IReloadable
{
    [SerializeField] Camera screenSpaceCamera = null;
    [SerializeField] Canvas overlayCanvas = null;

    [Header("Object References")]
    [SerializeField] PlayerBehaviour player = null;
    [SerializeField] ScoreSystem score = null;
    [SerializeField] Sakuya sakuya = null;
    [SerializeField] RailShooterLogic railShooterLogic = null;
    [SerializeField] RailShooterEffects railShooterEffects = null;
    public RailShooterEffects ShooterEffects { get { return railShooterEffects; } }
    [SerializeField] GameObject[] personalObjects = null;
    [SerializeField] OptimizedCanvas pauseMenu = null;

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
    [SerializeField] Image baseDamageImage = null;
    [SerializeField] Image slashDamageImage = null;
    [SerializeField] Image bulletDamageImage = null;
    [SerializeField] Image remoteDamageImage = null;

    [Header("Score System")]
    [SerializeField] TextMeshProUGUI scoreText = null;

    [Header("Lives Display")]
    [SerializeField] Image[] livesImages = null;
    [SerializeField] Image infiniteLifeImage = null;

    [Header("Enemy UI")]
    [SerializeField] Image enemyHealthBar = null;
    [SerializeField] Image[] enemySpellcards = null;

    [Header("Game Over UI")]
    [SerializeField] GameOverUI gameOverUI1P;
    [SerializeField] GameOverUI gameOverUI2P;

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

    public void Reinitialize()
    {
        if (Photon.Pun.PhotonNetwork.IsMasterClient)
        {
            player2Puck.enabled = false;
        }
        else
        {
            player1Puck.enabled = false;
            remoteDamageImage.transform.localScale = new Vector3(-1, 1, 1);
        }

        cachedTime = 0;

        // Initialize lives
        UpdateLivesDisplay(DamageType.Bullet);

        // Hide game over items
        railShooterEffects.InMenu = false;
        railShooterEffects.PlaySound = false;
        railShooterEffects.SpawnMuzzleFlash = false;

        if (pauseMenu.IsVisible)
        {
            pauseMenu.Hide();
            pauseMenu.gameObject.SetActive(true);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);

        transform.SetParent(null, false);

        var cameraData = Camera.main.GetUniversalAdditionalCameraData();
        cameraData.cameraStack.Add(screenSpaceCamera);

        Reinitialize();
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

        player.OnShotFired += SpawnMuzzleFlash;
        player.OnReload += HideReloadGraphic;
        player.OnReload += UpdateAmmoCount;
        player.OnTakeDamage += FadeDamageEffect;
        player.OnTakeDamage += UpdateLivesDisplay;
        player.OnTakeDamageRemote += FadeDamageRemote;
        player.OnFireNoAmmo += ShowReloadGraphic;
        player.OnSwapWeapon += OnSwapWeapon;
        player.OnAmmoChanged += UpdateAmmoCount;
        player.OnEnterTransit += FlashWaitGraphic;
        player.OnExitTransit += FlashActionGraphic;

        score.OnScoreChanged += UpdateScore;

        if (sakuya != null)
        {
            sakuya.OnShot += UpdateBossHealth;
            sakuya.OnChangePhase += RefillBossHealth;
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

        player.OnShotFired -= SpawnMuzzleFlash;
        player.OnReload -= HideReloadGraphic;
        player.OnReload -= UpdateAmmoCount;
        player.OnTakeDamage -= FadeDamageEffect;
        player.OnTakeDamage -= UpdateLivesDisplay;
        player.OnTakeDamageRemote -= FadeDamageRemote;
        player.OnFireNoAmmo -= ShowReloadGraphic;
        player.OnSwapWeapon -= OnSwapWeapon;
        player.OnAmmoChanged -= UpdateAmmoCount;
        player.OnEnterTransit -= FlashWaitGraphic;
        player.OnExitTransit -= FlashActionGraphic;

        score.OnScoreChanged -= UpdateScore;

        if (sakuya != null)
        {
            sakuya.OnShot -= UpdateBossHealth;
            sakuya.OnChangePhase -= RefillBossHealth;
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
        if (!player.InCover || coverTutorial.IsVisible || !player.CanPlay || pauseMenu.IsVisible) return;

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

    /// <summary>
    /// Spawn a special muzzle flash depending on the weapon fired
    /// </summary>
    /// <param name="missed"></param>
    /// <param name="hitPosition"></param>
    public void SpawnMuzzleFlash(bool missed, Vector2 hitPosition)
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

        bullet.position = hitPosition;
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
    }
    #endregion

    void UpdateAmmoCount()
    {
        ammoText.text = player.CurrentAmmo.ToString();
    }

    void OnSwapWeapon(WeaponObject newWeapon)
    {
        if (player.CurrentAmmo > 0) HideReloadGraphic();
        UpdateAmmoCount();
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
        baseDamageImage.DOColor(Color.white, 0);
        baseDamageImage.DOColor(Color.clear, 0.5f).SetDelay(1);
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

    void FadeDamageRemote()
    {
        UpdateLivesDisplay(new DamageType());
        remoteDamageImage.DOColor(Color.white, 0);
        remoteDamageImage.DOColor(Color.clear, 0.5f).SetDelay(1);
        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.PlayerHurtRemote);
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
            enemySpellcards[Mathf.Clamp(enemySpellcards.Length - sakuya.CurrentPhase, 0, enemySpellcards.Length - 1)].enabled = false;
        }
    }

    void RefillBossHealth(int currentPhase)
    {
        enemyHealthBar.DOFillAmount(1, 0.5f);
    }

    void GameOverSequence()
    {
        PlayerManager.Instance.LocalPlayer.RemovePlayerControl();

        railShooterEffects.InMenu = true;
        railShooterEffects.PlaySound = true;
        railShooterEffects.SpawnMuzzleFlash = true;

        //pauseMenu.gameObject.SetActive(false);
        pauseMenu.Hide();
    }

    public void WinSequence()
    {
        GameOverSequence();

        JSAM.AudioManager.PlayMusic(TouhouCrisisMusic.GameOverWin);

        if (Photon.Pun.PhotonNetwork.OfflineMode) gameOverUI1P.RunGameOverSequence(true);
        else gameOverUI2P.RunGameOverSequence(true);
    }

    public void LoseSequence()
    {
        GameOverSequence();

        JSAM.AudioManager.PlayMusic(TouhouCrisisMusic.GameOverLose);

        if (Photon.Pun.PhotonNetwork.OfflineMode) gameOverUI1P.RunGameOverSequence(false);
        else gameOverUI2P.RunGameOverSequence(false);
    }

    Coroutine delayRoutine = null;
    public void ReloadLevel()
    {
        GameManager.Instance.ReloadScene();
    }

    public void ReturnToTitle()
    {
        if (delayRoutine != null) return;

        delayRoutine = StartCoroutine(DelayedReturnToTitle());
    }

    IEnumerator DelayedReturnToTitle()
    {
        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.MenuButton);

        yield return StartCoroutine(loadingScreen.ShowRoutine());

        GameManager.Instance.LeaveRoom();
    }
}