using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionDependentExcluder : MonoBehaviour
{
    [Header("Object will be disabled at below resolutions")]
    [SerializeField] Vector2[] resolutions;

    private void Awake()
    {
        var current = (float)Screen.width / (float)Screen.height;
        
        for (int i = 0; i < resolutions.Length; i++)
        {
            var r = resolutions[i].x / resolutions[i].y;
            if (FastApproximately(r, current, 0.1f))
            {
                gameObject.SetActive(false);
                break;
            }
        }
    }

    public static bool FastApproximately(float a, float b, float threshold)
    {
        if (threshold > 0f)
        {
            return Mathf.Abs(a - b) <= threshold;
        }
        else
        {
            return Mathf.Approximately(a, b);
        }
    }
}