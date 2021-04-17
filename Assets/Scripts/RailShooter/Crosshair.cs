using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Image image;

    static Crosshair instance = null;
    public static Crosshair Instance
    { 
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Crosshair>();
            }
            return instance;
        }
    }

    string HideCursorKey
    {
        get
        {
            return PauseMenu.HideCursorKey;
        }
    }


    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Confined;
        //Cursor.visible = !(PlayerPrefs.GetInt(HideCursorKey) == 0);
        image.enabled = true;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        image.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Why this works: 
        // https://answers.unity.com/questions/849117/46-ui-image-follow-mouse-position.html?_ga=2.45598500.148015968.1612849553-1895421686.1612849553
#if UNITY_ANDROID && !UNITY_EDITOR
        transform.position = Input.touches[Input.touchCount - 1].position;
#else
        transform.position = Input.mousePosition;
#endif
    }
}