using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Range(0, 10)]
    public int axisCount;

    [Range(0, 20)]
    public int buttonCount;

    //This should be the final form of the controller variable
    //public Controller controller { get; private set; }
    public Controller controller;
    /// <summary>
    /// This is the Controller that is waiting to resume control after the current Controller is done controlling. For example, if a player gets in a vehicle, the player becomes the 
    /// standbyController and the vehicle becomes the current Controller and once the vehicle is done controlling (the player exits the vehicle), the player resumes control.
    /// </summary>
    public Controller standbyController;
    private bool isNewController = false;
    private int waitFrameCount;


    public void Start()
    {
        if(controller)
            controller.Enable(this);
    }

    private void RequestStandby(Controller newStandbyController)
    {
        if(!standbyController) standbyController = newStandbyController;
        else Debug.LogError("There is already a standby Controller for this InputManager", gameObject);
    }

    public void RequestTransfer(Controller newController)
    {
        //Since the current standby Controller should have priority over any other Controller's to switch to, overwrite the newController with standbyController
        if (standbyController)
        {
            newController = standbyController;
            standbyController = null;
        }
        else
        {
            //If there is no current standby Controller, then the current Controller becomes the standby Controller
            standbyController = controller;
        }

        Debug.LogError($"Transferring from {controller.name} to {newController.name}");
        controller.Disable();
        controller = newController;
        isNewController = true;
        waitFrameCount = 2;
        controller.Enable(this);

        //Reset DeviceTracker so that the newController does not take the input from the last Controller (because that can cause mutiple transfers for example if a player presses a button to enter a
        //vehicle, the vehicle will also pick up the input for exitting the vehicle and it will switch back to the player)
        ResetTracker();
    }

    public void PassInput(InputData data)
    {
        if(waitFrameCount == 0)
        {
            if(isNewController)
            {
                controller.Enable(this);
                isNewController = false;
            }
            else
            {
                if(controller.active) controller.ReadInput(data);
            }

        }
        else if(isNewController)
        {
            waitFrameCount--;

            if(waitFrameCount < 0)
                waitFrameCount = 0;
        }

    }

    public void RefreshTracker()
    {
        DeviceTracker dt = GetComponent<DeviceTracker>();

        if(dt != null)
            dt.Refresh();
    }

    public void ResetTracker()
    {
        DeviceTracker dt = GetComponent<DeviceTracker>();

        if (dt != null)
            dt.ResetInput();
    }
}

public struct InputData
{
    //public float[] axes;
    //public bool[] buttons;

    public InputFloat[] axes;
    //private bool[] m_buttons;
    //public bool[] buttons { get { Debug.Log("Buttons use Input.OnKeyUp(). Change this for more input flexibility."); return m_buttons; } }

    //New buttons input
    private InputBool[] m_buttons;
    public InputBool[] buttons { get => m_buttons; }


    public InputData(int axisCount, int buttonCount)
    {
        axes = new InputFloat[axisCount];
        //m_buttons = new bool[buttonCount];
        m_buttons = new InputBool[buttonCount];
    }

    public void Reset()
    {
        for(int i = 0; i < axes.Length; i++)
        {
            axes[i].input = 0f;
        }

        //for(int i = 0; i < buttons.Length; i++)
        //{
        //    buttons[i] = false;
        //}

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].keyDown = false;
            buttons[i].key = false;
            buttons[i].keyUp = false;
        }
    }
}

public struct InputBool
{
    public bool keyDown;
    public bool key;
    public bool keyUp;

    //private bool m_input;
    //public bool input
    //{
    //    get
    //    {
    //        return m_input;
    //    }

    //    set
    //    {
    //        //Record last value before setting the new value
    //        lastInput = m_input;
    //        m_input = value;

    //        //If input continues to be held OR input went from false to true, then increment the time that input has been held for
    //        if(lastInput && m_input || !lastInput && m_input)
    //            timeDelta += Time.deltaTime;
    //        //Else if input continues to not be held OR input went from true to false, then reset the time
    //        else if(!lastInput && !m_input || lastInput && !m_input)
    //            timeDelta = 0f;
    //    }
    //}

    public bool lastInput { get; private set; }
    public float timeDelta { get; private set; }
}

public struct InputFloat
{
    private float m_input;
    public float input
    {
        get
        {
            return m_input;
        }

        set
        {
            //Record last value before setting the new value
            lastInput = m_input;
            m_input = value;

            ////If input continues to be held OR input went from none to something, then increment the time that input has been held for
            //if(lastInput != 0 && m_input != 0 || lastInput == 0 && m_input != 0)
            //    timeDelta += Time.deltaTime;
            ////Else if input continues to not be held OR input went from something to nothing, then reset the time
            //else if(lastInput == 0 && m_input == 0 || lastInput != 0 && m_input == 0)
            //    timeDelta = 0f;


            //Compare using any side of axis because there can only be input on one side of the axis at a time
            //If input continues to be held OR input went from none to something, then increment the time that input has been held for
            if(val(lastInput) && val(m_input) || !val(lastInput) && val(m_input))
                timeDelta += Time.deltaTime;
            //Else if input continues to not be held OR input went from something to nothing, then reset the time
            else if(!val(lastInput) && !val(m_input) || val(lastInput) && !val(m_input))
                timeDelta = 0f;
        }
    }
    public float lastInput { get; private set; }
    public float timeDelta { get; private set; }

    public float deadzone { get; private set; }


    public InputFloat(float _deadzone)
    {
        m_input = lastInput = timeDelta = 0f;
        deadzone = _deadzone;
    }
    
    public bool GetKeyDown(int dir = 0)
    {
        //Key is down on the frame that input went from 0 to not 0 (or generally none to some)
        //return (lastInput == 0f && m_input != 0f);
        return (!val(lastInput, dir) && val(m_input, dir));
    }

    public bool GetKey()
    {
        //Key is held when there is some input (on any side of axis because there can only be input on one side of axis at a time anyways) for more than 0 seconds AND is not the same frame that the key went down on
        return timeDelta > 0 && !GetKeyDown();
    }

    /// <summary>
    /// GetKeyUp() is not currently working (Seems to maybe skip over the frame where input goes from 1 to 0 so key up is not registered(maybe it is skipping passing input on that frame?))
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public bool GetKeyUp(int dir = 0)
    {
        //Key is up on the frame that input went from not 0 to 0
        //return (lastInput != 0f && m_input == 0f);
        Debug.LogError("InputFloat.GetKeyUp() is not functioning properly yet");
        return (val(lastInput, dir) && !val(m_input, dir));
    }

    /// <summary>
    /// Private method for detecting if a value is above the deadzone threshold to decide if there is input or not. 
    /// If dir ==  0, it returns true if there is input on any (pos/neg) side of the axis, 
    /// if dir ==  1, it returns true if there is input on the positive  side of the axis, 
    /// if dir == -1, it returns true if there is input on the negative  side of the axis, 
    /// else, it returns false
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private bool val(float value, int dir = 0)
    {
        //Compare non-inclusive because if the deadzone is 0 and the input is 0, then this should return false (because it should not register as input)
        if(dir == 0)
            return Mathf.Abs(value) > deadzone;
        else if(dir == 1)
            return value > deadzone;
        else if(dir == -1)
            return value < -deadzone;
        else
            return false;
    }
}