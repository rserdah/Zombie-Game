using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardTracker : DeviceTracker
{
    public AxisKeys[] axisKeys;
    public KeyCode[] buttonKeys;

    /// <summary>
    /// By default, the Tracker only passes input when it detects a change in input, this bool makes it always pass input on each frame of Update()
    /// </summary>
    [Tooltip("By default, the Tracker only passes input when it detects a change in input, this bool makes it always pass input on each frame of Update()")]
    public bool alwaysPassInput = false;


    private void Reset()
    {
        im = GetComponent<InputManager>();
        axisKeys = new AxisKeys[im.axisCount];
        buttonKeys = new KeyCode[im.buttonCount];
    }

    private void Update()
    {
        //Check for inputs, if inputs detected, then newData = true
        //Populate InputData to pass to the InputManager

        for(int i = 0; i < axisKeys.Length; i++)
        {
            float val = 0f;
            if(Input.GetKey(axisKeys[i].positive))
            {
                val += 1f;
                newData = true;
            }

            if(Input.GetKey(axisKeys[i].negative))
            {
                val -= 1f;
                newData = true;
            }

            data.axes[i].input = val;
        }

        for(int i = 0; i < buttonKeys.Length; i++)
        {
            //Old
            //if(Input.GetKey(buttonKeys[i]))
            //{
            //    data.buttons[i] = true;
            //    newData = true;
            //}

            bool b;
            
            //GetKeyDown
            b = Input.GetKeyDown(buttonKeys[i]); //New - Make sure buttons[i] value always reflects the current button state because it will no longer be reset everyframe
            data.buttons[i].keyDown = b;
            newData = b ? true : newData; //Only set flag if there is input change, if not, DON'T CHANGE newData value

            //GetKey
            b = Input.GetKey(buttonKeys[i]);
            data.buttons[i].key = b;
            newData = b ? true : newData;

            //GetKeyUp
            b = Input.GetKeyUp(buttonKeys[i]);
            data.buttons[i].keyUp = b;
            newData = b ? true : newData;
        }


        //REMOVE!!!!!!!!!!!!!!!!!!!!!!
        //Debug.Log("(" + data.axes[0].lastInput + ", " + data.axes[0].input + ", " + data.axes[0].timeDelta + ")\n(lastInput, input, timeDelta)");




        if(newData || alwaysPassInput)
        {
            im.PassInput(data);
            newData = false;

            //The new InputFloat and InputBool struct system does not work with this function (because they use last frame data to keep track of input time, but this function resets values to 0/false each frame, making those values always 
            //reset and not work properly)
            //data.Reset();
        }
    }

    public override void Refresh()
    {
        im = GetComponent<InputManager>();

        //Create 2 temp. arrays for buttons and axes
        KeyCode[] newButtons = new KeyCode[im.buttonCount];
        AxisKeys[] newAxes = new AxisKeys[im.axisCount];

        if(buttonKeys != null)
        {
            for(int i = 0; i < Mathf.Min(newButtons.Length, buttonKeys.Length); i++)
            {
                newButtons[i] = buttonKeys[i];
            }
        }

        buttonKeys = newButtons;

        if(axisKeys != null)
        {
            for(int i = 0; i < Mathf.Min(newAxes.Length, axisKeys.Length); i++)
            {
                newAxes[i] = axisKeys[i];
            }
        }

        axisKeys = newAxes;
    }
}

[System.Serializable]
public struct AxisKeys
{
    public KeyCode positive;
    public KeyCode negative;
}
