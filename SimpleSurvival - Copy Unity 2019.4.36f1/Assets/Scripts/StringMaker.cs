using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using Extensions;

[ExecuteInEditMode]
public class StringMaker : MonoBehaviour
{
    [ReadOnly]
    [SerializeField]
    private string formattedString = "";

    public string[] strings;
    public UnityEvent[] events;


    public void Add(string addString)
    {
        formattedString += addString;
    }

    public void Add(int addInt)
    {
        formattedString += addInt;
    }

    public string FormatString()
    {
        if(strings.Length > 0 && strings.Length == events.Length)
        {
            formattedString = "";

            for(int i = 0; i < strings.Length; i++)
            {
                Add(strings[i]);
                events[i].Invoke();
            }
        }


        return formattedString;
    }

    ///// <summary>
    ///// A method that is "compatible" with StringMaker is one that calls this callback at the end of its own method
    ///// </summary>
    //public void WildCardCallback(string newWildCard)
    //{
    //    foundWildCard = newWildCard;
    //}

    //public string InterpolateString()
    //{
    //    //This is like regular string interpolation except instead of inputting $"Hi, my name is {name}" into Inspector, one would just put Hi, my name is {GetName} (can only call methods and don't put the parentheses (b/c going to use Invoke()))

    //    string str = "", wildCard = "";

    //    int i = 0;
    //    wildCard = StringHelper.GetWildCard(interpolationString, "{", "}", i, out i);

    //    //while(!wildCard.Equals(""))
    //    {
    //        string replace = "{" + wildCard + "}";

    //        //If wildcard is the name of a method that is "compatible" with StringMaker, it will call StringMaker.WildCardCallback(string newWildCard) to set the real value of what the wildcard represents so the formatted string can be built
    //        monoBehaviour.Invoke(wildCard, 0f);

    //        Debug.LogError("Replacing: " + replace + " with: " + foundWildCard);
    //        str = interpolationString.Replace(replace, foundWildCard); //finish !!!!!!!!!!!!!!!!!!!!!!

    //        Debug.LogError("Replacing: " + replace + " with: " + foundWildCard);
    //    }


    //    return str;
    //}
}


