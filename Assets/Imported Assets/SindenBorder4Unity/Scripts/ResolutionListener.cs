using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionListener : MonoBehaviour
{
    [SerializeField] bool logEvents = true;

    Resolution prevResolution = new Resolution();

    public static System.Action OnResolutionChanged;

    // Start is called before the first frame update
    void Start()
    {
        prevResolution = Screen.currentResolution;
    }

    // Update is called once per frame
    void Update()
    {
        if (
            Screen.currentResolution.width != prevResolution.width ||
            Screen.currentResolution.height != prevResolution.height
            )
        {
            OnResolutionChanged?.Invoke();
            prevResolution = Screen.currentResolution;

            if (logEvents)
            {
                Debug.Log("Resolution changed to " + Screen.currentResolution);
            }
        }
    }
}
