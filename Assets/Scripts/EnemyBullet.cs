using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] protected float speed;

    [SerializeField] protected Rigidbody rb;

    [SerializeField] protected DamageType damageType;

    [SerializeField] protected GameObject effect;

    [SerializeField] protected float lifeTime;

    [SerializeField] AudioFileSoundObject bulletSound;
    public bool playSound;

    protected Vector3 ogPos;
    protected Vector3 targetPos;

    // Start is called before the first frame update
    //void Start()
    //{
    //}

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
            AudioManager.instance.PlaySoundInternal(bulletSound);
        }

        Invoke("DisableSelf", lifeTime);
    }

    private void OnDisable()
    {
        if (effect)
        {
            effect.SetActive(false);
        }

        rb.velocity = Vector3.zero;
        if (IsInvoking("DisableSelf")) CancelInvoke("DisableSelf");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HitTarget();
        }
        gameObject.SetActive(false);
        Debug.Log(other.name);
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