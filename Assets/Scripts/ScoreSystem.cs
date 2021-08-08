using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ScoreSystem : MonoBehaviour,/* IPunObservable,*/ IReloadable
{
    public const float SCORE_ON_HIT = 100;
    public const float MAX_ACCURACY_BONUS = 10000;
    public const float DAMAGE_TAKEN_PENALTY = -3000;

    [SerializeField] float score = 0;
    public int CurrentScore
    {
        get
        {
            return (int)score;
        }
    }

    [Header("Object References")]
    [SerializeField] PlayerBehaviour player = null;

    [SerializeField] ComboPuck comboPuck = null;

    public Action<int> OnScoreChanged;

    public void Reinitialize()
    {
        score = 0;
        OnScoreChanged?.Invoke((int)score);
    }

    // Start is called before the first frame update
    void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);
        Reinitialize();
    }

    private void OnEnable()
    {
        player.OnShotFired += AddScoreOnHit;
    }

    private void OnDisable()
    {
        player.OnShotFired -= AddScoreOnHit;
    }

    private void AddScoreOnHit(bool miss, Vector2 arg2)
    {
        if (miss) return;
        float scoreToAdd = SCORE_ON_HIT * comboPuck.Multliplier;
        score += scoreToAdd;
        OnScoreChanged?.Invoke((int)score);
        player.PhotonView.RPC(nameof(RemoteAddScoreOnHit), RpcTarget.Others, scoreToAdd);
    }

    [PunRPC]
    void RemoteAddScoreOnHit(object scoreToAdd)
    {
        score += (float)scoreToAdd;
        OnScoreChanged?.Invoke((int)score);
    }

    public void AddArbitraryScore(float scoreToAdd)
    {
        score += scoreToAdd * comboPuck.Multliplier;
        OnScoreChanged?.Invoke((int)score);
        player.PhotonView.RPC(nameof(AddScore), RpcTarget.Others, scoreToAdd);
    }

    [PunRPC]
    void AddScore(float scoreToAdd)
    {
        score += scoreToAdd * comboPuck.Multliplier;
        OnScoreChanged?.Invoke((int)score);
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        // We own this player: send the others our data
    //        stream.SendNext(score);
    //    }
    //    else
    //    {
    //        // Network player, receive data
    //        float newScore = (float)stream.ReceiveNext();
    //        if (newScore != score)
    //        {
    //            score = newScore;
    //            OnScoreChanged?.Invoke((int)score);
    //        }
    //    }
    //}
}