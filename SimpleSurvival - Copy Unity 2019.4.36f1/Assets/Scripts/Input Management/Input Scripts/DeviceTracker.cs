﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
public abstract class DeviceTracker : MonoBehaviour
{
    protected InputManager im;
    protected InputData data;
    protected bool newData;


    private void Awake()
    {
        im = GetComponent<InputManager>();
        data = new InputData(im.axisCount, im.buttonCount);
    }

    public void ResetInput()
    {
        data.Reset();
    }

    public abstract void Refresh();
}
