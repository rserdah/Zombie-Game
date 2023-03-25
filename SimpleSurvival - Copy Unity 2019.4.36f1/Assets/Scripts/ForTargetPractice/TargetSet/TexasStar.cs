using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexasStar : TargetSet
{
    private Rigidbody rb;
    public float spinTorquePerShot = 5f;


    public override void Start()
    {
        base.Start();


        rb = GetComponentInChildren<Rigidbody>();
        //Reset center of mass to local zero position because it defaults to taking the center of all child Colliders, this makes it rotate incorrectly
        rb.centerOfMass = Vector3.zero;
    }

    public override void OnHit()
    {
        base.OnHit();

        rb.AddTorque(spinTorquePerShot * transform.forward);
    }
}
