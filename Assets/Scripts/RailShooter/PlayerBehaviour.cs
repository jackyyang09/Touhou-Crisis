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

public class PlayerBehaviour : MonoBehaviour
{
    [SerializeField] string gameSceneName = "";

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

    bool canPlay = true;
    public bool CanPlay
    {
        get
        {
            return canPlay;
        }
    }

    public Action<bool, Vector2> OnBulletFired;
    public Action OnExitCover;
    public Action OnReload;
    public Action OnFireNoAmmo;
    public Action<DamageType> OnTakeDamage;
    public Action OnTakeDamageRemote;
    public Action OnPlayerDeath;

    public Action OnEnterTransit;
    public Action OnExitTransit;
    public Action OnEnterSubArea;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.scene.name.Equals(gameSceneName))
        {
            OnEnterScene(gameObject.scene, LoadSceneMode.Single);
        }

        coverKey = (KeyCode)PlayerPrefs.GetInt(PauseMenu.CoverInputKey);

        // Just in case the player is spawned in the tween scene
        DontDestroyOnLoad(this);
    }

    private void OnEnable()
    {
        railShooter.OnShoot += HandleShooting;
        OnPlayerDeath += RemovePlayerControl;
        SceneManager.sceneLoaded += OnEnterScene;
    }

    /// <summary>
    /// Run this if the scene is the game scene
    /// </summary>
    /// <param name="arg0"></param>
    /// <param name="arg1"></param>
    private void OnEnterScene(Scene newScene, LoadSceneMode arg1)
    {
        if (!newScene.name.Equals(gameSceneName)) return;

        var modifiers = FindObjectOfType<GameplayModifiers>();
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

        currentLives = maxLives;
        for (int i = 0; i < weapons.Count; i++)
        {
            ammoCount[i] = weapons[i].ammoCapacity;
        }
        EnterTransit();
        GameManager.OnLeaveScene += DestroySelf;
    }

    void DestroySelf() => Destroy(gameObject);

    private void OnDisable()
    {
        GameManager.OnLeaveScene -= DestroySelf;
        railShooter.OnShoot -= HandleShooting;
        OnPlayerDeath -= RemovePlayerControl;
        SceneManager.sceneLoaded -= OnEnterScene;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (!PhotonView.IsMine) return;
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

    void HandleShooting(Ray ray, Vector2 screenPoint)
    {
        if (inCover || inTransit || Time.timeScale == 0) return;

        if (CurrentAmmo > 0)
        {
            RaycastHit hit;

            bool miss = true;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, shootableLayers) && hit.rigidbody != null)
            {
                PhotonView photonView = null;
                if (hit.transform.TryGetComponent(out photonView))
                {
                    photonView.RPC("OnShotBehaviour", RpcTarget.All, ActiveWeaponDamage);
                }
                // Hit an offline object
                else
                {
                    hit.transform.GetComponent<IShootable>().OnShotBehaviour(ActiveWeaponDamage);
                }

                miss = false;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            Vector3 hitPosition = Input.touches[Input.touchCount - 1].position;
#else
            Vector3 hitPosition = Input.mousePosition;
#endif
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
            ammoCount[activeWeaponIndex] = ActiveWeapon.ammoCapacity;
            OnReload?.Invoke();
        }

        Transform coverTransform = AreaLogic.Instance.Player1CoverTransform;
        Transform fireTransform = AreaLogic.Instance.Player1FireTransform;
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
        currentLives--;
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

    public void GetPlayerControl()
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