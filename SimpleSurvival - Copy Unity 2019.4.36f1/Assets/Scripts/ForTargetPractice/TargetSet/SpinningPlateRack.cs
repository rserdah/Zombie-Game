using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningPlateRack : TargetSet
{
    public Transform rack;
    public float speed = 1f;
    public Vector3 axis;

    private void FixedUpdate()
    {
        rack.transform.Rotate(axis, Time.fixedDeltaTime * speed);
    }
}
