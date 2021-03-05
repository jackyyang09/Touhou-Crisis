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

    [SerializeField] float scoreValue = 100;

    [Header("Green Properties")]
    [SerializeField] float greenTravelTime = 8;

    [Header("Red Properties")]
    [SerializeField] float redChargeTime = 2;
    [SerializeField] float redRotateTime = 0.5f;

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
        switch (ufoType)
        {
            case UFOType.Green:
                {
                    Vector3 destination = transform.position.ScaleBetter(new Vector3(-1, 1, 1));
                    transform.DOMove(destination, greenTravelTime).SetEase(Ease.Linear);
                    yield return new WaitForSeconds(greenTravelTime);
                    Destroy(gameObject);
                }
                break;
            case UFOType.Blue:
                {
                    int wanderNum = Random.Range((int)timesToWander.x, (int)timesToWander.y + 1);
                    while (true)
                    {
                        Vector3 destination = box.GetRandomPointInBox();
                        float moveTime = Random.Range(wanderTime.x, wanderTime.y);
                        transform.DOMove(destination, moveTime);
                        yield return new WaitForSeconds(moveTime);
                        yield return new WaitForSeconds(Random.Range(waitTime.x, waitTime.y));
                    }
                }
            case UFOType.Red:
                {
                    int wanderNum = Random.Range((int)timesToWander.x, (int)timesToWander.y + 1);
                    for (int i = 0; i < wanderNum; i++)
                    {
                        Vector3 destination = box.GetRandomPointInBox();
                        transform.DOMove(destination, Random.Range(wanderTime.x, wanderTime.y));
                        yield return new WaitForSeconds(Random.Range(waitTime.x, waitTime.y));
                    }
                    Vector3 target = AreaLogic.Instance.Player1FireTransform.position;
                    transform.DOMove(target, redChargeTime).SetEase(Ease.InSine);
                    transform.DOLookAt(target, redRotateTime).SetEase(Ease.Linear);
                    Destroy(gameObject, redChargeTime);
                    yield return new WaitForSeconds(redRotateTime);
                    transform.DOLocalRotate(new Vector3(30, 0, 0), redRotateTime, RotateMode.LocalAxisAdd).SetEase(Ease.Linear);
                }
                break;
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player"))
        {
            PlayerManager.Instance.LocalPlayer.TakeDamage(DamageType.Slash);
            Destroy(gameObject);
        }
    }

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

        PlayerManager.Instance.LocalPlayer.GetComponent<ScoreSystem>().AddArbitraryScore(scoreValue);
        collider.isTrigger = false;
        rBody.useGravity = true;
        rBody.AddExplosionForce(explosionForce, explosionOrigin.position, 1, upwardsModifier);
        Destroy(gameObject, timeToDestroy);
    }
}