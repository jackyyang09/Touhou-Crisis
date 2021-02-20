using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UFOBehaviour : BaseEnemy
{
    ModularBox box = null;

    [SerializeField] Vector2 wanderTime = new Vector2(0.3f, 0.8f);
    [SerializeField] Vector2 waitTime = new Vector2(0.3f, 0.8f);
    [SerializeField] Vector2 timesToWander = new Vector2(3, 5);

    [SerializeField] float timeToDestroy = 2;

    Coroutine behaviourRoutine;

    void Awake()
    {
        //box = GameObject.Find("Special Knife Spawn Zone").GetComponent<ModularBox>();
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
        animator.Play("UFO_Hit");
    }

    protected override void Die()
    {
        if (behaviourRoutine != null)
        {
            StopCoroutine(behaviourRoutine);
        }

        rBody.useGravity = true;
        Destroy(gameObject, timeToDestroy);
    }
}
