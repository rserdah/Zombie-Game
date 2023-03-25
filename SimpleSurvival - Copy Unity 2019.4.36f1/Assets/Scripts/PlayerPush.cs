using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    public float pushForce = 100;
    public float rigidbodyDetectionRadius = 1f;
    public Collider[] cols;


    public void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            cols = Physics.OverlapSphere(transform.position, rigidbodyDetectionRadius);

            Rigidbody r;
            foreach(Collider c in cols)
            {
                if (r = c.GetComponent<Rigidbody>())
                {
                    r.AddForce(transform.forward * pushForce);
                    //Debug.LogError($"Pushing { r.name }");
                    //Since some objects can have more than one Collider, you should break the loop after hitting the first Collider with a Rigidbody so the force is only applied once per frame.
                    //Also, this makes it so you can only push one object at a time
                    break;
                }
            }
        }
    }
}
