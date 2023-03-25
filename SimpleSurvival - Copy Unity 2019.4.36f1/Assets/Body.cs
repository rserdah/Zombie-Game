using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body : MonoBehaviour
{
    [Header("References")]
    public Entity entity;
    public List<BodyPart> bodyParts = new List<BodyPart>();
    public BodyPart[] organizedBodyParts = new BodyPart[11];
    public Agility agility = new Agility(); //Probably rename b/c agility refers to speed of movement
    public FightingSkills fightingSkills = new FightingSkills();
    public BodyPartStats bodyPartStats = new BodyPartStats();
    public bool setBodyPartStats = true;
    public bool setBodyPartsToTriggers;
    public bool debugNames;


    /*[Header("Stats")]
    [Header("Combat")]
    public float maxHealth = 100f;
    public float health = 100f;
    [Tooltip("How much damage this Body can do.")]
    public float strength = 2f;
    [Tooltip("Resistance to damage.")]
    public float toughness = 2f;
    [Tooltip("How much to multiply Body.toughness by when blocking.")]
    public float blockingMultiplier = 2f;*/

    
    /*[Header("Agility")]
    public float maxStamina = 100f;
    public float stamina = 100f;*/

    [Serializable]
    public class Agility //Probably rename b/c agility refers to speed of movement
    {
        //Stats
        public float groundSpeed;
        public float climbSpeed;
        public float swimSpeed;
        public float parkourSideMoveSpeed = 2.5f;
        public float jumpHeight;
        public int jumpsLeft = 2;
        public int maxJumps = 2;

        public float maxStamina = 100f;
        public float stamina = 100f;
        public float staminaRegainDelay = 2f;
        public float staminaRegainRate = 0.3f;

        [Tooltip("This is subtracted from stamina EACH FRAME so make this number relatively low.")]
        public float runningStaminaCost = 0.3f;
        public float heavyAttackStaminaCost = 10f;

        public float throwForce = 2500f;


        //Instances
        public Walk walk = new Walk();
        public Crouch crouch = new Crouch();
        public Prone prone = new Prone();
        public Run run = new Run();
        public Jump jump = new Jump();
        public Climb climb = new Climb();
        public Swim swim = new Swim();

        [Serializable]
        public class Walk
        {
            public float normal = 1f;
            public float fast = 1.75f;
        }

        [Serializable]
        public class Run
        {
            public float normal = 3f;
            public float fast = 7f;
        }

        [Serializable]
        public class Crouch
        {
            public float normal = 0.5f;
            public float fast = 0.75f;
        }

        [Serializable]
        public class Prone
        {
            public float normal = 0.25f;
            public float fast = 0.35f;
        }

        [Serializable]
        public class Jump
        {
            public float normal = 25f;
            public float high = 50f;
        }

        [Serializable]
        public class Climb
        {
            public float normal = 5f;
            public float fast = 8f;
        }

        [Serializable]
        public class Swim
        {
            public float normal = 5f;
            public float fast = 7f;
        }
    } //Agility

    public enum StaminaUses
    {
        RUNNING, HEAVYATTACK
    }

    public enum DamageType
    {
        DEFAULT, COLLISION, EXPLOSION, GUNSHOT, FIRE //, KNIFE, BULLET, POISON, etc.
    }

    public interface IDamageType
    {
        DamageType GetDamageType();

        float GetDamage(Entity e);
    }

    [Serializable]
    public class FightingSkills
    {
        public float maxHealth = 100f;
        public float health = 100f;
        public int lives = 1;
        public bool regenerateHealth;
        public float healthRegenerationDelay = 4f;
        public float healthRegenerationSpeed = 0f;
        [HideInInspector]
        public float lastTimeHurt = 0f;
        [Tooltip("How much damage this Body can do.")]
        public float strength = 2f;
        public float normalStrengthMultiplier = 1f;
        public float heavyStrengthMultiplier = 2f;
        [HideInInspector]
        public float strengthMultiplier = 1f;
        [Tooltip("Resistance to damage.")]
        [Range(0.01f, Mathf.Infinity)]
        public float toughness = 2f;
        [Tooltip("How much to multiply Body.toughness by when blocking.")]
        public float blockingMultiplier = 2f;
    } //FightingSkills

    [Serializable]
    public class BodyPartStats
    {
        public Sensitivity sensitivity = new Sensitivity();

        [Serializable]
        public class Sensitivity
        {
            public float head = 10f;
            public float neck = 4.5f;
            public float upperChest = 4f;
            public float leftUpperArm = 1.5f;
            public float rightUpperArm = 1.5f;
            public float chest = 4f;
            public float spine = 4f;
            public float leftUpperLeg = 2f;
            public float leftLowerLeg = 1.5f;
            public float rightUpperLeg = 2f;
            public float rightLowerLeg = 1.5f;
            public float other = 1f;
        }
    }

    //public AttackReaction.SurfaceType surfaceType = AttackReaction.SurfaceType.HUMANBODY;

    public bool isAttacking;
    public bool isPlayingAttack;
    public bool isBlocking;
    [Tooltip("A temporary dead (player has lost all health)")]
    public bool isDead;
    //[Tooltip("A permanent dead (player ran out of lives/revives/etc.)")]
    //public bool isDeadDead;

    //=== Temp./Holder Variables ===
    public bool isGrounded;
    float lastTimeUsedStamina;
    public bool ragdollOnDie;


    public virtual void Awake()
    {
        foreach(Weapon w in GetComponentsInChildren<Weapon>()) //BodyCol derives from Weapon b/c you can use fists as a weapon etc. Need to set the reference when a new Weapon is picked up though.
        {
            w.body = this;
            if(!w.entity && entity) w.entity = entity;
        }
    }

    public virtual void Setup()
    {
        InitializeBodyParts();

        if(debugNames)
        {
            Debug.LogWarning(entity.components.anim.GetBoneTransform(HumanBodyBones.Head).name);

            Debug.LogWarning(entity.components.anim.GetBoneTransform(HumanBodyBones.Neck).name);

            Debug.LogWarning(entity.components.anim.GetBoneTransform(HumanBodyBones.UpperChest).name);

            Debug.LogWarning(entity.components.anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).name);

            Debug.LogWarning(entity.components.anim.GetBoneTransform(HumanBodyBones.RightUpperArm).name);

            Debug.LogWarning(entity.components.anim.GetBoneTransform(HumanBodyBones.Chest).name);

            Debug.LogWarning(entity.components.anim.GetBoneTransform(HumanBodyBones.Spine).name);

            Debug.LogWarning(entity.components.anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).name);

            Debug.LogWarning(entity.components.anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).name);

            Debug.LogWarning(entity.components.anim.GetBoneTransform(HumanBodyBones.RightUpperLeg).name);

            Debug.LogWarning(entity.components.anim.GetBoneTransform(HumanBodyBones.RightLowerLeg).name);
        }
    }

    private void Update()
    {
        //if(TimeHelper.EnoughTimeSince(lastTimeUsedStamina, agility.staminaRegainDelay))
        //{
        //    agility.stamina = Mathf.Clamp(agility.stamina + agility.staminaRegainRate, 0, agility.maxStamina);
        //}
        //else
        //{
        //    //Debug.Log(string.Format("{0}'s stamina regain: {1} / {2}", name, Time.time - lastTimeUsedStamina, agility.staminaRegainDelay));
        //}

        //if(fightingSkills.regenerateHealth && fightingSkills.healthRegenerationSpeed > 0)
        //{
        //    if(TimeHelper.EnoughTimeSince(fightingSkills.lastTimeHurt, fightingSkills.healthRegenerationDelay))
        //        fightingSkills.health = Mathf.Lerp(fightingSkills.health, fightingSkills.maxHealth, Time.deltaTime * fightingSkills.healthRegenerationSpeed);
        //}

        //For debugging/testing purposes; remove when necessary
        if(fightingSkills.health <= 0)
            Die();

        if(Perk.nukeActive && entity is Enemy && !isDead)
        {
            fightingSkills.health = 0;
            Die();
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        //Debug.LogError(name + " was hit by " + col.name);

        /*Weapon hitWeapon = null;
        Body hitBody = null;

        try
        {
            hitWeapon = col.GetComponent<Weapon>();
            hitBody = hitWeapon.body;

            if(hitWeapon && hitWeapon.body != this)
            {
                if(hitBody.isAttacking && !isAttacking) //If they attacked and we're not attacking
                {
                    Debug.LogWarning(name + " was hit by an enemy.");
                    if(entity)
                    {
                        AnimatorHelper.PlayIfNotPlayingAlready(entity.components.anim, "GettingHit");
                        //ResetBoolOnStateFinish(entity.anim, "isGettingHit"); ///////////////////////////////////////////FIX !!!!!!!!!!!!!!!!!!!!!!!!!!
                    }
                }
                //Put in stuff for blocking here
            }
        }
        catch(Exception) { }*/

        //Debug.LogError("OnTriggerEnter from Body " + name, gameObject);

        GetHit(col.transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.LogError("OnCollisionEnter from " + name + " (Body); collided with " + collision.gameObject.name);

        //DebugCollision(gameObject, collision);

        if(collision.gameObject.layer == LayerMask.NameToLayer("Obstacle") || collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            //!!! Check orientation/the way the Body hit the Obstacle (b/c currently they can hit the ceiling in order to refill jumps) !!!
            //!!! Check orientation/the way the Body hit the Obstacle (b/c currently they can hit the ceiling in order to refill jumps) !!!
            //!!! Check orientation/the way the Body hit the Obstacle (b/c currently they can hit the ceiling in order to refill jumps) !!!
            //!!! Check orientation/the way the Body hit the Obstacle (b/c currently they can hit the ceiling in order to refill jumps) !!!

            isGrounded = true;
            agility.jumpsLeft = agility.maxJumps;

            TakeDamage(null, GetCollisionUnits(collision), DamageType.COLLISION);
        }

        //Old
        //TakeDamage(collision.gameObject.GetComponent<Entity>(), GetCollisionUnits(collision), DamageType.COLLISION);


        /****/
        //New
            //Go to part above where it checks the layer of the collided with object

        /****/

        GetHit(collision.transform);

        /*Weapon hitWeapon = null;
        Body hitBody = null;

        try
        {
            hitWeapon = collision.gameObject.GetComponent<Weapon>();
            hitBody = hitWeapon.body;
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }

        if(hitWeapon && !hitWeapon.body.Equals(this))
        {
            Debug.LogError("Is a Weapon");

            if(hitBody.isAttacking && !isAttacking) //If they attacked and we're not attacking
            {
                Debug.LogError(name + " was hit by an enemy.");

                hitBody.isAttacking = false; //So multiple hits are not made

                TakeDamage(hitBody.entity, hitBody.fightingSkills.strength, DamageType.DEFAULT); //Maybe move this to BodyPart so that it does damage whenever it hits something, MAYBE

                //if(entity)
                //{
                //    AnimatorHelper.PlayIfNotPlayingAlready(entity.components.anim, "GettingHit");
                //    //ResetBoolOnStateFinish(entity.anim, "isGettingHit"); ///////////////////////////////////////////FIX !!!!!!!!!!!!!!!!!!!!!!!!!!
                //}
            }
            //Put in stuff for blocking here
        }
        else
        {
            Debug.LogError("Not a Weapon");
        }*/
    }

    public static void DebugCollision(GameObject thisGameObject, Collision collision, BodyPart fromBodyPart = null)
    {
        Debug.Log((fromBodyPart ? fromBodyPart.name + " (" + fromBodyPart.GetType() + ") of " : "") + thisGameObject.name + " collided with " + collision.gameObject.name + " with " + GetCollisionUnits(collision) + " collision units", thisGameObject);
    }

    private void InitializeBodyParts()
    {
        AddBodyParts();

        if(setBodyPartStats)
            SetBodyParts();

        foreach(BodyPart b in GetComponentsInChildren<BodyPart>())
        {
            //b.SetSurfaceType(surfaceType);

            if(setBodyPartsToTriggers)
                try { b.GetComponent<Collider>().isTrigger = true; } catch(Exception) { }
        }
    }

    private void AddBodyParts()
    {
        Rigidbody[] rigidBodies = GetComponentsInChildren<Rigidbody>();

        foreach(Rigidbody rb in rigidBodies)
        {
            if(rb.gameObject.layer != LayerMask.NameToLayer("Explosive")) //If the Rigidbody is not an Explosive (b/c don't want an explosive Enemy to set its Explosives as its BodyParts)
            {
                rb.useGravity = false;

                BodyPart part = rb.gameObject.GetComponent<BodyPart>();
                if(!part)
                    part = rb.gameObject.AddComponent<BodyPart>();

                part.rb = rb;

                if(!part.entity && entity)
                    part.entity = entity;
            }
        }
    }

    private void SetBodyParts()
    {
        foreach(Weapon w in GetComponentsInChildren<Weapon>()) //BodyCol derives from Weapon b/c you can use fists as a weapon etc. Need to set the reference when a new Weapon is picked up though.
        {
            if(!w.body) w.body = this;
        }

        Animator a = null;

        if(entity.components.anim)
            a = entity.components.anim;

        foreach(BodyPart b in GetComponentsInChildren<BodyPart>())
        {
            bodyParts.Add(b);

            if(a) //If the temp. Animator is not null
            {
                if(b.transform.Equals(a.GetBoneTransform(HumanBodyBones.Head)))
                {
                    b.sensitivity = bodyPartStats.sensitivity.head;
                    b.bodyPart = HumanBodyBones.Head;
                    organizedBodyParts[0] = b;
                }

                else if(b.transform.Equals(a.GetBoneTransform(HumanBodyBones.Neck)))
                {
                    b.sensitivity = bodyPartStats.sensitivity.neck;
                    b.bodyPart = HumanBodyBones.Neck;
                    organizedBodyParts[1] = b;
                }

                else if(b.transform.Equals(a.GetBoneTransform(HumanBodyBones.UpperChest)))
                {
                    b.sensitivity = bodyPartStats.sensitivity.upperChest;
                    b.bodyPart = HumanBodyBones.UpperChest;
                    organizedBodyParts[2] = b;
                }

                else if(b.transform.Equals(a.GetBoneTransform(HumanBodyBones.LeftUpperArm)))
                {
                    b.sensitivity = bodyPartStats.sensitivity.leftUpperArm;
                    b.bodyPart = HumanBodyBones.LeftUpperArm;
                    organizedBodyParts[3] = b;
                }

                else if(b.transform.Equals(a.GetBoneTransform(HumanBodyBones.RightUpperArm)))
                {
                    b.sensitivity = bodyPartStats.sensitivity.rightUpperArm;
                    b.bodyPart = HumanBodyBones.RightUpperArm;
                    organizedBodyParts[4] = b;
                }

                else if(b.transform.Equals(a.GetBoneTransform(HumanBodyBones.Chest)))
                {
                    b.sensitivity = bodyPartStats.sensitivity.chest;
                    b.bodyPart = HumanBodyBones.Chest;
                    organizedBodyParts[5] = b;
                }

                else if(b.transform.Equals(a.GetBoneTransform(HumanBodyBones.Spine)))
                {
                    b.sensitivity = bodyPartStats.sensitivity.spine;
                    b.bodyPart = HumanBodyBones.Spine;
                    organizedBodyParts[6] = b;
                }

                else if(b.transform.Equals(a.GetBoneTransform(HumanBodyBones.LeftUpperLeg)))
                {
                    b.sensitivity = bodyPartStats.sensitivity.leftUpperLeg;
                    b.bodyPart = HumanBodyBones.LeftUpperLeg;
                    organizedBodyParts[7] = b;
                }

                else if(b.transform.Equals(a.GetBoneTransform(HumanBodyBones.LeftLowerLeg)))
                {
                    b.sensitivity = bodyPartStats.sensitivity.leftLowerLeg;
                    b.bodyPart = HumanBodyBones.LeftLowerLeg;
                    organizedBodyParts[8] = b;
                }

                else if(b.transform.Equals(a.GetBoneTransform(HumanBodyBones.RightUpperLeg)))
                {
                    b.sensitivity = bodyPartStats.sensitivity.rightUpperLeg;
                    b.bodyPart = HumanBodyBones.RightUpperLeg;
                    organizedBodyParts[9] = b;
                }

                else if(b.transform.Equals(a.GetBoneTransform(HumanBodyBones.RightLowerLeg)))
                {
                    b.sensitivity = bodyPartStats.sensitivity.rightLowerLeg;
                    b.bodyPart = HumanBodyBones.RightLowerLeg;
                    organizedBodyParts[10] = b;
                }

                else
                    b.sensitivity = bodyPartStats.sensitivity.other;
            }
            else
            {
                Debug.LogError(name + "'s temp. Animator is null.");
            }
        }
    }

    public BodyPart GetBodyPart(HumanBodyBones bone)
    {
        switch(bone)
        {
            case HumanBodyBones.Head:
                return organizedBodyParts[0];

            case HumanBodyBones.Neck:
                return organizedBodyParts[1];

            case HumanBodyBones.UpperChest:
                return organizedBodyParts[2];

            case HumanBodyBones.LeftUpperArm:
                return organizedBodyParts[3];

            case HumanBodyBones.RightUpperArm:
                return organizedBodyParts[4];

            case HumanBodyBones.Chest:
                return organizedBodyParts[5];

            case HumanBodyBones.Spine:
                return organizedBodyParts[6];

            case HumanBodyBones.LeftUpperLeg:
                return organizedBodyParts[7];

            case HumanBodyBones.LeftLowerLeg:
                return organizedBodyParts[8];

            case HumanBodyBones.RightUpperLeg:
                return organizedBodyParts[9];

            case HumanBodyBones.RightLowerLeg:
                return organizedBodyParts[10];

            default:
                return null;

        }
    }

    public HumanBodyBones GetHumanBodyBone(BodyPart bodyPart)
    {
        //CHECK METHOD
        //CHECK METHOD
        //CHECK METHOD
        //CHECK METHOD
        //CHECK METHOD
        //CHECK METHOD

        bool contains = false; //Instead of using List.Contains() to avoid having to go through the list twice (once for Contains() and once to get index of bodyPart)
        int i = 0;

        foreach(BodyPart b in bodyParts)
        {
            if(b.Equals(bodyPart))
            {
                contains = true;
                break;
            }

            i++;
        }

        if(contains)
        {
            if(i == 0)
                return HumanBodyBones.Head;
            else if(i == 1)
                return HumanBodyBones.Neck;
            else if(i == 2)
                return HumanBodyBones.UpperChest;
            else if(i == 3)
                return HumanBodyBones.LeftUpperArm;
            else if(i == 4)
                return HumanBodyBones.RightUpperArm;
            else if(i == 5)
                return HumanBodyBones.Chest;
            else if(i == 6)
                return HumanBodyBones.Spine;
            else if(i == 7)
                return HumanBodyBones.LeftUpperLeg;
            else if(i == 8)
                return HumanBodyBones.LeftLowerLeg;
            else if(i == 9)
                return HumanBodyBones.RightUpperLeg;
            else if(i == 10)
                return HumanBodyBones.RightLowerLeg;
        }

        return HumanBodyBones.LastBone;
    }

    public void ResetBoolOnStateFinish(Animator a, string boolName)
    {
        StartCoroutine(ResetBoolAfterDelay(a, boolName, a.GetCurrentAnimatorStateInfo(0).length));
    }

    private IEnumerator ResetBoolAfterDelay(Animator a, string boolName, float delay)
    {
        yield return new WaitForSeconds(delay);
        a.SetBool(boolName, false);
        Debug.LogWarning("State finished.");
    }

    public void OnAttackExit()
    {
        isAttacking = false;
        isPlayingAttack = false;
    }

    /*/// <summary>
    /// Returns the Body that is doing the attacking.
    /// </summary>
    /// <param name="body1"></param>
    /// <param name="body2"></param>
    /// <returns></returns>
    public static Body GetAttackingBody(Body body1, Body body2)
    {
        if(body1 && body2)
        {
            if(body1.isAttacking && !body2.isAttacking)
                return body1;
            else if(body2.isAttacking && !body1.isAttacking)
                return body2;
        }


        return null;
    }*/

    /*/// <summary>
    /// Like the opposite of Body.GetAttackingBody() in that it returns the Body that is being attacked.
    /// </summary>
    /// <param name="body1"></param>
    /// <param name="body2"></param>
    /// <returns></returns>
    public static Body GetAttackedBody(Body body1, Body body2)
    {
        if(body1 && body2)
        {
            if(body1.isAttacking && !body2.isAttacking)
                return body2;
            else if(body2.isAttacking && !body1.isAttacking)
                return body1;
        }


        return null;
    }*/

    /// <summary>
    /// Returns true if this Body is attacking Body other and false otherwise.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsAttacking(Body other)
    {
        return isAttacking && !other.isAttacking;
    }

    /// <summary>
    /// Makes this Body take damage if conditions are correct (if a Weapon is passed in, that Weapon's Body isAttacking, and this Body is not attacking).
    /// </summary>
    /// <param name="possibleWeapon"></param>
    private void GetHit(Transform possibleWeapon)
    {
        Weapon hitWeapon = null;
        Body hitBody = null;

        try
        {
            hitWeapon = possibleWeapon.gameObject.GetComponent<Weapon>();
            hitBody = hitWeapon.body;
        }
        catch(Exception e)
        {
            //Debug.LogError("caught: " + e.Message, gameObject);
        }

        if(hitWeapon && !hitWeapon.body.Equals(this))
        {
            //Debug.LogError("Is a Weapon");

            if(hitBody.isAttacking && !isAttacking) //If they attacked and we're not attacking
            {
                //Debug.LogError(name + " was hit by an enemy.", gameObject);

                hitBody.isAttacking = false; //So multiple hits are not made

                entity.AddForce(hitWeapon.forward * hitWeapon.stats.impactForce); //Maybe find better way to add force (e.g. if not realistic if hitting a Ragdoll) and may need to change the direction b/c if the BodyPart forward isn't right

                TakeDamage(hitBody.entity, hitBody.fightingSkills.strength, DamageType.DEFAULT); //Maybe move this to BodyPart so that it does damage whenever it hits something, MAYBE

                //if(entity)
                //{
                //    AnimatorHelper.PlayIfNotPlayingAlready(entity.components.anim, "GettingHit");
                //    //ResetBoolOnStateFinish(entity.anim, "isGettingHit"); ///////////////////////////////////////////FIX !!!!!!!!!!!!!!!!!!!!!!!!!!
                //}
            }
            //Put in stuff for blocking here
        }
        else
        {
            //Debug.LogError("Weapon did not hit Body " + name, gameObject);
        }
    }

    /// <summary>
    /// !!! Maybe combine this with GetHit() b/c they do similar things !!!
    /// </summary>
    /// <param name="other"></param>
    /// <param name="fromBodyPart"></param>
    /// <param name="useKnockBackDirection"></param>
    /// <param name="knockBackDirection"></param>
    //public void Hit(Transform other, BodyPart fromBodyPart = null, bool useKnockBackDirection = false, Vector3 knockBackDirection = new Vector3())
    //{
    //    //TODO: Maybe combine this with GetHit() b/c they do similar things

    //    Body hitBody = other.GetComponent<Body>();
    //    BodyPart hitBodyPart = other.GetComponent<BodyPart>();
    //    IMeleeable iMeleeable = other.GetComponent<IMeleeable>();
    //    Rigidbody hitRB = null;

    //    if(!hitBody && !hitBodyPart && iMeleeable == null)
    //        hitRB = other.GetComponent<Rigidbody>(); //To avoid always getting the Rigidbody Component

    //    if(hitBody && !Equals(hitBody) && IsAttacking(hitBody)) //If we hit a Body AND we are not hitting ourself AND we are attacking while the other Body is not attacking
    //    {
    //        Debug.LogError(name + " hit Body " + hitBody.name, gameObject);
    //        hitBody.TakeDamage(entity, fromBodyPart ? fromBodyPart.stats.damage : 0f, DamageType.DEFAULT);

    //        isAttacking = false; //To avoid multiple hits (!!! CHECK !!!)
    //    }
    //    else if(hitBodyPart && hitBodyPart.body && !Equals(hitBodyPart.body) && IsAttacking(hitBodyPart.body))
    //    {
    //        hitBody = hitBodyPart.body;

    //        Debug.LogError(name + " hit BodyPart " + hitBodyPart.name + " of Body " + hitBodyPart.body.name, gameObject);
    //        hitBody.TakeDamage(entity, fromBodyPart ? fromBodyPart.stats.damage : 0f, DamageType.DEFAULT);

    //        isAttacking = false; //To avoid multiple hits (!!! CHECK !!!)
    //    }
    //    else if(iMeleeable != null)
    //    {
    //        iMeleeable.GetMeleed();
    //    }
    //    else if(hitRB && entity.components.rb && !entity.components.rb.Equals(hitRB))
    //    {
    //        Debug.LogError(name + " only hit Rigidbody " + hitRB.name, gameObject);

    //        if(useKnockBackDirection)
    //            entity.AddForceTo(hitRB, fromBodyPart ? knockBackDirection * fromBodyPart.stats.impactForce : knockBackDirection * 75f);
    //        else
    //            entity.AddForceTo(hitRB, fromBodyPart ? fromBodyPart.forward * fromBodyPart.stats.impactForce : transform.forward * 75f);
    //    }

    //    if(useKnockBackDirection && hitBody && hitBody is Ragdoll && hitBody.isDead)
    //    {
    //        ((Ragdoll)hitBody).AddForce(knockBackDirection * (fromBodyPart ? fromBodyPart.stats.impactForce : 50f), hitBodyPart ? hitBodyPart : null);
    //    }
    //}

    /// <summary>
    /// Input the inflictedDamage (often another Body's Body.strength) and TakeDamage() will do the math based on this Body's toughness and blockingMultiplier.
    /// Returns true if health was lost/damage was taken, false if not
    /// </summary>
    /// <param name="inflictedDamage"></param>
    /// <returns>Returns true if health was lost/damage was taken, false if not</returns>
    public virtual bool TakeDamageByValue(float inflictedDamage) //Used to be the 'base' method for taking damage (but now if you want to go back to that way, replace all instances of TakeDamage(float rawValue, DamageType damageType) in this class (MAYBE))
    {
        if(inflictedDamage <= 0)
            return false;

        fightingSkills.lastTimeHurt = Time.time;

        if(!isDead)
        {
            if(fightingSkills.health > 0)
            {
                fightingSkills.health -= isBlocking ? inflictedDamage / (fightingSkills.toughness * fightingSkills.blockingMultiplier) : inflictedDamage / fightingSkills.toughness;
                //fightingSkills.health = Mathf.Clamp(fightingSkills.health, 0f, fightingSkills.maxHealth);
            }
            else
                Die();

            entity.sounds.Hurt();

            //If health is <= 0 AFTER doing damage, call Body.Die()
            if(fightingSkills.health <= 0 || (entity is Enemy && Perk.instaKillActive))
                Die();
        }

        return true;
    }

    /// <summary>
    /// Optionally include (and can pass in neither) DamageType or IDamageType parameter; iDamageType.GetDamageType() WILL REPLACE damageType (AND iDamageType.GetDamage() WILL REPLACE inflictedDamage) if an IDamageType is passed in.
    /// </summary>
    /// <param name="fromEntity"></param>
    /// <param name="inflictedDamage"></param>
    /// <param name="damageType"></param>
    /// <param name="iDamageType"></param>
    /// <returns></returns>
    public virtual bool TakeDamage(Entity fromEntity, float inflictedDamage, DamageType damageType = DamageType.DEFAULT, IDamageType iDamageType = null) //Maybe for IDamageType make a method GetDamage() & for BodyPart implement so it returns strength
    {
        //!!! Check if not dead !!!

        if(!fromEntity)
            return TakeDamageByType(inflictedDamage, damageType);

        if(fromEntity && !entity.Equals(fromEntity)) //If Entity did not damage itself (and its not null)
        {
            //Debug.LogError(fromEntity.name + " is not the same as " + entity.name);

            //if(entity.team.IsEnemy(fromEntity))
            //{
            //    //GameManager.instance.AddPoints(fromEntity, entity); //!!! Maybe pass in damage type, iDamageType, or the bodypart that was hit so can give the correct score (e.g. the points for a head shot etc.) !!!

            //    //Debug.LogError(fromEntity.name + " is an enemy of " + entity.name);
            //}

            //if(entity is Enemy)
            //    ((Enemy)entity).DrawAttentionTo(fromEntity.transform.position); //Makes Enemy go to where the damage came from
        }

        if(iDamageType != null)
        {
            Debug.LogError(" doing " + iDamageType.GetDamage(entity) + "damage to " + entity.name);
            return TakeDamageByType(iDamageType.GetDamage(entity), iDamageType.GetDamageType()); //CHECK
        }
        else
            return TakeDamageByType(inflictedDamage, damageType);
    }

    /// <summary>
    /// Takes damage based on type (rawValue is an arbitrary/general value that could come from multiple sources (for example rawValue could be the impact force of a Collision and if DamageType.COLLISION is passed in, it will take damage accordingly))
    /// Returns true if health was lost/damage was taken, false if not
    /// 
    /// Probably better to make params like this: DamageType damageType, float [rawValue OR multiplier] = 1f since the whole point of taking damage by type is to have a consistent system of taking damage
    /// so it makes almost no sense to require a rawValue in the first place, instead you simply specify the type of damage and it damages based off of the values given to that type of damage
    /// for example, fire does (basically) the same amount of damage regardless (i.e. there is not really a way to make one fire damage more than others or something) but maybe you can have an optional
    /// multiplier if necessary
    /// </summary>
    /// <param name="rawValue"></param>
    /// <param name="damageType"></param>
    /// <returns>Returns true if health was lost/damage was taken, false if not</returns>
    public virtual bool TakeDamageByType(float rawValue, DamageType damageType)
    {
        //Do different things for different types of damage (like continuous damage if fire/poison etc.)
        switch(damageType)
        {
            case DamageType.DEFAULT:
                return TakeDamageByValue(rawValue);

            case DamageType.COLLISION:
                if(rawValue > 210) //Baseline for falling/collision damage
                {
                    return TakeDamageByValue(rawValue * 0.1f); //Assumption: rawValue is in collision units
                }
                return false;

            case DamageType.EXPLOSION:
                return TakeDamageByValue(rawValue); //Currently has no special value for this specific DamageType

            case DamageType.GUNSHOT:
                return TakeDamageByValue(rawValue); //Currently has no special value for this specific DamageType

            case DamageType.FIRE:
                //Fire damage overrides mostly overpowers rawValue, allowing only a slight influence of total damage
                return TakeDamageByValue(Mathf.Clamp(rawValue / 1000f, 0.001f, 3f) * 80f + 80f);
        }

        return false;
    }

    public void TakeDamage(Entity fromEntity, float inflictedDamage, float delay, DamageType damageType)
    {
        StartCoroutine(TakeDamageDelayed(fromEntity, inflictedDamage, delay, damageType));
    }

    public IEnumerator TakeDamageDelayed(Entity fromEntity, float damage, float delay, DamageType damageType)
    {
        yield return new WaitForSeconds(delay);
        TakeDamage(fromEntity, damage, damageType);
    }

    public virtual void Die()
    {
        if(!isDead)
        {
            if(ragdollOnDie)
                entity.components.anim.enabled = false;

            foreach(BodyPart b in bodyParts)
            {
                if(b.components.colliders.Count > 0) b.components.colliders[0].isTrigger = false;

                if(ragdollOnDie)
                {
                    b.rb.useGravity = true;
                    b.rb.isKinematic = false;
                }
            }

            //Multiple trys b/c if they are put in one try, if one statement fails, the rest fail
            //try { GetComponent<Destructable>().Destruct(); } catch(Exception) { }
            //try
            //{
            //    Entity e = GetComponent<Entity>();
            //    e.OnPreDie(); //CHECK!!!
            //    e.enabled = false;
            //} catch(Exception) { }
            //try { GetComponent<PlayerInput>().OnPreDie(); } catch(Exception) { }
            //Body is disabled at the end of Die();

            foreach(BodyPart b in bodyParts)
                Destroy(b);
            bodyParts.Clear();

            if(entity.networkModel) entity.networkModel.SetActive(true);
            try
            {
                //Ragdoll r = entity.networkModel.GetComponent<Ragdoll>();
                //r.GoRagdoll();
                //Destroy(r);
            }
            catch(Exception) { }
            if(entity.localModel) entity.localModel.SetActive(false);

            //SendMessage("DebugDie", SendMessageOptions.DontRequireReceiver);
            //Debug.Log(name + " died.");

            if(entity.notifyOnDie)
                entity.notifyOnDie.SendMessage("NotifyOnDie", SendMessageOptions.DontRequireReceiver);

            if(entity is Enemy)
            {
                //Only if the Enemy dies inside the PlayerWalkable Area will it have a chance to drop a Perk. Make sure to do this before calling Enemy.DisableOnDie() because the NavMeshAgent needs
                //to be enabled in order to call NavMeshAgent.Raycast()
                UnityEngine.AI.NavMeshHit hit;
                ((Enemy)entity).agent.Raycast(transform.position, out hit);

                if(hit.mask == 1 << UnityEngine.AI.NavMesh.GetAreaFromName("PlayerWalkable"))
                {
                    if(MathHelper.PercentChance(5f))
                        Perk.CreatePerk(null, transform.position);
                }

                Destroy(gameObject, 5f);
            }

            entity.DisableOnDie();

            isDead = true;


            enabled = false;
        }
    }

    public void UseStamina(StaminaUses useType)
    {
        switch(useType)
        {
            case StaminaUses.RUNNING:
                UseStamina(agility.runningStaminaCost);
                break;
            case StaminaUses.HEAVYATTACK:
                UseStamina(agility.heavyAttackStaminaCost);
                break;
        }
        lastTimeUsedStamina = Time.time;
    }

    public void UseStamina(float staminaCost)
    {
        agility.stamina = Mathf.Clamp(agility.stamina - staminaCost, 0, Mathf.Infinity);
        lastTimeUsedStamina = Time.time;
    }

    public static float GetCollisionUnits(Collision c)
    {
        /*
         * 50 collision units = 1 kg Rigidbody falling 1m (between colliding edges) onto another Collider
         * General examples
         * - Apple (1/3lb = 0.1511974566666666kg) falling 1m ==> 7.408676 collision units
         * - Basketball (1.37777777lb = 0.625kg) falling 1m ==> 30.625 collision units
        */
        return c.relativeVelocity.sqrMagnitude * (c.rigidbody ? c.rigidbody.mass : 1);
    }
} //Body
