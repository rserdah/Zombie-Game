using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Serializable]
    public class Components
    {
        public Renderer renderer;
        public MeshFilter meshFilter;
        public List<Collider> colliders = new List<Collider>();
        public Rigidbody rb;
        public AudioSource audioSource;
        public Animator anim;
    }

    [Serializable]
    public class ChildComponents
    {
        [Tooltip("Make sure to enable every necessary child before Awake or else this might not get inactive children.")]
        public List<Transform> transforms = new List<Transform>();
        public List<Renderer> renderers = new List<Renderer>();
        public List<MeshFilter> meshFilters = new List<MeshFilter>();
        public List<Collider> colliders = new List<Collider>();
        public List<Rigidbody> rbs = new List<Rigidbody>();
    }

    /// <summary>
    /// Properties like physical/general properties such as center of mass, weight, maybe color, etc.
    /// </summary>
    [Serializable]
    public class Properties
    {
        [Tooltip("Back-end name; Name that should be used for animation state names or otherwise used in code/back-end.")]
        public string name;
        [Tooltip("Front-end name; Name that should be displayed to the player (often more formal, descriptive, or more characters).")]
        public string displayName;
        public Transform centerOfMass;
    }


    [Serializable]
    public class Effects
    {
        public AudioClip useSound;
        public AudioClip emptySound;
        public AudioClip recoverSound;
        public AudioClip rechamberSound;

        public ParticleSystem useEffect;
        [Tooltip("Can make this ParticleSystem that lasts a little while so as the player uses the weapon more (usually shooting) more, this effect becomes more saturated, creating an overheating effect.")]
        public GameObject overHeatEffect;
        [Tooltip("Drag in parts of the Weapon (usually Gun) that will overheat as the player keeps using it. They should all have the same Material (in Play Mode, they will all have the same Material based on the first Renderer.material " +
            "in this List).")]
        public List<Renderer> overHeatParts = new List<Renderer>();
        public Material overHeatMaterial;
        public ParticleSystem dischargeEffect;
        public GameObject impactEffect;
    }

    [Serializable]
    public class Stats
    {
        public int maxUses = 5;
        public bool hasRechamber;
        public bool needsRechamber;
        public bool rechambered;
        public int rechamberRounds = 0;

        public int usesLeft = 5;

        public float recoverTime = 2f;
        public float rechamberTime = 1f;

        public float damage = 5f;
        public float fireRate = 10f;
        public float range = 50000f;
        public float accuracy = 90f;
        public float spread = 0f;
        public int shotRounds = 1;
        public float impactForce = 2000f;
        public float aimFOV = 30f;
        public float stability = 1f;
    }

    public static class IDs //Maybe change to enum
    {
        public static int soumiKP31 = 0;
        public static int vssVintorez = 1;
    }

    //=== States ===
    public bool isInteractable = true;
    [Tooltip("Is this Interactable an accessory/secondary to some 'main' Interactable? (For example, a Watch would be peripheral and the Gun would be the 'main' Interactable).")]
    public bool isPeripheral;
    [Tooltip("Does this Interactable need a controller (like a Gun would need a Player to shoot it).")]
    public bool needsControllingEntity;
    public bool isRecovering;

    //public InteractPrompt[] interactPrompts;
    //public string interactPrompt = "Press ? to interact with ";

    [Tooltip("NOT unique to every Interactable; Is like a type (e.g. Two Fire Axe weapons would have the same ID b/c the ID refers to Fire Axe, not the specific Fire Axe obj./instance) (Determines what animation(s) this Interactable should use and/or be used with among other things.)")]
    public int ID = 0; //Maybe change name to index or something else b/c ID implies it's unique to every Interactable instance
    public Body body;
    public Entity entity;

    [Tooltip("Some general Components that might be needed in a given situation.")]
    public Components components = new Components();

    [Tooltip("Some general Components on the children that might be needed in a given situation.")]
    public ChildComponents childComponents = new ChildComponents();

    [Tooltip("Properties like physical/general properties such as center of mass, weight, maybe color, etc.")]
    public Properties properties = new Properties();

    [Tooltip("Sounds, ParticleSystems, etc. used by this Interactable.")]
    public Effects effects = new Effects();

    public Stats stats = new Stats();


    public virtual void Awake()
    {
        //If this Interactable is meant to stand alone as its own thing, then it may have an Entity Component; if it is meant to be a part of a thing, then its parent (or whatever it is a part of) will set this Entity reference for it
        if(!needsControllingEntity) //Or just have null check (!!! But this may mess up things that ARE meant to override previous Entity references !!!)
            entity = GetComponent<Entity>();

        //Get this GameObject's Components
        GetComponents();

        //Get this GameObject's Children's Components
        if(!(this is BodyPart)) //Because BodyParts used in rigs often have extremely many children so it would be inefficient and often unneccessary to for each BodyPart to have a reference to every one of its child Components. The BodyPart.Entity 
                                //or BodyPart.Body GameObject should have all these references
            GetChildComponents();

        //interactPrompts = GetComponentsInChildren<InteractPrompt>();
        //interactPrompt = "Press ? to interact with " + name;

        //foreach(InteractPrompt i in interactPrompts)
        //{
        //    i.interactable = this;
        //}

        if(effects.overHeatParts.Count > 0)
            effects.overHeatMaterial = new Material(effects.overHeatParts[0].material);

        foreach(Renderer r in effects.overHeatParts)
        {
            if(effects.overHeatMaterial)
                r.material = effects.overHeatMaterial;
        }
    }

    public void GetComponents()
    {
        if(!components.renderer) components.renderer = GetComponent<Renderer>();
        if(!components.meshFilter) components.meshFilter = GetComponent<MeshFilter>();
        GetColliders();
        if(!components.rb) components.rb = GetComponent<Rigidbody>();
        if(components.rb && properties.centerOfMass) components.rb.centerOfMass = properties.centerOfMass.localPosition;
        if(!components.audioSource) components.audioSource = GetComponent<AudioSource>();
        if(!components.audioSource && !(this is BodyPart)) components.audioSource = gameObject.AddComponent<AudioSource>();
        if(!components.anim) components.anim = GetComponent<Animator>();
    }

    public void GetColliders()
    {
        foreach(Collider col in GetComponents<Collider>())
        {
            if(!col.isTrigger)
            {
                components.colliders.Add(col);
            }
        }
    }

    public void GetChildComponents()
    {
        foreach(Transform t in GetComponentsInChildren<Transform>())
            if(!t.Equals(transform) && !childComponents.transforms.Contains(t))
                childComponents.transforms.Add(t);

        foreach(Renderer r in GetComponentsInChildren<Renderer>())
            if(!r.Equals(components.renderer) && !childComponents.renderers.Contains(r))
                childComponents.renderers.Add(r);

        foreach(MeshFilter m in GetComponentsInChildren<MeshFilter>())
            if(!m.Equals(components.meshFilter) && !childComponents.meshFilters.Contains(m))
                childComponents.meshFilters.Add(m);

        foreach(Collider c in GetComponentsInChildren<Collider>())
            if(!components.colliders.Contains(c) && !childComponents.colliders.Contains(c))
                childComponents.colliders.Add(c);

        foreach(Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
            if(!rb.Equals(components.rb) && !childComponents.rbs.Contains(rb))
                childComponents.rbs.Add(rb);
        }
    }

    public List<Renderer> GetRenderers()
    {
        return childComponents.renderers;
    }

    public void SetCollidersActive(bool active)
    {
        foreach(Collider col in components.colliders)
        {
            col.enabled = active;
        }
    }

    public virtual void Action1()
    {

    }

    public virtual void Action2()
    {

    }

    public virtual void RecoverUses()
    {

    }

    public virtual void Aim()
    {

    }

    public void PlayOneShot(AudioClip clip)
    {
        if(components.audioSource)
        {
            components.audioSource.PlayOneShot(clip);
        }
    }
} //Interactable
