using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyCol : Weapon
{
    public float sensitivity = 1f;


    //OnCollisionEnter() is called in BodyCol and NOT in Weapon because BodyCols are the actual Colliders (like real body parts) that register damage
    private void OnTriggerEnter(Collider col)
    {
        Weapon hitWeapon = col.gameObject.GetComponent<Weapon>();
        Body hitBody = null;

        if(hitWeapon)
        {
            Debug.LogError(name + " and " + hitWeapon.gameObject.name + " hit.");
            hitBody = hitWeapon.body;
        }

        if(hitWeapon && hitBody)
        {
            if(hitBody.isAttacking && !body.isAttacking) //If they are attacking and we are not attacking
            {
                body.TakeDamage(hitBody.entity, hitBody.fightingSkills.strength * hitBody.fightingSkills.strengthMultiplier, Body.DamageType.DEFAULT);
                //body.TakeDamage(col.relativeVelocity.x + col.relativeVelocity.y + col.relativeVelocity.z); //This can be used for getting hit by falling of moving objects like cars or falling boulders etc.
            }
        }
    }
} //BodyCol
