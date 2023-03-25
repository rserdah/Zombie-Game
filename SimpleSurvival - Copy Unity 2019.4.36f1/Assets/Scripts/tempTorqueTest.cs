using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempTorqueTest : MonoBehaviour
{
    public bool set;

    public Rigidbody rb;
    public Transform forwardTorque;
    public float torqueMagnitude;
    public ForceMode forceMode;


    private void Update()
    {
        if(set)
        {
            rb.AddTorque(forwardTorque.right * torqueMagnitude, forceMode);


            set = false;
        }
    }
}
