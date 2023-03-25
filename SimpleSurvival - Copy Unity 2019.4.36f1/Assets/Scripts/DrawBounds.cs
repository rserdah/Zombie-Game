using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DrawBounds : MonoBehaviour
{
    public BoxCollider collider;


    private void Start()
    {
        collider = GetComponent<BoxCollider>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 size = Vector3.zero;
        size += transform.right * transform.localScale.x * collider.size.x;
        size += transform.up * transform.localScale.y * collider.size.y;
        size += transform.forward * transform.localScale.z * collider.size.z;
        Gizmos.DrawWireCube(transform.position, size);
    }
}
