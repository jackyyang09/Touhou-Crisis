using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace JSAM
{
    public class PauseMenu : MonoBehaviour
    {
        public const string MasterVolumeKey = "MASTER_VOLUME";
        public const string MusicVolumeKey = "MUSIC_VOLUME";
        public const string SoundVolumeKey = "SOUND_VOLUME";
        public const string CrosshairKey = "USE_CROSSHAIR";
        public const string ScreenFlashKey = "USE_SCREENFLASH";
        public const string HideCursorKey = "HIDE_CURSOR";
        public const string FireInputKey = "FIRE_KEYBIND";
        public const string CoverInputKey = "COVER_KEYBIND";

        [SerializeField] Slider masterSlider = null;
        [SerializeField] Slider musicSlider = null;
        [SerializeField] Slider soundSlider = null;

        [SerializeField] KeyInputEvents inputEvents;

        /// <summary>
        /// Button used to toggle the pause menu, incompatible with Unity's new input manager
        /// </summary>
        [Tooltip("Button used to toggle the pause menu, incompatible with Unity's new input manager")]
        [SerializeField]
        KeyCode toggleButton = KeyCode.Escape;

        [SerializeField] OptimizedCanvas canvas = null;

        [SerializeField] Image crossHairButton = null;
        [SerializeField] Image screenFlashButton = null;
        [SerializeField] Image hideCursorButton = null;

        [SerializeField] OptimizedCanvas rebindInterface = null;
        [SerializeField] TMPro.TextMeshProUGUI rebindText = null;

        GameObject flashEffect = null;

        [SerializeField] Image rebindMask;
        Coroutine rebindRoutine = null;

        RailShooterLogic railShooter = null;

        void Awake()
        {
            var flashEffects = FindObjectsOfType<RailShooterEffects>();

            if (flashEffects.Length > 1)
            {
                for (int i = 0; i < flashEffects.Length; i++)
                {
                    // Do these objects share the same root?
                    if (flashEffects[i].transform.root == transform.root)
                    {
                        flashEffect = flashEffects[i].transform.GetChild(0).gameObject;
                        break;
                    }
                }
            }
            else
            {
                flashEffect = flashEffects[0].transform.GetChild(0).gameObject;
            }

            if (!PlayerPrefs.HasKey(CrosshairKey))
            {
                PlayerPrefs.SetInt(CrosshairKey, 1);
            }

            if (!PlayerPrefs.HasKey(ScreenFlashKey))
            {
                PlayerPrefs.SetInt(ScreenFlashKey, 0);
            }

            if (!PlayerPrefs.HasKey(HideCursorKey))
            {
                PlayerPrefs.SetInt(HideCursorKey, 0);
            }

            if (!PlayerPrefs.HasKey(FireInputKey))
            {
                PlayerPrefs.SetInt(FireInputKey, (int)KeyCode.Space);
            }

            if (!PlayerPrefs.HasKey(CoverInputKey))
            {
                PlayerPrefs.SetInt(CoverInputKey, (int)KeyCode.Space);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (AudioManager.instance)
            {
                LoadVolumeSettings();
                LoadSliderSettings();
                LoadMiscSettings();
                LoadToggleSettings();
            }
        }

        private void OnEnable()
        {
            railShooter = FindObjectOfType<RailShooterLogic>();

            railShooter.OnShoot += DeselectEverything;
        }

        void OnDisable()
        {
            railShooter.OnShoot -= DeselectEverything;
        }

        private void DeselectEverything(Ray obj, Vector2 vector2)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(toggleButton))
            {
                canvas.SetActive(true);
            }

            //if (canvas.IsVisible)
            //{
            //    if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
            //    {
            //        // Sometimes the user has custom cursor locking code
            //        Cursor.lockState = CursorLockMode.None;
            //        Cursor.visible = true;
            //    }
            //}
        }

        public void LoadVolumeSettings()
        {
            if (PlayerPrefs.HasKey(MasterVolumeKey))
            {
                AudioManager.SetMasterVolume(PlayerPrefs.GetFloat(MasterVolumeKey));
            }

            if (PlayerPrefs.HasKey(MusicVolumeKey))
            {
                AudioManager.SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumeKey));
            }

            if (PlayerPrefs.HasKey(SoundVolumeKey))
            {
                AudioManager.SetSoundVolume(PlayerPrefs.GetFloat(SoundVolumeKey));
            }
        }

        public void LoadSliderSettings()
        {
            masterSlider.value = AudioManager.GetMasterVolume();
            musicSlider.value = AudioManager.GetMusicVolume();
            soundSlider.value = AudioManager.GetSoundVolume();
        }

        public void LoadMiscSettings()
        {
            if (PlayerPrefs.HasKey(CrosshairKey) && Crosshair.Instance != null)
            {
                Crosshair.Instance.enabled = PlayerPrefs.GetInt(CrosshairKey) == 1;
            }

            if (PlayerPrefs.HasKey(ScreenFlashKey))
            {
                flashEffect.gameObject.SetActive(PlayerPrefs.GetInt(ScreenFlashKey) == 1);
            }

            if (PlayerPrefs.HasKey(HideCursorKey))
            {
                Cursor.visible = !(PlayerPrefs.GetInt(HideCursorKey) == 1);
            }
        }

        /// <summary>
        /// Sets the corresponding settings menu buttons to be red or white
        /// </summary>
        public void LoadToggleSettings()
        {
            if (PlayerPrefs.HasKey(CrosshairKey))
            {
                crossHairButton.color = PlayerPrefs.GetInt(CrosshairKey) == 1 ? Color.red : Color.white;
            }

            if (PlayerPrefs.HasKey(ScreenFlashKey))
            {
                screenFlashButton.color = PlayerPrefs.GetInt(ScreenFlashKey) == 1 ? Color.red : Color.white;
            }

            if (PlayerPrefs.HasKey(HideCursorKey))
            {
                hideCursorButton.color = PlayerPrefs.GetInt(HideCursorKey) == 1 ? Color.red : Color.white;
            }
        }

        public void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, AudioManager.GetMasterVolume());
            PlayerPrefs.SetFloat(MusicVolumeKey, AudioManager.GetMusicVolume());
            PlayerPrefs.SetFloat(SoundVolumeKey, AudioManager.GetSoundVolume());
            PlayerPrefs.Save();
        }

        public void ApplyMasterVolume()
        {
            AudioManager.SetMasterVolume(masterSlider.value);
        }

        public void ApplyMusicVolume()
        {
            AudioManager.SetMusicVolume(musicSlider.value);
        }

        public void ApplySoundVolume()
        {
            AudioManager.SetSoundVolume(soundSlider.value);
        }

        public void ApplyScreenFlashSettings()
        {
            PlayerPrefs.SetInt(ScreenFlashKey, (int)Mathf.Repeat(PlayerPrefs.GetInt(ScreenFlashKey) + 1, 2f));
            PlayerPrefs.Save();
            flashEffect.gameObject.SetActive(PlayerPrefs.GetInt(ScreenFlashKey) == 1);
            LoadToggleSettings();
        }

        public void ApplyCrosshairSettings()
        {
            PlayerPrefs.SetInt(CrosshairKey, (int)Mathf.Repeat(PlayerPrefs.GetInt(CrosshairKey) + 1, 2f));
            PlayerPrefs.Save();
            Crosshair.Instance.enabled = PlayerPrefs.GetInt(CrosshairKey) == 1;
            LoadToggleSettings();
        }

        public void ApplyCursorSettings()
        {
            PlayerPrefs.SetInt(HideCursorKey, (int)Mathf.Repeat(PlayerPrefs.GetInt(HideCursorKey) + 1, 2f));
            PlayerPrefs.Save();
            Cursor.visible = !(PlayerPrefs.GetInt(HideCursorKey) == 1);
            LoadToggleSettings();
        }

        public void RebindKeys()
        {
            if (rebindRoutine == null)
            {
                rebindRoutine = StartCoroutine(KeyRebindInterface());
            }

            inputEvents.OnKeyDown += OnKeyDown;
        }

        KeyCode lastKeyDown;
        void OnKeyDown(KeyCode keyCode) => lastKeyDown = keyCode;

        IEnumerator KeyRebindInterface()
        {
            bool cancel = false;
            inputEvents.enabled = true;
            lastKeyDown = KeyCode.None;
            rebindInterface.Show();
            rebindInterface.rectTransform.DOScaleX(0, 0);

            rebindMask.enabled = true;

            #region Rebind Cover Key
            KeyCode newCoverKey = (KeyCode)PlayerPrefs.GetInt(CoverInputKey);

            // Set new crouch key binding
            bool keyFound = false;
            if (!cancel)
            {
                AudioManager.PlaySound(MainMenuSounds.MenuButton);
                rebindText.text =
                "PRESS COVER BUTTON, \"ESC\" TO CANCEL\n" +
                "(CURRENTLY " + "\"" + newCoverKey + "\"" + ")";
                rebindInterface.rectTransform.DOScaleX(1, 0.125f);
                yield return new WaitForSeconds(0.25f);
            }

            if (!cancel)
            {
                do
                {
                    if (lastKeyDown != KeyCode.None)
                    {
                        if (lastKeyDown == KeyCode.Escape)
                        {
                            cancel = true;
                            break;
                        }
                        else
                        {
                            keyFound = true;
                            newCoverKey = lastKeyDown;
                            break;
                        }
                    }
                    yield return null;
                }
                while (!keyFound);
            }

            // Confirm new bindings
            keyFound = false;
            lastKeyDown = KeyCode.None;
            if (!cancel)
            {
                AudioManager.PlaySound(MainMenuSounds.MenuButton);
                rebindInterface.rectTransform.DOScaleX(0, 0.125f);
                rebindText.text =
                "NEW COVER BUTTON: " + newCoverKey + "\n" +
                "PRESS " + "\"" + newCoverKey + "\"" + " TO CONFIRM, PRESS \"ESC\" TO CANCEL";
                rebindInterface.rectTransform.DOScaleX(1, 0.125f).SetDelay(0.125f);
                yield return new WaitForSeconds(0.25f);
            }

            if (!cancel)
            {
                // Set new crouch key binding
                do
                {
                    if (lastKeyDown != KeyCode.None)
                    {
                        if (lastKeyDown == KeyCode.Escape)
                        {
                            cancel = true;
                            break;
                        }
                        else if (lastKeyDown == newCoverKey)
                        {
                            keyFound = true;
                            newCoverKey = lastKeyDown;
                            break;
                        }
                    }
                    yield return null;
                }
                while (!keyFound);
            }

            if (!cancel)
            {
                AudioManager.PlaySound(MainMenuSounds.PlayerJoin);
                PlayerPrefs.SetInt(CoverInputKey, (int)newCoverKey);
                PlayerPrefs.Save();

                rebindInterface.rectTransform.DOScaleX(0, 0.125f);
                rebindText.text = string.Empty;
                yield return new WaitForSeconds(0.125f);
            }
            #endregion

            lastKeyDown = KeyCode.None;

            // Don't actually implement this unless you want to work with PointerEvents again
            #region Rebind Fire Key
            //KeyCode newFireKey = (KeyCode)PlayerPrefs.GetInt(FireInputKey);
            //
            //// Set new crouch key binding
            //keyFound = false;
            //if (!cancel)
            //{
            //    AudioManager.PlaySound(MainMenuSounds.MenuButton);
            //    rebindText.text =
            //    "PRESS FIRE BUTTON, \"ESC\" TO CANCEL\n" +
            //    "(CURRENTLY " + "\"" + newFireKey + "\"" + ")";
            //    rebindInterface.rectTransform.DOScaleX(1, 0.125f);
            //    yield return new WaitForSeconds(0.25f);
            //}
            //
            //if (!cancel)
            //{
            //    do
            //    {
            //        if (lastKeyDown != KeyCode.None)
            //        {
            //            if (lastKeyDown == KeyCode.Escape)
            //            {
            //                cancel = true;
            //                break;
            //            }
            //            else
            //            {
            //                keyFound = true;
            //                newFireKey = lastKeyDown;
            //                break;
            //            }
            //        }
            //        yield return null;
            //    }
            //    while (!keyFound);
            //}
            //
            //// Confirm new bindings
            //keyFound = false;
            //lastKeyDown = KeyCode.None;
            //if (!cancel)
            //{
            //    AudioManager.PlaySound(MainMenuSounds.MenuButton);
            //    rebindInterface.rectTransform.DOScaleX(0, 0.125f);
            //    rebindText.text =
            //    "NEW FIRE BUTTON: " + newFireKey + "\n" +
            //    "PRESS " + "\"" + newFireKey + "\"" + " TO CONFIRM, PRESS \"ESC\" TO CANCEL";
            //    rebindInterface.rectTransform.DOScaleX(1, 0.125f).SetDelay(0.125f);
            //    yield return new WaitForSeconds(0.25f);
            //}
            //
            //if (!cancel)
            //{
            //    // Set new crouch key binding
            //    do
            //    {
            //        if (lastKeyDown != KeyCode.None)
            //        {
            //            if (lastKeyDown == KeyCode.Escape)
            //            {
            //                cancel = true;
            //                break;
            //            }
            //            else if (lastKeyDown == newFireKey)
            //            {
            //                keyFound = true;
            //                newFireKey = lastKeyDown;
            //                break;
            //            }
            //        }
            //        yield return null;
            //    }
            //    while (!keyFound);
            //}
            //
            //if (!cancel)
            //{
            //    AudioManager.PlaySound(MainMenuSounds.PlayerJoin);
            //    PlayerPrefs.SetInt(FireInputKey, (int)newFireKey);
            //    PlayerPrefs.Save();
            //    FindObjectOfType<RailShooterLogic>().RebindFireKey(newFireKey);
            //
            //    rebindInterface.rectTransform.DOScaleX(0, 0.125f);
            //    rebindText.text = string.Empty;
            //    yield return new WaitForSeconds(0.125f);
            //}
            #endregion

            rebindMask.enabled = false;
            inputEvents.OnKeyDown -= OnKeyDown;
            inputEvents.enabled = false;
            rebindRoutine = null;
            rebindInterface.Hide();
        }
    }
}