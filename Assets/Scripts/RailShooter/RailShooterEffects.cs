using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
using DG.Tweening;

public class RailShooterEffects : MonoBehaviour
{
    public bool playSound = false;
    [SerializeField] RailShooterLogic railShooter = null;
    [SerializeField] PlayerBehaviour player = null;
    [SerializeField] AudioFileSoundObject shootSound = null;

    [SerializeField] Color flashColor = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] UnityEngine.UI.Image screenFlash = null;
    [SerializeField] float fadeEffectTime = 0.05f;

    [SerializeField] GameObject muzzleFlashPrefab = null;
    public bool spawnMuzzleFlash = false;

    [SerializeField] bool inMenu = false;

    //private void Start()
    //{
    //}

    private void OnEnable()
    {
        if (inMenu)
        {
            railShooter.OnShoot += PlayEffect;
        }
        else if (player != null && !inMenu)
        {
            player.OnBulletFired += PlayEffect;
        }
    }

    private void OnDisable()
    {
        railShooter.OnShoot -= PlayEffect;
        if (player != null)
        {
            player.OnBulletFired -= PlayEffect;
        }
    }

    public void SetInMenu(bool isInMenu)
    {
        inMenu = isInMenu;
        if (inMenu)
        {
            railShooter.OnShoot += PlayEffect;
            if (player != null)
            {
                player.OnBulletFired -= PlayEffect;
            }
        }
        else
        {
            railShooter.OnShoot -= PlayEffect;
            if (player != null)
            {
                player.OnBulletFired += PlayEffect;
            }
        }
    }

    void PlayEffect(Ray ray)
    {
        screenFlash.DOColor(flashColor, 0);
        screenFlash.DOColor(Color.clear, 0).SetDelay(fadeEffectTime);

        if (spawnMuzzleFlash)
        {
            //Spawn bullet on the canvas
            var bullet = Instantiate(muzzleFlashPrefab, transform.GetChild(1).transform as RectTransform).transform as RectTransform;

            bullet.position = Input.mousePosition;
        }

        if (playSound)
        {
            AudioManager.instance.PlaySoundInternal(shootSound);
        }
    }

    void PlayEffect(bool miss, Vector2 hitPosition)
    {
        screenFlash.DOColor(flashColor, 0);
        screenFlash.DOColor(Color.clear, 0).SetDelay(fadeEffectTime);
    }
}