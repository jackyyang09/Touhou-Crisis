using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using UnityEngine.UI;
using JSAM;

public class Sakuya : BaseEnemy
{
    [System.Serializable]
    struct BehaviourStruct
    {
        public Vector2 timeBetweenWander;
        public Vector2 timesToWander;
    }

    [SerializeField] int[] healthPhases = new int[] { 75, 150, 200 };
    
    int currentPhase;
    public int CurrentPhase
    {
        get
        {
            return currentPhase;
        }
    }

    [SerializeField] new SpriteRenderer renderer = null;
    [SerializeField] Color damagedColour;

    [SerializeField] ModularBox box = null;
    [SerializeField] ModularBox knifeBox = null;

    [SerializeField] Transform handTransform = null;

    [SerializeField] BehaviourStruct[] behaviourStructs = null;

    [SerializeField] ObjectPool[] pools = null;

    [SerializeField] Vector3 targetOffset = Vector3.zero;
    [SerializeField] Transform target = null;

    [SerializeField] Transform magicCirclePrimary = null;
    [SerializeField] SpriteRenderer magicCircleSecondary = null;

    [SerializeField] Animator timeStopAnim;

    public System.Action<int> OnChangePhase;
    public System.Action OnBossDefeat;

    Coroutine behaviourRoutine;
    Coroutine attackRoutine;
    bool changingPhase = false;
    /// <summary>
    /// Prevent Sakuya from stopping time consecutively
    /// </summary>
    bool canStopTime = false;

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
    //void Update()
    //{
    //
    //}

    public override void TakeDamage(float d = 1)
    {
        if (currentPhase < healthPhases.Length)
        {
            DamageFlash();
        }

        if (!canTakeDamage) return;

        health -= d;

        if (changingPhase) return;

        if (health <= 0)
        {
            canTakeDamage = false;
            changingPhase = true;
            if (PhotonNetwork.IsMasterClient)
            {
                StopCoroutine(behaviourRoutine);
                behaviourRoutine = null;
                photonView.RPC("StartPhaseChange", RpcTarget.All);
            }
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }
        }
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
        target.position = AreaLogic.Instance.Player1FireTransform.position;

        target.position += targetOffset;
        newKnife.Init(target);
        newKnife.transform.SetParent(null);
        newKnife.gameObject.SetActive(true);
    }

    [PunRPC]
    public void ArrangeKnivesClockwise()
    {
        if (attackRoutine == null)
        {
            attackRoutine = StartCoroutine(ArrangeKnifeRoutine());
        }
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
            newKnife.moveDelay = 2 + i * 0.05f;

            if (i >= 3)
            {
                newKnife.moveDelay += 0.5f;
            }
            if (i >= 6)
            {
                newKnife.moveDelay += 0.5f;
            }

            newKnife.transform.eulerAngles = new Vector3(360 * i / 12, 90, 180);
            newKnife.transform.Translate(Vector3.forward * 1);
            newKnife.gameObject.SetActive(true);

            target.position = AreaLogic.Instance.Player1FireTransform.position;
            target.position += targetOffset;
            newKnife.SpecialInit(target);
            AudioManager.PlaySound(TouhouCrisisSounds.KnifePlace);
            yield return new WaitForSeconds(0.04f);
        }
        attackRoutine = null;
    }

    protected override void DamageFlash()
    {
        renderer.DOComplete();
        renderer.DOColor(damagedColour, 0);
        renderer.DOColor(Color.white, 0.25f);
        renderer.transform.localEulerAngles = new Vector3(5, 0, -2.5f);
        renderer.transform.DORotate(Vector3.zero, 0.25f);
    }

    [PunRPC]
    void StartPhaseChange()
    {
        StartCoroutine(ChangePhase());
    }

    IEnumerator ChangePhase()
    {
        currentPhase++;

        // Deactivate all knives in the scene
        var knives = FindObjectsOfType<EnemyBullet>();
        for (int i = 0; i < knives.Length; i++)
        {
            knives[i].gameObject.SetActive(false);
        }

        AudioManager.PlaySound(TouhouCrisisSounds.SpellcardBreak);

        if (currentPhase < healthPhases.Length)
        {
            animator.Play("Sakuya Hurt");

            yield return new WaitForSeconds(1);

            GoToCenter();

            yield return new WaitForSeconds(1);

            OnChangePhase?.Invoke(currentPhase);

            maxHealth = healthPhases[currentPhase];
            health = maxHealth;

            switch (currentPhase)
            {
                case 1:
                    magicCirclePrimary.DOScale(3, 1);
                    break;
                case 2:
                    magicCircleSecondary.enabled = true;
                    animator.Play("Secondary MagicCircle Intro");
                    AudioManager.CrossfadeMusic(TouhouCrisisMusic.LunaDial, 5);
                    break;
            }

            yield return new WaitForSeconds(1);

            canTakeDamage = true;
            changingPhase = false;

            if (PhotonNetwork.IsMasterClient)
            {
                behaviourRoutine = StartCoroutine(BehaviourTree());
            }
        }
        else
        {
            collider.enabled = false;

            if (behaviourRoutine != null)
            {
                StopCoroutine(behaviourRoutine);
                behaviourRoutine = null;
            }

            AudioManager.FadeMusicOut(3);

            canTakeDamage = false;

            animator.Play("Sakuya Defeat");

            renderer.DOComplete();
            renderer.DOColor(Color.clear, 2).SetDelay(1);

            yield return new WaitForSeconds(3);

            OnBossDefeat?.Invoke();

            Destroy(gameObject);
        }
    }

    void SpawnKnifeBundle(float moveDelay)
    {
        AudioManager.PlaySound(TouhouCrisisSounds.KnifePlace);
        Vector3 referencePosition = knifeBox.GetRandomPointInBox();
        for (int i = 0; i < 5; i++)
        {
            Vector3 pos = referencePosition + Random.insideUnitSphere * 1.5f;
            SpecialBullet newKnife = pools[2].GetObject().GetComponent<SpecialBullet>();
            newKnife.transform.position = pos;
            newKnife.gameObject.SetActive(true);
            newKnife.transform.SetParent(null);
            newKnife.moveDelay = moveDelay;

            target.position = AreaLogic.Instance.Player1FireTransform.position;
            target.position += targetOffset;

            newKnife.transform.LookAt(target.position);

            if (i == 4) newKnife.playSound = true;
            else newKnife.playSound = false;

            newKnife.SpecialInit(target);
        }
    }

    public void StopTime()
    {
        Time.timeScale = 0;
        AudioManager.CrossfadeMusic(TouhouCrisisMusic.LunaDialLowPass, 0.1f, true);
        AudioManager.PlaySound(TouhouCrisisSounds.TimeStop);
        timeStopAnim.Play("Time Stop");
    }

    public void TimeResumes()
    {
        //Time.timeScale = 1;
        //AudioManager.CrossfadeMusic(TouhouCrisisMusic.LunaDial, 0.1f, true);
    }

    [PunRPC]
    void TimeStopCombo()
    {
        StartCoroutine(TimeStop());
    }

    IEnumerator TimeStop()
    {
        GoToCenter();

        animator.Play("Sakuya Time Stop");

        yield return new WaitForSecondsRealtime(4.5f);

        SpawnKnifeBundle(0.01f);

        yield return new WaitForSecondsRealtime(0.5f);

        SpawnKnifeBundle(0.5f);

        yield return new WaitForSecondsRealtime(0.5f);

        SpawnKnifeBundle(1.25f);

        yield return new WaitForSecondsRealtime(1.5f);

        timeStopAnim.Play("Time Resumes");
        AudioManager.PlaySound(TouhouCrisisSounds.TimeResumes);
        float timer = 0;
        float timeToFlowAgain = 2;
        AudioManager.CrossfadeMusic(TouhouCrisisMusic.LunaDial, 2, true);
        while (timer < timeToFlowAgain)
        {
            Time.timeScale = Mathf.Lerp(0, 1, timer / timeToFlowAgain);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1;

        yield return new WaitForSecondsRealtime(1f);
    }

    IEnumerator BehaviourTree()
    {
        yield return new WaitForSeconds(2);

        while (true)
        {
            var behaviourStruct = behaviourStructs[currentPhase];
            int wanderNum = (int)Random.Range(behaviourStruct.timesToWander.x, behaviourStruct.timesToWander.y);
            for (int i = 0; i < wanderNum; i++)
            {
                Vector3 destination = GetRandomPosition();
                photonView.RPC("WanderTo", RpcTarget.All, destination);
                float wanderTime = Random.Range(behaviourStruct.timeBetweenWander.x, behaviourStruct.timeBetweenWander.y);
                yield return new WaitForSeconds(wanderTime);
            }

            int attackIndex = 0;
            do
            {
                //attackIndex = (/*Random.Range(0, currentPhase + 1)*/2);
                attackIndex = Random.Range(0, currentPhase + 1);
            } while (attackIndex == 2 && !canStopTime);

            switch (attackIndex)
            {
                case 0:
                    canStopTime = true;
                    photonView.RPC("SakuyaVolley", RpcTarget.All);
                    yield return new WaitForSeconds(5);
                    break;
                case 1:
                    canStopTime = true;
                    photonView.RPC("ArrangeKnivesClockwise", RpcTarget.All);
                    yield return new WaitForSeconds(8);
                    break;
                case 2:
                    canStopTime = false;
                    photonView.RPC("TimeStopCombo", RpcTarget.All);
                    yield return new WaitForSeconds(5);
                    break;
            }

            photonView.RPC("PlayIdle", RpcTarget.All);
        }
    }
}