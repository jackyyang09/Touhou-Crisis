using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ComboPuck : MonoBehaviour, IReloadable
{
    [SerializeField] PlayerBehaviour player;
    [SerializeField] bool hasPuck;
    [SerializeField] float incomingMultiplier = -1;

    [SerializeField] float maxMultiplier = 3;
    [SerializeField] float damageMultiplier = 1;
    public float Multliplier
    {
        get
        {
            if (hasPuck) return damageMultiplier;
            else return 1;
        }
    }

    [SerializeField] float comboDecayTime = 4;
    float comboDecayTimer;
    public float ComboDecayPercentage
    {
        get
        {
            return comboDecayTimer / comboDecayTime;
        }
    }

    public System.Action<float> OnUpdateMultiplier;
    public System.Action OnReceivePuck;
    public System.Action OnPassPuck;

    bool soloMode = false;

    public void Reinitialize()
    {
        hasPuck = PhotonNetwork.IsMasterClient || soloMode;
        comboDecayTimer = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);
        if (PhotonNetwork.OfflineMode || GameplayModifiers.Instance.GameMode == GameplayModifiers.GameModes.Versus)
        {
            soloMode = true;
        }
        Reinitialize();
    }

    private void OnEnable()
    {
        player.OnShotFired += CountCombo;
        player.OnEnterCover += PassReceivePuck;
        player.OnTakeDamage += DropCombo;
    }

    private void OnDisable()
    {
        player.OnShotFired -= CountCombo;
        player.OnEnterCover -= PassReceivePuck;
        player.OnTakeDamage -= DropCombo;
    }


    // Update is called once per frame
    void Update()
    {
        if (hasPuck)
        {
            comboDecayTimer = Mathf.Clamp(comboDecayTimer - Time.deltaTime, 0, comboDecayTime);
            if (comboDecayTimer == 0)
            {
                DropCombo(new DamageType());
            }
        }
    }

    private void CountCombo(bool miss, Vector2 arg2)
    {
        if (miss || !hasPuck) return;
        damageMultiplier = Mathf.Clamp(damageMultiplier + 0.1f, 0, maxMultiplier);
        comboDecayTimer = comboDecayTime;
        OnUpdateMultiplier?.Invoke(damageMultiplier);
    }

    void DropCombo(DamageType dmg)
    {
        comboDecayTimer = 0;
        damageMultiplier = 1;
        OnUpdateMultiplier?.Invoke(damageMultiplier);
    }

    private void PassReceivePuck()
    {
        if (hasPuck)
        {
            if (soloMode)
            {
                player.PhotonView.RPC(nameof(QueuePuck), RpcTarget.All, damageMultiplier);
            }
            else
            {
                PlayerManager.Instance.OtherPlayer.RPC(nameof(QueuePuck), RpcTarget.All, damageMultiplier);
            }
            hasPuck = false;
            damageMultiplier = 1;
            OnPassPuck?.Invoke();
        }
        else if (incomingMultiplier != -1)
        {
            comboDecayTimer = comboDecayTime;
            damageMultiplier = incomingMultiplier;
            incomingMultiplier = -1;
            hasPuck = true;
            OnReceivePuck?.Invoke();
            OnUpdateMultiplier?.Invoke(damageMultiplier);
        }
    }

    [PunRPC]
    public void QueuePuck(float multiplier)
    {
        incomingMultiplier = multiplier;
        if (!soloMode)
        {
            if (player.InCover)
            {
                PassReceivePuck();
            }
        }
    }
}
