using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] PlayerBehaviour player;
    [SerializeField] ComboPuck puck;
    [SerializeField] ScoreSystem score;
    [SerializeField] Sakuya sakuya;

    [Header("Ammo Count")]
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] OptimizedCanvas[] bulletImages = null;

    [Header("HUD Alerts")]
    [SerializeField] OptimizedCanvas actionImage;
    [SerializeField] OptimizedCanvas reloadImage;
    [SerializeField] Image waitImage;
    [SerializeField] TextMeshProUGUI timeText;

    [Header("Combo System")]
    [SerializeField] Image puckImage;
    [SerializeField] Image puckFill;
    [SerializeField] Color puckFlashColour;
    [SerializeField] TextMeshProUGUI comboText;

    [Header("Damage Effects")]
    [SerializeField] Image slashDamageImage;

    [Header("Score System")]
    [SerializeField] TextMeshProUGUI scoreText;

    [Header("Lives Display")]
    [SerializeField] Image[] livesImages = null;

    [Header("Enemy UI")]
    [SerializeField] Image enemyHealthBar;

    private void Awake()
    {
        if (!player.PhotonView.IsMine)
        {
            Destroy(gameObject);
        }

        sakuya = FindObjectOfType<Sakuya>();
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.SetParent(null, false);
    }

    private void OnEnable()
    {
        player.OnBulletFired += SpawnMuzzleFlash;
        player.OnReload += HideReloadGraphic;
        player.OnReload += ReloadAmmo;
        player.OnTakeDamage += FadeDamageEffect;
        player.OnTakeDamage += ShakePuck;
        player.OnTakeDamage += UpdateLivesDisplay;
        player.OnFireNoAmmo += ShowReloadGraphic;
        player.OnEnterTransit += FlashWaitGraphic;
        player.OnExitTransit += FlashActionGraphic;

        puck.OnPassPuck += PassPuckEffect;
        puck.OnReceivePuck += ReceivePuckEffect;
        puck.OnUpdateMultiplier += UpdateComboMultiplier;

        score.OnScoreChanged += UpdateScore;

        if (sakuya != null)
        {
            sakuya.OnShot += UpdateBossHealth;
            sakuya.OnChangePhase += RefillBossHealth;
        }
    }

    private void OnDisable()
    {
        player.OnBulletFired -= SpawnMuzzleFlash;
        player.OnReload -= HideReloadGraphic;
        player.OnReload -= ReloadAmmo;
        player.OnTakeDamage -= FadeDamageEffect;
        player.OnTakeDamage -= ShakePuck;
        player.OnTakeDamage -= UpdateLivesDisplay;
        player.OnFireNoAmmo -= ShowReloadGraphic;
        player.OnEnterTransit -= FlashWaitGraphic;
        player.OnExitTransit -= FlashActionGraphic;

        puck.OnPassPuck -= PassPuckEffect;
        puck.OnReceivePuck -= ReceivePuckEffect;
        puck.OnUpdateMultiplier -= UpdateComboMultiplier;

        score.OnScoreChanged -= UpdateScore;

        if (sakuya != null)
        {
            sakuya.OnShot -= UpdateBossHealth;
            sakuya.OnChangePhase -= RefillBossHealth;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimeCounter();
        UpdateComboDecay();
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
        var bullet = Instantiate(muzzleFlash, transform).transform as RectTransform;

        bullet.anchorMax = hitPosition;
        bullet.anchorMin = hitPosition;

        UpdateAmmoCount();
        ExpendAmmo();
    }

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
                break;
        }
    }

    void PassPuckEffect()
    {
        puckImage.rectTransform.DORotate(new Vector3(0, 0, -180), 0.5f, RotateMode.LocalAxisAdd);
    }

    void ReceivePuckEffect()
    {
        puckImage.rectTransform.DORotate(new Vector3(0, 0, 180), 0.25f, RotateMode.LocalAxisAdd);
        puckFill.DOColor(puckFlashColour, 0).SetDelay(0.25f);
        puckFill.DOColor(Color.white, 0.5f).SetDelay(0.5f);
    }

    void ShakePuck(DamageType type)
    {
        // Prevent the shakes from overlapping
        puckImage.rectTransform.DOComplete();
        puckImage.rectTransform.DOShakeAnchorPos(1, 50, 50, 80);
    }

    void UpdateComboDecay()
    {
        puckFill.fillAmount = puck.ComboDecayPercentage;
    }

    void UpdateComboMultiplier(float comboCount)
    {
        comboText.text = comboCount.ToString("0.0") + "x";
    }

    void UpdateScore(int newScore)
    {
        scoreText.text = newScore.ToString();
    }

    void UpdateLivesDisplay(DamageType type)
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

    void UpdateBossHealth()
    {
        enemyHealthBar.fillAmount = sakuya.HealthPercentage;
    }

    void RefillBossHealth()
    {
        enemyHealthBar.DOFillAmount(1, 0.5f);
    }
}