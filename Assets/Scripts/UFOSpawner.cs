using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOSpawner : MonoBehaviour
{
    [SerializeField] GameObject redUfoPrefab = null;
    [SerializeField] GameObject greenUfoPrefab = null;
    [SerializeField] GameObject blueUfoPrefab = null;

    [SerializeField] ModularBox areaBox = null;

    [SerializeField] Sakuya sakuya = null;

    [SerializeField] float[] spawnDelay = new float[3];

    [Range(0, 8)]
    [SerializeField] int maximumUfos = 0;

    [SerializeField] Transform[] redSpawnPoints = null;
    [SerializeField] Transform[] greenSpawnPoints = null;
    [SerializeField] Transform[] blueSpawnPoints = null;

    int activeUfos = 0;

    void OnEnable()
    {
        GameManager.OnSpawnLocalPlayer += Initialize;
        sakuya.OnChangePhase += UpdateSpawnBehaviour;
        UpdateSpawnBehaviour(0);
    }

    void OnDisable()
    {
        sakuya.OnChangePhase -= UpdateSpawnBehaviour;
    }

    void Initialize(PlayerBehaviour player)
    {
        GameManager.OnSpawnLocalPlayer -= Initialize;
    }

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

            int ufoType = Random.Range(0, 3);

            // Temporary allocation
            GameObject newFO = gameObject;
            switch ((UFOBehaviour.UFOType)ufoType)
            //switch (UFOBehaviour.UFOType.Red)
            {
                case UFOBehaviour.UFOType.Green:
                    newFO = Instantiate(greenUfoPrefab, greenSpawnPoints[Random.Range(0, greenSpawnPoints.Length)].position, Quaternion.identity);
                    break;
                case UFOBehaviour.UFOType.Blue:
                    newFO = Instantiate(blueUfoPrefab, blueSpawnPoints[Random.Range(0, blueSpawnPoints.Length)].position, Quaternion.identity);
                    break;
                case UFOBehaviour.UFOType.Red:
                    newFO = Instantiate(redUfoPrefab, redSpawnPoints[Random.Range(0, redSpawnPoints.Length)].position, Quaternion.identity);
                    break;
            }

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
