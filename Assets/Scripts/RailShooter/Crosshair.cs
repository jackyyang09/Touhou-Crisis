using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    //// Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        var rect = gameObject.transform as RectTransform;
        var mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        mousePos.Scale(new Vector2(1920, 1080));
        rect.anchoredPosition = mousePos - new Vector3(960, 540);
    }
}
