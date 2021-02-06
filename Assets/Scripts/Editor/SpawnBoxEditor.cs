using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

[CustomEditor(typeof(SpawnBox))]
public class SpawnBoxEditor : Editor
{
    BoxBoundsHandle boxHandle = new BoxBoundsHandle();

    private void OnSceneGUI()
    {
        var myScript = (SpawnBox)target;

        boxHandle.center = myScript.box.center;
        boxHandle.size = myScript.box.size;

        EditorGUI.BeginChangeCheck();
        boxHandle.DrawHandle();
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(myScript, "Changed Bounds");

            myScript.box.center = boxHandle.center;
            myScript.box.size = boxHandle.size;
        }
    }
}

