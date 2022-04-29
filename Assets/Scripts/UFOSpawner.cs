using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class UFOSpawner : MonoBehaviourPun, IReloadable
{
    [SerializeField] GameObject redUfoPrefab = null;
    [SerializeField] GameObject greenUfoPrefab = null;
    [SerializeField] GameObject blueUfoPrefab = null;

    [SerializeField] ModularBox areaBox = null;
    public ModularBox AreaBox { get { return areaBox; } }

    [SerializeField] Sakuya sakuya = null;

    [SerializeField] float[] spawnDelay = new float[3];

    [Range(0, 8)]
    [SerializeField] int maximumUfos = 0;

    [SerializeField] Transform[] redSpawnPoints = null;
    [SerializeField] Transform[] greenSpawnPoints = null;
    [SerializeField] Transform[] blueSpawnPoints = null;

    [SerializeField] ObjectPool blueBulletPool = null;
    [SerializeField] ObjectPool greenBulletPool = null;

    int selectedSpawnDelay;

    int activeUfos = 0;

    static UFOSpawner instance;
    public static UFOSpawner Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UFOSpawner>();
            }
            return instance;
        }
    }

    public void Reinitialize()
    {
        switch (GameplayModifiers.Instance.GameMode)
        {
            case GameplayModifiers.GameModes.Versus:
                DisableSpawner();
                InvokeRepeating("SpawnLocalUFO", spawnDelay[selectedSpawnDelay], spawnDelay[selectedSpawnDelay]);
                break;
            case GameplayModifiers.GameModes.Coop:
                if (PhotonNetwork.IsMasterClient)
                {
                    DisableSpawner();
                    InvokeRepeating("SpawnUFO", spawnDelay[selectedSpawnDelay], spawnDelay[selectedSpawnDelay]);
                }
                break;
        }
    }

    void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);

        var modifiers = GameplayModifiers.Instance;
        if (modifiers)
        {
            switch (modifiers.UFOSpawnRate)
            {
                case GameplayModifiers.UFOSpawnRates.Disabled:
                    enabled = false;
                    return;
                case GameplayModifiers.UFOSpawnRates.Low:
                case GameplayModifiers.UFOSpawnRates.Normal:
                case GameplayModifiers.UFOSpawnRates.High:
                    selectedSpawnDelay = (int)modifiers.UFOSpawnRate - 1;
                    break;
            }
        }

        Reinitialize();
    }

    void OnEnable()
    {
        GameManager.OnSpawnLocalPlayer += Initialize;
        //sakuya.OnChangePhase += UpdateSpawnBehaviour;
        sakuya.OnBossDefeat += DisableSpawner;
        //UpdateSpawnBehaviour(0);

        GameOverUI.OnGameOver += DisableSpawner;
    }

    void OnDisable()
    {
        //sakuya.OnChangePhase -= UpdateSpawnBehaviour;
        sakuya.OnBossDefeat -= DisableSpawner;

        GameOverUI.OnGameOver -= DisableSpawner;
    }

    void DisableSpawner()
    {
        if (IsInvoking(nameof(SpawnUFO))) CancelInvoke(nameof(SpawnUFO));
        if (IsInvoking(nameof(SpawnLocalUFO))) CancelInvoke(nameof(SpawnLocalUFO));
    } 

    void Initialize(PlayerBehaviour player)
    {
        GameManager.OnSpawnLocalPlayer -= Initialize;
    }

    [ContextMenu("Spawn UFO Now")]
    void SpawnUFO()
    {
        if (activeUfos < maximumUfos)
        {
            activeUfos++;
            int ufoType = Random.Range(0, 3);
            photonView.RPC("SyncSpawnUFO", RpcTarget.MasterClient, ufoType);
        }
    }

    void SpawnLocalUFO()
    {
        if (activeUfos < maximumUfos)
        {
            activeUfos++;
            int ufoType = Random.Range(0, 3);
            SyncSpawnUFO((UFOBehaviour.UFOType)ufoType);
        }
    }

    [PunRPC]
    public void SyncSpawnUFO(UFOBehaviour.UFOType ufoType)
    {
        // Temporary allocation
        GameObject newFO = gameObject;
        switch (ufoType)
        {
            case UFOBehaviour.UFOType.Green:
                newFO = PhotonNetwork.Instantiate(greenUfoPrefab.name, greenSpawnPoints[Random.Range(0, greenSpawnPoints.Length)].position, Quaternion.identity);
                break;
            case UFOBehaviour.UFOType.Blue:
                newFO = PhotonNetwork.Instantiate(blueUfoPrefab.name, blueSpawnPoints[Random.Range(0, blueSpawnPoints.Length)].position, Quaternion.identity);
                break;
            case UFOBehaviour.UFOType.Red:
                newFO = PhotonNetwork.Instantiate(redUfoPrefab.name, redSpawnPoints[Random.Range(0, redSpawnPoints.Length)].position, Quaternion.identity);
                break;
        }

        newFO.GetComponent<UFOBehaviour>().Init();
    }

    public GameObject GetUFOBullet(UFOBehaviour.UFOType ufoType)
    {
        switch (ufoType)
        {
            case UFOBehaviour.UFOType.Green:
                return greenBulletPool.GetObject();
            case UFOBehaviour.UFOType.Blue:
                return blueBulletPool.GetObject();
            case UFOBehaviour.UFOType.Red:
            default:
                return null;
        }
    }

    public void ReportUFODeath()
    {
        activeUfos--;
    }
}