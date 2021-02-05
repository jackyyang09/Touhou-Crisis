using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    [SerializeField]
    RectTransform rect = null;

    [SerializeField]
    float disappearTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        rect.Rotate(new Vector3(0, 0, Random.Range(0, 360)));
        Destroy(gameObject, disappearTime);
    }
}
