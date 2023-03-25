using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSDebug : MonoBehaviour
{
    public Animator anim;


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            anim.Play("Shoot", 0, 0f);
        }
    }
}
