using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] PlayerBehaviour player;

    [SerializeField] TextMeshProUGUI ammoText;

    // Start is called before the first frame update
    void Start()
    {
        transform.parent = null;
    }

    private void OnEnable()
    {
        player.OnBulletFired += SpawnMuzzleFlash;
        player.OnReload += UpdateAmmoCount;
    }

    private void OnDisable()
    {
        player.OnBulletFired -= SpawnMuzzleFlash;
        player.OnReload -= UpdateAmmoCount;
    }

    // Update is called once per frame
    void Update()
    {
        
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
    }

    void UpdateAmmoCount()
    {
        ammoText.text = player.CurrentAmmo.ToString();
    }
}