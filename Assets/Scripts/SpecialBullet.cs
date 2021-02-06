using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpecialBullet : EnemyBullet
{
    [SerializeField] public float moveDelay;

    [SerializeField] public float lookDelay = 0;
    [SerializeField] public float smoothLookTime = -1;
    [SerializeField] public bool lookAtTarget;

    Transform target;

    public void SpecialInit(Transform target)
    {
        this.target = target;
        StartCoroutine(BulletBehaviour(target));
        if (lookAtTarget)
        {
            StartCoroutine(LookAtTarget());
        }
    }

    private void OnDisable()
    {
        rb.velocity = Vector3.zero;
    }

    IEnumerator LookAtTarget()
    {
        yield return new WaitForSeconds(lookDelay);
        if (smoothLookTime > -1)
        {
            transform.DOLookAt(target.position, smoothLookTime);
            yield return new WaitForSeconds(smoothLookTime);
        }
        while (gameObject.activeSelf)
        {
            transform.LookAt(target);
            yield return null;
        }
    }

    IEnumerator BulletBehaviour(Transform target)
    {
        yield return new WaitForSeconds(moveDelay);

        Init(target);

        yield return null;
    }
}
