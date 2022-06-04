using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
using Photon.Pun;

public class AudioHelper : MonoBehaviour, IReloadable
{
    [SerializeField] PhotonView photonView;
    [SerializeField] PlayerBehaviour player = null;
    [SerializeField] ComboPuck puck;

    public void Reinitialize()
    {
        AudioManager.MainMusicHelper.Stop();
        AudioManager.PlayMusic(TouhouCrisisMusic.FloweringNight, true);
        AudioManager.PlaySound(TouhouCrisisSounds.WaitBeep);
    }

    private void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);
        if (player.PhotonView.IsMine)
        {
            Reinitialize();
        }
    }

    private void OnEnable()
    {
        if (!photonView.IsMine) return;

        player.OnRoundExpended += PlayFire;
        player.OnFireNoAmmo += PlayDryFire;
        player.OnReload += PlayReload;
        puck.OnReceivePuck += PuckReceive;
        player.OnTakeDamage += PlayHurt;
        player.OnEnterSubArea += PlayAction;
    }

    private void OnDisable()
    {
        if (!photonView.IsMine) return;

        player.OnRoundExpended -= PlayFire;
        player.OnFireNoAmmo -= PlayDryFire;
        player.OnReload -= PlayReload;
        puck.OnReceivePuck -= PuckReceive;
        player.OnTakeDamage -= PlayHurt;
        player.OnEnterSubArea -= PlayAction;
    }

    private void PlayFire(bool miss)
    {
        AudioManager.PlaySound(player.ActiveWeapon.fireSound);
        AudioManager.PlaySound(player.ActiveWeapon.casingSound);
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
