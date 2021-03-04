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
        public int casingPoolSize;
    }

    [SerializeField] PlayerBehaviour player = null;

    [SerializeField] float bulletTweenSpeed = 0.15f;
    [SerializeField] float casingLifetime = 1;

    [SerializeField] Vector3 minForce = Vector3.zero;
    [SerializeField] Vector3 maxForce = Vector3.zero;
    [SerializeField] Vector3 minRotation = Vector3.zero;
    [SerializeField] Vector3 maxRotation = Vector3.zero;

    [SerializeField] List<WeaponToBullet> armory = null;
    Dictionary<WeaponObject, WeaponToBullet> armoryDictionary = new Dictionary<WeaponObject, WeaponToBullet>();
    [SerializeField] Transform bulletHolder = null;
    Dictionary<WeaponObject, List<GameObject>> bullets = new Dictionary<WeaponObject, List<GameObject>>();
    Dictionary<WeaponObject, List<GameObject>> casings = new Dictionary<WeaponObject, List<GameObject>>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < armory.Count; i++)
        {
            armoryDictionary[armory[i].weapon] = armory[i];

            Instantiate(new GameObject(armory[i].weapon.name), bulletHolder);
            bullets[armory[i].weapon] = new List<GameObject>();
            for (int j = 0; j < armory[i].weapon.ammoCapacity; j++)
            {
                bullets[armory[i].weapon].Add(Instantiate(armory[i].bulletPrefab, bulletHolder.GetChild(i)));
                bullets[armory[i].weapon][j].SetActive(false);
            }

            casings[armory[i].weapon] = new List<GameObject>();
            for (int j = 0; j < armory[i].casingPoolSize; j++)
            {
                casings[armory[i].weapon].Add(Instantiate(armory[i].casingPrefab, bulletHolder));
                casings[armory[i].weapon][j].SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        player.OnBulletFired += ExpendAmmo;
        player.OnReload += OnReload;
    }


    private void OnDisable()
    {
        player.OnBulletFired += ExpendAmmo;
        player.OnReload -= OnReload;
    }

    private void ExpendAmmo(bool arg1, Vector2 arg2)
    {
        var ammo = bullets[player.ActiveWeapon];
        ammo[player.CurrentAmmo].transform.DOComplete();
        ammo[player.CurrentAmmo].SetActive(false);

        var originBullet = ammo[0].transform;
        for (int i = player.CurrentAmmo - 1; i >= 0; i--)
        {
            ammo[i].transform.DOComplete();
            ammo[i].transform.DOMove(ammo[i + 1].transform.position, bulletTweenSpeed);
        }

        var casingList = casings[player.ActiveWeapon];
        for (int i = 0; i < casingList.Count; i++)
        {
            if (!casingList[i].activeSelf)
            {
                casingList[i].transform.position = ammo[player.CurrentAmmo].transform.position;
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
        for (int i = 0; i < ammo.Count; i++)
        {
            ammo[i].transform.DOKill();
            ammo[i].transform.localPosition = Vector3.zero;
            ammo[i].transform.DOMove(originBullet.position + originBullet.right * armoryDictionary[player.ActiveWeapon].uiSpacing * i, bulletTweenSpeed);
            ammo[i].SetActive(true);
        }
    }
}
