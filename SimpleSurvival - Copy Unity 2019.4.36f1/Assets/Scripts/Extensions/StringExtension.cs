using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class StringExtension
    {
        public static string SubstringFromTo(this string str, int start, int end)
        {
            return str.Substring(start, end - start);
        }
    }
}
