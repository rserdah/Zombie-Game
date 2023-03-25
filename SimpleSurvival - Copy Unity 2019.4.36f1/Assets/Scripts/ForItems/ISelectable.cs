using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectable
{
    GameObject iGameObject { get; }

    string prompt
    {
        get;
    }

    //string GetPrompt();

    void Use(IItemUser iItemUser);
}
