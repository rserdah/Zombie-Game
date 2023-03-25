using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

[RequireComponent(typeof(Body), typeof(NavMeshAgent)/*, typeof(FOV)*/)]
//[RequireComponent(typeof(Animator))]
public class Enemy : Entity
{
    [Header("Stats")]
    public float stopDistance = 1f;
    public float closeSight = 5f;
    public float farSight = 15f;
    [Tooltip("Buffer between the current sight stage and the next. Used to prevent harsh changes between different animations when target is continuously on the line between one sight section and another.")]
    public float sightAngle = 45f; //Find better name
    [Tooltip("How long the Enemy continues to pursue target after it leaves its FOV.")]
    public float attentionSpan = 2f;
    public float currentAttention;
    public float speed; //Instead use Body's agility !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public float pursuitSpeedMultiplier = 1.5f; //Instead use Body's agility !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public Vector3 lookOffset;
    //public float initialAttackDelay = 0.5f; //Once Player is within Enemy's stopDistance (distance where Enemy attacks), this delay is used for the first attack and for all following attacks, attackDelay is used (resets when player exits stopDistance)
    //bool willAttackInitial; //True when Player is not within stopDistance (attacking distance), becomes false on the first call to Attack() while Player is within stopDistance
    //float playerEnterStopDistanceTime;
    public float attackDelay = 7f;
    /// <summary>
    /// Should the Enemy always pursue target even if it does not see target?
    /// </summary>
    [Tooltip("Should the Enemy always pursue target even if it does not see target?")]
    public bool constantPursuit;
    [Tooltip("If there is a certain arousal of the Enemy (a loud sound/a player shot it), the Enemy will be temporarily interested in the target and pursue it.")]
    public bool attentionDrawn;
    [Tooltip("Will be true while the Enemy is distracted.")]
    public bool distracted;

    public float maxPlaySoundDistance;

    public float lastTimeAttacked; //temp
    public bool attacking; //temp

    //public Vector3 sightRangeOrigin;
    //public Vector3 sightRangeHalfExtents;
    //public Mesh cubeMesh;

    public Transform player;
    public PlayerInput playerPlayerInput;
    public bool seesPlayer;
    public static bool playerIsDead;
    public Vector3 m_playerOrigin;
    public Vector3 playerOrigin
    {
        get
        {
            return player.TransformPoint(m_playerOrigin);
        }
    }

    public Vector3 m_playerIKOrigin;
    public Vector3 playerIKOrigin
    {
        get
        {
            if(player)
                return player.TransformPoint(m_playerIKOrigin);
            else
                return Vector3.zero;
        }
    }

    public Vector3 m_sightRayOrigin;
    private Vector3 sightRayOrigin
    {
        get
        {
            return transform.TransformPoint(m_sightRayOrigin);
        }
    }
    public Ray sightRay;
    public RaycastHit sightRayHit = new RaycastHit();

    public float rightDot, upDot, forwardDot;
    public float rightAngle, upAngle, forwardAngle;

    public float sightRayAngle = 30;

    public Vector3 directionToPlayer
    {
        get
        {
            return playerOrigin - sightRayOrigin;
        }
    }
    public float maxSightRayDistance = 500f;
    //ONLY CHECK RAYCAST TO Player IF PLAYER IS IN MAXSIGHTRAYDISTANCE
    //Going to check dot product between sightRay (always in the direction bewteen enemy & player) and transform.forward AND transform.up & transform.right

    /// <summary>
    /// The target of this Enemy, has more priority than targetPosition (i.e. if target != null and targetPosition is set to a position, the Enemy will pursue target before pursuing targetPosition).
    /// </summary>
    [Header("References")]
    public Transform target;

    public Vector3 lastKnownPosition; //Set this to target.position when the target leaves the FOV
    public Quaternion lastKnownRotation;
    public NavMeshAgent agent { get; private set; }
    //private FOV fov;

    public float rightHandIKWeight;
    public float leftHandIKWeight;

    public bool overrideLookAtIK;
    public bool overrideLeftHandIK;
    public bool overrideRightHandIK;

    public Transform lookAtTarget;
    public Transform leftHandIKTarget;
    public Transform rightHandIKTarget;



    [Serializable]
    public class AnimatorHashes
    {
        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Can use Animator.StringToHash("LayerName.StateName") !!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 
         * Might want to use so you can do RunningLayer.Jump, StandingLayer.Jump, etc.
         */

        public int horizAttack = Animator.StringToHash("HorizAttack");
        public int vertAttack = Animator.StringToHash("VertAttack");
        public int attack = Animator.StringToHash("Attack");
        public int gettingHit = Animator.StringToHash("GettingHit");

        public int idle = Animator.StringToHash("Idle");

        public int walking = Animator.StringToHash("Walking");

        public int running = Animator.StringToHash("Running");

        public int standingJump = Animator.StringToHash("StandingJump");
        //public int walkingJump = Animator.StringToHash("WalkingJump");
        public int runningJump = Animator.StringToHash("RunningJump");
    }
    public AnimatorHashes hashes = new AnimatorHashes();

    //=== Temp./Holder Variables ===
    private float distance;
    private bool idle;
    private bool seesTarget;
    private bool sawTarget;
    private bool reachedLastKnownPosition;
    private float lastTimeSawTarget;
    public float lookAtWeight;
    public float lookAtSpeed = 1f;
    [Range(0.0f, 1.0f)]
    //Set Animator.speed = 0 to make freeze effect
    public float animSpeed = 1.0f;
    Vector3 newPos;
    public Transform debugTarget;
    public float stopBuffer = 0.5f;

    public bool debugNavmeshAgentPath;
    public GameObject pathMarkerPrefab;
    public GameObject destinationMarker;
    public Transform[] pathMarkers;
    public Vector3[] corners;

    public float lookAtStopAngle; //The max. angle between the directionToPlayer and the direction to the next corner in the NavMeshAgent's path that is allowed. If the angle is greater than this value, Enemy will stop looking at the Player
    //and allow the NavMeshAgent to control the transform.forward so the Enemy can avoid obstacles correctly (overwriting transform.forward messes up obstacle avoidance)
    public float angleBetweenDirs;
    public Vector3 directionToNextCorner
    {
        get
        {
            if(agent && agent.path.corners.Length > 2)
            {
                return agent.path.corners[1] - agent.path.corners[0];
            }

            return Vector3.zero;
        }
    }

    public bool temporaryObstacleAvoid;
    public bool canTakeParticleDamage = true;


    public float time;

    public override void Awake()
    {
        base.Awake();

        allyTypes.Add(typeof(Enemy));

        try
        {
            agent = GetComponent<NavMeshAgent>();
            agent.stoppingDistance = stopDistance;
            //fov = GetComponent<FOV>();
            //if(!fov) fov = GetComponentInChildren<FOV>();
            //fov.viewRadius = farSight;
            //fov.viewAngle = sightAngle;
        }
        catch(Exception e)
        {
            Debug.LogError(e.StackTrace);
        }

        maxPlaySoundDistance = components.audioSource.maxDistance;

        newPos = Vector3.right * UnityEngine.Random.Range(5f, 10f) + Vector3.forward * UnityEngine.Random.Range(5f, 10f);
    }

    public override void Update()
    {
        if(!body.isDead && player && ((playerPlayerInput && !playerPlayerInput.isDead) || (!playerPlayerInput)))
        {
            base.Update();


            time = Time.time; //temp
            corners = agent.path.corners;

            if(!IsPathToPlayerComplete())
            {
                agent.destination = corners[corners.Length - 1];
            }

            if(debugNavmeshAgentPath && pathMarkerPrefab && destinationMarker)
            {
                DebugNavMeshAgentPath();

                destinationMarker.transform.position = agent.destination;
            }

            Vector3 playerHeading = directionToPlayer.normalized;
            playerHeading.y = 0f;
            Vector3 cornerHeading = directionToNextCorner.normalized;
            cornerHeading.y = 0;

            angleBetweenDirs = Vector3.Angle(playerHeading, cornerHeading);

            if(angleBetweenDirs <= lookAtStopAngle)
            {
                //If previously temporarily avoiding an obstacle, resume LookAt(player) for one frame (b/c Enemy was looking at Player before having to stop looking at Player in order to avoid an obstacle)
                if(temporaryObstacleAvoid)
                {
                    LookAt(player);
                }

                temporaryObstacleAvoid = false;
                //Debug.LogError("Looking at Player");
            }
            else if(agent.path.corners.Length > 2) //Else if there is an obstacle in the way of the Enemy's path (a path straight to the destination w/o any obstacles in the way has two corners: the Enemy position and the destination)
            {
                //Only set temporaryObstacleAvoid = true if angle is greater AND if there is an obstacle in the path (if path corners.length is greater than 2 (b/c if there are only 2 corners, there are only two points: the Enemy and the destination 
                //(Player))); This will fix the problem of being able to quickly run around Enemies in order to make the angle > lookAtStopAngle and make the Enemy stop following Player

                //Also, when the angle becomes <= lookAtStopAngle, check if temporaryObstacleAvoid was previously true, if so, then look back at the Player (b/c Enemy had to have been looking at Player before it temporarily looked away to avoid 
                //obstacle so it now needs to resume looking at and chasing Player)

                temporaryObstacleAvoid = true;
                //Debug.LogError("Avoiding obstacle");
            }

            //---------------------------------------------------------
            //---------------------------------------------------------
            //---------------------------------------------------------

            //Muting Enemy based on player distance
            if(directionToPlayer.sqrMagnitude > maxPlaySoundDistance * maxPlaySoundDistance)
                sounds.muted = true;
            else
                sounds.muted = false;

            //Finding player
            //If player is within range, check the Raycast between the Enemy and the player to see if player is hiding behind wall or if Enemy can see player
            if(directionToPlayer.sqrMagnitude <= maxSightRayDistance * maxSightRayDistance)
            {
                sightRay = new Ray();
                sightRay.origin = sightRayOrigin;
                sightRay.direction = directionToPlayer.normalized;

                //TODO: Make tolerance for sightray so player can peek out from corners w/ being immediately seen (shoot out 4 more rays angled by bufferAngle: so that they land slightly to the right, left, up, and down of the player; then check if these rays hit an obstacle (a non-Player and non-Enemy Layer), if so, player is peeking out from a corner, so do not target them until they are farther from an obstacle)

                if((!playerIsDead && Physics.Raycast(sightRay, out sightRayHit, maxSightRayDistance, ~(1 << LayerMask.NameToLayer("BodyPart")), QueryTriggerInteraction.Ignore)) || constantPursuit)
                {
                    if(player.Equals(sightRayHit.transform) /*sightRayHit.transform.Equals(player)*/ || constantPursuit)
                    {
                        //rightDot = Vector3.Dot(transform.right, directionToPlayer.normalized);
                        //upDot = Vector3.Dot(transform.up, directionToPlayer.normalized);
                        //forwardDot = Vector3.Dot(transform.forward, directionToPlayer.normalized);

                        //Take out unneeded components of the direction Vectors in order to get a better angle reading (ex. If the direction used for the forwardAngle has a y (up component), the angle will be incorrect)
                        Vector3 rightHeading, upHeading, forwardHeading;
                        rightHeading = upHeading = forwardHeading = directionToPlayer.normalized;

                        rightHeading.y = forwardHeading.y = 0;

                        rightAngle = Vector3.Angle(transform.right, rightHeading);
                        upAngle = Vector3.Angle(transform.up, upHeading);
                        forwardAngle = Vector3.Angle(transform.forward, forwardHeading);

                        if(Mathf.Abs(forwardAngle) <= sightRayAngle || constantPursuit)
                        {
                            seesPlayer = true;

                            //NavMeshHit edgeHit;
                            //if(NavMesh.FindClosestEdge(transform.position, out edgeHit, NavMesh.AllAreas))
                            //{
                            //    //make it resume following the player once it is far enough from an edge
                            //    if()
                            //}

                            lookAtWeight += lookAtSpeed * Time.deltaTime;
                            lookAtWeight = Mathf.Clamp01(lookAtWeight);

                            //Debug.LogError("Sees player");

                            //temp.; if destination is not the last corner, set it to player pos.
                            /*if(Mathf.Abs((agent.destination - corners[corners.Length - 1]).sqrMagnitude) > 0.15f)*/ agent.destination = player.position;


                            /*transform.*/if(!temporaryObstacleAvoid && !attacking) LookAt(player);
                            components.anim.SetLookAtPosition(playerOrigin);

                            Vector3 distVect = directionToPlayer;
                            distVect.y = 0;
                            //Debug.LogError(distVect.sqrMagnitude);


                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                            //if((seesTarget || constantPursuit || distracted) && distance > Mathf.Pow(farSight, 2)) //If (seestarget OR constantPursuit OR distracted) AND target is too far
                            if(distVect.sqrMagnitude > Mathf.Pow(farSight, 2)) //If (seestarget OR constantPursuit OR distracted) AND target is too far
                            {
                                Run();
                                //Debug.LogError("Running.");
                            }
                            //else if((player.position - transform.position).magnitude <= Mathf.Pow(stopDistance, 2))    //Stop
                            else if(LessThanEqualTo(distVect.sqrMagnitude, Mathf.Pow(stopDistance, 2), stopBuffer) || LessThanEqualTo((transform.position - agent.destination).sqrMagnitude, Mathf.Pow(stopDistance, 2), stopBuffer))    //Stop
                            {
                                //Debug.LogError("Stopping");

                                ResetLoopingBools(); //Check

                                if(/*!attacking &&*/ /*(willAttackInitial && Time.time - playerEnterStopDistanceTime >= initialAttackDelay) || */(Time.time - lastTimeAttacked >= attackDelay))
                                {
                                    //Debug.LogError("Attacking");
                                    Attack();

                                    lastTimeAttacked = Time.time;
                                    attacking = true;
                                }
                                else
                                {
                                    //Debug.LogError("Waiting to attack");

                                    //attacking = false;
                                    if(!body.isAttacking)
                                        Idle();
                                    //Debug.LogError("Not Running, Idling");
                                }
                            }
                            else if(LessThanEqualTo(distVect.sqrMagnitude, Mathf.Pow(closeSight, 2), stopBuffer)) //Close
                            {
                                //Debug.LogError("Walking");
                                Walk();
                                //Debug.LogError("Not Running or Idling, Walking");
                            }
                            else if(LessThanEqualTo(distVect.sqrMagnitude, Mathf.Pow(farSight, 2), stopBuffer))   //Far
                            {
                                //Debug.LogError("Running");
                                Run();
                                //Debug.LogError("Not Running (first if statement, Idling, or Walking, Running (second if statement))");
                            }
                            else
                            {
                                //Debug.LogError("None of the above");
                            }

                            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                            //if(distVect.sqrMagnitude <= Mathf.Pow(stopDistance, 2))    //Stop
                            //if(LessThanEqualTo(distVect.sqrMagnitude, Mathf.Pow(stopDistance, 2), stopBuffer))    //Stop
                            //{
                            //    Debug.LogError("Stopping");

                            //    if(/*!attacking &&*/ Time.time - lastTimeAttacked >= 2.5f) //TEMP
                            //    {
                            //        Debug.LogError("Attacking");
                            //        Attack();

                            //        lastTimeAttacked = Time.time;
                            //        attacking = true;
                            //    }
                            //    else
                            //    {
                            //        //attacking = false;
                            //        Idle();
                            //        //Debug.LogError("Not Running, Idling");
                            //    }
                            //}
                        }
                        else
                        {
                            seesPlayer = false;

                            lookAtWeight -= lookAtSpeed * Time.deltaTime;
                            lookAtWeight = Mathf.Clamp01(lookAtWeight);

                            Debug.LogError("Doesn't see player");
                        }
                    }
                    else
                    {
                        //If seesPlayer is still true, set destination one last time before setting seesPlayer to false
                        if(seesPlayer)// && !constantPursuit /*TEMP!!!!!!!!!!!!!!!!!!!!!!!!!*/)
                        {
                            agent.destination = player.position;
                        }

                        seesPlayer = false;
                        Debug.LogError("Doesn't see player; Sees " + sightRayHit.transform.name);
                    }
                }
                else if(playerIsDead)
                {
                    Idle();
                }
            }
            else
            {
                Debug.LogError("hi");
            }

            //Set Animator.speed = 0 to make freeze effect
            components.anim.speed = animSpeed;

            //Set NavMeshAgent.speed to a small number because character is currently controlled by root motion; IF SPEED IS 0, NAVMESHAGENT DOES NOTHING/MAKES CHARACTER GO IN A RANDOM DIRECTION!!!!!!!!!!!!!!!!!!!!!
            agent.speed = 0.05f;

            if(target)
                distance = (target.position - transform.position).sqrMagnitude;
            else if(distracted || attentionDrawn) //Need to check for both
                distance = (lastKnownPosition - transform.position).sqrMagnitude;

            //Old Stuff
            {
                ////if(/*target &&*/ !components.anim.GetBool(hashes.gettingHit))
                ////{
                //    //Changing it to be if the Enemy sees a target, then pursue it (instead of only having one pre-set target)
                //    //if(/*fov.Sees(target)*/ /*target || */ /*fov.GetTarget(0)*/ true/*temp*/ || (constantPursuit && target) || (attentionDrawn && !target)) //If we see a target OR Enemy is meant to constantly pursue target OR our attention is drawn to something
                //    /*{
                //        //if(!target) target = fov.GetTarget(0); //Set target to the first seen target

                //        idle = false;
                //        sawTarget = false;
                //        reachedLastKnownPosition = false;
                //        seesTarget = true;
                //        if(!attentionDrawn) distracted = false;
                //
                //        //if(target)
                //        //    Debug.LogError(name + " sees its target " + target.name);
                //        if(attentionDrawn)
                //        {
                //            seesTarget = true;
                //            distracted = true;
                //            Debug.LogError(name + "'s attention was drawn.");


                //            attentionDrawn = false;
                //        }

                //        PursueTarget();

                //        //if(idle && distance <= Mathf.Pow(stopDistance, 2))
                //        //{
                //        //    Debug.LogError("Attacking");
                //        //    Attack();
                //        //}
                //    }
                //    else if(seesTarget) //Else if target just left our FOV (If they are no longer in sight but the bool is still temporarily true so we know when to set sawTarget to true)
                //    {
                //        lastTimeSawTarget = Time.time;
                //        reachedLastKnownPosition = false;

                //        if(target)
                //        {
                //            lastKnownPosition = target.position;
                //            //Debug.LogError(target.name + " left " + name + "'s FOV.");
                //        }

                //        //Debug.LogError(name + "'s target's lastKnownPosition is " + lastKnownPosition);
                //        //DebugHelper.DebugPose(lastKnownPosition);


                //        seesTarget = false;
                //        sawTarget = true;
                //        //Increase agent's speed with the multiplier when in pursuit to make it more realistic because they are eager to find the fleeing target.
                //    }*/
                //    //else if(sawTarget && !reachedLastKnownPosition && (lastKnownPosition - transform.position).sqrMagnitude >= 1f /*(0.0f * 0.08f)*/) //!!! (# * #) must be in parentheses !!!
                //    /*{
                //        lastTimeSawTarget = Time.time;
                //        agent.destination = lastKnownPosition;
                //        //Debug.LogError("Going to lastKnownPosition");
                //        //Debug.LogError("sqrDist: " + (lastKnownPosition - transform.position).sqrMagnitude);
                //    }
                //    else if(sawTarget && !reachedLastKnownPosition) //If we sawTarget and we just reached the lastKnownPosition, set reachedLastKnownPosition to true
                //    {
                //        reachedLastKnownPosition = true;
                //    }
                //    else if(sawTarget && currentAttention <= attentionSpan) //Else if we just saw the target and we still have the attention span to look for them
                //    {
                //        //LookForTarget();

                //        agent.destination = lastKnownPosition + newPos;

                //        //DebugHelper.DebugPose(lastKnownPosition + newPos);

                //        //if(target) Debug.LogError(string.Format("{0}'s time spent looking for {1}: {2} / {3}", name, target.name, currentAttention, attentionSpan));
                //        //if(target) Debug.LogError(string.Format("{0}'s time spent looking for {1}: {2} / {3}", name, target.name, Time.time - lastTimeSawTarget, attentionSpan));
                //    }
                //    else if(!idle) //Else we don't see the target and we have exceeded our attentionSpan for looking for the target
                //    {
                //        //AnimatorHelper.PlayIfNotPlayingAlready(anim, "Idle");
                //        ResetLoopingBools();
                //        components.anim.SetBool(hashes.idle, true);
                //        agent.speed = 0;
                //        sawTarget = false;
                //        reachedLastKnownPosition = false;
                //        attentionDrawn = false;
                //        distracted = false;

                //        Debug.LogError(name + " exceeded its attentionSpan");
                //        //DebugHelper.DebugPose(transform.position);
                //        //Play the idle animation, look for the target (again?), look for any target, etc.

                //        if(!constantPursuit)
                //            target = null;


                //        idle = true;
                //    }
                //}

                //currentAttention = Time.time - lastTimeSawTarget;*/


                ////Debug.LogError("REMEMBER TO KEEP NAVMESHAGENT SPEED AT A LOW NUMBER LIKE 0.05 IF CHARACTER IS USING ROOT MOTION (B/C IF SPEED IS 0, NAVMESHAGENT WILL DO NOTHING/MAKE CHARACTER GO IN A RANDOM DIRECTION).");
            }
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        //if(seesPlayer)
        //{
        //    components.anim.SetLookAtWeight(0.75f);
        //    components.anim.SetLookAtPosition(playerOrigin);
        //}
        //else
        //{
        //    components.anim.SetLookAtWeight(0f);
        //}

        components.anim.SetLookAtWeight(lookAtWeight);

        components.anim.SetLookAtPosition(overrideLookAtIK ? lookAtTarget.position : playerIKOrigin);

        components.anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
        components.anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandIKWeight);

        components.anim.SetIKPosition(AvatarIKGoal.LeftHand, (overrideLeftHandIK && leftHandIKTarget) ? leftHandIKTarget.position : playerIKOrigin);
        components.anim.SetIKPosition(AvatarIKGoal.RightHand, (overrideRightHandIK && rightHandIKTarget) ? rightHandIKTarget.position : playerIKOrigin);

        //if(body.isAttacking)
        {
            //components.anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.75f);
            //components.anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            //components.anim.SetIKPosition(AvatarIKGoal.RightHand, playerOrigin);
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if(other.layer == LayerMask.NameToLayer("Explosive") && canTakeParticleDamage)
        {
            body.TakeDamageByType(3000f, Body.DamageType.FIRE);
            canTakeParticleDamage = false;
            Invoke(nameof(ParticleDamageCooldown), 0.75f);
        }
    }

    private void ParticleDamageCooldown()
    {
        canTakeParticleDamage = true;
    }

    public void SetAttacking()
    {
        body.isAttacking = true;
    }

    public void StopAttacking()
    {
        body.isAttacking = false;
        attacking = false; //the temp. var. in Enemy (this script)
    }

    public void Attack()
    {
        //body.isAttacking = true; //This bool will be set in the attacking Animation (by calling SetAttacking()) so that the Enemy does not hit the Player too early (ex. so it doesn't hit the Player when it is winding up an attack)

        components.anim.SetBool(hashes.idle, false);
        components.anim.SetFloat(hashes.horizAttack, -1f);
        components.anim.SetFloat(hashes.vertAttack, 0f);
        components.anim.SetTrigger("isPunchingTrigger"); //CHECK
    }

    public void Idle()
    {
        //AnimatorHelper.PlayIfNotPlayingAlready(anim, "Idle");
        ResetLoopingBools();
        components.anim.SetBool(hashes.idle, true);
        if(target && !temporaryObstacleAvoid) /*transform.*/LookAt(target);
        agent.speed = 0;
        //agent.destination = target.position;
    }

    public void Walk()
    {
        //AnimatorHelper.PlayIfNotPlayingAlready(anim, "Walking");
        ResetLoopingBools();
        components.anim.SetBool(hashes.walking, true);
        /*transform.*/if(!temporaryObstacleAvoid) LookAt(target);
        agent.speed = 0.05f;
        //agent.destination = target.position;
    }

    public void Run()
    {
        //AnimatorHelper.PlayIfNotPlayingAlready(anim, "Running");
        ResetLoopingBools();
        components.anim.SetBool(hashes.running, true);
        if(target && !temporaryObstacleAvoid) /*transform.*/LookAt(target);
        else if(distracted && !temporaryObstacleAvoid) /*transform.*/LookAt(lastKnownPosition);
        agent.speed = 0.05f;
        if(target) agent.destination = target.position;
        else if(distracted) agent.destination = lastKnownPosition;
    }

    //public void PursueTarget()
    //{
    //    if((seesTarget || constantPursuit || distracted) && distance > Mathf.Pow(farSight, 2)) //If (seestarget OR constantPursuit OR distracted) AND target is too far
    //    {
    //        Run();
    //        //Debug.LogError("Running.");
    //    }
    //    else if((player.position - transform.position).magnitude <= Mathf.Pow(stopDistance, 2))    //Stop
    //    {
    //        if(/*!attacking &&*/ Time.time - lastTimeAttacked >= 2.5f) //TEMP
    //        {
    //            //Debug.LogError("Attacking");
    //            Attack();

    //            lastTimeAttacked = Time.time;
    //            attacking = true;
    //        }
    //        else
    //        {
    //            //attacking = false;
    //            Idle();
    //            //Debug.LogError("Not Running, Idling");
    //        }
    //    }
    //    else if(distance <= Mathf.Pow(closeSight, 2)) //Close
    //    {
    //        Walk();
    //        //Debug.LogError("Not Running or Idling, Walking");
    //    }
    //    else if(distance <= Mathf.Pow(farSight, 2))   //Far
    //    {
    //        Run();
    //        //Debug.LogError("Not Running (first if statement, Idling, or Walking, Running (second if statement))");
    //    }
    //    else
    //    {
    //        //Debug.LogError("None of the above");
    //    }
    //}

    public void LookForTarget()
    {
        agent.destination = lastKnownPosition;
        //Debug.LogError(name + "'s target's lastKnownPosition is " + lastKnownPosition);

        //Have some animations and stuff to make it look around to possibly put the target back into its FOV
    }

    public void LookAt(Vector3 worldPosition)
    {
        Vector3 heading = worldPosition - transform.position;
        heading.y = 0;

        transform.forward = heading;
    }

    public void LookAt(Transform target)
    {
        if(target)
        {
            LookAt(target.position);
        }
    }

    public void SetFOVStats(float viewRadius, float viewAngle)
    {
        //farSight = viewRadius;
        //fov.viewRadius = viewRadius;

        //sightAngle = viewAngle;
        //fov.viewAngle = viewAngle;
    }

    public bool IsEnemy(Entity e)
    {
        Debug.LogError("Entity Type for " + e.name + ": " + e.GetType());
        return !allyTypes.Contains(e.GetType());
    }

    /*public void DrawAttentionTo(Transform t)
    {
        idle = false;

        if(!target || sawTarget)
        {
            if(sawTarget)
                target = null;

            lastTimeSawTarget = Time.time; //For starting the timer for going to the distraction position (will end when attentionSpan is exceeded)

            lastKnownPosition = t.position; //Maybe going to use this instead

            attentionDrawn = true;
        }
    }*/

    public void DrawAttentionTo(Vector3 position)
    {
        idle = false;

        if(!target || sawTarget)
        {
            if(sawTarget)
                target = null;

            lastTimeSawTarget = Time.time; //For starting the timer for going to the distraction position (will end when attentionSpan is exceeded)

            lastKnownPosition = position; //Maybe going to use this instead

            attentionDrawn = true;
        }
    }

    private void DebugNavMeshAgentPath()
    {
        if(pathMarkers.Length > 0)
        {
            foreach(Transform t1 in pathMarkers)
                Destroy(t1.gameObject);
        }

        pathMarkers = new Transform[corners.Length];
        Transform t;

        for(int i = 0; i < pathMarkers.Length; i++)
        {
            pathMarkers[i] = t = Instantiate(pathMarkerPrefab).transform;
            t.position = corners[i];
        }
    }

    private bool IsPathToPlayerComplete()
    {
        //Have to calculate new path or else doesn't show the correct status (if checking the status of the current path)
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(player.position, path);

        if(path.status == NavMeshPathStatus.PathComplete)
        {
            //Debug.LogError("Path Complete");

            return true;
        }
        else
        {
            //Debug.LogError("Path NOT Complete");

            return false;
        }
    }

    public override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();


        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        //Gizmos.DrawWireMesh(cubeMesh, 0, transform.TransformPoint(sightRangeOrigin), transform.rotation, 2f * sightRangeHalfExtents);
        //Gizmos.DrawFrustum(transform.position,) //Maybe draw frustrum for sightAngles

        if(player)
        {
            Gizmos.DrawLine(sightRayOrigin, playerOrigin);
            Gizmos.DrawLine(sightRayOrigin, sightRayOrigin + Quaternion.AngleAxis(-sightRayAngle, transform.up) * transform.forward * 50);
            Gizmos.DrawLine(sightRayOrigin, sightRayOrigin + Quaternion.AngleAxis(sightRayAngle, transform.up) * transform.forward * 50);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, closeSight);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, farSight);

        //Draw buffer spheres; NEED TO IMPLEMENT ABOVE WHEN CHECKING DISTANCES (use LessThanEqualTo()) !!!!!!!!!!!!!!
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, stopDistance + 0.25f);
        Gizmos.DrawWireSphere(transform.position, stopDistance - 0.25f);

        Gizmos.DrawWireSphere(transform.position, closeSight + 0.25f);
        Gizmos.DrawWireSphere(transform.position, closeSight - 0.25f);

        Gizmos.DrawWireSphere(transform.position, farSight + 0.25f);
        Gizmos.DrawWireSphere(transform.position, farSight - 0.25f);

        if(agent)
        {
            Gizmos.color = Color.green;
            foreach(Vector3 c in agent.path.corners)
            {
                Gizmos.DrawWireSphere(c, 0.075f);
            }
        }
    }

    //private void OnAnimatorIK(int layerIndex)
    //void hiaaaa()
    //{
    //    if(target)
    //    {
    //        if(/*fov.Sees(target)*/true/*temp*/)
    //        {
    //            lookAtWeight = Mathf.Clamp(lookAtWeight + 2.75f * Time.deltaTime, 0, 0.8f);
    //            components.anim.SetLookAtWeight(lookAtWeight);
    //            components.anim.SetLookAtPosition(target.position + lookOffset);
    //        }
    //        else
    //        {
    //            lookAtWeight = Mathf.Clamp01(lookAtWeight - 2.75f * Time.deltaTime);
    //            components.anim.SetLookAtWeight(lookAtWeight);
    //            components.anim.SetLookAtPosition(target.position + lookOffset);
    //        }
    //    }
    //}

    private bool LessThanEqualTo(float value, float compareTo, float error)
    {
        //return value <= compareTo + error || value <= compareTo - error;
        bool b = value <= compareTo + error || value <= compareTo - error;

        //if(b)
        //{
        //    Debug.LogError($"{value} <= {compareTo + error} || {value} <= {compareTo - error}");
        //}

        return b;
    }

    public void ResetLoopingBools()
    {
        components.anim.SetBool(hashes.idle, false);
        components.anim.SetBool(hashes.walking, false);
        components.anim.SetBool(hashes.running, false);
    }

    public void ResetBoolOnStateFinish(Animator a, string boolName)
    {
        StartCoroutine(ResetBoolAfterDelay(a, boolName, a.GetCurrentAnimatorStateInfo(0).length));
    }

    public IEnumerator ResetBoolAfterDelay(Animator a, string boolName, float delay)
    {
        yield return new WaitForSeconds(delay);
        a.SetBool(boolName, false);
        Debug.LogWarning("State finished.");
    }

    public override void DisableOnDie()
    {
        base.DisableOnDie();


        agent.enabled = false;
        components.audioSource.enabled = false;
        GetComponent<BoxCollider>().enabled = false;


        enabled = false;
    }
} //Enemy
