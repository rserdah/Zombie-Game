using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperTargetPlate : Target
{
    public List<Transform> shards;
    public Vector3 lastHit = Vector3.zero;

    private void Start()
    {
        Init(null);
    }
    public override void Hit(RaycastHit hit)
    {
        base.Hit(hit);


        Transform t = GetNearestShard(hit.point);
        Debug.LogError("Nearest piece is: " + t.name, t.gameObject);
        t.gameObject.SetActive(false);
    }

    private Transform GetNearestShard(Vector3 position)
    {
        float sqrDist = SqrDist(shards[0].position, position), tmp;
        int closestIndex = 0;

        for(int i = 1; i < shards.Count; i++)
        {
            tmp = SqrDist(shards[i].position, position);

            if(tmp < sqrDist)
            {
                sqrDist = tmp;
                closestIndex = i;
            }
        }

        return shards[closestIndex];
    }

    private float SqrDist(Vector3 from, Vector3 to)
    {
        return (to - from).sqrMagnitude;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(lastHit, 0.01f);
    }
}
