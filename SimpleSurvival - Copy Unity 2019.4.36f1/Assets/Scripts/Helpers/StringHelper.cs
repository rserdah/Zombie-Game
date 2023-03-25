using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringHelper
{
    public static string GetWildCard(string str, string leftHand, string rightHand, int startIndex, out int nextStartIndex)
    {
        //Example:
        //string str = "Hi, my name is {GetName}"
        //GetWildCard(str, "{", "}", 0) will return "GetName"

        string wildCard;
        int wildCardStart, wildCardEnd, length;

        wildCardStart = str.IndexOf(leftHand, startIndex) + leftHand.Length;
        wildCardEnd = str.IndexOf(rightHand, startIndex);

        nextStartIndex = wildCardEnd + rightHand.Length;

        length = wildCardEnd - wildCardStart;

        if(wildCardStart > 0 && length > 0)
            wildCard = str.Substring(wildCardStart, length);
        else
            wildCard = "";

        return wildCard;
    }
}
