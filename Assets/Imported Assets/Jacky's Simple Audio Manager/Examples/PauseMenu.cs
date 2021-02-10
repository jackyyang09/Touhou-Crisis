using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JSAM
{
    public class PauseMenu : MonoBehaviour
    {
        const string MasterVolumeKey = "MASTER_VOLUME";
        const string MusicVolumeKey = "MUSIC_VOLUME";
        const string SoundVolumeKey = "SOUND_VOLUME";
        const string CrosshairKey = "USE_CROSSHAIR";
        const string ScreenFlashKey = "USE_SCREENFLASH";

        [SerializeField] Slider masterSlider = null;
        [SerializeField] Slider musicSlider = null;
        [SerializeField] Slider soundSlider = null;

        /// <summary>
        /// Button used to toggle the pause menu, incompatible with Unity's new input manager
        /// </summary>
        [Tooltip("Button used to toggle the pause menu, incompatible with Unity's new input manager")]
        [SerializeField]
        KeyCode toggleButton = KeyCode.Escape;

        [SerializeField] OptimizedCanvas canvas;

        [SerializeField] Image crossHairButton;
        [SerializeField] Image screenFlashButton;

        Crosshair crosshair;
        GameObject flashEffect;

        void Awake()
        {
            crosshair = FindObjectOfType<Crosshair>();
            flashEffect = GameObject.Find("Lightgun Flash");

            if (!PlayerPrefs.HasKey(CrosshairKey))
            {
                PlayerPrefs.SetInt(CrosshairKey, 1);
            }

            if (!PlayerPrefs.HasKey(ScreenFlashKey))
            {
                PlayerPrefs.SetInt(ScreenFlashKey, 1);
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

        //void OnDisable()
        //{
        //    SaveVolumeSettings();
        //}

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(toggleButton))
            {
                canvas.SetActive(!canvas.IsVisible);
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
            if (PlayerPrefs.HasKey(CrosshairKey))
            {
                crosshair.gameObject.SetActive(PlayerPrefs.GetInt(CrosshairKey) == 1);
            }

            if (PlayerPrefs.HasKey(ScreenFlashKey))
            {
                flashEffect.gameObject.SetActive(PlayerPrefs.GetInt(ScreenFlashKey) == 1);
            }
        }

        public void LoadToggleSettings()
        {
            if (PlayerPrefs.HasKey(CrosshairKey))
            {
                if (PlayerPrefs.GetInt(CrosshairKey) == 1)
                {
                    crossHairButton.color = Color.red;
                }
                else
                {
                    crossHairButton.color = Color.white;
                }
            }

            if (PlayerPrefs.HasKey(ScreenFlashKey))
            {
                if (PlayerPrefs.GetInt(ScreenFlashKey) == 1)
                {
                    screenFlashButton.color = Color.red;
                }
                else
                {
                    screenFlashButton.color = Color.white;
                }
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
            crosshair.enabled = PlayerPrefs.GetInt(CrosshairKey) == 1;
            LoadToggleSettings();
        }
    }
}