using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDependentExcluder : PlatformDependent
{
    [SerializeField] bool destroyIfExclude = false;

    protected override void DependentAction()
    {
        if (destroyIfExclude)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
