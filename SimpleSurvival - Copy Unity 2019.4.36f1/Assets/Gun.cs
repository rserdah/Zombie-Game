using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Gun : Item, IUsableItem
{
    /*
    
    For guns and items in general
    1. Make a new ScriptableObject type called ItemInstance
    2. Move all item SkinnedMeshRenderers in a Resources folder called Models\Item or just Item
    3. Maybe have a JSON string in each item's ItemID ScriptableObject storing all the specs of that item (essentially all the vars that are stored in the Gun script of the Gun prefab (magazineSize, fireMode, etc.) also remove all unnecessary vars.)
    4. Make console so that can send commands like "give [playerIndex = 0] i_[itemName] [quantity]" where itemName corresponds to the itemName of ItemID and has a same named folder in Resources\Item (for playerIndex, need to make spawner, but can use fake playerIndex's for now)
    5. 

     */

    protected static string folder = "Gun/";

    public static GameObject ray_shot { get; protected set; }
    public static GameObject flame_shot { get; protected set; }
    public static bool prefabs_loaded { get; protected set; }

    public delegate RaycastHit ShootAction(Gun g);

    public ShootAction onShoot;

    /// <summary>
    /// A struct to store information about a hit (from a gun shot, melee hit, or any weapon)
    /// </summary>
    public struct Hit
    {
        public Entity instigator; //Who initiated the hit (shot the gun, threw the grenade, etc.)?

        /// <summary>
        /// Did this Hit actually land or hit something?
        /// </summary>
        public bool hit;

        //////////Don't need to store an array like Body[] hitBodies etc, instead, just have an array of Hit's where necessary (so a grenade/explosive will likely hit multiple zombies so instead of storing
        //an array of hit Body's, just store an array of Hit's, one for each Body that was hit), just make sure that only one Hit per Body is processed or else, the grenade that hit multiple BodyPart's 
        //will do multiple damage on the same Body
        public Body hitBody;
        public BodyPart hitBodyPart;

        public Vector3 point;
        public Vector3 normal;

        public RaycastHit? raycastHit;

        public bool isKillingHit;
    }

    public interface HitInstigator
    {
        void ProcessHit(QueuedHit hit);
    }

    /// <summary>
    /// Interface for things that can create and manipulate QueuedHit's
    /// </summary>
    public interface QueuedHitSender
    {
        QueuedHit SendHit(HitInstigator instigator);
    }

    /// <summary>
    /// Class to allow queued Hit's. For example, Raycast gun shot happens instantly and so has no need to be queued, however certain projectiles (like a grenade, bow and arrow, etc.) have a delay
    /// before they can actually do damage and produce a Hit. Therefore, an instigator must know what type of attack they are performing (instant or queued) and queue the Hit accordingly.
    /// 
    /// An idea is to have an option for what type of hit the current weapon produces (instant/queued) and when the player attacks (shoots, melees, throws grenade, etc.), if it is instant, 
    /// then they just process the hit, but if it is queued (maybe call it queueable), then add the expected hit to the List (queue) and await the landing of the hit, and when the hit lands, it should
    /// have a callback to the instigator so they can process the Hit and remove it from the queue
    /// 
    /// Steps:
    /// player presses attack
    ///     check if the weapon produces an instant or queued hit
    ///     if instant
    ///         handle it immediately
    ///     if queued
    ///         create a new QueuedHit
    ///         add it to the hit queue (the QueuedHit should send a callback when the Hit lands so that the instigator only has to wait for it without doing anything else)
    ///         
    /// 
    /// </summary>
    public class QueuedHit
    {
        public HitInstigator instigator;
        public Action<QueuedHit> instigatorCallback { get; set; }
        public Hit[] hits; //QueuedHit's need a Hit array because the instigator may not know exactly how many hits will be produced (for example a player throwing an explosive may not know how many
                           //hits it will produce when it explodes)

        public QueuedHit(HitInstigator _instigator)
        {
            instigator = _instigator;
            
            //Once created, the QueuedHit subscribes its HitInstigator to the callback
            instigatorCallback += instigator.ProcessHit;
        }
    }

    public enum FireMode
    {
        SEMIAUTOMATIC, AUTOMATIC, BURST //CONTINUOUS OR FLOW (for things like flame thrower/fluid gun where ammo is more like an amount like gallons or something and shooting is like a flow of something)
    }

    public enum FireType
    {
        BULLET, RAY, FLAME
    }

    public Animator anim;

    public int magazineSize = 6;
    public int ammoLeft = 6;

    public int maxAmmoReserved = 36;
    public int ammoReserved = 36;

    public bool ammoReplenished
    {
        get
        {
            return ammoReserved >= maxAmmoReserved;
        }
    }

    public FireMode fireMode = FireMode.SEMIAUTOMATIC;
    public FireType fireType = FireType.BULLET;

    public float fireRate = 15f;

    private float lastTimeShot;

    public float burstInterval = 0.05f;
    public int burstCount = 3;

    public bool hasRechamber;
    private bool rechambered;
    public int rechamberRounds;
    public bool needsRechamber
    {
        get
        {
            return hasRechamber && !rechambered && (ammoLeft % rechamberRounds == 0 && ammoLeft != magazineSize && ammoLeft != 0);
        }
    }

    private bool canManualIncrementalReload; //Only if Gun.hasRechamber is true, a bool that is true for a small window. If player presses reload while this bool is true, they will add rechamberRounds number of bullets while needing to rechamber 
    //(used so if player wants to top off the magazine while they are rechambering so they don't always have to reload all bullets at once)
    private bool willManualIncrementalReload;
    public float manualIncrementalReloadTime;

    public bool hasIncrementalReload; //Does this Gun reload in increments (such as putting a bullet in a revolver six times to fill the cylinder) or does it just reload with a clip?
    public int reloadIncrement; //Only if this Gun.hasIncrementalReload is true, how many bullets are added each time ...?
    public float reloadEndTime;

    public Vector3 localShootOrigin;

    public enum ShootAxis
    {
        RIGHT, UP, FORWARD
    }
    public bool invertShootDirection;

    public ShootAxis shootAxis;

    public Vector3 shootDirection
    {
        get
        {
            switch(shootAxis)
            {
                case ShootAxis.RIGHT:
                    return (invertShootDirection ? -1f : 1f) * transform.right;
                case ShootAxis.UP:
                    return (invertShootDirection ? -1f : 1f) * transform.up;
                case ShootAxis.FORWARD:
                    return (invertShootDirection ? -1f : 1f) * transform.forward;
            }

            return transform.forward;
        }
    }

    public Vector3 shootOrigin
    {
        get
        {
            return transform.position + (transform.right * localShootOrigin.x) + (transform.up * localShootOrigin.y) + (transform.forward * localShootOrigin.z);
        }
    }
    public float damage;

        public AudioClip rechamberSound; //Add to a new UnityEvent for rechamber when ready!!!!!!!!!!!!!!!!!!!!!!!

    [Header("Use Event")]
    public UnityEvent m_useEvent;

    [Header("Events")]
    public UnityEvent m_onEnableEvent;

    public UnityEvent m_onDisableEvent;

    //------------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------------

        //{ get { return ; } set {  = value; } }

    public float u_damage { get { return damage; } set { damage = value; } }

    public int u_usesLeft { get { return ammoLeft; } set { ammoLeft = value; } }

    public int u_maxUses { get { return magazineSize; } set { magazineSize = value; } }

    public int u_usesReserved { get { return ammoReserved; } set { ammoReserved = value; } }

    public int u_maxUsesReserved { get { return maxAmmoReserved; } set { maxAmmoReserved = value; } }

    public bool u_usesReplenished { get { return ammoReplenished; } }

    public bool u_hasRechamber { get { return hasRechamber; } }
    public bool u_rechambered { get { return rechambered; } set { rechambered = value; } }
    public int u_rechamberRounds { get { return rechamberRounds; } }
    public bool u_needsRechamber { get { return needsRechamber; } }

    public UseMode u_useMode { get { return useMode; } set { useMode = value; } }
    public UseType u_useType { get { return useType; } set { useType = value; } }

    public float u_lastTimeUsed { get { return lastTimeShot; } set { lastTimeShot = value; } }

    public AnimationClip idle { get { return animations.idle; } }
    public AnimationClip aim { get { return animations.aim; } }
    public AnimationClip aimUse { get { return animations.aimUse; } }
    public AnimationClip reload { get { return animations.reload; } }
    public AnimationClip rechamber { get { return animations.rechamber; } }
    public AnimationClip use { get { return animations.use; } }
    public AnimationClip swapStart { get { return animations.swapStart; } }
    public AnimationClip swapEnd { get { return animations.swapEnd; } }

    public ItemID u_itemID { get { return itemID; } }

    public float u_useRate { get { return fireRate; } }

    public float u_reloadEndTime { get { return reloadEndTime; } }

    public bool u_canManualIncrementalReload { get { return canManualIncrementalReload; } set { canManualIncrementalReload = value; } }
    public bool u_willManualIncrementalReload { get { return willManualIncrementalReload; } set { willManualIncrementalReload = value; } }
    public float u_manualIncrementalReloadTime { get { return manualIncrementalReloadTime; } set { manualIncrementalReloadTime = value; } }

    public UnityEvent u_useEvent { get { return m_useEvent; } }

    public UnityEvent u_onEnableEvent { get { return m_onEnableEvent; } }

    public UnityEvent u_onDisableEvent { get { return m_onDisableEvent; } }

    public Animator u_anim { get { return anim; } }

    public Vector3 u_shootOrigin { get { return shootOrigin; } }

    public Vector3 u_shootDirection { get { return shootDirection; } }

    public float u_burstInterval { get { return burstInterval; } }

    public int u_burstCount { get { return burstCount; } }


    public int u_IncrementalReload()
    {
        return IncrementalReload();
    }

    public int u_Reload()
    {
        return Reload();
    }


    //------------------------------------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------------------------------------


    private RaycastHit shootGunTest(Gun g)
    {
        return new RaycastHit();
    }


    private void Awake()
    {
        //RayBlasterShooter r = null;
        //onShoot += shootGunTest;
        //onShoot += r.Shoot;

        if(!prefabs_loaded)
        {
            ray_shot = Load(pathof("shoot/ray_shot"));


            prefabs_loaded = true;
        }
    }

    public override void Start()
    {
        base.Start();

        
        
        ammoLeft = magazineSize;

        lastTimeShot = 0;
    }

    //public static void Shoot(Ray ray, out RaycastHit hit)
    //{
    //    if(Physics.Raycast(ray, out hit, 500f, PlayerInput.LayerMasks.everything, QueryTriggerInteraction.Ignore))
    //    {
    //        BodyPart hitBodyPart = hit.transform.GetComponent<BodyPart>();

    //        if(hitBodyPart && hitBodyPart.body)
    //        {
    //            hitBodyPart.TakeDamage(null, gun.damage);
    //            audioSource.PlayOneShot(hitmarkerSound);
    //        }
    //        else if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Explosive"))
    //        {
    //            hit.transform.GetComponent<Explosive>().set = true; //temp.; way of setting off explosives from other classes
    //            Debug.LogError("Hit " + hit.transform.name);
    //        }

    //        Transform hitEffect = Instantiate(bulletHit).transform;

    //        hitEffect.position = hit.point;
    //        hitEffect.forward = hit.normal;

    //        //cube.transform.forward = hit.point - gun.shootOrigin;
    //        //cube.transform.position = (hit.point + gun.shootOrigin) / 2f;
    //        //cube.transform.localScale = new Vector3(0.025f, 0.025f, (hit.point - gun.shootOrigin).magnitude);
    //        //cube.GetComponent<Renderer>().material.color = Color.green;

    //        //sphere.transform.position = hit.point;
    //        //sphere.transform.localScale = new Vector3(0.075f, 0.075f, 0.075f);
    //        //sphere.GetComponent<MeshRenderer>().material = null;

    //        //Debug.LogError("Hit " + hit.collider.name);
    //    }
    //}

    /*public int Reload(ref int ammoReserved)
    {
        int ammoNeeded = magazineSize - ammoLeft;

        //If have enough reserved ammo to fill up Gun
        if(ammoReserved >= ammoNeeded)
        {
            ammoLeft += ammoNeeded;
            ammoReserved -= ammoNeeded;
        }
        else if(ammoReserved > 0) //Else has less than needed to fill up gun && > 0, so add all reserved ammo to Gun
        {
            ammoLeft += ammoReserved;
            ammoReserved = 0;
        }

        return ammoNeeded;
    }*/

    public override int Reload()
    {
        if(ammoReserved > 0)
        {
            int ammoNeeded = magazineSize - ammoLeft;

            //If have enough reserved ammo to fill up Gun
            if(ammoReserved >= ammoNeeded)
            {
                ammoLeft += ammoNeeded;
                ammoReserved -= ammoNeeded;
            }
            else if(ammoReserved > 0) //Else has less than needed to fill up gun && > 0, so add all reserved ammo to Gun
            {
                ammoLeft += ammoReserved;
                ammoReserved = 0;
            }

            return ammoNeeded; 
        }


        return 0;
    }

    public int IncrementalReload()
    {
        if(hasIncrementalReload)
        {
            ammoLeft += reloadIncrement;


            return reloadIncrement;
        }


        return 0;
    }

    public void ReplenishAmmo()
    {
        ammoLeft = magazineSize;
        ammoReserved = maxAmmoReserved;
    }

    public void ReplenishReservedAmmo()
    {
        ammoReserved = maxAmmoReserved;
    }

    public void OnEnable()
    {
        m_onEnableEvent.Invoke();
    }

    private void OnDisable()
    {
        m_onDisableEvent.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(shootOrigin, 0.05f);
    }

    private static GameObject Load(string path)
    {
        return Resources.Load<GameObject>(path);
    }

    private static string pathof(string relative)
    {
        return folder + relative;
    }
}
