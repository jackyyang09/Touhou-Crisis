using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadoutSystemUI : MonoBehaviour, IReloadable
{
    [SerializeField] Image icon = null;
    [SerializeField] Image fillImage = null;
    [SerializeField] Image pulseImage = null;
    [SerializeField] Image equippedImage = null;
    [SerializeField] float pulseScale = 1.25f;
    [SerializeField] float pulseTime = 0.5f;
    bool pulsing = false;

    [SerializeField] OptimizedCanvas weaponSwitchCanvas = null;
    [SerializeField] TMPro.TextMeshProUGUI weaponSwitchText = null;

    [SerializeField] PlayerBehaviour player = null;
    [SerializeField] LoadoutSystem loadoutSystem = null;

    private void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);

        if (Lean.Localization.LeanLocalization.CurrentLanguage.Equals("English"))
        {
            weaponSwitchText.text =
                loadoutSystem.SpecialWeapon.name + " ready!\n" +
                "Shoot to switch weapons!";
        }
        else
        {
            weaponSwitchText.text =
                loadoutSystem.SpecialWeapon.name + "TRANSLATION NEEDED";
        }

        weaponSwitchCanvas.Hide();
    }

    void OnDestroy()
    {
        if (SoftSceneReloader.Instance)
        {
            SoftSceneReloader.Instance.RemoveReloadable(this);
        }
    }

    private void OnEnable()
    {
        player.OnEnterCover += ShowWeaponSwitchCanvas;
        player.OnExitCover += HideWeaponSwitchCanvas;
        player.OnRoundExpended += UpdateAmmoUI;
        player.OnSwapWeapon += OnSwapWeapon;
        loadoutSystem.OnChargeChanged += UpdateChargeUI;
    }

    private void OnDisable()
    {
        player.OnEnterCover -= ShowWeaponSwitchCanvas;
        player.OnExitCover -= HideWeaponSwitchCanvas;
        player.OnRoundExpended -= UpdateAmmoUI;
        loadoutSystem.OnChargeChanged -= UpdateChargeUI;
    }

    public void Reinitialize()
    {
        pulseImage.transform.DOKill(true);
        pulseImage.enabled = false;
        pulseImage.transform.localScale = Vector2.one;
        pulsing = false;
        weaponSwitchCanvas.Hide();
    }

    public void ShowWeaponSwitchCanvas()
    {
        if (!loadoutSystem.WeaponReady) return;
        weaponSwitchCanvas.Show();
    }

    public void OnSwapWeapon(WeaponObject obj)
    {
        equippedImage.enabled = !obj.infiniteAmmo;
        weaponSwitchCanvas.Hide();
    }

    public void HideWeaponSwitchCanvas() => weaponSwitchCanvas.Hide();

    private void UpdateChargeUI()
    {
        fillImage.fillAmount = loadoutSystem.ChargePercentage;

        if (loadoutSystem.ChargePercentage >= 1 && !pulsing)
        {
            JSAM.AudioManager.PlaySound(TouhouCrisisSounds.Weapon_Unlock);
            icon.color = Color.white;
            pulseImage.enabled = true;

            pulseImage.transform.DOKill(true);
            pulseImage.transform.localScale = Vector2.one;
            pulseImage.CrossFadeAlpha(1, 0, false);
            pulseImage.CrossFadeAlpha(0, pulseTime, false);

            pulseImage.transform.DOScale(pulseScale, pulseTime).OnStepComplete(() =>
            {
                pulseImage.transform.localScale = Vector2.one;
                pulseImage.CrossFadeAlpha(1, 0, false);
                pulseImage.CrossFadeAlpha(0, pulseTime, false);
            }).SetLoops(-1);
            pulsing = true;
        }
        else if (player.AmmoCount[1] == 0)
        {
            icon.color = Color.black;
        }
    }

    private void UpdateAmmoUI(bool arg1)
    {
        if (loadoutSystem.WeaponReady)
        {
            fillImage.fillAmount = (float)player.AmmoCount[1] / player.Loadout[1].ammoCapacity;
        }

        if (pulsing)
        {
            if (player.AmmoCount[1] < player.Loadout[1].ammoCapacity)
            {
                pulseImage.transform.DOKill(true);
                pulseImage.transform.localScale = Vector2.one;
                pulseImage.enabled = false;
                pulsing = false;
            }
        }
    }
}