using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entity represents a being: it holds all the necessary references for the other scripts that it works with (PlayerInput, Body, etc.). Currently, the two types of Entities are Player and AI
/// </summary>
[RequireComponent(typeof(Body))] //Can put multiple RequireComponent lines if need more than 3 (b/c the max you can put in a single RequireComponent line is 3)
public class Entity : MonoBehaviour
{
    private static bool entitiesInitialized;
    private static Dictionary<string, GameObject> entities = new Dictionary<string, GameObject>();

    [Serializable]
    public class EntityClass //Have all classes that hold references inherit this class so they can have other references in common (i.e. the containing Entity, etc.)
    {
        public Entity entity;
    }

    [Serializable]
    public class Components
    {
        public Animator anim;
        public Rigidbody rb;
        public ConstantForce cf;
        public AudioSource audioSource;
        public List<Collider> colliders = new List<Collider>();
        //public Renderer renderer;
        //public MeshFilter meshFilter;
    }

    [Serializable]
    public class ChildComponents
    {
        public List<Renderer> renderers = new List<Renderer>();
    }

    [Serializable]
    public class Sounds : EntityClass
    {
        public AudioSource audioSource;

        public string folder = "/";
        //private List<AudioClip> allAudioClips;
        private Dictionary<string, AudioClip> allAudioClips;

        private AudioClip footstepWalk;
        private AudioClip footstepRun;
        private List<AudioClip> vocalization;
        private AudioClip jump;
        private AudioClip land;
        private AudioClip attack;
        private List<AudioClip> hurt;
        public float minHurtRestTime = 0f;
        private float lastTimeHurt = 0;
        private float currentHurtClipLength = 0;
        private int lastHurtIndex = 0;

        private List<AudioClip> idle;
        public float minIdleRestTime = 3.5f;
        private float lastTimeIdle = 0;
        private float currentIdleClipLength = 0;
        private int lastIdleIndex = 0;

        private List<AudioClip> breatheIn;
        private List<AudioClip> breatheOut;

        public float volume = 1f;
        public bool muted;


        public Sounds()
        {

        }

        public Sounds(AudioSource source)
        {
            audioSource = source;
        }

        public void Initialize(Entity entity, AudioSource source)
        {
            this.entity = entity;
            audioSource = source;

            if(audioSource)
            {
                audioSource.volume = volume;
                audioSource.spatialBlend = 1f;
            }

            //Debug.LogError(audioSource.name + " Entity.sounds is muted: " + muted);

            allAudioClips = ArrayToDictionary(Resources.LoadAll<AudioClip>(folder));

            footstepWalk = Load("FootstepWalk");
            footstepRun = Load("FootstepRun");
            jump = Load("Jump");
            land = Load("Land");
            attack = Load("Attack");

            idle = PopulateList("Idle");
            hurt = PopulateList("Hurt");
            //breatheIn = PopulateList("BreatheIn");
            //breatheOut = PopulateList("BreatheOut");
            vocalization = PopulateList("Vocalization");

            ////////////////////If the Entity is not already muted, mute the Entity if none of the sounds/lists of sounds are available/set/etc. (This is here b/c if the Entity is not muted and has none of the sounds, Unity freezes)
            //////////////////if(!muted && !(footstepWalk || footstepRun || jump || land || attack || idle.Count > 0 || hurt.Count > 0 || vocalization.Count > 0))
            //////////////////    muted = true;
        }

        /*public void PlayOneShot(AudioClip clip)
        {
            audioSource.PlayOneShot(clip);

            Collider[] colliders = Physics.OverlapSphere(entity.transform.position, audioSource.maxDistance * audioSource.volume);
            ArrayList notifiedEntities = new ArrayList();

            foreach(Collider c in colliders)
            {
                Entity e = c.gameObject.GetComponent<Entity>();
                BodyPart b = c.gameObject.GetComponent<BodyPart>();
                Entity hitEntity = null;

                if(e && !e.Equals(entity))
                    hitEntity = e;
                else if(b && b.entity && !b.entity.Equals(entity))
                    hitEntity = b.entity;

                if(hitEntity && !notifiedEntities.Contains(hitEntity))
                {
                    notifiedEntities.Add(hitEntity);

                    if(hitEntity is Enemy)
                        ((Enemy)hitEntity).DrawAttentionTo(entity.transform.position);

                    Debug.LogError(hitEntity.gameObject.name + " heard that.");
                }
            }
        }*/

        public void PlayOneShot(AudioClip clip)
        {
            //World.PlayOneShot(audioSource, clip, entity.transform, entity);
        }

        public void Update()
        {
            if(!muted)
            {
                if(Time.time - lastTimeIdle >= currentIdleClipLength + minIdleRestTime + UnityEngine.Random.Range(0, 3))
                {
                    Idle();


                    lastTimeIdle = Time.time;
                }
            }
        }

        public void Play(string audioClipName)
        {
            if(!muted)
            {
                audioSource.PlayOneShot(allAudioClips[audioClipName]);
            }
        }

        public void FootStep(int speed)
        {
            if(!muted)
            {
                if(speed == 0 && footstepWalk)
                    audioSource.PlayOneShot(footstepWalk);
                else if(speed == 1 && footstepWalk /*&& footstepRun*/)
                    audioSource.PlayOneShot(footstepWalk); //Shouldn't this be footstepRun?
            }
        }

        public void Vocalize()
        {
            //Old
            if(!muted) audioSource.PlayOneShot(vocalization[Rand(vocalization)]);

            ////New
            //PlayRand(vocalization);
        }

        public void Attack()
        {
            if(!muted /*&& attack*/)
                audioSource.PlayOneShot(attack);
        }

        public void Idle()
        {
            if(!muted && idle.Count > 0)
            {
                int r = 0;
                do { r = Rand(idle); }
                while(r == lastIdleIndex);

                audioSource.PlayOneShot(idle[r]);

                currentIdleClipLength = idle[r].length;
                lastIdleIndex = r;
            }
        }

        //public void BreatheIn()
        //{
        //    //Old
        //    //if(!muted) audioSource.PlayOneShot(breatheIn[Rand(breatheIn)]);

        //    //New
        //    PlayRand(breatheIn);
        //}

        //public void BreatheOut()
        //{
        //    //Old
        //    //if(!muted) audioSource.PlayOneShot(breatheOut[Rand(breatheOut)]);

        //    //New
        //    PlayRand(breatheOut);
        //}1

        public void Hurt()
        {
            try
            {
                if(!muted && Time.time - lastTimeHurt >= currentHurtClipLength + minHurtRestTime)
                {
                    int r = 0;
                    do { r = Rand(hurt); }
                    while(r == lastHurtIndex);

                    audioSource.PlayOneShot(hurt[r]);

                    currentHurtClipLength = hurt[r].length;
                    lastHurtIndex = r;


                    lastTimeHurt = Time.time;
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        private void PlayRand(List<AudioClip> sounds)
        {
            if(!muted && sounds.Count > 0)
                audioSource.PlayOneShot(sounds[Mathf.RoundToInt(UnityEngine.Random.Range(0, sounds.Count))]);
        }

        private AudioClip Load(string fileName)
        {
            return Resources.Load<AudioClip>(folder + fileName);
        }

        private List<AudioClip> PopulateList(string name)
        {
            int i = 0;
            AudioClip currentSound = Load(name + i);
            List<AudioClip> sounds = new List<AudioClip>();

            while(currentSound)
            {
                sounds.Add(currentSound);
                i++;
                currentSound = Load(name + i);
            }

            return sounds;
        }

        private int Rand(List<AudioClip> sounds)
        {
            if(sounds.Count > 0)
                return Mathf.RoundToInt(UnityEngine.Random.Range(0, sounds.Count));
            else
                return -1;
        }

        private List<AudioClip> ArrayToList(AudioClip[] arr)
        {
            List<AudioClip> list = new List<AudioClip>();

            foreach(AudioClip a in arr)
                list.Add(a);

            return list;
        }

        private Dictionary<string, AudioClip> ArrayToDictionary(AudioClip[] arr)
        {
            Dictionary<string, AudioClip> dictionary = new Dictionary<string, AudioClip>();

            foreach(AudioClip a in arr)
                dictionary.Add(a.name, a);

            return dictionary;
        }
    }

    [Serializable]
    public class Properties //: EntityClass
    {
        [Tooltip("Back-end name; Name that should be used for animation state names or otherwise used in code/back-end.")]
        public string name;
        [Tooltip("Front-end name; Name that should be displayed to the player (often more formal, descriptive, or more characters).")]
        public string displayName;
        public Transform centerOfMass;
        public Vector3 pivot;
    }

    public Components components = new Components();
    public ChildComponents childComponents = new ChildComponents();
    public Sounds sounds = new Sounds();
    public Properties properties = new Properties();

    //public World world;
    public Body body;
    //public Hand leftHand;
    //public Hand rightHand;
    //public HUDManager hudManager;
    public GameObject networkModel;
    public GameObject localModel;
    //FINISH IMPLEMENTING/USING allyTypes and allyTypeNames (allyTypeNames is just used so you can see the names of the Types in the Inspector b/c it doesn't show it if it is a List of Type objs)
    public List<Type> allyTypes = new List<Type>(); //Use this for checking if any character is an enemy or an ally
    public List<string> allyTypeNames = new List<string>();

    public GameObject notifyOnDie;

    //GameMode Variables
    //public GameMode.Team team;
    //public GameMode.PlayerInfo playerInfo;


    public virtual void Initialize()
    {
        Awake();
    }

    public virtual void Awake()
    {
        GetComponents();
        GetChildComponents();
        sounds.Initialize(this, components.audioSource);

        //if(!world) world = FindObjectOfType<World>();

        if(!body) body = GetComponent<Body>();
        if(body)
        {
            //Debug.Log(name + "'s Body reference is set.");
            body.entity = this;
            body.Setup();
        }
        else
        {
            Debug.LogError(name + "'s Body reference is NOT set.");
        }

        if(components.anim) SetAnimatorReferences(components.anim);

        /* try
        {
            foreach(Transform child in GetComponentsInChildren<Transform>())
            {
                if((child.name.Contains("Left") || child.name.Contains("left")) && child.GetComponent<Hand>())
                {
                    leftHand = child.GetComponent<Hand>();
                }
                if((child.name.Contains("Right") || child.name.Contains("right")) && child.GetComponent<Hand>())
                {
                    rightHand = child.GetComponent<Hand>();
                }
            }
        }
        catch(Exception)
        {
            throw;
        } */
    } //Awake()

    public virtual void Update()
    {
        sounds.Update();
    }

    public virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sounds.audioSource.maxDistance * sounds.audioSource.volume);
    }

    /// <summary>
    /// Called by World on Awake()
    /// </summary>
    public static void InitializeEntities()
    {
        if(!entitiesInitialized)
        {
            entities.Add("Enemy", Resources.Load<GameObject>("Entity/AI/Enemy/StitchedZombieRagdoll Variant"));
            //entities.Add("Enemy", Resources.Load<GameObject>("Entity/AI/Enemy/StitchedZombieRagdoll Variant TEST")); //This Prefab has Body Component removed b/c it was setting RagdollBodyPart.Body to it instead of to the Ragdoll Component that was 
            //also attached; 


            entitiesInitialized = true;
        }
    }

    //public static GameObject SpawnEntity<T>(Vector3 position = new Vector3(), Quaternion rotation = new Quaternion())
    //{
    //    //Debug.LogError("Base Type: " + typeof(T).BaseType);

    //    GameObject entityPrefab = null;
    //    GameObject spawnedEntity = null;

    //    if(typeof(T) == typeof(Enemy))
    //    {
    //        //Spawn Enemy
    //        entities.TryGetValue("Enemy", out entityPrefab);
    //        if(entityPrefab)
    //        {
    //            spawnedEntity = Instantiate(entityPrefab, position, rotation);
    //            spawnedEntity.GetComponent<Entity>().Initialize();
    //        }
    //        else
    //        {
    //            Debug.LogError("Entity of type" + typeof(T).ToString() + " is not available");
    //        }

    //        return spawnedEntity;
    //    }
    //    else if(typeof(T) == typeof(Player))
    //    {
    //        //Spawn Player
    //    }
    //    //...
    //    else
    //    {
    //        //Not a kind of Entity
    //    }

    //    return null;
    //}

    public void Play(string audioClipName)
    {
        sounds.Play(audioClipName);
    }

    public void FootStep(int speed)
    {
        sounds.FootStep(speed);
    }

    public void AttackSound()
    {
        sounds.Attack();
    }

    public virtual void AddForce(Vector3 force)
    {
        //components.rb.velocity += force * components.rb.mass / world.gravity;
    }

    public virtual void AddForceTo(Rigidbody rb, Vector3 force)
    {
        //rb.velocity += force * rb.mass / world.gravity;
    }

    void GetComponents()
    {
        if(!components.anim) components.anim = GetComponent<Animator>();
        if(!components.rb) components.rb = GetComponent<Rigidbody>();
        if(!components.cf) components.cf = GetComponent<ConstantForce>();
        if(!components.audioSource) components.audioSource = GetComponent<AudioSource>();
        if(!components.audioSource) components.audioSource = gameObject.AddComponent<AudioSource>();
        //if(!components.renderer) components.renderer = GetComponent<Renderer>();
        //if(!components.meshFilter) components.meshFilter = GetComponent<MeshFilter>();
        GetColliders();
    }

    void GetChildComponents()
    {
        //if(childComponents.renderers.Count <= 0) //Uncomment if needed
        //{
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach(Renderer r in renderers)
            if(!r.GetComponent<GeneralExclusion>())
                childComponents.renderers.Add(r);
        //}
    }

    void GetColliders()
    {
        foreach(Collider col in GetComponents<Collider>())
        {
            if(!col.isTrigger)
            {
                components.colliders.Add(col);
            }
        }
    }

    public virtual void SetAnimatorReferences(Animator a)
    {
        //try
        //{
        //    //foreach(AttackingBehaviour behaviour in components.anim.GetBehaviours<AttackingBehaviour>())
        //    foreach(AttackingBehaviour behaviour in a.GetBehaviours<AttackingBehaviour>())
        //    {
        //        behaviour.entity = this;
        //    }

        //}
        //catch(Exception e)
        //{
        //    Debug.LogError(e.StackTrace);
        //}
    } //SetAnimatorReferences(Animator a)

    public virtual void DisableOnDie()
    {

    }

    //Game Methods
    //private void AddPoints(int points)
    //{
    //    team.AddPoints(points);
    //}

    //public void DamagedEntity(Entity e, float inflictedDamage)
    //{

    //}

    //public void DamagedEntity(Entity e)
    //{

    //}

    /*public void DebugDie() //Test of SendMessage() called from Body
    {
        Debug.Log(name + " died (called from Entity).");
    }*/

    //public virtual void OnPreDie()
    //{
    //    if(team != null)
    //        GameManager.instance.NotifyPlayerDeath(this);
    //}
} //Entity
