using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerBehaviour : MonoBehaviour
{
    [SerializeField] List<WeaponObject> weapons = new List<WeaponObject>();

    public WeaponObject ActiveWeapon
    {
        get
        {
            return weapons[activeWeaponIndex];
        }
    }

    public int[] ammoCount = new int[4];

    public int CurrentAmmo
    {
        get
        {
            return ammoCount[activeWeaponIndex];
        }
    }

    [SerializeField]  float coverEnterTime = 0.5f;
    float coverEnterTimer;
    [Range(0, 1)]
    [SerializeField] float coverThreshold = 0.7f;

    bool inCover = false;

    [SerializeField] int activeWeaponIndex = 0;

    [SerializeField] LayerMask shootableLayers;

    [SerializeField] RailShooterLogic railShooter = null;

    public System.Action<bool, Vector2> OnBulletFired;
    public System.Action OnReload;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            ammoCount[i] = weapons[i].ammoCapacity;
        }
    }

    private void OnEnable()
    {
        railShooter.OnShoot += HandleShooting;
    }

    private void OnDisable()
    {
        railShooter.OnShoot -= HandleShooting;
    }

    // Update is called once per frame
    void Update()
    {
        if (!railShooter.photonView.IsMine) return;

        if (Input.GetKey(KeyCode.Space))
        {
            StepOnPedal();
        }
        else
        {
            ReleasePedal();
        }
    }

    void HandleShooting(Ray ray)
    {
        if (inCover) return;

        if (CurrentAmmo > 0)
        {
            RaycastHit hit;

            bool miss = true;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, shootableLayers) && hit.rigidbody != null)
            {
                //hit.transform.GetComponent<IShootable>().OnShotBehaviour(ActiveWeapon);
                var photonView = hit.transform.GetComponent<PhotonView>();
                photonView.RPC("OnShotBehaviour", RpcTarget.All, ActiveWeapon.bulletDamage);

                miss = false;
            }

            Vector3 hitPosition = railShooter.Cam.ScreenToViewportPoint(Input.mousePosition);

            ammoCount[activeWeaponIndex]--;

            OnBulletFired?.Invoke(miss, hitPosition);
        }
        else
        {

        }
    }

    public void StepOnPedal()
    {
        coverEnterTimer = Mathf.Clamp(coverEnterTimer + Time.deltaTime, 0, coverEnterTime);
        if (coverEnterTimer / coverEnterTime < coverThreshold)
        {
            inCover = false;
        }
    }

    public void ReleasePedal()
    {
        coverEnterTimer = Mathf.Clamp(coverEnterTimer - Time.deltaTime, 0, coverEnterTime);
        if (coverEnterTimer / coverEnterTime >= coverThreshold && !inCover)
        {
            inCover = true;
            ammoCount[activeWeaponIndex] = ActiveWeapon.ammoCapacity;
            OnReload?.Invoke();
        }
    }
}
