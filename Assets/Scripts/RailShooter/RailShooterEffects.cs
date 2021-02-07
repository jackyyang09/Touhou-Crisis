using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
using DG.Tweening;

public class RailShooterEffects : MonoBehaviour
{
    [SerializeField] RailShooterLogic railShooter;
    [SerializeField] AudioFileSoundObject shootSound;

    [SerializeField] UnityEngine.UI.Image screenFlash;
    [SerializeField] float fadeEffectTime = 0.05f;

    private void OnEnable()
    {
        railShooter.OnShoot += PlayEffect;
    }

    private void OnDisable()
    {
        railShooter.OnShoot += PlayEffect;
    }

    void PlayEffect(Ray ray)
    {
        screenFlash.DOColor(Color.white, 0);
        screenFlash.DOColor(Color.clear, 0).SetDelay(fadeEffectTime);
        AudioManager.instance.PlaySoundInternal(shootSound);
    }
}
