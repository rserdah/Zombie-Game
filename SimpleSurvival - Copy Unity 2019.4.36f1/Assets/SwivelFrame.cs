using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwivelFrame : MonoBehaviour
{
    public Transform axle;
    public bool thisX;
    public bool thisY;
    public bool thisZ;

    public bool axleX;
    public bool axleY;
    public bool axleZ;


    private void Update()
    {
        if(thisX)
        {
            if(axleX)
                transform.right = axle.right;
            else if(axleY)
                transform.right = axle.up;
            else if(axleZ)
                transform.right = axle.forward;
        }
        else if(thisY)
        {
            if(axleX)
                transform.up = axle.right;
            else if(axleY)
                transform.up = axle.up;
            else if(axleZ)
                transform.up = axle.forward;
        }
        else if(thisZ)
        {
            if(axleX)
                transform.forward = axle.right;
            else if(axleY)
                transform.forward = axle.up;
            else if(axleZ)
                transform.forward = axle.forward;
        }
    }
}
