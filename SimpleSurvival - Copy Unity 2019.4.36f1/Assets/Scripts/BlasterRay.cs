using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlasterRay : MonoBehaviour
{
    public float speed = 1f;
    public float trailInterval = 0.5f;
    public float targetTrailScale = 1.25f;
    public float trailScaleSpeed = 1f;
    public bool active = true;

    public Color color;
    public bool setColor;
    public bool rainbow;
    public bool random;

    public GameObject trail;
    public GameObject explosion;

    Coroutine coroutine;
    float hue = 0f;

    public Gun.QueuedHit hit;


    private void Awake()
    {
        coroutine = StartCoroutine(ShootCoroutine());

        if(rainbow || random)
        {
            hue = Mathf.Repeat(Random.Range(0, 11) * 0.1f, 1f);
            color = Color.HSVToRGB(hue, 1f, 1f);
        }

        if(setColor || rainbow || random)
            SetStartColor(gameObject, color);
    }

    private IEnumerator ShootCoroutine()
    {
        float timer = 0f;
        while(active)
        {
            transform.position += speed * Time.fixedDeltaTime * transform.forward;

            timer += Time.fixedDeltaTime;

            if(timer > trailInterval && trail)
            {
                GameObject trailGameObject;
                StartCoroutine(FadeScaleCoroutine(trailGameObject = Instantiate(trail, transform.position, transform.rotation, null)));

                if(rainbow)
                {
                    color = Color.HSVToRGB(hue, 1f, 1f);
                    hue = Mathf.Repeat(hue + 0.1f, 1f);
                }

                if(setColor)
                    SetStartColor(trailGameObject, color);

                timer = 0f;
            }

            yield return new WaitForFixedUpdate();
        }

        yield return null;
    }

    private IEnumerator FadeScaleCoroutine(GameObject g)
    {
        float scale = 1f;
        Vector3 startScale = g.transform.localScale;
        while(scale <= targetTrailScale)
        {
            scale += trailScaleSpeed * Time.fixedDeltaTime;

            //If the ray trail still exists, then scale it according to the scale speed
            if(g)
                g.transform.localScale = scale * startScale;
            //Else if the ray trail has deleted itself before this Coroutine has finished scaling the GameObject, then just break out of the loop
            else
                break;

            yield return new WaitForFixedUpdate();
        }

        if(g)
            g.transform.localScale = targetTrailScale * startScale;

        yield return null;
    }

    public void SetColor(Color newColor)
    {
        color = newColor;
        SetStartColor(gameObject, color);
    }

    private void SetStartColor(GameObject g, Color color)
    {
        foreach(ParticleSystem p in g.transform.GetComponentsInChildren<ParticleSystem>())
        {
            ParticleSystem.MainModule main = p.main;
            main.startColor = color;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        active = false;
        StopCoroutine(coroutine);

        if(explosion)
        {
            GameObject explosionGameObject;
            explosionGameObject = Instantiate(explosion, transform.position, new Quaternion());

            if(setColor || rainbow)
                SetStartColor(explosionGameObject, color);
        }

        hit.instigatorCallback?.Invoke(hit);

        Destroy(gameObject, 0.25f);
    }
}
