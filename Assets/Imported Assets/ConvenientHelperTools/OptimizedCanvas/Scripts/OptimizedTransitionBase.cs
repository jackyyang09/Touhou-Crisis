using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OptimizedTransitionBase : MonoBehaviour
{
    public abstract Coroutine TransitionIn();
    public abstract Coroutine TransitionOut();

    public abstract void EditorTransitionIn();
    public abstract void EditorTransitionOut();
}