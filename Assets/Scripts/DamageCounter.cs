using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DamageCounter : MonoBehaviour, IReloadable
{
    [SerializeField] PlayerBehaviour player;

    int damageTaken = 0;
    public int DamageTaken
    {
        get
        {
            return damageTaken;
        }
    }

    public void Reinitialize()
    {
        damageTaken = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);
    }

    private void OnEnable()
    {
        player.OnTakeDamage += SyncCountHit;
    }

    private void OnDisable()
    {
        player.OnTakeDamage -= SyncCountHit;
    }

    private void SyncCountHit(DamageType obj)
    {
        player.PhotonView.RPC("CountHit", RpcTarget.All);
    }

    [PunRPC]
    void CountHit()
    {
        damageTaken++;
    }
}
