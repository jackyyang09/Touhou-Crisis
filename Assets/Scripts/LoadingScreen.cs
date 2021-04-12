using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] bool hideOnSceneChange = true;
    [SerializeField] List<string> scenesToIgnore = null;
    [SerializeField] bool ignoreStartup = true;
    bool startupIgnored = false;

    [SerializeField] float fadeToBlackTime = 0.5f;
    [SerializeField] float fadeFromBlackTime = 1;

    [SerializeField] TMPro.TextMeshProUGUI loadingText = null;
    [SerializeField] Image blackImage = null;
    [SerializeField] OptimizedCanvas loadingAnimation = null;
    [SerializeField] CanvasGroup loadingAnimationGroup = null;

    private void Awake()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (hideOnSceneChange)
        {
            SceneManager.activeSceneChanged += HideLoadScreen;
        }
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= HideLoadScreen;
    }

    public void HideLoadScreen(Scene oldScene, Scene newScene)
    {
        if (ignoreStartup && !startupIgnored)
        {
            startupIgnored = true;
            return;
        }
        if (!scenesToIgnore.Contains(newScene.name))
        {
            StartCoroutine(HideRoutine());
        }
    }

    /// <summary>
    /// Proper usage
    /// yield return StartCoroutine(loadingScreen.ShowRoutine());
    /// </summary>
    /// <returns></returns>
    public IEnumerator ShowRoutine()
    {
        if (Lean.Localization.LeanLocalization.CurrentLanguage.Equals("English"))
        {
            loadingText.text = "Loading...";
        }
        else
        {
            loadingText.text = "少女祈祷中…";
        }

        blackImage.DOFade(1, fadeToBlackTime);

        loadingAnimation.Show();
        loadingAnimationGroup.enabled = true;

        DOTween.To(() => loadingAnimationGroup.alpha, x => loadingAnimationGroup.alpha = x, 1f, fadeToBlackTime);

        yield return new WaitForSeconds(fadeToBlackTime);

        yield return null;
    }

    public IEnumerator HideRoutine()
    {
        blackImage.DOFade(0, fadeFromBlackTime);
        DOTween.To(() => loadingAnimationGroup.alpha, x => loadingAnimationGroup.alpha = x, 0, fadeFromBlackTime);

        yield return new WaitForSeconds(fadeFromBlackTime);

        loadingAnimationGroup.enabled = false;
        loadingAnimation.Hide();

        // Self destruct if OG LoadingScreen is in this scene
        if (FindObjectsOfType<LoadingScreen>().Length > 1)
        {
            Destroy(gameObject);
        }

        yield return null;
    }
}
