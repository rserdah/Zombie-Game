using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayBlasterShooter : MonoBehaviour, Gun.QueuedHitSender
{
    public GameObject rayPrefab;
    public Transform shootPoint;
    public Animator anim;

    public float rainbowSpeed = 1;
    private float hue;
    private float intensity = 5f;
    private Color color;


    public string matName = "emission";
    private Material[] mats;


    private void Start()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        mats = new Material[renderers.Length];

        for(int i = 0; i < renderers.Length; i++)
            foreach(Material m in renderers[i].materials)
                if(m.name.ToLower().Contains(matName.ToLower()))
                    mats[i] = m;

        //RandomizeColor();
    }

    private void Update()
    {
        hue += 0.1f * rainbowSpeed * Time.deltaTime;
        hue = Mathf.Repeat(hue, 1f);
        color = Color.HSVToRGB(hue, 1f, 1f);

        SetColor();
    }

    public RaycastHit Shoot(Gun g)
    {
        return new RaycastHit();
    }

    public BlasterRay Shoot()
    {
        BlasterRay ray = null;

        //if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            anim.Play("Shoot", 0, 0f);

            if(rayPrefab)
            {
                GameObject g = Instantiate(rayPrefab, shootPoint.position, shootPoint.rotation, null);
                ray = g.GetComponent<BlasterRay>();
                ray.SetColor(color);
            }

            //RandomizeColor();
        }

        return ray;
    }

    private void RandomizeColor()
    {
        hue = Mathf.Repeat(Random.Range(0, 11) * 0.1f, 1f);
        color = Color.HSVToRGB(hue, 1f, 1f);

        SetColor();
    }

    private void SetColor()
    {
        foreach(Material m in mats)
        {
            m.SetColor("_Color", color);
            m.SetColor("_EmissionColor", color * intensity);
        }
    }

    public Gun.QueuedHit SendHit(Gun.HitInstigator instigator)
    {
        Gun.QueuedHit hit = new Gun.QueuedHit(instigator);

        Shoot().hit = hit;

        return hit;
    }
}
