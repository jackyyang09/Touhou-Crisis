using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSlideshow : MonoBehaviour
{
    int currentSlide;

    [SerializeField] Sprite[] graphics;
    public Sprite[] Graphics { get { return graphics; } }

    [SerializeField] UnityEngine.UI.Image graphicDisplay;
    [SerializeField] TMPro.TextMeshProUGUI tutorialLabel;
    [SerializeField] OptimizedCanvas canvas;

    public void GoToPreviousSlide()
    {
        currentSlide = (int)Mathf.Repeat(currentSlide - 1, graphics.Length);
        UpdateSlideDeck();
    }

    public void GoToNextSlide()
    {
        currentSlide = (int)Mathf.Repeat(currentSlide + 1, graphics.Length);
        UpdateSlideDeck();
    }

    public void UpdateSlideDeck()
    {
        tutorialLabel.text = Lean.Localization.LeanLocalization.GetTranslationText("Text/How to Play/Tutorial" + currentSlide.ToString("D2"));
        graphicDisplay.sprite = graphics[currentSlide];
    }

    public void ShowWithSlide(int index)
    {
        currentSlide = index;
        canvas.ShowDelayed(0.1f);
        UpdateSlideDeck();
    }
}
