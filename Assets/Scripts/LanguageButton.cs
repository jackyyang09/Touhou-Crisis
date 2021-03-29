using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Localization;

public class LanguageButton : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI text = null;

    private void OnEnable()
    {
        UpdateUI();
    }

    public void ToggleLanguage()
    {
        JSAM.AudioManager.PlaySound(MainMenuSounds.MenuButton);
        if (LeanLocalization.CurrentLanguage.Equals("English"))
        {
            LeanLocalization.CurrentLanguage = "Japanese";
            text.text = "JP";
        }
        else if (LeanLocalization.CurrentLanguage.Equals("Japanese"))
        {
            LeanLocalization.CurrentLanguage = "English";
            text.text = "EN";
        }
    }

    void UpdateUI()
    {
        if (LeanLocalization.CurrentLanguage.Equals("English"))
        {
            text.text = "EN";
        }
        else if (LeanLocalization.CurrentLanguage.Equals("Japanese"))
        {
            text.text = "JP";
        }
    }
}
