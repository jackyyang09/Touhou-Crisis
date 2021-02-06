using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using UnityEngine.UI;
using JSAM;

public class Sakuya : BaseEnemy
{
    [SerializeField] int[] healthPhases = new int[] { 75, 150, 200 };
    
    int currentPhase;

    [SerializeField] new SpriteRenderer renderer;
    [SerializeField] Color damagedColour;
    [SerializeField] ModularBox box;

    [SerializeField] Transform handTransform;

    [SerializeField] Vector2 timeBetweenWander = new Vector2(1.5f, 4);
    [SerializeField] Vector2 timesToWander = new Vector2(3, 5);

    [SerializeField] ObjectPool[] pools;

    [SerializeField] Transform debugTarget;

    [SerializeField] int[] preselectedPoints;

    [SerializeField] Transform magicCirclePrimary;

    public System.Action OnChangePhase;

    Coroutine behaviourRoutine;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            behaviourRoutine = StartCoroutine(BehaviourTree());
        }

        maxHealth = healthPhases[currentPhase];
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void TakeDamage(float d = 1)
    {
        if (!canTakeDamage) return;

        health -= d;

        if (health <= 0)
        {
            canTakeDamage = false;
            if (PhotonNetwork.IsMasterClient)
            {
                StopCoroutine(behaviourRoutine);
                StartCoroutine(ChangePhase());
            }
        }

        DamageFlash();
    }

    [PunRPC]
    void PlayIdle()
    {
        animator.Play("Idle");
    }

    Vector3 GetRandomPosition()
    {
        return box.GetRandomPointInBox();
    }

    [PunRPC]
    void WanderTo(Vector3 destination)
    {
        transform.DOMove(destination, 0.5f);
        animator.Play("Sakuya Move");
        renderer.flipX = destination.x - transform.position.x < 0;
    }

    [PunRPC]
    void GoToCenter()
    {
        Vector3 destination = box.GetBoxCenter();
        transform.DOMove(destination, 0.5f);
        animator.Play("Sakuya Move");
        renderer.flipX = destination.x - transform.position.x < 0;
    }

    void Dash()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("DashTo", RpcTarget.All, GetRandomPosition());
        }
    }

    [PunRPC]
    void DashTo(Vector3 destination)
    {
        transform.DOMove(destination, 0.25f);
    }

    [PunRPC]
    void SakuyaVolley()
    {
        animator.Play("Sakuya Volley");
    }

    public void ThrowAccurateKnife()
    {
        AudioManager.PlaySound(TouhouCrisisSounds.EnemySword);
        EnemyBullet newKnife = pools[0].GetObject().GetComponent<EnemyBullet>();
        newKnife.transform.position = handTransform.position;
        newKnife.Init(debugTarget);
        newKnife.transform.SetParent(null);
        newKnife.gameObject.SetActive(true);
    }

    [PunRPC]
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

    IEnumerator ChangePhase()
    {
        if (currentPhase < healthPhases.Length)
        {
            photonView.RPC("GoToCenter", RpcTarget.All);

            yield return new WaitForSeconds(2);

            OnChangePhase?.Invoke();

            currentPhase++;
            maxHealth = healthPhases[currentPhase];
            health = maxHealth;

            switch (currentPhase)
            {
                case 1:
                    magicCirclePrimary.DOScale(3, 1);
                    break;
                case 2:

                    break;
            }

            yield return new WaitForSeconds(1);

            canTakeDamage = true;

            behaviourRoutine = StartCoroutine(BehaviourTree());
        }
        else
        {

        }
    }

    IEnumerator BehaviourTree()
    {
        yield return new WaitForSeconds(2);

        while (true)
        {
            int wanderNum = (int)Random.Range(timesToWander.x, timesToWander.y);
            for (int i = 0; i < wanderNum; i++)
            {
                Vector3 destination = GetRandomPosition();
                photonView.RPC("WanderTo", RpcTarget.All, destination);
                float wanderTime = Random.Range(timeBetweenWander.x, timeBetweenWander.y);
                yield return new WaitForSeconds(wanderTime);
            }

            switch (Random.Range(0, currentPhase + 1))
            {
                case 0:
                    photonView.RPC("SakuyaVolley", RpcTarget.All);
                    yield return new WaitForSeconds(5);
                    break;
                case 1:
                    photonView.RPC("ArrangeKnivesClockwise", RpcTarget.All);
                    yield return new WaitForSeconds(10);
                    break;
            }

            photonView.RPC("PlayIdle", RpcTarget.All);
        }
    }
}