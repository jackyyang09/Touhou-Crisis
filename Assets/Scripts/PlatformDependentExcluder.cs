using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDependentExcluder : MonoBehaviour
{
    [SerializeField] bool destroyIfExclude = false;

    [SerializeField] bool excludeWeb = false;
    [SerializeField] bool excludeMobile = false;

    void Awake()
    {
#if UNITY_WEBGL
        if (excludeWeb)
        {
            Exclude();
        }
#endif
#if UNITY_ANDROID
        if (excludeMobile)
        {
            Exclude();
        }
#endif
    }

    void Exclude()
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
