using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

public class UFOBehaviour : BaseEnemy, IReloadable
{
    public enum UFOType
    {
        Green,
        Blue,
        Red
    }

    [SerializeField] UFOType ufoType = UFOType.Blue;

    [SerializeField] Vector2 wanderTime = new Vector2(0.3f, 0.8f);
    [SerializeField] Vector2 waitTime = new Vector2(0.3f, 0.8f);
    [SerializeField] Vector2 timesToWander = new Vector2(3, 5);

    [SerializeField] float timeToDestroy = 2;

    [SerializeField] float explosionForce = 20;
    [SerializeField] float upwardsModifier = 1.5f;
    [SerializeField] Transform explosionOrigin = null;

    [SerializeField] float chargeupAnimTime = 0;
    [SerializeField] float chargeupTime = 0;

    [SerializeField] float magicCircleMaxScale = 1;

    [SerializeField] float scoreValue = 100;

    [SerializeField] Renderer ufoRenderer;
    [SerializeField] int flashMaterialIndex = 1;
    Material flashMaterial;
    int flashID;

    [SerializeField] Light[] lights = null;

    [SerializeField] SpriteRenderer magicCircle = null;

    [SerializeField] Transform bulletSpawnPoint = null;

    [SerializeField] GameObject destructionParticle = null;

    [Header("Green Properties")]
    [SerializeField] float greenTravelTime = 8;

    [Header("Red Properties")]
    [SerializeField] float redChargeTime = 2;
    [SerializeField] float redRotateTime = 0.5f;

    [Header("Blue Properties")]
    [SerializeField] int movesPerShot = 4;

    Coroutine behaviourRoutine;

    public System.Action OnUFOExpire;

    public void Reinitialize()
    {
        Destroy(gameObject);
    }

    void Start()
    {
        flashMaterial = ufoRenderer.materials[flashMaterialIndex];
        flashID = Shader.PropertyToID("_EmissionColor");

        collider.isTrigger = true;

        SoftSceneReloader.Instance.AddNewReloadable(this);
    }

    private void OnDisable()
    {
        if (!photonView.IsMine) return;
        if (UFOSpawner.Instance != null)
        {
            UFOSpawner.Instance.ReportUFODeath();
        }
        OnUFOExpire?.Invoke();
        OnUFOExpire = null;
    }

    private void OnDestroy()
    {
        if (SoftSceneReloader.Instance != null)
        {
            SoftSceneReloader.Instance.RemoveReloadable(this);
        }

        transform.DOKill(false);
        magicCircle.DOKill(false);
        magicCircle.transform.DOKill(false);

        // Destroy instanced material
        if (flashMaterial != null)
        {
            flashMaterial.DOKill(false);
            Destroy(flashMaterial);
        }
    }

    public void Init()
    {
        if (behaviourRoutine == null)
        {
            behaviourRoutine = StartCoroutine(BehaviourTree());
        }
    }

    [PunRPC]
    public void GreenSyncFlyTo(Vector3 destination)
    {
        transform.DOMove(destination, greenTravelTime).SetEase(Ease.Linear);
    }

    [PunRPC]
    public void SyncFlyTo(Vector3 destination, float moveTime)
    {
        transform.DOMove(destination, moveTime);
    }

    [PunRPC]
    public void SyncRedFlyTo(Vector3 target)
    {
        transform.DOMove(target, redChargeTime).SetEase(Ease.InSine);
        transform.DOLookAt(target, redRotateTime).SetEase(Ease.Linear);
        transform.DOLocalRotate(new Vector3(30, 0, 0), redRotateTime, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetDelay(redRotateTime);
    }

    IEnumerator BehaviourTree()
    {
        bool isLocal = GameplayModifiers.Instance.GameMode == GameplayModifiers.GameModes.Versus;

        switch (ufoType)
        {
            case UFOType.Green:
                {
                    Vector3 destination = transform.position.ScaleBetter(new Vector3(-1, 1, 1));
                    if (isLocal) GreenSyncFlyTo(destination);
                    else photonView.RPC("GreenSyncFlyTo", RpcTarget.All, destination);
                    yield return new WaitForSeconds(greenTravelTime / 2);
                    if (isLocal) SyncShoot();
                    else photonView.RPC("SyncShoot", RpcTarget.All);
                    yield return new WaitForSeconds(greenTravelTime / 2);
                    Destroy(gameObject);
                }
                break;
            case UFOType.Blue:
                {
                    int wanderNum = Random.Range((int)timesToWander.x, (int)timesToWander.y + 1);
                    int moves = 1;
                    while (true)
                    {
                        Vector3 destination = UFOSpawner.Instance.AreaBox.GetRandomPointInBox();
                        float moveTime = Random.Range(wanderTime.x, wanderTime.y);
                        if (isLocal) SyncFlyTo(destination, moveTime);
                        else photonView.RPC("SyncFlyTo", RpcTarget.All, new object[] { destination, moveTime} );
                        yield return new WaitForSeconds(moveTime);
                        if (moves % movesPerShot == 0)
                        {
                            if (isLocal) SyncShoot();
                            else photonView.RPC("SyncShoot", RpcTarget.All);
                        }
                        moves++;
                        yield return new WaitForSeconds(Random.Range(waitTime.x, waitTime.y));
                    }
                }
            case UFOType.Red:
                {
                    int wanderNum = Random.Range((int)timesToWander.x, (int)timesToWander.y + 1);
                    for (int i = 0; i < wanderNum; i++)
                    {
                        Vector3 destination = UFOSpawner.Instance.AreaBox.GetRandomPointInBox();
                        float moveTime = Random.Range(wanderTime.x, wanderTime.y);

                        if (isLocal) SyncFlyTo(destination, moveTime);
                        else photonView.RPC("SyncFlyTo", RpcTarget.All, new object[] { destination, moveTime} );

                        yield return new WaitForSeconds(Random.Range(waitTime.x, waitTime.y));
                    }
                    Vector3 target = AreaLogic.Instance.Player1FireTransform.position;

                    if (isLocal) SyncRedFlyTo(target);
                    else photonView.RPC("SyncRedFlyTo", RpcTarget.All, new object[] { target });

                    Destroy(gameObject, redChargeTime);
                }
                break;
        }
    }

    [PunRPC]
    void SyncShoot()
    {
        StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        magicCircle.DOFade(1, 0);
        magicCircle.transform.DOScale(magicCircleMaxScale, chargeupAnimTime);
        magicCircle.transform.DORotate(new Vector3(0, 0, 360), chargeupAnimTime, RotateMode.LocalAxisAdd).SetLoops(-1);
        magicCircle.DOFade(0, chargeupAnimTime);

        yield return new WaitForSeconds(chargeupTime);

        EnemyBullet enemyBullet = UFOSpawner.Instance.GetUFOBullet(ufoType).GetComponent<EnemyBullet>();
        enemyBullet.transform.position = bulletSpawnPoint.position;

        enemyBullet.Init(AreaLogic.Instance.Player1FireTransform);
        enemyBullet.transform.SetParent(null);
        enemyBullet.gameObject.SetActive(true);

        magicCircle.transform.DOScale(0, chargeupAnimTime);
        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.UFOBullet);
        //magicCircle.DOFade(0, chargeupAnimTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (health == 0) return;
        if (other.transform.root.CompareTag("Player"))
        {
            PlayerManager.Instance.LocalPlayer.TakeDamage(DamageType.Bullet);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Make sure to set emissive value to RGB(1, 1, 1)
    /// so the emission value is enabled during runtime
    /// </summary>
    [ContextMenu(nameof(DamageFlash))]
    protected override void DamageFlash()
    {
        animator.SetTrigger("Hit");
        flashMaterial.SetColor(flashID, Color.white);
        flashMaterial.DOColor(Color.black, flashID, 0).SetDelay(0.15f);
    }

    protected override void Die()
    {
        if (behaviourRoutine != null)
        {
            StopCoroutine(behaviourRoutine);
        }
        transform.DOKill(false);

        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].enabled = false;
        }
        PlayerManager.Instance.LocalPlayer.ScoreSystem.AddArbitraryScore(scoreValue);
        JSAM.AudioManager.PlaySound(TouhouCrisisSounds.EnemyDeath);
        collider.isTrigger = false;
        rBody.useGravity = true;
        rBody.AddExplosionForce(explosionForce, explosionOrigin.position, 1, upwardsModifier);
        Instantiate(destructionParticle, transform.position, Quaternion.identity);
        Destroy(gameObject, timeToDestroy);
    }
}