using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;

public class EnemyBullet : MonoBehaviour, IReloadable
{
    [SerializeField] protected float speed;

    [SerializeField] protected Rigidbody rb;

    [SerializeField] protected DamageType damageType;

    [SerializeField] protected GameObject effect;

    [SerializeField] protected float lifeTime;

    [SerializeField] protected bool collideWithEnvironment = true;

    [SerializeField] JSAMSoundFileObject bulletSound;
    public bool playSound;

    protected Vector3 ogPos;
    protected Vector3 targetPos;

    public void Reinitialize()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        SoftSceneReloader.Instance.AddNewReloadable(this);
    }

    public void Init(Transform target)
    {
        if (target != null)
        {
            transform.LookAt(target);
            targetPos = target.position;
            ogPos = transform.position;
        }

        Init();
    }

    public void Init(Vector3 target)
    {
        transform.LookAt(target);
        targetPos = target;
        ogPos = transform.position;

        Init();
    }

    public void Init()
    {
        if (effect)
        {
            effect.SetActive(true);
        }

        rb.velocity = transform.forward * speed;

        if (playSound)
        {
            AudioManager.PlaySound(bulletSound);
        }

        Invoke(nameof(DisableSelf), lifeTime);
    }

    private void OnDisable()
    {
        if (effect)
        {
            effect.SetActive(false);
        }

        rb.velocity = Vector3.zero;
        if (IsInvoking(nameof(DisableSelf))) CancelInvoke(nameof(DisableSelf));
    }

    private void OnDestroy()
    {
        if (SoftSceneReloader.Instance != null)
        {
            SoftSceneReloader.Instance.RemoveReloadable(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        bool disableSelf = false;

        if (other.CompareTag("Player"))
        {
            HitTarget();
            disableSelf = true;
        }
        else if (collideWithEnvironment)
        {
            disableSelf = true;
        }

        if (disableSelf) gameObject.SetActive(false);
    }

    public void HitTarget()
    {
        PlayerManager.Instance.LocalPlayer.TakeDamage(damageType);
    }

    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    void DisableSelf()
    {
        gameObject.SetActive(false);
    }
}