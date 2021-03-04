using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
using Photon.Pun;

public class AudioHelper : MonoBehaviour
{
    [SerializeField] PhotonView photonView;
    [SerializeField] PlayerBehaviour player = null;
    [SerializeField] ComboPuck puck;

    private void OnEnable()
    {
        if (!photonView.IsMine) return;

        AudioManager.PlaySound(TouhouCrisisSounds.WaitBeep);

        player.OnBulletFired += PlayFire;
        player.OnFireNoAmmo += PlayDryFire;
        player.OnReload += PlayReload;
        puck.OnReceivePuck += PuckReceive;
        player.OnTakeDamage += PlayHurt;
        player.OnEnterSubArea += PlayAction;
    }

    private void OnDisable()
    {
        if (!photonView.IsMine) return;

        player.OnBulletFired -= PlayFire;
        player.OnFireNoAmmo -= PlayDryFire;
        player.OnReload -= PlayReload;
        puck.OnReceivePuck -= PuckReceive;
        player.OnTakeDamage -= PlayHurt;
        player.OnEnterSubArea -= PlayAction;
    }

    private void PlayFire(bool miss, Vector2 hitPosition)
    {
        AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Fire);
        AudioManager.PlaySound(TouhouCrisisSounds.BulletCasings);
    }

    bool shoutReloadOnce = false;
    private void PlayDryFire()
    {
        if (!shoutReloadOnce)
        {
            AudioManager.PlaySound(TouhouCrisisSounds.ReloadVO);
            shoutReloadOnce = true;
        }
        AudioManager.PlaySound(TouhouCrisisSounds.Handgun_Dryfire);
    }

    private void PlayReload()
    {
        shoutReloadOnce = false;
        AudioManager.PlaySound(TouhouCrisisSounds.Reload);
    }

    private void PlayHurt(DamageType damage)
    {
        AudioManager.PlaySound(TouhouCrisisSounds.PlayerHurt);
    }

    private void PuckReceive()
    {
        AudioManager.PlaySound(TouhouCrisisSounds.PuckChange);
    }

    private void PlayAction()
    {
        AudioManager.PlaySound(TouhouCrisisSounds.ActionVO);
    }
}
