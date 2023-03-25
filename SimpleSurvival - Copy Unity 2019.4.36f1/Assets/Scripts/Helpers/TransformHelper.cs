using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformHelper
{
    //public Transform FindChild(Transform parent, string name)
    //{
    //    foreach(Transform child in )
    //}

    public static Transform FindRecursive(Transform parent, string name)
    {
        //Debug.LogError($"Checking {parent.name}");

        if(parent.name.Equals(name))
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            //Not sure if there is a way to make it cleaner/more efficient by only checking the name once instead of twice (outside and inside of the loop)
            Transform t = FindRecursive(parent.GetChild(i), name);
            if (t && t.name.Equals(name))
                return t;
        }

        return null;
    }

    /// <summary>
    /// Returns true if had to disable the tranform (b/c check was null), else returns false
    /// </summary>
    /// <param name="t"></param>
    /// <param name="check"></param>
    /// <param name="debugMessage"></param>
    /// <param name="debugContext"></param>
    /// <returns></returns>
    public static bool DisableIfNull(Transform t, object check, object debugMessage = null, Object debugContext = null)
    {
        if(check == null)
        {
            t.gameObject.SetActive(false);
            if(debugMessage != null)
                Debug.LogError(debugMessage, debugContext);

            return true;
        }

        return false;
    }
}
