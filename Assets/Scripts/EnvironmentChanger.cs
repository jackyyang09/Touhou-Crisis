using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class EnvironmentChanger : MonoBehaviour
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
    [SerializeField] SakuraPetals[] presets;

    [SerializeField] Sakuya sakuya = null;

    [SerializeField] int currentPhase = 0;

    int constantSpawnRateID;
    int setRandomVelocityID;
    int setLifetimeRandomID;
    int setSizeRandomID;
    int gravityID;

    private void Start()
    {
        constantSpawnRateID = Shader.PropertyToID("Constant Spawn Rate");
        setRandomVelocityID = Shader.PropertyToID("Set Random Velocity");
        setLifetimeRandomID = Shader.PropertyToID("Set Lifetime Random");
        setSizeRandomID = Shader.PropertyToID("Set Size Random");
        gravityID = Shader.PropertyToID("Gravity");
    }

    private void OnEnable()
    {
        sakuya.OnChangePhase += ChangePhase;
    }

    private void OnDisable()
    {
        sakuya.OnChangePhase -= ChangePhase;
    }

    void ChangePhase()
    {
        currentPhase++;

        var preset = presets[currentPhase];

        environmentEffect.SetInt(constantSpawnRateID, preset.constantSpawnRate);
        environmentEffect.SetVector2(setRandomVelocityID, preset.setRandomVelocity);
        environmentEffect.SetVector2(setLifetimeRandomID, preset.setLifetimeRandom);
        environmentEffect.SetVector2(setSizeRandomID, preset.setSizeRandom);
        environmentEffect.SetVector3(gravityID, preset.gravity);
    }
}
