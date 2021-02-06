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

    [SerializeField] ComboPuck comboPuck;

    public Action<int> OnScoreChanged;

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        OnScoreChanged?.Invoke((int)score);
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
        if (miss) return;
        score += scoreOnHit * comboPuck.Multliplier;
        OnScoreChanged.Invoke((int)score);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
