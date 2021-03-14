using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PlatformDependentRectTransform : PlatformDependent
{
    [SerializeField] [HideInInspector] RectTransform rectTransform = null;
    [SerializeField] RectTransform alternateRect = null;

    private void OnValidate()
    {
        rectTransform = transform as RectTransform;
    }

    [ContextMenu("Debug RectTransform Copy")]
    protected override void DependentAction()
    {
        rectTransform.anchoredPosition = alternateRect.anchoredPosition;
        rectTransform.pivot = alternateRect.pivot;
        rectTransform.anchorMin = alternateRect.anchorMin;
        rectTransform.anchorMax = alternateRect.anchorMax;
    }
}