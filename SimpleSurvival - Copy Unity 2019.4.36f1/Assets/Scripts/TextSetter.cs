using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to animate the text of a Text element (because they are not animatable by default)
/// </summary>
public class TextSetter : MonoBehaviour
{
    public Text text;
    public string message;
    public int hi;


    private void Update()
    {
        text.text = message;
    }
}
