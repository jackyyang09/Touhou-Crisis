﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutSystem : MonoBehaviour, IReloadable
{
    [SerializeField] float maxCharge = 100;
    float currentCharge = 0;
    public float ChargePercentage
    {
        get
        {
            return currentCharge / maxCharge;
        }
    }

    [SerializeField] RailShooterLogic railShooter = null;
    [SerializeField] PlayerBehaviour player = null;
    [SerializeField] ComboPuck combo = null;

    WeaponObject specialWeapon = null;
    [SerializeField] bool weaponReady = false;
    public bool WeaponReady { get { return weaponReady; } }

    public Action OnChargeChanged;

    public void Reinitialize()
    {
        weaponReady = false;
        currentCharge = 0;
        OnChargeChanged?.Invoke();
    }

    // Start is called before the first frame update
    void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);

        specialWeapon = player.Loadout[1];

        Reinitialize();
    }

    private void OnEnable()
    {
        railShooter.OnShoot += OnTriggerPull;
        player.OnRoundExpended += OnBulletFired;
        player.OnEnterCover += OnEnterCover;
    }

    private void OnDisable()
    {
        railShooter.OnShoot -= OnTriggerPull;
        player.OnRoundExpended -= OnBulletFired;
        player.OnEnterCover -= OnEnterCover;
    }

    private void OnTriggerPull(Ray arg1, Vector2 arg2)
    {
        if (weaponReady && player.InCover)
        {
            player.SwapWeapon((int)Mathf.Repeat(player.ActiveWeaponIndex + 1, player.Loadout.Length));
        }
    }

    private void OnBulletFired(bool hit)
    {
        if (weaponReady)
        {
            if (player.ActiveWeapon == specialWeapon)
            {
                currentCharge = 0;
                OnChargeChanged?.Invoke();
            }
        }
        else
        {
            if (!hit) return;

            currentCharge = Mathf.Clamp(currentCharge + combo.Multliplier, 0, maxCharge);

            if (currentCharge >= maxCharge)
            {
                GivePlayerAmmo();
            }

            OnChargeChanged?.Invoke();
        }
    }

    public void GivePlayerAmmo()
    {
        weaponReady = true;
        player.SetAmmo(1, specialWeapon.ammoCapacity);
    }

    private void OnEnterCover()
    {
        if (weaponReady)
        {
            if (player.ActiveWeapon == specialWeapon)
            {
                if (player.CurrentAmmo == 0)
                {
                    player.SwapWeapon(0);
                    weaponReady = false;
                }
            }
        }
    }

    [CommandTerminal.RegisterCommand(Help = "Enables use of secondary weapon", MaxArgCount = 0)]
    static void GiveAmmo(CommandTerminal.CommandArg[] args)
    {
        LoadoutSystem loadout = PlayerManager.Instance.LocalPlayer.GetComponent<LoadoutSystem>();
        loadout.GivePlayerAmmo();
        loadout.OnChargeChanged?.Invoke();
    }
}