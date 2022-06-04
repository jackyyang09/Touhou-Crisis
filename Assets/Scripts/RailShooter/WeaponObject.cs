using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireType
{
    SemiAuto,
    FullAuto
}

[CreateAssetMenu(fileName = "New Weapon", menuName = "ScriptableObjects/New Weapon", order = 1)]
public class WeaponObject : ScriptableObject
{
    [Header("Weapon Stats")]
    public FireType weaponType;
    public int bulletDamage = 1;
    public int ammoCapacity = 30;
    public bool infiniteAmmo = false;

    [Header("Full Auto-Specific Properties")]
    public float fireDelay = 0.1f;

    [Header("Shell-Specific Properties")]
    public int pellets = 0;
    /// <summary>
    /// Only applies if number of pellets is greater than 0. 
    /// Measured in pixels
    /// </summary>
    public float bulletSpread = 0;

    [Header("Object References")]
    public GameObject hitFlashPrefab = null;
    public GameObject missFlashPrefab = null;
    public JSAM.JSAMSoundFileObject fireSound = null;
    public JSAM.JSAMSoundFileObject casingSound = null;
}
