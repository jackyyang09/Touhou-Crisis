using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AmmoUI : MonoBehaviour
{
    [System.Serializable]
    public struct WeaponToBullet
    {
        public WeaponObject weapon;
        public GameObject bulletPrefab;
        public GameObject casingPrefab;
        public float uiSpacing;
        public int bulletsToShow;
        public int casingPoolSize;
    }

    [SerializeField] PlayerBehaviour player = null;

    [SerializeField] float bulletTweenSpeed = 0.15f;
    [SerializeField] float casingLifetime = 1;

    [SerializeField] Vector3 minForce = Vector3.zero;
    [SerializeField] Vector3 maxForce = Vector3.zero;
    [SerializeField] Vector3 minRotation = Vector3.zero;
    [SerializeField] Vector3 maxRotation = Vector3.zero;

    [SerializeField] Transform weaponHolder = null;
    [SerializeField] List<WeaponToBullet> armory = null;

    Dictionary<WeaponObject, WeaponToBullet> armoryDictionary = new Dictionary<WeaponObject, WeaponToBullet>();
    Dictionary<WeaponObject, GameObject> bulletHolders = new Dictionary<WeaponObject, GameObject>();
    Dictionary<WeaponObject, List<GameObject>> bullets = new Dictionary<WeaponObject, List<GameObject>>();
    Dictionary<WeaponObject, List<GameObject>> casings = new Dictionary<WeaponObject, List<GameObject>>();

    void Awake()
    {
        for (int i = 0; i < armory.Count; i++)
        {
            armoryDictionary[armory[i].weapon] = armory[i];

            //Instantiate(new GameObject(armory[i].weapon.name), weaponHolder).transform.SetSiblingIndex(i);

            bulletHolders[armory[i].weapon] = new GameObject(armory[i].weapon.name);
            bulletHolders[armory[i].weapon].transform.SetParent(weaponHolder);
            bulletHolders[armory[i].weapon].transform.localPosition = Vector3.zero;
            bulletHolders[armory[i].weapon].transform.localScale = Vector3.one;
            bulletHolders[armory[i].weapon].transform.SetSiblingIndex(i);

            bullets[armory[i].weapon] = new List<GameObject>();
            for (int j = 0; j < armory[i].bulletsToShow; j++)
            {
                bullets[armory[i].weapon].Add(Instantiate(armory[i].bulletPrefab, bulletHolders[armory[i].weapon].transform));
                bullets[armory[i].weapon][j].SetActive(false);
            }

            casings[armory[i].weapon] = new List<GameObject>();
            for (int j = 0; j < armory[i].casingPoolSize; j++)
            {
                casings[armory[i].weapon].Add(Instantiate(armory[i].casingPrefab, weaponHolder));
                casings[armory[i].weapon][j].SetActive(false);
            }
        }

        // Perform OnReload but tween time is 0 seconds
        var ammo = bullets[player.ActiveWeapon];
        var originBullet = ammo[0].transform;
        for (int i = 0; i < ammo.Count; i++)
        {
            ammo[i].transform.DOKill();
            ammo[i].transform.localPosition = Vector3.zero;
            ammo[i].transform.DOLocalMove(originBullet.localPosition + originBullet.right * armoryDictionary[player.ActiveWeapon].uiSpacing * i, bulletTweenSpeed);
            ammo[i].SetActive(true);
        }
    }

    private void OnEnable()
    {
        player.OnRoundExpended += ExpendAmmo;
        player.OnReload += OnReload;
        player.OnAmmoChanged += OnAmmoChanged;
        player.OnSwapWeapon += OnSwapWeapon;
    }

    private void OnDisable()
    {
        player.OnRoundExpended -= ExpendAmmo;
        player.OnReload -= OnReload;
        player.OnAmmoChanged -= OnAmmoChanged;
        player.OnSwapWeapon -= OnSwapWeapon;
    }

    private void ExpendAmmo(bool arg1)
    {
        var ammo = bullets[player.ActiveWeapon];

        int indexToEject = Mathf.Clamp(player.CurrentAmmo, 0, ammo.Count - 1);

        ammo[indexToEject].transform.DOComplete();

        var originBullet = ammo[0].transform;

        if (player.CurrentAmmo <= player.ActiveWeapon.ammoCapacity)
        {
            ammo[indexToEject].SetActive(false);
        }
        else
        {
            for (int i = 0; i < ammo.Count; i++)
            {
                ammo[i].transform.DOComplete();
            }

            // Shift all bullets except the first one back
            for (int i = ammo.Count - 1; i > 0; i--)
            {
                ammo[i].transform.position = ammo[i - 1].transform.position;
            }

            // First bullet is shifted back
            originBullet.localPosition -= originBullet.right * armoryDictionary[player.ActiveWeapon].uiSpacing;

            // Last bullet is shifted forward
            ammo[ammo.Count - 1].transform.DOLocalMove(originBullet.localPosition + originBullet.right * armoryDictionary[player.ActiveWeapon].uiSpacing * ammo.Count, bulletTweenSpeed);

            // Shift all bullets except the last forward
            for (int i = 0; i < ammo.Count - 1; i++)
            {
                ammo[i].transform.DOMove(ammo[i + 1].transform.position, bulletTweenSpeed);
            }
        }

        var casingList = casings[player.ActiveWeapon];
        for (int i = 0; i < casingList.Count; i++)
        {
            if (!casingList[i].activeSelf)
            {
                casingList[i].transform.position = ammo[indexToEject].transform.position;
                StartCoroutine(DisableObjectDelayed(casingList[i].gameObject, casingLifetime));
                casingList[i].transform.rotation = Quaternion.identity;
                casingList[i].SetActive(true);

                var rb = casingList[i].GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                float randomValue = UnityEngine.Random.value;
                rb.AddForce(Vector3.Lerp(minForce, maxForce, randomValue), ForceMode.Impulse);
                casingList[i].transform.DORotate(Vector3.Lerp(minRotation, maxRotation, randomValue), 1, RotateMode.WorldAxisAdd);
                break;
            }
        }
    }

    IEnumerator DisableObjectDelayed(GameObject go, float time)
    {
        yield return new WaitForSeconds(time);
        go.SetActive(false);
    }

    private void OnReload()
    {
        var ammo = bullets[player.ActiveWeapon];
        var originBullet = ammo[0].transform;
        int ammoCount = player.ActiveWeapon.infiniteAmmo ? ammo.Count : Mathf.Clamp(player.CurrentAmmo, 0, player.ActiveWeapon.ammoCapacity);

        for (int i = 0; i < ammo.Count; i++)
        {
            ammo[i].transform.DOKill();
            ammo[i].transform.localPosition = Vector3.zero;
            ammo[i].transform.DOLocalMove(originBullet.localPosition + originBullet.right * armoryDictionary[player.ActiveWeapon].uiSpacing * i, bulletTweenSpeed);
            ammo[i].SetActive(true);
        }

        for (int i = ammoCount; i < ammo.Count; i++)
        {
            ammo[i].SetActive(false);
        }
    }

    private void OnAmmoChanged()
    {
        if (player.ActiveWeapon.infiniteAmmo) return;

        var ammo = bullets[player.ActiveWeapon];
        var originBullet = ammo[0].transform;
        int ammoCount = Mathf.Clamp(player.CurrentAmmo, 0, player.ActiveWeapon.ammoCapacity);

        for (int i = 0; i < ammo.Count; i++)
        {
            ammo[i].transform.DOKill();
            ammo[i].transform.localPosition = originBullet.localPosition + originBullet.right * armoryDictionary[player.ActiveWeapon].uiSpacing * i;
            ammo[i].SetActive(true);
        }

        for (int i = ammoCount; i < ammo.Count; i++)
        {
            ammo[i].SetActive(false);
        }
    }

    private void OnSwapWeapon(WeaponObject newWeapon)
    {
        for (int i = 0; i < player.Loadout.Length; i++)
        {
            bulletHolders[player.Loadout[i]].gameObject.SetActive(i == player.ActiveWeaponIndex);
        }
        OnReload();
    }
}