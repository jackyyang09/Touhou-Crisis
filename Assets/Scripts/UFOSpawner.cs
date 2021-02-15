using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOSpawner : MonoBehaviour
{
    [SerializeField] GameObject ufoPrefab;
    [SerializeField] ModularBox areaBox;

    [SerializeField] Transform[] spawnPositions;

    ComboPuck puck;

    void OnEnable()
    {
        puck = PlayerManager.Instance.LocalPlayer.GetComponent<ComboPuck>();

        puck.OnPassPuck += SpawnUFO;
    }

    void OnDisable()
    {
        puck.OnPassPuck -= SpawnUFO;
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
        Transform spawnPos = spawnPositions[Random.Range(0, spawnPositions.Length)];
        var newFO = Instantiate(ufoPrefab, spawnPos.position, Quaternion.identity);
        UFOBehaviour ufo = newFO.GetComponent<UFOBehaviour>();
        //ufo.
    }
}
