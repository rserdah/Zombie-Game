using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VariantModifier : MonoBehaviour
{
    [MenuItem("Tools/Modifiers/Variant/Set Target Index")]
    public static void SetTargetIndex()
    {
        VariantModifier v = null;
        Transform t = Selection.activeTransform, t1 = Selection.activeTransform;
        List<int> targetIndex = new List<int>();

        while(t && !(v = t.GetComponent<VariantModifier>()))
        {
            targetIndex.Add(t.GetSiblingIndex());
            t = t.parent;
        }
        targetIndex.Reverse();
        if(v)
        {
            v.t = t1;
            v.targetIndex = targetIndex.ToArray();
        }
    }


    public int[] targetIndex;
    public Transform t;
}
