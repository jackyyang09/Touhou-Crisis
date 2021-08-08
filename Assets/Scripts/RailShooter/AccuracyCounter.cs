using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AccuracyCounter : MonoBehaviour, IReloadable
{
    [SerializeField] int shotsFired = 0;
    public int ShotsFired { get { return shotsFired; } }

    [SerializeField] int shotsHit = 0;
    public float Accuracy
    {
        get
        {
            return (float)shotsHit / (float)shotsFired;
        }
    }

    [Header("Object References")]
    [SerializeField] PlayerBehaviour player;

    public void Reinitialize()
    {
        shotsFired = 0;
        shotsHit = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);
        Reinitialize();
    }

    private void OnEnable()
    {
        player.OnRoundExpended += SyncCountPlayerShot;
    }

    private void OnDisable()
    {
        player.OnRoundExpended -= SyncCountPlayerShot;
    }

    void SyncCountPlayerShot(bool hit)
    {
        player.PhotonView.RPC("CountPlayerShot", RpcTarget.All, hit);
    }

    [PunRPC]
    void CountPlayerShot(bool hit)
    {
        if (hit) shotsHit++;
        shotsFired++;
    }
}
