using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using DG.Tweening;

public class EnvironmentChanger : MonoBehaviour, IReloadable
{
    [System.Serializable]
    public struct SakuraPetals
    {
        public int constantSpawnRate;
        public Vector2 setRandomVelocity;
        public Vector2 setLifetimeRandom;
        public Vector2 setSizeRandom;
        public Vector3 gravity;
    }

    [SerializeField] VisualEffect environmentEffect = null;
    [SerializeField] SakuraPetals[] presets = null;

    [SerializeField] float environmentChangeTime = 3;

    [Header("Post-Processing Effects")]
    [SerializeField] UnityEngine.Rendering.Volume normalVolume = null;
    [SerializeField] UnityEngine.Rendering.Volume finalVolume = null;

    [Header("Skybox")]
    [SerializeField] Material[] skyboxes = null;
    Material activeSkybox = null;

    [SerializeField] Light mainLight = null;
    [SerializeField] Light[] referenceLights = null;

    [SerializeField] Sakuya sakuya = null;

    [SerializeField] GameObject[] environments = null;

    int constantSpawnRateID;
    int setRandomVelocityID;
    int setLifetimeRandomID;
    int setSizeRandomID;
    int gravityID;

    int exposureID;
    int tintColorID;

    public void Reinitialize()
    {
        ChangePhase(0);

        environmentEffect.Reinit();

        activeSkybox.SetColor(tintColorID, skyboxes[0].GetColor(tintColorID));
        activeSkybox.SetFloat(exposureID, skyboxes[0].GetFloat(exposureID));

        normalVolume.weight = 1;
        finalVolume.weight = 0;
    }

    private void Awake()
    {
        constantSpawnRateID = Shader.PropertyToID("Constant Spawn Rate");
        setRandomVelocityID = Shader.PropertyToID("Set Random Velocity");
        setLifetimeRandomID = Shader.PropertyToID("Set Lifetime Random");
        setSizeRandomID = Shader.PropertyToID("Set Size Random");
        gravityID = Shader.PropertyToID("Gravity");

        exposureID = Shader.PropertyToID("_Exposure");
        tintColorID = Shader.PropertyToID("_Tint");

        activeSkybox = RenderSettings.skybox;
    }

    private void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);

        activeSkybox.SetColor(tintColorID, skyboxes[0].GetColor(tintColorID));
        activeSkybox.SetFloat(exposureID, skyboxes[0].GetFloat(exposureID));
    }

    private void OnEnable()
    {
        sakuya.OnChangePhase += ChangePhase;
    }

    private void OnDisable()
    {
        sakuya.OnChangePhase -= ChangePhase;
    }

    void ChangePhase(int currentPhase)
    {
        var preset = presets[currentPhase];

        environmentEffect.SetInt(constantSpawnRateID, preset.constantSpawnRate);
        environmentEffect.SetVector2(setRandomVelocityID, preset.setRandomVelocity);
        environmentEffect.SetVector2(setLifetimeRandomID, preset.setLifetimeRandom);
        environmentEffect.SetVector2(setSizeRandomID, preset.setSizeRandom);
        environmentEffect.SetVector3(gravityID, preset.gravity);

        for (int i = 0; i < environments.Length; i++)
        {
            environments[i].SetActive(false);
        }
        environments[currentPhase].SetActive(true);

        mainLight.DOIntensity(referenceLights[currentPhase].intensity, environmentChangeTime);

        if (currentPhase == 3)
        {
            DOTween.To(() => normalVolume.weight, x => normalVolume.weight = x, 0f, environmentChangeTime);
            DOTween.To(() => finalVolume.weight, x => finalVolume.weight = x, 1, environmentChangeTime);

            DOTween.To(() => activeSkybox.GetColor(tintColorID), 
                x => activeSkybox.SetColor(tintColorID, x), 
                skyboxes[currentPhase].GetColor(tintColorID), 
                environmentChangeTime);

            DOTween.To(() => activeSkybox.GetFloat(exposureID),
                x => activeSkybox.SetFloat(exposureID, x),
                skyboxes[currentPhase].GetFloat(exposureID),
                environmentChangeTime);
        }
    }
}