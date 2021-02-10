using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ScoreSystem : MonoBehaviour, IPunObservable
{
    [SerializeField] float score;
    public int CurrentScore
    {
        get
        {
            return (int)score;
        }
    }

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
        player.PhotonView.RPC("AddScoreOnSuccessfulHit", RpcTarget.All);
    }

    [PunRPC]
    void AddScoreOnSuccessfulHit()
    {
        score += scoreOnHit * comboPuck.Multliplier;
        OnScoreChanged?.Invoke((int)score);
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(score);
        }
        else
        {
            // Network player, receive data
            float newScore = (float)stream.ReceiveNext();
            if (newScore != score)
            {
                score = newScore;
                OnScoreChanged?.Invoke((int)score);
            }
        }
    }
}
