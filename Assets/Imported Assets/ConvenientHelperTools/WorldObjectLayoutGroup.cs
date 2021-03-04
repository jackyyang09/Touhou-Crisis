using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObjectLayoutGroup : MonoBehaviour
{
    [SerializeField] float spacing;

    private void OnEnable()
    {
        ApplyLayout();
    }

    private void OnValidate()
    {
        ApplyLayout();
    }

    void ApplyLayout()
    {
        if (!enabled) return;
        if (transform.childCount == 0) return;

        var firstChild = transform.GetChild(0);
        for (int i = 1; i < transform.childCount; i++)
        {
            transform.GetChild(i).position = firstChild.position + firstChild.right * spacing * i;
        }
    }
}
