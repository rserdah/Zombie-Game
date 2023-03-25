using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelRotate : MonoBehaviour
{
    public enum RotationAxis
    {
        X, Y, Z
    }


    private float step
    {
        get
        {
            return 360f / numberOfBarrels;
        }
    }
    private float direction
    {
        get
        {
            return flipDirection ? -1f : 1f;
        }
    }

    public int numberOfBarrels = 6;
    public Quaternion[] snapToRotations;
    //public float speed = 1;
    public RotationAxis rotationAxis;
    public bool flipDirection = false;
    private float startRot;

    /// <summary>
    /// The number of rotations by step degrees made
    /// </summary>
    private int steps;
    public Quaternion rotation;

    public bool set;
    public float time;
    public Transform rotationHolder;

    public Rigidbody rb;
    public Vector3 torque;
    public ForceMode forceMode;



    public float speed2 = 10;
    public float maxSpeed = 10;
    public float decel = 0.5f;

    public float error = 0.001f;
    public float stopSpeed = 0.1f;



    private void Start()
    {
        rotation = transform.localRotation;

        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        if(set)
        {
            time = 0;

            //rb.AddTorque(torque, forceMode);

            speed2 = maxSpeed;


            //transform.RotateAround(transform.position, transform.up, direction * step);


            set = false;
        }

        speed2 -= Time.deltaTime * decel;

        speed2 = Mathf.Clamp(speed2, 0f, maxSpeed);

        transform.RotateAround(transform.position, transform.up, Time.deltaTime * direction * speed2);

        rotation = transform.localRotation;

        //if(speed2 <= stopSpeed && (QuaternionAbsValueEqual(rotation, snapToRotations[0], error) || QuaternionAbsValueEqual(rotation, snapToRotations[1], error) || QuaternionAbsValueEqual(rotation, snapToRotations[2], error)))
        //{
        //    Debug.Break();
        //}

        //transform.localRotation = Quaternion.Slerp(transform.localRotation, rotationHolder.localRotation, time);
        //time += Time.deltaTime * speed;

        //transform.RotateAround(transform.position, transform.up, Time.deltaTime * speed);
    }

    public void Rotate()
    {
        steps++;

        rotationHolder.RotateAround(rotationHolder.position, transform.up, step);

        //rotation = transform.localRotation;
    }

    public void Set()
    {
        set = true;
    }

    private bool QuaternionAbsValueEqual(Quaternion q1, Quaternion q2, float error)
    {
        float x, y, z, w;

        x = Mathf.Abs(Mathf.Abs(q1.x) - Mathf.Abs(q2.x));
        y = Mathf.Abs(Mathf.Abs(q1.y) - Mathf.Abs(q2.y));
        z = Mathf.Abs(Mathf.Abs(q1.z) - Mathf.Abs(q2.z));
        w = Mathf.Abs(Mathf.Abs(q1.w) - Mathf.Abs(q2.w));

        return x < error && y < error && z < error && w < error;
    }
}
