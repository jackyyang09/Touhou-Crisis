using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoftSceneReloader : MonoBehaviour
{
    List<IReloadable> reloadables = new List<IReloadable>();

    static SoftSceneReloader instance;
    public static SoftSceneReloader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SoftSceneReloader>();
            }
            return instance;
        }
    }

    public void AddNewReloadable(IReloadable newReloadable)
    {
        reloadables.Add(newReloadable);
    }

    public void RemoveReloadable(IReloadable reloadable)
    {
        reloadables.Remove(reloadable);
    }

    [ContextMenu("Execute Soft Reload")]
    public void ExecuteSoftReload()
    {
        for (int i = 0; i < reloadables.Count; i++)
        {
            reloadables[i].Reinitialize();
        }
    }
}
