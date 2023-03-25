using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Edited from luizgpa's answer on Unity Answers (https://answers.unity.com/questions/192895/hideshow-properties-dynamically-in-inspector.html?_ga=2.255826184.1870527448.1620843940-2109119494.1545346813)
/// </summary>

public class ToggleInspectorVariable : MonoBehaviour
{
    public bool clickToShowOrHideVariable;
    public int i = 1;
}

[CustomEditor(typeof(ToggleInspectorVariable))]
public class MyScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var myScript = target as ToggleInspectorVariable;

        myScript.clickToShowOrHideVariable = GUILayout.Toggle(myScript.clickToShowOrHideVariable, "Click To Show Or Hide Variable");

        if(myScript.clickToShowOrHideVariable)
            myScript.i = EditorGUILayout.IntSlider("I field:", myScript.i, 1, 100);

    }
}
