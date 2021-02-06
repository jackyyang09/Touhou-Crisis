using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Sakuya : BaseEnemy
{
    [SerializeField] new SpriteRenderer renderer;
    [SerializeField] Color damagedColour;
    [SerializeField] ModularBox box;

    [SerializeField] Transform handTransform;

    [SerializeField] Vector2 timeBetweenWander = new Vector2(1.5f, 4);
    [SerializeField] Vector2 timesToWander = new Vector2(3, 5);

    [SerializeField] ObjectPool[] pools;

    [SerializeField] Transform debugTarget;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Behaviour");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Wander()
    {
        Vector3 destination = box.GetRandomPointInBox();
        transform.DOMove(destination, 0.5f);
        animator.Play("Sakuya Move");
        renderer.flipX = destination.x - transform.position.x > 0;
    }

    void GoToCenter()
    {
        Vector3 destination = box.GetBoxCenter();
        transform.DOMove(destination, 0.5f);
        animator.Play("Sakuya Move");
        renderer.flipX = destination.x - transform.position.x > 0;
    }

    void Dash()
    {
        transform.DOMove(box.GetRandomPointInBox(), 0.25f);
    }

    public void ThrowAccurateKnife()
    {
        EnemyBullet newKnife = pools[0].GetObject().GetComponent<EnemyBullet>();
        //newKnife.Init(AreaLogic.Instance.GetPlayer1Cover);
        newKnife.transform.position = handTransform.position;
        newKnife.Init(debugTarget);
        newKnife.transform.SetParent(null);
        newKnife.gameObject.SetActive(true);
    }

    [ContextMenu("TEST")]
    public void ArrangeKnivesClockwise()
    {
        StartCoroutine(ArrangeKnifeRoutine());
    }

    IEnumerator ArrangeKnifeRoutine()
    {
        GoToCenter();
        animator.Play("Sakuya 4 Combo");
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < 12; i++)
        {
            SpecialBullet newKnife = pools[1].GetObject().GetComponent<SpecialBullet>();
            newKnife.transform.position = renderer.transform.position;
            newKnife.moveDelay = 2 + i * 0.15f;
            newKnife.transform.eulerAngles = new Vector3(360 * i / 12, 90, 180);
            newKnife.transform.Translate(Vector3.forward * 1);
            newKnife.gameObject.SetActive(true);
            newKnife.SpecialInit(debugTarget);
            yield return new WaitForSeconds(0.04f);
        }
    }

    protected override void DamageFlash()
    {
        renderer.DOComplete();
        renderer.DOColor(damagedColour, 0);
        renderer.DOColor(Color.white, 0.25f);
    }

    IEnumerator Behaviour()
    {
        yield return new WaitForSeconds(2);

        while (true)
        {
            int wanderNum = (int)Random.Range(timesToWander.x, timesToWander.y);
            for (int i = 0; i < /*wanderNum*/0; i++)
            {
                Wander();
                float wanderTime = Random.Range(timeBetweenWander.x, timeBetweenWander.y);
                yield return new WaitForSeconds(wanderTime);
                //yield return null;
            }

            switch (/*Random.Range(0, 3)*/0)
            {
                case 0:
                    animator.Play("Sakuya Volley");
                    yield return new WaitForSeconds(5);
                    break;
                case 1:
                    ArrangeKnivesClockwise();
                    yield return new WaitForSeconds(10);
                    break;
            }

            animator.Play("Idle");
        }
    }
}