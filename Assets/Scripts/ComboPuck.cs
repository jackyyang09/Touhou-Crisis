using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class ComboPuck : MonoBehaviour, IPunObservable
{
    [SerializeField] PlayerBehaviour player;
    [SerializeField] bool hasPuck;

    [SerializeField] float maxMultiplier = 3;
    [SerializeField] float damageMultiplier;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        player.OnBulletFired += CountCombo;
        player.OnReload += PassReceivePuck;
    }

    private void OnDisable()
    {
        player.OnBulletFired -= CountCombo;
    }

    private void CountCombo(bool missed, Vector2 hitPosition)
    {
        if (missed || !hasPuck) return;
        damageMultiplier = Mathf.Clamp(damageMultiplier + 0.1f, 0, maxMultiplier);
    }

    private void PassReceivePuck()
    {
        //PlayerManager.Instance.
        //var photonView = .transform.GetComponent<PhotonView>();
        //photonView.RPC("QueuePuck", RpcTarget.All, ActiveWeapon.bulletDamage);
    }

    public void QueuePuck()
    {

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        //{
        //    // We own this player: send the others our data
        //    stream.SendNext(gameTimer);
        //}
        //else
        //{
        //    // Network player, receive data
        //    remoteGameTimer = (float)stream.ReceiveNext();
        //}
    }
}
