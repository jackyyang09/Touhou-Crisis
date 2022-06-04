using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
using DG.Tweening;

public class RailShooterEffects : MonoBehaviour
{
    public bool PlaySound = false;
    public bool SpawnMuzzleFlash = false;
    public bool InMenu = false;

    [SerializeField] RailShooterLogic railShooter = null;
    [SerializeField] PlayerBehaviour player = null;
    [SerializeField] JSAMSoundFileObject shootSound = null;

    [SerializeField] Color flashColor = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] UnityEngine.UI.Image screenFlash = null;
    [SerializeField] float fadeEffectTime = 0.05f;

    [SerializeField] GameObject muzzleFlashPrefab = null;
    [SerializeField] RectTransform muzzleFlashCanvas = null;

    //private void Start()
    //{
    //}

    private void OnEnable()
    {
        TryEnableShootEffects();
        if (player)
        {
            EnableScreenFlashes();
        }
    }

    private void OnDisable()
    {
        railShooter.OnShoot -= PlayEffect;
        if (player)
        {
            DisableScreenFlashes();
        }
    }

    public void TryEnableShootEffects()
    {
        if (InMenu)
        {
            railShooter.OnShoot += PlayEffect;
        }
    }

    public void DisableShootEffects()
    {
        railShooter.OnShoot -= PlayEffect;
    }

    /// <summary>
    /// Plays the default muzzle flash effect when in the game over screen
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="screenPoint"></param>
    public void PlayEffect(Ray ray, Vector2 screenPoint)
    {
        if (!InMenu) return;

        if (SpawnMuzzleFlash)
        {
            //Spawn bullet on the canvas
            var bullet = Instantiate(muzzleFlashPrefab, muzzleFlashCanvas).transform as RectTransform;

            bullet.position = screenPoint;
        }

        if (PlaySound)
        {
            AudioManager.PlaySound(shootSound);
        }
    }

    bool screenFlashesSubbed = false;
    public void EnableScreenFlashes()
    {
        if (!screenFlashesSubbed && PlayerPrefs.GetInt(PauseMenu.ScreenFlashKey) == 1)
        {
            player.OnShotFired += PlayScreenFlashEffect;
            screenFlashesSubbed = true;
        }
    }

    public void DisableScreenFlashes()
    {
        player.OnShotFired -= PlayScreenFlashEffect;
        screenFlashesSubbed = false;
    }

    /// <summary>
    /// Play Screen Flash effect, but only when in-game
    /// </summary>
    /// <param name="miss"></param>
    /// <param name="hitPosition"></param>
    void PlayScreenFlashEffect(bool miss, Vector2 hitPosition)
    {
        if (InMenu) return;
        screenFlash.DOColor(flashColor, 0);
        screenFlash.DOColor(Color.clear, 0).SetDelay(fadeEffectTime);
    }
}