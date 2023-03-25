using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Interactable
{

    public enum ForwardVector
    {
        FORWARD, BACKWARD, RIGHT, LEFT, UP, DOWN
    }

    public ForwardVector forwardVector;

    public Vector3 forward
    {
        get
        {
            if(forwardVector == ForwardVector.FORWARD)
                return transform.forward;

            if(forwardVector == ForwardVector.BACKWARD)
                return -transform.forward;

            if(forwardVector == ForwardVector.RIGHT)
                return transform.right;

            if(forwardVector == ForwardVector.LEFT)
                return -transform.right;

            if(forwardVector == ForwardVector.UP)
                return transform.up;

            if(forwardVector == ForwardVector.DOWN)
                return -transform.up;


            return transform.forward;
        }
    }

    //=== States ===
    public bool pickedUp;


    public override void Awake()
    {
        needsControllingEntity = true; //Set this before calling base.Awake() b/c base.Awake() checks this bool

        base.Awake();


        SetCollidersActive(!pickedUp);
    }
} //Weapon
