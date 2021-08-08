using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates a large pool of objects so we don't have to constantly instantiate and destroy them at runtime
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [Tooltip("The reference object to pool")]
    [SerializeField] GameObject prefab = null;
    
    [Tooltip("Spawn this many objects on start")]
    [SerializeField] int objectsToSpawn = 100;

    [SerializeField] bool parentObjects = true;

    List<GameObject> pool;

    // Start is called before the first frame update
    void Start()
    {
        pool = new List<GameObject>();

        for (int i = 0; i < objectsToSpawn; i++)
        {
            Transform parent = parentObjects ? transform : null;
            pool.Add(Instantiate(prefab, parent));
            pool[i].SetActive(false);
        }
    }

    /// <summary>
    /// Returns the first available object
    /// </summary>
    /// <returns></returns>
    public GameObject GetObject()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeSelf)
            {
                return pool[i];
            }
        }
        return null;
    }
}