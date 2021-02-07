using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ComboPuck : MonoBehaviour
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
            return damageMultiplier;
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

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            hasPuck = true;
        }
        if (PhotonNetwork.PlayerListOthers.Length == 0)
        {
            soloMode = true;
        }
        //else
        //{
            comboDecayTimer = 0;
        //}
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

    private void OnEnable()
    {
        player.OnBulletFired += CountCombo;
        player.OnReload += PassReceivePuck;
        player.OnTakeDamage += DropCombo;
    }

    private void OnDisable()
    {
        player.OnBulletFired -= CountCombo;
        player.OnReload -= PassReceivePuck;
        player.OnTakeDamage -= DropCombo;
    }

    private void CountCombo(bool missed, Vector2 hitPosition)
    {
        if (missed || !hasPuck) return;
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
                player.PhotonView.RPC("QueuePuck", RpcTarget.All, damageMultiplier);
            }
            else
            {
                PlayerManager.Instance.OtherPlayer.RPC("QueuePuck", RpcTarget.All, damageMultiplier);
            }
            hasPuck = false;
            damageMultiplier = 1;
            OnPassPuck?.Invoke();
        }
        else if (incomingMultiplier != -1)
        {
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
