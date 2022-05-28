using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;
using UnityEngine.UI;
using JSAM;

public class Sakuya : BaseEnemy, IReloadable
{
    [System.Serializable]
    struct BehaviourStruct
    {
        public Vector2 timeBetweenWander;
        public Vector2 timesToWander;
        public Vector2 closeInOutTime;
    }

    [SerializeField] int[] healthPhases = new int[] { 75, 150, 200 };
    
    int currentPhase;
    public int CurrentPhase { get { return currentPhase; } }

    [SerializeField] new SpriteRenderer renderer = null;
    [SerializeField] Color damagedColour;

    [SerializeField] ModularBox box = null;
    [SerializeField] ModularBox knifeBox = null;

    [SerializeField] Transform handTransform = null;

    [SerializeField] BehaviourStruct[] behaviourStructs = null;
    [SerializeField] BehaviourStruct designateStruct = new BehaviourStruct();

    [SerializeField] Vector2 timeStopKnifeThrows = Vector2.zero;

    [SerializeField] ObjectPool[] pools = null;

    [SerializeField] Vector3 targetOffset = Vector3.zero;
    [SerializeField] Transform target = null;

    [SerializeField] Transform magicCirclePrimary = null;
    [SerializeField] SpriteRenderer magicCircleSecondary = null;

    [SerializeField] Animator timeStopAnim;

    bool isOwner
    {
        get
        {
            return PhotonNetwork.IsMasterClient || GameplayModifiers.Instance.GameMode == GameplayModifiers.GameModes.Versus;
        }
    }

    public System.Action<int> OnChangePhase;
    public System.Action OnBossDefeat;

    Coroutine behaviourRoutine;
    Coroutine attackRoutine;
    bool changingPhase = false;
    /// <summary>
    /// Prevent Sakuya from stopping time consecutively
    /// </summary>
    bool canStopTime = false;

    GameplayModifiers modifiers = null;

    Vector3 startPosition;

    public void Reinitialize()
    {
        animator.SetTrigger("Reset");
        animator.Play("SecondaryMagicCircle.Idle");
        magicCirclePrimary.localScale = Vector3.zero;
        magicCircleSecondary.enabled = false;
        renderer.color = Color.white;
        collider.enabled = true;

        canTakeDamage = true;
        changingPhase = false;

        transform.DOKill();

        PlayIdle();

        transform.position = startPosition;

        currentPhase = 0;

        maxHealth = healthPhases[currentPhase];
        health = maxHealth;

        // Update health UI
        OnShot?.Invoke();

        StopAllCoroutines();

        if (isOwner)
        {
            behaviourRoutine = StartCoroutine(BehaviourTree());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);

        modifiers = GameplayModifiers.Instance;
        if (modifiers)
        {
            switch (modifiers.BossActionSpeed)
            {
                case GameplayModifiers.BossActionSpeeds.Slow:
                    break;
                case GameplayModifiers.BossActionSpeeds.Normal:
                    break;
                case GameplayModifiers.BossActionSpeeds.Fast:
                    break;
                case GameplayModifiers.BossActionSpeeds.Length:
                    break;
            }
            designateStruct.timeBetweenWander = behaviourStructs[(int)modifiers.BossMoveSpeed].timeBetweenWander;
            designateStruct.timesToWander = behaviourStructs[(int)GameplayModifiers.BossActionSpeeds.Fast - (int)modifiers.BossActionSpeed].timesToWander;
            designateStruct.closeInOutTime = behaviourStructs[(int)modifiers.BossMoveSpeed].closeInOutTime;
        }

        startPosition = transform.position;

        Reinitialize();
    }

    private void OnEnable()
    {
        GameManager.OnReloadScene += StopMoving;
        GameOverUI.OnGameOver += StopAttacking;

        SickDev.DevConsole.DevConsole.singleton.AddCommand(new SickDev.CommandSystem.ActionCommand(CrippleBoss)
        {
            alias = "CrippleBoss",
            description = "Reduces bosses health and subsequent health bars to 1"
        });
    }

    private void OnDisable()
    {
        GameManager.OnReloadScene -= StopMoving;
        GameOverUI.OnGameOver -= StopAttacking;
    }

    void StopAttacking()
    {
        if (behaviourRoutine != null)
        {
            StopCoroutine(behaviourRoutine);
        }
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    private void StopMoving()
    {
        if (behaviourRoutine != null)
        {
            StopCoroutine(behaviourRoutine);
        }
    }

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
            switch (GameplayModifiers.Instance.GameMode)
            {
                case GameplayModifiers.GameModes.Versus:
                    StopCoroutine(behaviourRoutine);
                    behaviourRoutine = null;
                    StartPhaseChange();
                    break;
                case GameplayModifiers.GameModes.Coop:
                    if (PhotonNetwork.IsMasterClient)
                    {
                        StopCoroutine(behaviourRoutine);
                        behaviourRoutine = null;
                        photonView.RPC(nameof(StartPhaseChange), RpcTarget.All);
                    }
                    break;
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
        switch (GameplayModifiers.Instance.GameMode)
        {
            case GameplayModifiers.GameModes.Versus:
                DashTo(GetRandomPosition());
                break;
            case GameplayModifiers.GameModes.Coop:
                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC("DashTo", RpcTarget.All, GetRandomPosition());
                }
                break;
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
        switch (currentPhase)
        {
            case 0:
                animator.Play("Sakuya Volley");
                break;
            case 1:
                if (Random.value > 0.5f)
                    animator.Play("Sakuya Volley");
                else
                    animator.Play("Sakuya Volley 2");
                break;
            case 2:
            case 3:
                animator.Play("Sakuya Volley 2");
                break;
        }
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

        switch (currentPhase)
        {
            case 1:
                animator.Play("Sakuya 4 Combo");
                break;
            case 2:
            case 3:
            case 4:
                if (Random.value > 0.5f)
                {
                    animator.Play("Sakuya 4 Combo");
                }
                else animator.Play("Sakuya 4 Combo 2");
                break;
        }

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

    [PunRPC]
    public void MeleeAttack()
    {
        StartCoroutine(DoMeleeAttack());
    }

    IEnumerator DoMeleeAttack()
    {
        float closeInTime = behaviourStructs[currentPhase].closeInOutTime.x;
        float closeOutTime = behaviourStructs[currentPhase].closeInOutTime.y;

        GameObject meleeBullet = pools[3].GetObject();

        if (health > 0)
        {
            renderer.flipX = false;
            animator.Play("Sakuya Melee Entrance");
        
            transform.DOMove(AreaLogic.Instance.Player1MeleeTransform.position, closeInTime).SetEase(Ease.Linear);

            yield return new WaitForSeconds(closeInTime);
        }

        if (health > 0)
        {
            animator.Play("Sakuya Melee Attack");
            AudioManager.PlaySound(TouhouCrisisSounds.EnemySword);

            meleeBullet.SetActive(true);

            yield return new WaitForSeconds(0.5f);
        }

        meleeBullet.SetActive(false);

        if (health > 0)
        {
            // Identical to GoToCenter()
            Vector3 destination = box.GetBoxCenter();
            transform.DOMove(destination, closeOutTime).SetEase(Ease.Linear);
            animator.Play("Sakuya Move");
            renderer.flipX = destination.x - transform.position.x < 0;
        }
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

        transform.DOKill();

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
                case 2:
                    magicCirclePrimary.DOScale(3, 1);
                    break;
                case 3:
                    magicCircleSecondary.enabled = true;
                    animator.Play("Secondary MagicCircle Intro");
                    AudioManager.CrossfadeMusic(TouhouCrisisMusic.LunaDial, 5);
                    break;
            }

            yield return new WaitForSeconds(1);

            canTakeDamage = true;
            changingPhase = false;

            if (isOwner)
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

            yield return new WaitForSeconds(2);

            OnBossDefeat?.Invoke();
        }
    }

    void SpawnKnifeBundlePointBlank()
    {
        AudioManager.PlaySound(TouhouCrisisSounds.KnifePlace);
        
        for (int i = 0; i < 5; i++)
        {
            Vector3 pos = Vector3.Lerp(
                knifeBox.GetRandomPointInBox(),
                AreaLogic.Instance.Player1FireTransform.position, 0.7f);

            SpecialBullet newKnife = pools[2].GetObject().GetComponent<SpecialBullet>();
            newKnife.transform.position = pos;
            newKnife.gameObject.SetActive(true);
            newKnife.transform.SetParent(null);
            newKnife.moveDelay = 0;

            target.position = AreaLogic.Instance.Player1FireTransform.position;
            target.position += targetOffset;

            newKnife.transform.LookAt(target.position);

            if (i == 4) newKnife.playSound = true;
            else newKnife.playSound = false;

            newKnife.SpecialInit(target);
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
        if (SteamManager.Initialized)
        {
#if UNITY_STANDALONE
            if (Steamworks.SteamUserStats.GetAchievement("ACHIEVE_4", out bool unlocked))
            {
                if (!unlocked)
                {
                    if (!PlayerManager.Instance.LocalPlayer.InCover)
                    {
                        Steamworks.SteamUserStats.SetAchievement("ACHIEVE_4");
                        Steamworks.SteamUserStats.StoreStats();
                    }
                }
            }
#endif
        }

        Time.timeScale = 0;
        AudioManager.CrossfadeMusic(TouhouCrisisMusic.LunaDialLowPass, 0.1f, true);
        AudioManager.PlaySound(TouhouCrisisSounds.TimeStop);
        timeStopAnim.Play("Time Stop");
    }

    [PunRPC]
    void TimeStopCombo(int knifeThrows)
    {
        StartCoroutine(TimeStop(knifeThrows));
    }

    IEnumerator TimeStop(int knifeThrows)
    {
        GoToCenter();

        animator.Play("Sakuya Time Stop");

        yield return new WaitForSecondsRealtime(4.5f);

        SpawnKnifeBundlePointBlank();
        yield return new WaitForSecondsRealtime(0.5f);

        for (int i = 1; i < knifeThrows; i++)
        {
            float delay = i * 0.5f;
            SpawnKnifeBundle(delay);
            yield return new WaitForSecondsRealtime(0.5f);
        }
        animator.Play("Sakuya Time Resumes");

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

        bool isLocal = GameplayModifiers.Instance.GameMode == GameplayModifiers.GameModes.Versus;

        while (true)
        {
            // If no modifiers found, fallback to debug system
            BehaviourStruct behaviourStruct = modifiers ? designateStruct : behaviourStructs[currentPhase];

            int wanderNum = (int)Random.Range(behaviourStruct.timesToWander.x, behaviourStruct.timesToWander.y);
            for (int i = 0; i < wanderNum; i++)
            {
                Vector3 destination = GetRandomPosition();
                if (isLocal) WanderTo(destination);
                else photonView.RPC(nameof(WanderTo), RpcTarget.All, destination);
                float wanderTime = Random.Range(behaviourStruct.timeBetweenWander.x, behaviourStruct.timeBetweenWander.y);
                yield return new WaitForSeconds(wanderTime);
            }

            int attackIndex = 0;
            do
            {
                attackIndex = Random.Range(0, currentPhase + 1);
            } while (attackIndex == 3 && !canStopTime);

            switch (attackIndex)
            {
                case 0:
                    canStopTime = true;
                    if (isLocal) SakuyaVolley();
                    else photonView.RPC(nameof(SakuyaVolley), RpcTarget.All);
                    yield return new WaitForSeconds(5);
                    break;
                case 1:
                    canStopTime = true;
                    if (isLocal) MeleeAttack();
                    else photonView.RPC(nameof(MeleeAttack), RpcTarget.All);
                    float waitTime = behaviourStruct.closeInOutTime.x + behaviourStruct.closeInOutTime.y + 0.5f;
                    yield return new WaitForSeconds(waitTime);
                    break;
                case 2:
                    canStopTime = true;
                    if (isLocal) ArrangeKnivesClockwise();
                    else photonView.RPC(nameof(ArrangeKnivesClockwise), RpcTarget.All);
                    yield return new WaitForSeconds(7.5f);
                    break;
                case 3:
                    canStopTime = false;
                    int knifeThrows = (int)Random.Range(timeStopKnifeThrows.x, timeStopKnifeThrows.y + 1);
                    if (isLocal) TimeStopCombo(knifeThrows);
                    else photonView.RPC(nameof(TimeStopCombo), RpcTarget.All, knifeThrows);
                    yield return new WaitForSeconds(5);
                    break;
            }

            if (isLocal) PlayIdle();
            else photonView.RPC(nameof(PlayIdle), RpcTarget.All);
        }
    }

    void CrippleBoss()
    {
        for (int i = 0; i < healthPhases.Length; i++)
        {
            healthPhases[i] = 1;
            health = 2;
        }
        TakeDamage();
        Debug.Log("Sakuya has been punished!");
    }
}