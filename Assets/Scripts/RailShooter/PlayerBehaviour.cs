using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System;

public enum DamageType
{
    Slash,
    Bullet
}

public class PlayerBehaviour : MonoBehaviour, IReloadable
{
    [SerializeField] bool infiniteLives = false;
    public bool BuddhaMode { get { return infiniteLives; } }
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

    [SerializeField] KeyCode coverKey = KeyCode.Space;

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
    public WeaponObject[] Loadout { get { return weapons.ToArray(); } }

    public WeaponObject ActiveWeapon
    {
        get
        {
            return weapons[activeWeaponIndex];
        }
    }

    [SerializeField] int[] ammoCount = new int[4];
    public int[] AmmoCount { get { return ammoCount; } }

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
    public int ActiveWeaponIndex { get { return activeWeaponIndex; } }

    bool triggerPulled = false;

    float fireDelay = 0;

    [Header("Object References")]

    [SerializeField] LayerMask shootableLayers;

    [SerializeField] RailShooterLogic railShooter = null;

    [SerializeField] ScoreSystem scoreSystem = null;
    public ScoreSystem ScoreSystem
    {
        get
        {
            return scoreSystem;
        }
    }

    [SerializeField] AccuracyCounter accuracyCounter;
    public AccuracyCounter AccuracyCounter
    {
        get
        {
            return accuracyCounter;
        }
    }

    [SerializeField] DamageCounter damageCounter;
    public DamageCounter DamageCounter
    {
        get
        {
            return damageCounter;
        }
    }

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

    [SerializeField] Animator anim = null;

    bool canPlay = true;
    public bool CanPlay
    {
        get
        {
            return canPlay;
        }
    }

    
    /// <summary>
    /// Called when a shot is successfully fired. 
    /// Shotguns will invoke this several times
    /// bool miss, Vector2 hitPosition
    /// </summary>
    public Action<bool, Vector2> OnShotFired;
    /// <summary>
    /// Called when an entire round is spent. 
    /// bool hit
    /// </summary>
    public Action<bool> OnRoundExpended;
    public Action OnEnterCover;
    public Action OnExitCover;
    public Action OnReload;
    public Action OnFireNoAmmo;
    public Action<DamageType> OnTakeDamage;
    public Action OnTakeDamageRemote;
    public Action OnPlayerDeath;
    /// <summary>
    /// Called when active weapon changes.
    /// WeaponObject newWeapon
    /// </summary>
    public Action<WeaponObject> OnSwapWeapon;
    public Action OnAmmoChanged;
    public Action OnEnterTransit;
    public Action OnExitTransit;
    public Action OnEnterSubArea;

    public void Reinitialize()
    {
        if (PhotonView.IsMine)
        {
            head.transform.localPosition = new Vector3(0, 1.5f, 0);
            head.transform.localEulerAngles = Vector3.zero;
        }

        anim.Play("Start");

        currentLives = maxLives;

        for (int i = 0; i < weapons.Count; i++)
        {
            ammoCount[i] = weapons[i].ammoCapacity;
        }

        activeWeaponIndex = 0;

        OnSwapWeapon?.Invoke(ActiveWeapon);
        OnAmmoChanged?.Invoke();
        EnterTransit();
        ResumePlayerControl();
    }

    // Start is called before the first frame update
    private void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);

        var modifiers = GameplayModifiers.Instance;
        if (modifiers)
        {
            switch (modifiers.StartingLives)
            {
                case GameplayModifiers.LiveCounts.One:
                case GameplayModifiers.LiveCounts.Two:
                case GameplayModifiers.LiveCounts.Three:
                case GameplayModifiers.LiveCounts.Four:
                case GameplayModifiers.LiveCounts.Five:
                    maxLives = (int)modifiers.StartingLives + 1;
                    break;
                case GameplayModifiers.LiveCounts.Infinite:
                    infiniteLives = true;
                    break;
            }
        }

        coverKey = (KeyCode)PlayerPrefs.GetInt(PauseMenu.CoverInputKey);

        Reinitialize();
    }

    private void OnEnable()
    {
        if (weapons[activeWeaponIndex].weaponType == FireType.SemiAuto)
        {
            railShooter.OnShoot += HandleShooting;
        }
        railShooter.OnTriggerDown += OnTriggerDown;
        railShooter.OnTriggerUp += OnTriggerUp;
        OnPlayerDeath += RemovePlayerControl;
    }

    private void OnDisable()
    {
        //GameManager.OnLeaveScene -= DestroySelf;
        railShooter.OnShoot -= HandleShooting;
        railShooter.OnTriggerDown -= OnTriggerDown;
        railShooter.OnTriggerUp -= OnTriggerUp;
        OnPlayerDeath -= RemovePlayerControl;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (!PhotonView.IsMine) return;
        }

        if (canPlay)
        {
            if (ActiveWeapon.weaponType == FireType.FullAuto)
            {
                if (fireDelay > 0)
                {
                    fireDelay -= Time.deltaTime;
                }
                else if (triggerPulled && fireDelay <= 0)
                {
                    HandleShooting(railShooter.FireRay(), railShooter.GetCursorPosition());
                    fireDelay = ActiveWeapon.fireDelay;
                }
            }
        }

        if (inTransit) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (Input.touchCount > 0 && canPlay)
        {
            StepOnPedal();
        }
        else
        {
            ReleasePedal();
        }
#else
        if (Input.GetKey(coverKey) && canPlay)
        {
            StepOnPedal();
        }
        else
        {
            ReleasePedal();
        }
#endif
    }

    private void OnTriggerDown() => triggerPulled = true;
    private void OnTriggerUp() => triggerPulled = false;

    void HandleShooting(Ray ray, Vector2 screenPoint)
    {
        if (inCover || inTransit || Time.timeScale == 0 || !canPlay) return;

        bool hitOnce = false;
        if (CurrentAmmo > 0)
        {
            ammoCount[activeWeaponIndex]--;
            OnAmmoChanged?.Invoke();

            RaycastHit hit;

            // If the wielded weapon is a shotgun
            for (int i = 0; i < ActiveWeapon.pellets + 1; i++)
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                Vector2 hitPosition = Input.touches[Input.touchCount - 1].position;
#else
                Vector2 hitPosition = Input.mousePosition;
#endif
                bool miss = true;

                if (i > 0)
                {
                    hitPosition += UnityEngine.Random.insideUnitCircle * ActiveWeapon.bulletSpread;
                    ray = railShooter.GetRayFromScreenPoint(hitPosition);
                }

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, shootableLayers) && hit.rigidbody != null)
                {
                    PhotonView photonView = null;
                    if (hit.transform.TryGetComponent(out photonView))
                    {
                        if (GameplayModifiers.Instance.GameMode == GameplayModifiers.GameModes.Coop)
                        {
                            photonView.RPC("OnShotBehaviour", RpcTarget.All, ActiveWeaponDamage);
                        }
                        else
                        {
                            hit.transform.GetComponent<BaseEnemy>().OnShotBehaviour(ActiveWeaponDamage);
                        }
                    }
                    // Hit an offline object
                    else
                    {
                        hit.transform.GetComponent<IShootable>().OnShotBehaviour(ActiveWeaponDamage);
                    }

                    miss = false;
                    hitOnce = true;
                }

                OnShotFired?.Invoke(miss, hitPosition);
            }
            OnRoundExpended?.Invoke(hitOnce);
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
            OnExitCover?.Invoke();
        }

        Transform coverTransform = AreaLogic.Instance.Player1CoverTransform;
        Transform fireTransform = AreaLogic.Instance.Player1FireTransform;
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
            if (ActiveWeapon.infiniteAmmo)
            {
                ammoCount[activeWeaponIndex] = ActiveWeapon.ammoCapacity;
                OnAmmoChanged?.Invoke();
                OnReload?.Invoke();
            }
            OnEnterCover?.Invoke();
        }

        Transform coverTransform = AreaLogic.Instance.Player1CoverTransform;
        Transform fireTransform = AreaLogic.Instance.Player1FireTransform;
        float lerpValue = coverEnterTimer / coverEnterTime;
        head.transform.position = Vector3.Lerp(coverTransform.position, fireTransform.position, lerpValue);
        head.transform.rotation = Quaternion.Lerp(coverTransform.rotation, fireTransform.rotation, lerpValue);
    }

    public void SwapWeapon(int weaponIndex)
    {
        activeWeaponIndex = weaponIndex;
        railShooter.OnShoot -= HandleShooting;
        if (weapons[activeWeaponIndex].weaponType == FireType.SemiAuto)
        {
            railShooter.OnShoot += HandleShooting;
        }
        OnSwapWeapon?.Invoke(weapons[activeWeaponIndex]);
    }

    public void EnterTransit()
    {
        inTransit = true;
        anim.SetTrigger("InTransit");
        anim.SetInteger("Area", AreaLogic.Instance.CurrentArea + 1);
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
        impulse.GenerateImpulse();
        StartCoroutine(DamageRecovery());
        currentLives--;
        OnTakeDamage?.Invoke(damageType);

        if (currentLives == 0 && !infiniteLives)
        {
            RemovePlayerControl();
            OnPlayerDeath.Invoke();
        }
    }

    public void TakeDamageRemote()
    {
        if (GameplayModifiers.Instance.GameMode == GameplayModifiers.GameModes.Coop)
        {
            currentLives--;
        }
        OnTakeDamageRemote?.Invoke();

        if (currentLives == 0 && !infiniteLives)
        {
            RemovePlayerControl();
            OnPlayerDeath.Invoke();
        }
    }

    IEnumerator DamageRecovery()
    {
        collider.enabled = false;
        yield return new WaitForSeconds(damageRecoveryTime);
        collider.enabled = true;
    }

    public void SetAmmo(int weaponIndex, int amount)
    {
        ammoCount[weaponIndex] = amount;
        OnAmmoChanged?.Invoke();
    }

    public void SetLoadout(List<WeaponObject> newWeapons)
    {
        weapons = new List<WeaponObject>(newWeapons);
    }

    public void ResumePlayerControl()
    {
        canPlay = true;
    }

    public void RemovePlayerControl()
    {
        canPlay = false;
    }

    [CommandTerminal.RegisterCommand(Help = "Give player infinite lives", MaxArgCount = 0)]
    static void Buddha(CommandTerminal.CommandArg[] args)
    {
        var player = FindObjectOfType<PlayerBehaviour>();
        player.infiniteLives = true;
    }
}