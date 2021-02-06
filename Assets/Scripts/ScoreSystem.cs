using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    [SerializeField] float score;
    [SerializeField] float scoreOnHit;

    [Header("Object References")]
    [SerializeField] PlayerBehaviour player;

    public Action OnScoreChanged;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        player.OnBulletFired += AddScoreOnHit;
    }

    private void OnDisable()
    {
        player.OnBulletFired -= AddScoreOnHit;
    }


    private void AddScoreOnHit(bool miss, Vector2 position)
    {
        OnScoreChanged?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
