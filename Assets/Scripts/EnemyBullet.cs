using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] protected float speed;

    [SerializeField] protected Rigidbody rb;

    [SerializeField] protected DamageType damageType;

    protected Vector3 ogPos;
    protected Vector3 targetPos;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init(Transform target = null)
    {
        if (target != null)
        {
            transform.LookAt(target);
        }

        targetPos = target.position;
        ogPos = transform.position;

        rb.velocity = transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HitTarget();
        }
        gameObject.SetActive(false);
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
}