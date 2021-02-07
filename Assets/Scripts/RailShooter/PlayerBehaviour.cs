using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum DamageType
{
    Slash,
    Bullet
}

public class PlayerBehaviour : MonoBehaviour
{
    [SerializeField] int maxLives = 5;
    int currentLives;
    public int CurrentLives
    {
        get
        {
            return currentLives;
        }
    }

    [SerializeField] bool inTransit;

    [SerializeField] float coverEnterTime = 0.5f;
    float coverEnterTimer;
    [Range(0, 1)]
    [SerializeField] float coverThreshold = 0.7f;

    bool inCover = false;
    public bool InCover
    {
        get
        {
            return inCover;
        }
    }

    [SerializeField] float damageRecoveryTime = 1.5f;

    [Header("Weapon Logic")]
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

    public float ActiveWeaponDamage
    {
        get
        {
            return ActiveWeapon.bulletDamage * comboPuck.Multliplier;
        }
    }

    [SerializeField] int activeWeaponIndex = 0;

    [Header("Object References")]

    [SerializeField] LayerMask shootableLayers;

    [SerializeField] RailShooterLogic railShooter = null;
    public PhotonView PhotonView
    {
        get
        {
            return railShooter.photonView;
        }
    }

    [SerializeField] ComboPuck comboPuck;

    [SerializeField] Transform head;

    [SerializeField] new Collider collider;

    [SerializeField] Cinemachine.CinemachineImpulseSource impulse;

    public System.Action<bool, Vector2> OnBulletFired;
    public System.Action OnReload;
    public System.Action OnFireNoAmmo;
    public System.Action<DamageType> OnTakeDamage;

    public System.Action OnEnterTransit;
    public System.Action OnExitTransit;
    public System.Action OnEnterSubArea;

    // Start is called before the first frame update
    void Start()
    {
        currentLives = maxLives;
        for (int i = 0; i < weapons.Count; i++)
        {
            ammoCount[i] = weapons[i].ammoCapacity;
        }
        EnterTransit();
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
        if (PhotonNetwork.IsConnected)
        {
            if (!PhotonView.IsMine) return;
        }
        if (inTransit) return;

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
        if (inCover || inTransit || Time.timeScale == 0) return;

        if (CurrentAmmo > 0)
        {
            RaycastHit hit;

            bool miss = true;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, shootableLayers) && hit.rigidbody != null)
            {
                //hit.transform.GetComponent<IShootable>().OnShotBehaviour(ActiveWeapon);
                var photonView = hit.transform.GetComponent<PhotonView>();
                photonView.RPC("OnShotBehaviour", RpcTarget.All, ActiveWeaponDamage);

                miss = false;
            }

            Vector3 hitPosition = railShooter.Cam.ScreenToViewportPoint(Input.mousePosition);

            ammoCount[activeWeaponIndex]--;

            OnBulletFired?.Invoke(miss, hitPosition);
        }
        else
        {
            OnFireNoAmmo?.Invoke();
        }
    }

    public void StepOnPedal()
    {
        coverEnterTimer = Mathf.Clamp(coverEnterTimer + Time.deltaTime, 0, coverEnterTime);
        if (coverEnterTimer / coverEnterTime > coverThreshold)
        {
            inCover = false;
        }

        Transform coverTransform = AreaLogic.Instance.GetPlayer1Cover;
        Transform fireTransform = AreaLogic.Instance.GetPlayer1Fire;
        float lerpValue = coverEnterTimer / coverEnterTime;
        head.transform.position = Vector3.Lerp(coverTransform.position, fireTransform.position, lerpValue);
        head.transform.rotation = Quaternion.Lerp(coverTransform.rotation, fireTransform.rotation, lerpValue);
    }

    public void ReleasePedal()
    {
        coverEnterTimer = Mathf.Clamp(coverEnterTimer - Time.deltaTime, 0, coverEnterTime);
        if (coverEnterTimer / coverEnterTime <= coverThreshold && !inCover)
        {
            inCover = true;
            ammoCount[activeWeaponIndex] = ActiveWeapon.ammoCapacity;
            OnReload?.Invoke();
        }

        Transform coverTransform = AreaLogic.Instance.GetPlayer1Cover;
        Transform fireTransform = AreaLogic.Instance.GetPlayer1Fire;
        float lerpValue = coverEnterTimer / coverEnterTime;
        head.transform.position = Vector3.Lerp(coverTransform.position, fireTransform.position, lerpValue);
        head.transform.rotation = Quaternion.Lerp(coverTransform.rotation, fireTransform.rotation, lerpValue);
    }

    public void EnterTransit()
    {
        inTransit = true;
        OnEnterTransit?.Invoke();
    }

    public void ExitTransit()
    {
        inTransit = false;
        coverEnterTimer = coverEnterTime;
        OnExitTransit?.Invoke();
        OnEnterSubArea?.Invoke();

        if (PhotonView.IsMine)
        {
            AreaLogic.Instance.ReportPlayerArrival();
        }
    }

    public void TakeDamage(DamageType damageType)
    {
        if (inCover) return;
        Debug.Log("OOF!");
        impulse.GenerateImpulse();
        StartCoroutine("DamageRecovery");
        currentLives--;
        OnTakeDamage?.Invoke(damageType);
    }

    IEnumerator DamageRecovery()
    {
        collider.enabled = false;
        yield return new WaitForSeconds(damageRecoveryTime);
        collider.enabled = true;
    }
}