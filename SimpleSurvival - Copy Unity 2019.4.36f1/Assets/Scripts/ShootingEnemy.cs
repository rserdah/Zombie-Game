using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingEnemy : MonoBehaviour
{
    public bool set;

    public Body body;
    public AudioSource audioSource;

    public Transform player;
    public Vector3 m_playerOrigin;

    public Transform origin;
    public Transform rayVFX;

    public float damage;
    [Range(0f, 1f)]
    public float m_accuracy = 0.5f;
    public Vector2 accuracyRange; //Ranges will be based on the accuracy of the ShootingEnemy (i.e. more accurate -> quicker updating aim; less accurate -> slower updating aim)
    //public float m_shootDelay = 1f;
    public Vector2 shootDelayRange; //Ranges will be based on the accuracy of the ShootingEnemy (i.e. more accurate -> less shot delay for quicker shooting; less accurate -> slower shooting)

    public float lastTimeShot;

    public AudioClip shot;

    private float accuracy
    {
        get
        {
            return Random.Range(0, 0) + m_accuracy; //temp.; Finish implementing
        }
    }

    //private float shootDelay
    //{
    //    get
    //    {
    //        return Random.Range() + m_shootDelay;
    //        //Maybe change way of doing ShootingEnemy aiming because in this way, they are only aiming at player for one frame at a time, meaning there is no difference in how often they shoot; make it so they lerp to the player's position at a certain
    //        //speed and have a chance at over shooting (as in they can go past the player while aiming based on a random chance)
    //    }
    //}

    private Vector3 directionToPlayer
    {
        get
        {
            return playerOrigin - origin.position;
        }
    }

    private Vector3 playerOrigin
    {
        get
        {
            return player.position + m_playerOrigin;
        }
    }

    public Ray perfectRay; //Is the Ray that is perfectly aimed at Player;
    public Ray shootRay; //Is the current Ray that will be used for shooting

    public bool seesPlayer;
    public bool lerpToPlayer;


    private void Start()
    {
        StartCoroutine(Aim());
    }

    /*private void Update()
    {


        if(set)
        {
            Shoot();


            set = false;
        }

        perfectRay = new Ray(origin.position, directionToPlayer.normalized);

        Ray toPlayerRay = new Ray(origin.position, directionToPlayer.normalized);
        RaycastHit toPlayerHit;

        if(Physics.Raycast(toPlayerRay, out toPlayerHit, 500f, PlayerInput.LayerMasks.everything, QueryTriggerInteraction.Ignore))
        {
            if(toPlayerHit.transform.Equals(player))
            {
                //if(!seesPlayer) //If first frame seeing player
                //{
                //    lerpToPlayer = true;
                //}

                Debug.LogError("ShootingEnemy sees player");

                if(Time.time - lastTimeShot >= m_shootDelay)
                {
                    Shoot();
                    lastTimeShot = Time.time;
                }


                seesPlayer = true;
            }
            else
            {
                Debug.LogError("ShootingEnemy doesn't see player, sees " + toPlayerHit.transform.name);
                lastTimeShot = Time.time;


                seesPlayer = false;
            }
        }
        else
        {
            Debug.LogError("ShootingEnemy doesn't see anything");
            lastTimeShot = Time.time;


            seesPlayer = false;
        }
    }*/

    public IEnumerator Aim()
    {
        while(true)
        {
            yield return new WaitForSeconds(Mathf.Abs(1 - (m_accuracy + 0.025f)));


            if(player)
                transform.LookAt(player.position + m_playerOrigin);
        }
    }

    public IEnumerator KeepShooting()
    {
        while(true)
        {
            //yield return new WaitForSeconds(m_shootDelay);
            yield return new WaitForSeconds(1); //temp.; remove when necessary

            Shoot();
        }
    }

    private void Shoot()
    {
        Ray ray = new Ray(origin.position, transform.forward);
        RaycastHit hit;

        audioSource.PlayOneShot(shot);

        if(Physics.Raycast(ray, out hit, 500f, PlayerInput.LayerMasks.everything, QueryTriggerInteraction.Ignore))
        {
            Debug.LogError("Hit " + hit.transform.name);

            BodyPart hitBodyPart = hit.transform.GetComponent<BodyPart>();

            if(hitBodyPart && hitBodyPart.body)
            {
                hitBodyPart.TakeDamage(null, damage);
                //audioSource.PlayOneShot(hitmarkerSound);
            }
            else if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Explosive"))
            {
                hit.transform.GetComponent<Explosive>().set = true; //temp.; way of setting off explosives from other classes
                Debug.LogError("Hit " + hit.transform.name);
            }
            else if(hit.transform.GetComponent<CharacterController>())
            {
                PlayerInput playerInput = hit.transform.GetComponent<PlayerInput>();
                if(playerInput)
                {
                    playerInput.TakeDamage(body);
                    playerInput.audioSource.PlayOneShot(playerInput.gettHitSound);
                }
            }

            //Transform hitEffect = Instantiate(bulletHit).transform;

            //hitEffect.position = hit.point;
            //hitEffect.forward = hit.normal;

            //cube.transform.forward = hit.point - gun.shootOrigin;
            //cube.transform.position = (hit.point + gun.shootOrigin) / 2f;
            //cube.transform.localScale = new Vector3(0.025f, 0.025f, (hit.point - gun.shootOrigin).magnitude);
            //cube.GetComponent<Renderer>().material.color = Color.green;

            //sphere.transform.position = hit.point;
            //sphere.transform.localScale = new Vector3(0.075f, 0.075f, 0.075f);
            //sphere.GetComponent<MeshRenderer>().material = null;

            //Debug.LogError("Hit " + hit.collider.name);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerOrigin, 0.5f);
        //Gizmos.DrawLine(origin.position, transform.position + directionToPlayer.normalized * 500);

        Gizmos.DrawLine(origin.position, transform.position + perfectRay.direction.normalized * 500);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin.position, transform.position + transform.forward * 500);
    }
}
