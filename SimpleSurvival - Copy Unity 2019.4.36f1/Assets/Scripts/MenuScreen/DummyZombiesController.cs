using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyZombiesController : MonoBehaviour
{
    public Animator[] anims;

    public Vector2 minMaxSpeed = new Vector2(0, 1);
    public string speedParameterName = "Speed";

    public Vector2 minMaxCycleOffset = new Vector2(0, 1);
    public string cycleOffsetParameterName = "CycleOffset";


    private void Start()
    {
        foreach(Animator a in anims)
        {
            a.SetFloat(speedParameterName, Random.Range(minMaxSpeed.x, minMaxSpeed.y));
            a.SetFloat(cycleOffsetParameterName, Random.Range(minMaxCycleOffset.x, minMaxCycleOffset.y));
        }
    }
}
