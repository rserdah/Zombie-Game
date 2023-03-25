using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : Weapon
{
    [HideInInspector]
    public Rigidbody rb;

    [Tooltip("How much the taken damage force will be multiplied by to get overall damage to imitate a more sensitive body part (e.g. a head would have more sensitivity than a leg.)")]
    public float sensitivity = 5f;

    public HumanBodyBones bodyPart = HumanBodyBones.LastBone;


    public BodyPart()
    {
        isInteractable = false;
    }

    public override void Awake()
    {
        base.Awake();


        gameObject.layer = LayerMask.NameToLayer("BodyPart");
        //if(components.colliders.Count > 0) components.colliders[0].isTrigger = true; //Setting the BodyPart to tigger, not needed here though
    } //Awake()

    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.LogError("OnTriggerEnter from BodyPart " + name, gameObject);

    //    //Check who is attacking first !!!
    //    //Check who is attacking first !!!
    //    //Check who is attacking first !!!
    //    //Check who is attacking first !!!
    //    //Check who is attacking first !!!
    //    //Check who is attacking first !!!
    //    //Check who is attacking first !!!
    //    //Check who is attacking first !!!
    //    //Check who is attacking first !!!

    //    //if(body && body.entity && body.entity is Player)
    //    //    body.Hit(other.transform, this, true, ((Player)body.entity).camera.transform.forward);
    //    //else
    //    //    body.Hit(other.transform, this);
    //}

    //private void OnCollisionEnter(Collision col) //Not going to be called if BodyParts are triggers (so going to have to put this stuff in OnTriggerEnter() and take collision/fall damage, go ragdoll if it was a heavy object that hit the Ragdoll, etc.)
    //{
    //    Debug.LogError("OnCollisionEnter from BodyPart " + name, gameObject);

    //    Body.DebugCollision(gameObject, col, this);

    //    //if(col.gameObject.GetComponent<Weapon>())
    //    //{
    //    //    if(col.gameObject.GetComponent<Weapon>().body.isAttacking)
    //    //    {
    //    //        //ragdoll.ragdollHelper.ragdolled = true;
    //    //        ((Ragdoll)body).GoRagdoll();
    //    //        components.rb.AddForce(col.transform.forward * ((Ragdoll)body).ragdollPartHitForce);
    //    //        Debug.LogWarning(string.Format("{0} was hit by {1}", name, col.gameObject.name));
    //    //    }
    //    //}
    //    //else if(body.TakeDamage(col.gameObject.GetComponent<Entity>(), Body.GetCollisionUnits(col), Body.DamageType.COLLISION))
    //    //    if(body is Ragdoll)
    //    //        ((Ragdoll)body).GoRagdoll();
    //}

    /// <summary>
    /// Calls BodyPart.body.TakeDamage() with BodyPart.sensitivity * the given damage amount.
    /// </summary>
    /// <param name="amount"></param>
    public void TakeDamage(Entity fromEntity, float amount, Body.DamageType damageType = Body.DamageType.DEFAULT, Body.IDamageType iDamageType = null)
    {
        body.TakeDamage(fromEntity, sensitivity * amount, damageType, iDamageType);
    } //TakeDamage(float amount)

    /*public void TakeDamage(Entity fromEntity, float amount, Body.DamageType damageType)
    {
        body.TakeDamage(fromEntity, sensitivity * amount, damageType);
    }*/

    //public AttackReaction.SurfaceType GetSurfaceType()
    //{
    //    return body.surfaceType;
    //}

    //public void SetSurfaceType(AttackReaction.SurfaceType surfaceType)
    //{
    //    body.surfaceType = surfaceType;
    //}
} //EnemyBodyPart
