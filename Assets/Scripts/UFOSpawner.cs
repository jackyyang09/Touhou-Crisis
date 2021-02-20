using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOSpawner : MonoBehaviour
{
    [SerializeField] GameObject ufoPrefab;
    [SerializeField] ModularBox areaBox;

    [Range(0, 8)]
    [SerializeField] int maximumUFOs;

    [SerializeField] Transform[] spawnPositions;

    ComboPuck puck;

    void OnEnable()
    {
        GameManager.OnSpawnLocalPlayer += Initialize;
    }

    void OnDisable()
    {
        if (puck != null)
        {
            puck.OnPassPuck -= SpawnUFO;
        }
    }

    void Initialize(PlayerBehaviour player)
    {
        GameManager.OnSpawnLocalPlayer -= Initialize;

        puck = PlayerManager.Instance.LocalPlayer.GetComponent<ComboPuck>();
        puck.OnPassPuck += SpawnUFO;
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

    void SpawnUFO()
    {
        Debug.Log("UFO SPAWNED");

        Transform spawnPos = spawnPositions[Random.Range(0, spawnPositions.Length)];
        var newFO = Instantiate(ufoPrefab, spawnPos.position, Quaternion.identity);
        UFOBehaviour ufo = newFO.GetComponent<UFOBehaviour>();
        ufo.Init(areaBox);
    }
}
