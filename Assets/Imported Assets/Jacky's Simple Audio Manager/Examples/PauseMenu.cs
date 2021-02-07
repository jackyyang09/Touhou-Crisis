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

        [SerializeField] Slider masterSlider = null;
        [SerializeField] Slider musicSlider = null;
        [SerializeField] Slider soundSlider = null;

        /// <summary>
        /// Button used to toggle the pause menu, incompatible with Unity's new input manager
        /// </summary>
        [Tooltip("Button used to toggle the pause menu, incompatible with Unity's new input manager")]
        [SerializeField]
        KeyCode toggleButton = KeyCode.Escape;

        Canvas pauseMenu;

        // Start is called before the first frame update
        void Awake()
        {
            pauseMenu = GetComponent<Canvas>();

            if (AudioManager.instance)
            {
                LoadVolumeSettings();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(toggleButton))
            {
                pauseMenu.enabled = !pauseMenu.enabled;
            }

            if (pauseMenu.enabled)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
                {
                    // Sometimes the user has custom cursor locking code
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
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

        public void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, AudioManager.GetMasterVolume());
            PlayerPrefs.SetFloat(MusicVolumeKey, AudioManager.GetMusicVolume());
            PlayerPrefs.SetFloat(SoundVolumeKey, AudioManager.GetSoundVolume());
            PlayerPrefs.Save();
        }

        public void LoadSliderSettings()
        {
            masterSlider.value = AudioManager.GetMasterVolume();
            musicSlider.value = AudioManager.GetMusicVolume();
            soundSlider.value = AudioManager.GetSoundVolume();
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
    }
}