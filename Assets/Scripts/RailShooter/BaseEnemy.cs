using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class BaseEnemy : MonoBehaviour, IShootable
{
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float health;
    public float HealthPercentage
    {
        get
        {
            return health / maxHealth;
        }
    }

    [SerializeField] new protected Collider collider;
    [SerializeField] protected Rigidbody rBody;
    [SerializeField] protected Animator animator;

    [SerializeField] protected PlayerBehaviour player;

    [SerializeField] protected PhotonView photonView;

    [SerializeField] protected bool canTakeDamage = true;

    public System.Action OnShot;

    /// <summary>
    /// What do you do when you get hit?
    /// </summary>
    [PunRPC]
    public void OnShotBehaviour(float damage)
    {
        TakeDamage(damage);
        OnShot?.Invoke();
    }

    public virtual void TakeDamage(float d = 1)
    {
        if (!canTakeDamage) return;
        bool alreadyDead = health == 0;

        health -= d;

        if (health <= 0 && !alreadyDead)
        {
            StopAllCoroutines();
            Die();
        }

        DamageFlash();

        if (health <= 0)
        {

        }
    }

    protected abstract void DamageFlash();

    /// <summary>
    /// Plays a voice line, done through the animator
    /// </summary>
    public virtual void PlayRetort()
    {
    }

    protected virtual void Die()
    {
    }
}
