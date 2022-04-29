using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackButtonUI : MonoBehaviour
{
    [SerializeField] OptimizedCanvas optimizedCanvas = null;
    [SerializeField] UnityEngine.EventSystems.EventTrigger backButton = null;

    // Update is called once per frame
    void Update()
    {
        if (optimizedCanvas.IsVisible)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                backButton.triggers[0].callback.Invoke(new BaseEventData(GetComponent<EventSystem>()));
            }
        }
    }
}
