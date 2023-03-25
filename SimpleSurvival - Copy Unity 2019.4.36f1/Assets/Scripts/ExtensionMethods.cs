using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionMethodsTest
{
    public static class ExtensionMethods
    {
        public static void StringExtenstion(this string b)
        {
            //At top of script, write using ExtensionMethodsTest; and then you can use these extension methods. 
            //For example:

            //string s = "";
            //s.StringExtenstion();
        }

        public static void Vector3Extension(this Vector3 b)
        {

        }

        /// <summary>
        /// Allows a basic delay before invoking an Action using Coroutine. This requires some sort of MonoBehaviour to be attached to the GameObject. This extension method is used mainly as a way to 
        /// execute an anonymous function with a Coroutine delay on a GameObject that has no dedicated script attached to start coroutines with. This is useful for items that perform usually only one
        /// basic task and an entirely new script should not be written for them
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="delay"></param>
        /// <param name="doAction"></param>
        /// <returns></returns>
        public static Coroutine WaitAndDo(this GameObject gameObject, float delay, Action doAction)
        {
            MonoBehaviour behaviour = gameObject.GetComponent<MonoBehaviour>();

            if(behaviour)
            {
                return behaviour.StartCoroutine(WaitAndDoCoroutine(delay, doAction));
            }
            else
            {
                Debug.LogError($"GameObject {gameObject.name} does not have a MonoBehaviour attached to it. Cannot start a Coroutine on this GameObject.", gameObject);
                return null;
            }
        }

        public static IEnumerator WaitAndDoCoroutine(float delay, Action doAction)
        {
            yield return new WaitForSeconds(delay);

            doAction?.Invoke();
        }
    }
}
