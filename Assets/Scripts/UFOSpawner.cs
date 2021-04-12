using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class UFOSpawner : MonoBehaviourPun
{
    [SerializeField] GameObject redUfoPrefab = null;
    [SerializeField] GameObject greenUfoPrefab = null;
    [SerializeField] GameObject blueUfoPrefab = null;

    [SerializeField] ModularBox areaBox = null;
    public ModularBox AreaBox
    {
        get
        {
            return areaBox;
        }
    }

    [SerializeField] Sakuya sakuya = null;

    [SerializeField] float[] spawnDelay = new float[3];

    [Range(0, 8)]
    [SerializeField] int maximumUfos = 0;

    [SerializeField] Transform[] redSpawnPoints = null;
    [SerializeField] Transform[] greenSpawnPoints = null;
    [SerializeField] Transform[] blueSpawnPoints = null;

    [SerializeField] ObjectPool blueBulletPool = null;
    [SerializeField] ObjectPool greenBulletPool = null;

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

    void Start()
    {
        var modifiers = FindObjectOfType<GameplayModifiers>();
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
                    int spawnRate = (int)modifiers.UFOSpawnRate - 1;
                    if (PhotonNetwork.IsMasterClient)
                        InvokeRepeating("SpawnUFO", spawnDelay[spawnRate], spawnDelay[spawnRate]);
                    break;
            }
        }
    }

    void OnEnable()
    {
        GameManager.OnSpawnLocalPlayer += Initialize;
        //sakuya.OnChangePhase += UpdateSpawnBehaviour;
        sakuya.OnBossDefeat += DisableSpawner;
        //UpdateSpawnBehaviour(0);
    }

    void OnDisable()
    {
        //sakuya.OnChangePhase -= UpdateSpawnBehaviour;
        sakuya.OnBossDefeat -= DisableSpawner;
    }

    void DisableSpawner()
    {
        if (IsInvoking("SpawnUFO")) CancelInvoke("SpawnUFO");
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