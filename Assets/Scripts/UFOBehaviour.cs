using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UFOBehaviour : BaseEnemy
{
    public enum UFOType
    {
        Green,
        Blue,
        Red
    }

    [SerializeField] UFOType ufoType = UFOType.Blue;

    ModularBox box = null;

    [SerializeField] Vector2 wanderTime = new Vector2(0.3f, 0.8f);
    [SerializeField] Vector2 waitTime = new Vector2(0.3f, 0.8f);
    [SerializeField] Vector2 timesToWander = new Vector2(3, 5);

    [SerializeField] float timeToDestroy = 2;

    [SerializeField] float explosionForce = 20;
    [SerializeField] float upwardsModifier = 1.5f;
    [SerializeField] Transform explosionOrigin = null;

    Coroutine behaviourRoutine;

    public System.Action OnUFOExpire;

    void Awake()
    {
        //box = GameObject.Find("Special Knife Spawn Zone").GetComponent<ModularBox>();
        collider.isTrigger = true;
    }

    private void OnDisable()
    {
        OnUFOExpire?.Invoke();
        OnUFOExpire = null;
    }

    public void Init(ModularBox spawnBox)
    {
        box = spawnBox;

        if (behaviourRoutine == null)
        {
            behaviourRoutine = StartCoroutine(BehaviourTree());
        }
    }

    IEnumerator BehaviourTree()
    {
        int wanderNum = Random.Range((int)timesToWander.x, (int)timesToWander.y + 1);
        for (int i = 0; i < wanderNum; i++)
        {
            switch (ufoType)
            {
                case UFOType.Green:
                    break;
                case UFOType.Blue:
                    break;
                case UFOType.Red:
                    break;
            }
            Vector3 destination = box.GetRandomPointInBox();
            transform.DOMove(destination, Random.Range(wanderTime.x, wanderTime.y));
            yield return new WaitForSeconds(Random.Range(waitTime.x, waitTime.y));
        }

        transform.DOMoveY(10, 1);

        Destroy(gameObject, 1);
        //behaviourRoutine = null;
    }

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    protected override void DamageFlash()
    {
        animator.SetTrigger("Hit");
    }

    protected override void Die()
    {
        if (behaviourRoutine != null)
        {
            StopCoroutine(behaviourRoutine);
            transform.DOKill();
        }

        collider.isTrigger = false;
        rBody.useGravity = true;
        rBody.AddExplosionForce(explosionForce, explosionOrigin.position, 1, upwardsModifier);
        Destroy(gameObject, timeToDestroy);
    }
}