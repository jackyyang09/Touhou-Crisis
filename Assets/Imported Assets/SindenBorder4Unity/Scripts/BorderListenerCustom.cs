using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SindenUnity
{
    [RequireComponent(typeof(RectTransform))]
    public class BorderListenerCustom : MonoBehaviour
    {
        [SerializeField] UnityEngine.Events.UnityEvent onBorderUpdated = null;

        public bool IsReady
        {
            get
            {
                RectTransform rect = transform as RectTransform;
                return rect.anchorMin.x == 0 && rect.anchorMin.y == 0 && rect.anchorMax.x == 1 && rect.anchorMax.y == 1;
            }
        }

        private void OnEnable()
        {
            SindenBorder.OnBorderUpdated += InvokeEvent;
        }

        private void OnDisable()
        {
            SindenBorder.OnBorderUpdated -= InvokeEvent;
        }

        void InvokeEvent(BorderProperties borderPropObject)
        {
            onBorderUpdated.Invoke();
        }
    }
}