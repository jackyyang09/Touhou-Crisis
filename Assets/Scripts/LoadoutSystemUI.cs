using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutSystemUI : MonoBehaviour
{
    [SerializeField] Image fillImage = null;
    [SerializeField] PlayerBehaviour player = null;
    [SerializeField] LoadoutSystem loadoutSystem = null;

    private void OnEnable()
    {
        player.OnRoundExpended += UpdateAmmoUI;
        loadoutSystem.OnChargeChanged += UpdateChargeUI;
    }

    private void OnDisable()
    {
        player.OnRoundExpended -= UpdateAmmoUI;
        loadoutSystem.OnChargeChanged -= UpdateChargeUI;
    }

    private void UpdateChargeUI()
    {
        fillImage.fillAmount = loadoutSystem.ChargePercentage;
    }

    private void UpdateAmmoUI(bool arg1)
    {
        if (loadoutSystem.WeaponReady)
        {
            fillImage.fillAmount = (float)player.AmmoCount[1] / player.Loadout[1].ammoCapacity;
        }
    }
}
