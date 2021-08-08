using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

public class PuckUI : MonoBehaviour, IReloadable
{
    [SerializeField] PlayerBehaviour player = null;
    [SerializeField] ComboPuck puck = null;
    [SerializeField] RectTransform puckImage = null;
    [SerializeField] Image puckFill = null;
    [SerializeField] Color puckFlashColour = Color.white;
    [SerializeField] TextMeshProUGUI comboText = null;

    [SerializeField] bool isPlayerTwo = false;

    [SerializeField] OptimizedCanvas canvas = null;

    public void Reinitialize()
    {
        // Player 1 starts with puck
        if (isPlayerTwo)
        {
            // Pass
            puckImage.DORotate(new Vector3(0, 0, 180), 0, RotateMode.LocalAxisAdd);
        }
        else
        {
            // Receive
            puckImage.DORotate(new Vector3(0, 0, 180), 0, RotateMode.LocalAxisAdd);
        }
    }

    private void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);
    }

    private void OnEnable()
    {
        player.OnTakeDamage += ShakePuck;

        puck.OnPassPuck += PassPuckEffect;
        puck.OnReceivePuck += ReceivePuckEffect;
        puck.OnUpdateMultiplier += UpdateComboMultiplier;

        canvas.Show();
    }

    private void OnDisable()
    {
        player.OnTakeDamage -= ShakePuck;

        puck.OnPassPuck -= PassPuckEffect;
        puck.OnReceivePuck -= ReceivePuckEffect;
        puck.OnUpdateMultiplier -= UpdateComboMultiplier;

        canvas.Hide();
    }

    private void Update()
    {
        UpdateComboDecay();
    }

    void UpdateComboDecay()
    {
        puckFill.fillAmount = puck.ComboDecayPercentage;
    }

    private void PassPuckEffect()
    {
        puckImage.localEulerAngles = Vector3.zero;
        puckImage.DOComplete();
        if (isPlayerTwo)
        {
            puckImage.DORotate(new Vector3(0, 0, 180), 0.5f, RotateMode.LocalAxisAdd);
        }
        else
        {
            puckImage.DORotate(new Vector3(0, 0, -180), 0.5f, RotateMode.LocalAxisAdd);
        }
    }

    private void ReceivePuckEffect()
    {
        puckImage.localEulerAngles = new Vector3(0, 0, -180);
        puckImage.DOComplete();
        if (isPlayerTwo)
        {
            puckImage.DORotate(new Vector3(0, 0, -180), 0.25f, RotateMode.LocalAxisAdd);
        }
        else
        {
            puckImage.DORotate(new Vector3(0, 0, 180), 0.25f, RotateMode.LocalAxisAdd);
        }
        puckFill.DOColor(puckFlashColour, 0).SetDelay(0.25f);
        puckFill.DOColor(Color.white, 0.5f).SetDelay(0.5f);
    }

    void UpdateComboMultiplier(float comboCount)
    {
        comboText.text = comboCount.ToString("0.0") + "x";
    }

    void ShakePuck(DamageType type)
    {
        // Prevent the shakes from overlapping
        puckImage.DOComplete();
        puckImage.DOShakeAnchorPos(1, 50, 50, 80);
    }
}
