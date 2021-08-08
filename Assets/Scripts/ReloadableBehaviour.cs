using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This object can be soft reloaded, an alternative to reloading the scene (hard-reloading)
/// </summary>
public interface IReloadable
{
    void Reinitialize();
}

// TODO: Make a Monobehaviour for reloadable Photon monobehaviours too
public class ReloadableBehaviour : MonoBehaviour
{
    public void RegisterSelf()
    {
        //FindObjectOfType<SoftSceneReloader>().
    }

    public virtual void Reinitialize()
    {

    }
}