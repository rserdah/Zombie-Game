using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    public Rigidbody rb;
    public Collider col;
    public AudioSource audioSource;

    public GameObject explosionEffect;

    public bool set;
    public float radius = 10;
    public float damage = 20f;
    public float force = 1000f;
    public float addForceDelay = 0.05f;
    public float fuseTime = 4.5f;
    public float volume = 1f;
    public Vector2 minMaxPitch = Vector2.one;
    public AnimationCurve damageFalloff;
    public Collider[] hitCols;
    public Collider[] hitPlayers;
    public Body[] hitBodies;
    public Rigidbody[] hitRigidBodies;
    public bool dontDestroyOnExplode;

    private bool exploded;


    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Explosive");


        try
        {
            explosionEffect.GetComponent<AudioSource>().volume = volume;
        }
        catch(System.Exception) { }
    }

    private void Update()
    {
        if(set)
        {
            Explode();


            set = false;
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if(!exploded)
    //    {
    //        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
    //            Explode();
    //    }
    //}

    public void SetFuse()
    {
        StartCoroutine(WaitForFuse());
    }

    public IEnumerator WaitForFuse()
    {
        yield return new WaitForSeconds/*Realtime*/(fuseTime);

        Explode();
    }

    private void Explode()
    {
        if(explosionEffect)
        {
            if(audioSource)
                audioSource.pitch = Random.Range(minMaxPitch.x, minMaxPitch.y);

            explosionEffect.SetActive(true);
            explosionEffect.transform.parent = null;
        }

        hitCols = Physics.OverlapSphere(transform.position, radius, PlayerInput.LayerMasks.bodyPart, QueryTriggerInteraction.Collide);

        hitPlayers = Physics.OverlapSphere(transform.position, radius, PlayerInput.LayerMasks.player);

        if(hitCols.Length > 0)
        {
            hitBodies = new Body[hitCols.Length];

            for(int i = 0; i < hitCols.Length; i++)
            {
                BodyPart b = hitCols[i].GetComponent<BodyPart>();

                if(b && b.body)
                {
                    b.TakeDamage(null, damage);

                    //if(b.rb)
                    //    b.rb.AddExplosionForce(force, transform.position, radius);

                    hitBodies[i] = b.body;
                }
            }

            //Body[] newHitBodies = RemoveDoubles(hitBodies);
            //hitBodies = newHitBodies;

            //StartCoroutine(WaitThenAddForce());
        }

        if(hitPlayers.Length > 0)
        {
            PlayerInput player = hitPlayers[0].GetComponent<PlayerInput>();
            player.TakeDamage(this);
        }

        foreach(Collider c in Physics.OverlapSphere(transform.position, radius))
        {
            if(!c.gameObject.Equals(gameObject) && c.gameObject.layer != LayerMask.NameToLayer("Explosive"))
                try { c.GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, radius); } catch(System.Exception) { }
            else if(!c.gameObject.Equals(gameObject))
            {
                Explosive hitExplosive = c.GetComponent<Explosive>();
                if(hitExplosive)
                {
                    exploded = true;
                    if(!hitExplosive.exploded)
                    {
                        Debug.LogError("Ignited " + hitExplosive.name);
                        hitExplosive.Explode();
                    }
                }
            }
        }

        exploded = true;

        if(!dontDestroyOnExplode)
            Destroy(gameObject);
    }

    public void SetRandomPitch()
    {
        if(audioSource)
        {
            audioSource.pitch = Random.Range(minMaxPitch.x, minMaxPitch.y);
        }
    }

    //private IEnumerator WaitThenAddForce()
    //{
    //    yield return new WaitForSeconds(addForceDelay);

    //    foreach(Body b in hitBodies)
    //    {

    //    }
    //}

    private T[] RemoveDoubles<T>(T[] types)
    {
        return types.Distinct().ToArray();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
