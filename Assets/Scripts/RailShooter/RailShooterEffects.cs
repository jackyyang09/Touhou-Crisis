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
    [SerializeField] RectTransform muzzleFlashCanvas = null;
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
            player.OnShotFired += PlayScreenFlashEffect;
        }
    }

    private void OnDisable()
    {
        railShooter.OnShoot -= PlayEffect;
        if (player != null)
        {
            player.OnShotFired -= PlayScreenFlashEffect;
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
                player.OnShotFired -= PlayScreenFlashEffect;
            }
        }
        else
        {
            railShooter.OnShoot -= PlayEffect;
            if (player != null)
            {
                player.OnShotFired += PlayScreenFlashEffect;
            }
        }
    }

    /// <summary>
    /// Plays the default muzzle flash effect when in the game over screen
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="screenPoint"></param>
    public void PlayEffect(Ray ray, Vector2 screenPoint)
    {
        if (spawnMuzzleFlash)
        {
            //Spawn bullet on the canvas
            var bullet = Instantiate(muzzleFlashPrefab, muzzleFlashCanvas).transform as RectTransform;

            bullet.position = screenPoint;
        }

        if (playSound)
        {
            AudioManager.Instance.PlaySoundInternal(shootSound);
        }
    }

    /// <summary>
    /// Play Screen Flash effect, but only when in-game
    /// </summary>
    /// <param name="miss"></param>
    /// <param name="hitPosition"></param>
    void PlayScreenFlashEffect(bool miss, Vector2 hitPosition)
    {
        screenFlash.DOColor(flashColor, 0);
        screenFlash.DOColor(Color.clear, 0).SetDelay(fadeEffectTime);
    }
}