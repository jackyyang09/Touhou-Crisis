using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOSpawner : MonoBehaviour
{
    [SerializeField] GameObject ufoPrefab = null;
    [SerializeField] ModularBox areaBox = null;

    [SerializeField] Sakuya sakuya = null;

    [SerializeField] float[] spawnDelay = new float[3];

    [Range(0, 8)]
    [SerializeField] int maximumUfos = 0;

    [SerializeField] Transform[] spawnPositions = null;

    int activeUfos = 0;

    //ComboPuck puck = null;

    void OnEnable()
    {
        GameManager.OnSpawnLocalPlayer += Initialize;
        sakuya.OnChangePhase += UpdateSpawnBehaviour;
        UpdateSpawnBehaviour(0);
    }

    void OnDisable()
    {
        sakuya.OnChangePhase -= UpdateSpawnBehaviour;
        //if (puck != null)
        //{
        //    puck.OnPassPuck -= SpawnUFO;
        //}
    }

    void Initialize(PlayerBehaviour player)
    {
        GameManager.OnSpawnLocalPlayer -= Initialize;

        //puck = PlayerManager.Instance.LocalPlayer.GetComponent<ComboPuck>();
        //puck.OnPassPuck += SpawnUFO;
    }

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    void UpdateSpawnBehaviour(int currentPhase)
    {
        if (IsInvoking("SpawnUFO")) CancelInvoke("SpawnUFO");

        InvokeRepeating("SpawnUFO", spawnDelay[currentPhase], spawnDelay[currentPhase]);
    }

    [ContextMenu("Spawn UFO Now")]
    void SpawnUFO()
    {
        if (activeUfos < maximumUfos)
        {
            activeUfos++;
            Transform spawnPos = spawnPositions[Random.Range(0, spawnPositions.Length)];
            var newFO = Instantiate(ufoPrefab, spawnPos.position, Quaternion.identity);
            UFOBehaviour ufo = newFO.GetComponent<UFOBehaviour>();
            ufo.OnUFOExpire += ReportUFODeath;
            ufo.Init(areaBox);
        }
    }

    void ReportUFODeath()
    {
        activeUfos--;
    }
}
