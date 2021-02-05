using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class BaseEnemy : MonoBehaviour, IShootable
{
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int health;

    [SerializeField] protected Collider coliderl;
    [SerializeField] protected Rigidbody rBody;
    [SerializeField] protected Animator animator;

    [SerializeField] protected PlayerBehaviour player;

    [SerializeField] PhotonView photonView;

    /// <summary>
    /// What do you do when you get hit?
    /// </summary>
    [PunRPC]
    public void OnShotBehaviour(int damage)
    {
        TakeDamage(damage);
    }

    public virtual void TakeDamage(int d = 1)
    {
        bool alreadyDead = health == 0;

        health -= d;

        transform.Rotate(new Vector3(0, 0, 90));

        if (health == 0 && !alreadyDead)
        {
            StopAllCoroutines();
            Die();
        }

        //Flash();

        if (health <= 0)
        {

        }
    }

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
