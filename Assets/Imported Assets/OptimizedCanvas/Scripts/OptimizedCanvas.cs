﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class OptimizedCanvas : MonoBehaviour
{
    [SerializeField] Canvas canvas = null;
    public Canvas Canvas { get { return canvas; } }

    [SerializeField] bool hideOnAwake = false;
    [SerializeField] bool bakeLayoutOnStart = false;

    public RectTransform rectTransform
    {
        get
        {
            return transform as RectTransform;
        }
    }

    [SerializeField] GraphicRaycaster caster;

    List<OptimizedCanvas> children = new List<OptimizedCanvas>();
    OptimizedCanvas parent = null;

    [SerializeField] OptimizedTransitionBase[] transitions = new OptimizedTransitionBase[0];
    float transitionInTime;
    public float TransitionInTime { get { return transitionInTime; } }
    float transitionOutTime;
    public float TransitionOutTime { get { return transitionOutTime; } }

    [SerializeField] new OptimizedAnimationBase animation = null;

    [SerializeField] public UnityEngine.Events.UnityEvent onCanvasShow;
    [SerializeField] public UnityEngine.Events.UnityEvent onCanvasStartHide;
    [SerializeField] public UnityEngine.Events.UnityEvent onCanvasHide;

    System.Action OnCanvasShow;
    System.Action OnCanvasHide;

    void InvokeOnCanvasShow()
    {
        OnCanvasShow?.Invoke();
        onCanvasShow.Invoke();
    }

    void InvokeOnCanvasHide()
    {
        OnCanvasHide?.Invoke();
        onCanvasHide.Invoke();
    }

    Coroutine layoutRoutine = null;

    public bool IsVisible
    { 
        get 
        {
            if (parent) return parent.IsVisible && canvas.enabled;
            else return canvas.enabled;
        } 
    }

    void Awake()
    {
        if (hideOnAwake)
        {
            Hide();
        }
    }

    private void OnTransformParentChanged()
    {
        LocateParentCanvases();
    }

    private void OnEnable()
    {
        LocateChildCanvases();
        LocateParentCanvases();

        //ResolutionListener.OnResolutionChanged += FlashLayoutComponents;
    }

    private void OnDisable()
    {
        //ResolutionListener.OnResolutionChanged -= FlashLayoutComponents;

        if (parent)
        {
            parent.OnCanvasShow -= OnParentVisibilityChanged;
            parent.OnCanvasHide -= OnParentVisibilityChanged;
        }
    }

    bool previousVisibility;
    private void OnParentVisibilityChanged()
    {
        if (previousVisibility == IsVisible) return;
        if (IsVisible) ShowIfPreviouslyVisible();
        else HideWithParent();
        previousVisibility = IsVisible;
    }

    private IEnumerator Start()
    {
        for (int i = 0; i < transitions.Length; i++)
        {
            if (transitions[i].GetTransitionInTime() > transitionInTime)
            {
                transitionInTime = transitions[i].GetTransitionInTime();
            }
        }

        for (int i = 0; i < transitions.Length; i++)
        {
            if (transitions[i].GetTransitionInTime() > transitionOutTime)
            {
                transitionOutTime = transitions[i].GetTransitionOutTime();
            }
        }

        if (bakeLayoutOnStart) Show();

        yield return null;

        if (bakeLayoutOnStart) Hide();

        previousVisibility = IsVisible;
    }

#if UNITY_EDITOR
    [ContextMenu("Find Get References")]
    private void OnValidate()
    {
        if (canvas != GetComponent<Canvas>()) canvas = GetComponent<Canvas>();
        if (caster != GetComponent<GraphicRaycaster>()) caster = GetComponent<GraphicRaycaster>();
        if (animation != GetComponent<OptimizedAnimationBase>()) animation = GetComponent<OptimizedAnimationBase>();
    }

    public void RegisterUndo()
    {
        UnityEditor.Undo.RecordObject(canvas, "Modified canvas visibility");
        if (caster)
        {
            UnityEditor.Undo.RecordObject(caster, "Modified canvas visibility");
        }
    }

    [ContextMenu("Show Canvas")]
    public void EditorShow()
    {
        if (Application.isPlaying)
        {
            Show();
            return;
        }

        LocateChildCanvases();
        RegisterUndo();
        if (caster)
        {
            caster.enabled = true;
        }
        canvas.enabled = true;
        for (int j = 0; j < transitions.Length; j++)
        {
            transitions[j].EditorTransitionIn();
        }
    }

    [ContextMenu("Hide Canvas")]
    public void EditorHide()
    {
        if (Application.isPlaying)
        {
            Hide();
            return;
        }

        LocateChildCanvases();
        RegisterUndo();
        if (caster)
        {
            caster.enabled = false;
        }
        canvas.enabled = false;
        for (int j = 0; j < transitions.Length; j++)
        {
            transitions[j].EditorTransitionOut();
        }
    }
#endif

    public void LocateParentCanvases()
    {
        if (parent)
        {
            parent.OnCanvasShow -= OnParentVisibilityChanged;
            parent.OnCanvasHide -= OnParentVisibilityChanged;
        }

        if (transform.parent)
        {
            parent = transform.parent.GetComponentInParent<OptimizedCanvas>();
        }

        if (parent)
        {
            parent.OnCanvasShow += OnParentVisibilityChanged;
            parent.OnCanvasHide += OnParentVisibilityChanged;
        }
    }

    public void LocateChildCanvases()
    {
        children = new List<OptimizedCanvas>(GetComponentsInChildren<OptimizedCanvas>());
        children.Remove(this);
    }

    private void OnTransformChildrenChanged()
    {
        LocateChildCanvases();
    }

    public void Show() => SetActive(true);

    public void ShowDelayed(float time)
    {
        Invoke("Show", time);
    }

    public void Hide() => SetActive(false);

    public void SetActive(bool active)
    {
        if (canvas == null) return;

        // Disable GraphicRaycaster first to ensure buttons don't get pressed during transition
        if (caster)
        {
            caster.enabled = active;
        }

        // Nothing changed, skip rest
        if (canvas.enabled == active && Application.isPlaying) return;

        if (transitions.Length > 0)
        {
            if (Application.isPlaying && gameObject.activeInHierarchy)
            {
                StartCoroutine(DoTransition(active));
            }
            // Can't run a coroutine during editor time
            else
            {
                if (active)
                {
                    for (int i = 0; i < transitions.Length; i++) transitions[i].EditorTransitionIn();

                }
                else
                {
                    for (int i = 0; i < transitions.Length; i++) transitions[i].EditorTransitionOut();
                }
            }
        }

        if (!gameObject.activeInHierarchy) return;

        // Invoke events immediately, will not invoke if transitions are applied
        if (transitions.Length == 0)
        {
            canvas.enabled = active;

            if (canvas.enabled)
            {
                InvokeOnCanvasShow();
            }
            else
            {
                onCanvasStartHide.Invoke();
                InvokeOnCanvasHide();
            }
        }
    }

    public void ShowIfPreviouslyVisible()
    {
        if (!gameObject.activeInHierarchy) return;

        if (caster)
        {
            caster.enabled = true;
        }
        InvokeOnCanvasShow();
    }

    public void HideWithParent()
    {
        if (caster)
        {
            caster.enabled = false;
        }
        onCanvasStartHide.Invoke();
        InvokeOnCanvasHide();
    }

    IEnumerator DoTransition(bool active)
    {
        if (active)
        {
            canvas.enabled = active;
            InvokeOnCanvasShow();
            for (int i = 0; i < transitions.Length; i++)
            {
                transitions[i].TransitionIn();
            }
            yield return new WaitForSeconds(transitionInTime);
        }
        else
        {
            onCanvasStartHide.Invoke();
            for (int i = 0; i < transitions.Length; i++)
            {
                transitions[i].OnTransitionOut += InvokeOnCanvasHide;
                transitions[i].TransitionOut();
            }
            yield return new WaitForSeconds(transitionOutTime);
            for (int i = 0; i < transitions.Length; i++)
            {
                transitions[i].OnTransitionOut -= InvokeOnCanvasHide;
            }
            canvas.enabled = active;
        }
    }

    public void PlayAnimation()
    {
        if (animation != null) animation.StartAnimating();
    }

    public void StopAnimation()
    {
        if (animation != null) animation.StopAnimating();
    }
}