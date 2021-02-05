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
    public FireType weaponType;
    public int bulletDamage = 1;
    public int ammoCapacity = 30;
    public float fireRate = 10;

    public GameObject hitFlashPrefab;
    public GameObject missFlashPrefab;
}
