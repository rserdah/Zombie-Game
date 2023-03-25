using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBoundaries : MonoBehaviour
{
    public float radius = 50f;

    public Transform player;
    private PlayerInput playerInput;

    private float sqrDist;


    private void Start()
    {
        if(player)
            playerInput = player.GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if(player && playerInput)
        {
            sqrDist = (player.position - transform.position).sqrMagnitude;

            //If out of bounds and not yet set out of bounds
            if(sqrDist >= radius * radius && !playerInput.IsOutOfBounds())
            {
                playerInput.NotifyOutOfBounds(true);
            }
            //Else if in bounds and not yet set in bounds
            else if(sqrDist < radius * radius && playerInput.IsOutOfBounds())
            {
                playerInput.NotifyOutOfBounds(false);
            }
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
