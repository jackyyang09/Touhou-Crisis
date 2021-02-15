using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/**
* Because Unity refused to fix their bugs in Input.*, didn't fix the missing methods in UI.* text-input classes,
* and instead started work on a new InputSystem that still isn't complete and live :).
*
* c.f. https://forum.unity.com/threads/find-out-which-key-was-pressed.385250/
*/
public class KeyInputEvents : MonoBehaviour
{
    public Action<KeyCode> OnKeyDown, OnKeyUp, OnKeyPress;
    public Action<int> OnMouseDown, OnMouseUp, OnMousePress;

    private static readonly KeyCode[] keyCodes = Enum.GetValues(typeof(KeyCode))
        .Cast<KeyCode>()
        .Where(k => ((int)k < (int)KeyCode.Mouse0))
        .ToArray();

    private List<KeyCode> _keysDown;
    private List<int> mouseButtonsDown;

    public void OnEnable()
    {
        _keysDown = new List<KeyCode>();
        mouseButtonsDown = new List<int>();
    }

    public void OnDisable()
    {
        _keysDown = null;
        mouseButtonsDown = null;

        // Clear listeners
        OnKeyDown = null;
        OnKeyUp = null;
        OnKeyPress = null;
    }

    public void Update()
    {
        if (Input.anyKeyDown)
        {
            for (int i = 8; i < 330; i++)
            {
                KeyCode kc = (KeyCode)i;
                if (Input.GetKeyDown(kc))
                {
                    _keysDown.Add(kc);
                    OnKeyDown?.Invoke(kc);
                }
            }
        }
        
        if (_keysDown.Count > 0)
        {
            for (int i = 0; i < _keysDown.Count; i++)
            {
                KeyCode kc = _keysDown[i];
                if (Input.GetKeyUp(kc))
                {
                    _keysDown.RemoveAt(i);
                    i--;
                    OnKeyUp?.Invoke(kc);
                    OnKeyPress?.Invoke(kc);
                }
            }
        }
    }
}
