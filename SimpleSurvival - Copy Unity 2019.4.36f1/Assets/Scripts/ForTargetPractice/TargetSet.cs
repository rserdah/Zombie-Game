using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSet : MonoBehaviour
{
    List<TargetPlate> targetPlates = new List<TargetPlate>();


    public virtual void Start()
    {
        TargetPlate[] plates = GetComponentsInChildren<TargetPlate>();

        foreach(TargetPlate t in plates)
        {
            t.Init(this);
            targetPlates.Add(t);
        }
    }

    public virtual void OnHit()
    {

    }
}
