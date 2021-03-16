using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlatformDependent : MonoBehaviour
{
    [SerializeField] protected bool actOnWeb = false;
    [SerializeField] protected bool actOnMobile = false;
    [SerializeField] protected bool actOnStandalone = false;
    [SerializeField] protected bool actOnMultiplayer = false;

    void Awake()
    {
        bool act = false;
#if UNITY_WEBGL
        if (excludeWeb)
        {
            exclude = true;
        }
#endif
#if UNITY_ANDROID
        if (actOnMobile)
        {
            act = true;
        }
#endif
#if UNITY_STANDALONE
        if (excludeStandalone)
        {
            exclude = true;
        }
#endif
        if (!Photon.Pun.PhotonNetwork.OfflineMode && actOnMultiplayer)
        {
            act = true;
        }

        if (act)
        {
            DependentAction();
        }
    }

    protected abstract void DependentAction();
}
