using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralAnimation : MonoBehaviour
{
    public bool play;

    public AnimationCurve curve;
    public float speed = 1f;
    public float t;
    public Vector3 originalPos;


    private void Start()
    {
        originalPos = transform.position;
    }

    private void FixedUpdate()
    {
        if(play)
        {
            StopAllCoroutines();
            StartCoroutine(Play());



            play = false;
        }
    }

    public IEnumerator Play()
    {
        t = 0f;

        while(t < curve[curve.length - 1].time)
        {
            transform.position = originalPos + Vector3.up * curve.Evaluate(t);

            t += speed * Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
}
