using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controllable : MonoBehaviour
{
    [Tooltip("This is the Component that controls this Controllable. In most cases this will only be the player, but the name is kept general with the idea that later there could be an AIController " +
        "that can control Controllable's as well")]
    public PlayerInput controller;

    [Tooltip("Where will the controller be held while controlling this Controllable? (e.g. if the player is driving a car, they would be anchored (parented) to a place in the driver's seat)")]
    private Transform controllerAnchor;

    public bool isControlled
    {
        get
        {
            return controller != null;
        }
    }


    private void Start()
    {
        controllerAnchor = transform.Find("ControllerAnchor");
        TransformHelper.DisableIfNull(transform, controllerAnchor, "No ControllerAnchor found, disabling", gameObject);
    }

    private void Update()
    {
        if(isControlled)
        {
            
        }
    }

    public bool ControlBegin(PlayerInput newController)
    {
        if(newController && !isControlled)
        {
            controller = newController;

            controller.transform.position = controllerAnchor.position;
            controller.transform.rotation = controllerAnchor.rotation;
            controller.gravity = 0;

            Debug.LogError(newController.name + " is now controlling " + gameObject.name);

            return true;
        }

        return false;
    }

    public bool ControlEnd()
    {
        Debug.LogError(controller.gameObject.name + " ended control with " + gameObject.name);

        controller = null;

        return true;
    }
}
