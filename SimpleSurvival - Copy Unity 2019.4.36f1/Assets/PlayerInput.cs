using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class PlayerInput : Controller, IItemUser, IScoreUser, Gun.HitInstigator
{
    public GameObject iGameObject { get => gameObject; } //Implementation of IItemUser.iGameObject

    public static class LayerMasks
    {
        private static bool initialized;

        public static int everything = ~0;
        public static int everythingButPlayer;
        public static int player;
        public static int obstacle;
        public static int bodyPart;
        public static int item;

        public static void Initialize()
        {
            if(!initialized)
            {
                player = 1 << LayerMask.NameToLayer("Player");
                obstacle = 1 << LayerMask.NameToLayer("Obstacle");
                bodyPart = 1 << LayerMask.NameToLayer("BodyPart");
                item = 1 << LayerMask.NameToLayer("Item");
                everythingButPlayer = everything & ~player;
                //Debug.LogError("everything: " + everything + "player: " + player + "~player:" + ~player + "everythingButPlayer: " + everythingButPlayer);


                initialized = true;
            }
        }
    }

    [System.Serializable]
    public struct HeadBobValues
    {
        public float horizontalBobRange;
        public float verticalBobRange;
        public float strideInterval;


        public HeadBobValues(float _horizontalBobRange, float _verticalBobRange, float _strideInterval)
        {
            horizontalBobRange = _horizontalBobRange;
            verticalBobRange = _verticalBobRange;
            strideInterval = _strideInterval;
        }
    }

    [System.Serializable]
    public struct HalfExtents
    {
        public Vector3 halfExtents;
        [SerializeField]
        private Vector3 m_origin;
        public Vector3 origin
        {
            get
            {
                if(parent)
                    return parent.TransformPoint(m_origin);
                else
                    return Vector3.zero;
                //transform.TransformPoint(halfExtents.origin)
            }
        }
        public Transform parent;
    }
    public Mesh cubeMesh;

    public Animator anim;
    public CharacterController characterController;
    public AudioSource audioSource;

    public CameraController cameraController;
    public AnimatorOverrideController animatorOverrideController;
    public AnimationClipOverrides clipOverrides;

    [System.Serializable]
    public enum KeyState
    {
        NONE,
        PRESSED,
        HELD
    }

    [System.Serializable]
    public struct InputStream
    {
        public float mouseX, mouseY, vertical, horizontal;
        public KeyState key_E;
        

        public void Stream()
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseX = Input.GetAxis("Mouse X");
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");

            key_E = GetKeyState(KeyCode.E);
        }

        private KeyState GetKeyState(KeyCode key)
        {
            if(Input.GetKeyDown(key))
                return KeyState.PRESSED;
            if(Input.GetKey(key))
                return KeyState.HELD;

            return KeyState.NONE;
        }

        //maybe have function DecodeInputStream(InputStream is) on each frame that calls functions on the Controllable from this script
    }

    //public InputStream inputStream;

    public float maxHealth = 100f;
    public float health = 100f;
    public float regainFromHealth = 100f;
    public bool isDead;
    public bool paused;
    public Animator deadAnim;
    public float lastTimeHit;
    public float healthRegainSpeed = 1f;
    public float healthRegainT;
    public float healthRegainDelay = 4f;

    private HeadBob headBob;
    public HeadBobValues walkHeadBob = new HeadBobValues(0.00025f, 0.0005f, 0.5f);
    public HeadBobValues runHeadBob = new HeadBobValues(0.00025f * 2f, 0.0005f * 2f, 0.5f * 1.5f);
    public HeadBobValues aimHeadBob = new HeadBobValues(0f, 0.0001f, 1f);

    public AudioSourcePlayer audioSourcePlayer;

    private float yRot;
    public float mouseRotSensitivity; //Same as CameraController sensitivity

    public float swayX;
    public Vector2 fovUnaimedAimed = new Vector2(60f, 40f);
    public float swaySpeed = 1;
    public float aimSpeed = 15f;
    public float aimRelaxSpeed = 7f;
    public float aimValue = 0f;
    public float aimInterpolator;
    public Camera camera;
    public CinemachineVirtualCamera vcam;
    public Camera scopeCamera;
    public Vector2 screenPt;
    public Transform sightDot;

    public Light flashLight;
    public Vector2 flashLightRangeUnaimedAimed = new Vector2(10, 150);
    public Vector2 flashLightAngleUnaimedAimed = new Vector2(70, 20);

    public float speed = 4f;
    public float walkSpeed = 4f;
    public float runSpeed = 6f;
    public float jumpHeight = 2f;

    public float gravity = -9.81f;
    public Vector3 velocity;

    public Vector3 groundCheckOrigin;
    public Vector3 groundCheckHalfExtents;

    public HalfExtents bodyHalfExtents;
    public Collider[] hitParts;
    public int lastHitColCount;
    public List<Collider> currentHitColliders = new List<Collider>();
    public List<BodyPart> currentAttackingParts = new List<BodyPart>();
    public List<BodyPart> currentHitParts = new List<BodyPart>();

    public HalfExtents selectHalfExtents; //Going to follow rotation of cameraController
    public Collider[] selectedItems;
    public Animator promptAnim;
    public Text promptText;

    public AudioClip incrementalReloadClip;
    public Vector2 minMaxShootPitch = Vector2.one;
    public Vector2 minMaxIncrementalReloadPitch = Vector2.one;
    public GameObject bulletHit;
    public AudioClip hitmarkerSound;
    public CanvasGroup hitMarker;

    public AudioClip gettHitSound;

    public bool isGrounded;
    public Vector3 groundedMovement;

    public bool isCrouching;
    public float standHeight = 1.75f;
    public float crouchHeight = 0.75f;
    public float proneHeight = 0.25f;
    IEnumerator crouchCoroutine;
    public float crouchTime = 0.75f;
    public AnimationCurve standToCrouchCurve;
    public AnimationCurve crouchToStandCurve;
    public float crouchSpeed = 3f;
    private float crouchCurveT;
    private float crouchInterpolator;


    public bool isRunning;

    public (float forward, float strafe) movementInput;

    private bool isOutOfBounds;

    public IUsableItem item;
    public bool useGunBoneBackward;
    public Transform gunBone;

    public Text ammoText;

    [SerializeField]
    private int m_score = 0;

    public int score
    {
        get
        {
            return m_score;
        }

        set
        {
            m_score = value;

            if(scoreText)
                scoreText.text = score + "";
        }
    }

    public Text scoreText;
    public VerticalLayoutGroup addedScoreLine;
    public bool addScoreTest;
    public int scoreAddedtest;

    public ScoreManager scoreManager;

    public Image outOfBoundsImage;

    public GameObject explosivePrefab;
    public Explosive currentExplosive;
    public int grenadesLeft = 4;
    public Transform explosivePostionStart;
    public float throwForce = 7000f;
    public Vector3 explosiveTorque;

    public GameObject bloodEffect;

    public IUsableItem item1;
    public IUsableItem item2;
    public Explosive item3;
    public Explosive item4;
    public List<string> items
    {
        get
        {
            List<string> list = new List<string>();

            list.Add(item1 != null ? item1.name : "");
            list.Add(item2 != null ? item2.name : "");
            list.Add(item3 ? item3.name : ""); //Eventually add name and displayName vars. to Explosive class (right now Exlposive.name is just Object.name so add a new public string name;)
            list.Add(item4 ? item4.name : ""); //Eventually add name and displayName vars. to Explosive class (right now Exlposive.name is just Object.name so add a new public string name;)

            return list;
        }
    }
    public int currentItemIndex;

    public /*Item[]*/Gun[] m_embeddedItems;
    public IUsableItem[] embeddedItems = null;
    public Dictionary<string, IUsableItem> itemDictionary = new Dictionary<string, IUsableItem>();

    public /*KeyValuePair<int, int>*/ List<string> inventoryOLD;
    public /*List<ItemID>*/ Dictionary<string, ItemID> inventory = new Dictionary<string, ItemID>();
    public List<int> inventoryCounts = new List<int>();
    (List<int> counts, List<string> names, List<ItemID> itemIDs) inventory2 = (new List<int>(), new List<string>(), new List<ItemID>());

    [Header("Gun Debugging")]
    public bool lotsOfAmmo;
    public bool lowDamage;
    public bool fullAuto;


    public ItemID itemIDTest;
    public GameObject inventoryScreen;
    public Dropdown inventoryDropdown;
    public Dropdown recipesDropdown;
    public Transform ingredientsLayoutGroup;
    public GameObject ingredientTextPrefab;
    public Color successGreen;
    public Color failureRed;

    public Gun testGun;

    public Controllable controllable;

    private InputManager inputManager;
    private InputData data;
    //This is the offset of the player local to the Transform of the Controller they are currently controlling as of the moment they started controlling it. It is used to determing player orientation 
    //after exitting/stopping control of that Controller (e.g. vehicle)
    public Vector3 localStandbyControlOffset;

    private List<Gun.QueuedHit> queuedHits = new List<Gun.QueuedHit>();


    private void Start()
    {
        LayerMasks.Initialize();

        Game.onPaused += TogglePause;

        headBob = GetComponent<HeadBob>();

        vcam = GetComponentInChildren<CinemachineVirtualCamera>();

        anim.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        item = testGun;
        item1 = testGun;

        if(item1 != null)
            AddToInventory(item1.u_itemID, 1);

        if(item2 != null)
            AddToInventory(item2.u_itemID, 1);

        if(item is Gun)
            UpdateAnimatorOverrideController((Gun)item);
        //!!!!!! Going to have to also handle this for melee weapons and other usable Items that have animations !!!!!!

        UpdateAmmo();

        if(embeddedItems == null)
        {
            embeddedItems = new IUsableItem[m_embeddedItems.Length];

            Debug.LogError("When ready, make Item implement IUsableItem so can make m_embeddedItems of type Item[] instead of Gun[].");

            for(int i = 0; i < m_embeddedItems.Length; i++)
            {
                embeddedItems[i] = m_embeddedItems[i];
            }
        }

        foreach(IUsableItem i in embeddedItems)
        {
            itemDictionary.Add(i.u_itemID.itemName, i);
        }

        //Update scoreText (scoreText is set when score is set to a value)
        score = score;

        Invoke("MakePerk", 2f);
        //Perk.CreatePerk(Perk.PerkType.DOUBLEPOINTS, position: transform.position + transform.right * 2f);
        //Perk.CreatePerk(position: transform.position + transform.right * 4f);
    }

    private void MakePerk()
    {
    }

    public override void Enable(InputManager manager)
    {
        gameObject.SetActive(true);
        if(vcam) vcam.Priority += 1;


        inputManager = manager;
        active = true;
    }

    public override void ReadInput(InputData data)
    {
        if(active)
            this.data = data;
    }

    public override void Disable()
    {
        if(vcam) vcam.Priority -= 1;
        gameObject.SetActive(false);


        active = false;
        newInput = false;
    }

    //!!! Don't change to FixedUpdate() or else switching weapons input becomes nonresponsive !!!
    private void Update()
    {
        if(!paused && active)
        {
            //inputStream.Stream(); //Old input system

            //For Gun debugging
            if(lotsOfAmmo && item != null)
                item.u_usesLeft = item.u_maxUses = item.u_usesReserved = item.u_maxUsesReserved = 99999999;
            if(lowDamage && item != null)
                item.u_damage = 0.00001f;
            if(fullAuto)
                item.u_useMode = Item.UseMode.AUTOMATIC;

            if(!isDead)
            {
                //----Looking----
                yRot += Input.GetAxis("Mouse X") * mouseRotSensitivity;
                transform.rotation = Quaternion.Euler(0, yRot, 0);

                swayX = Input.GetAxis("Mouse X") * Time.deltaTime * swaySpeed;
                anim.SetFloat("SwayX", swayX);
                //---------------

                if(Input.GetKeyDown(KeyCode.Escape))
                    Game.paused = true;

                //----Selecting Items----
                selectedItems = GetCollisions(selectHalfExtents, LayerMasks.item, selectHalfExtents.parent.rotation);

                if(selectedItems.Length > 0)
                {
                    //Item item = selectedItems[0].GetComponent<Item>(); //Temp.; should cache result and see if selecting the same thing instead of always calling GetComponent() when selecting an Item
                    ISelectable iSelectable = selectedItems[0].gameObject.GetComponent<ISelectable>(); //Temp.; should cache result and see if selecting the same thing instead of always calling GetComponent() when selecting an Item

                    if(iSelectable != null)
                    {
                        SetPromptText(iSelectable.prompt);

                        promptAnim.SetInteger("ShowHide", 1);

                        if(data.buttons[1].keyDown) //if(Input.GetKeyDown(KeyCode.E))
                        {
                            iSelectable.Use(this);
                            promptAnim.SetInteger("ShowHide", 0);
                        }
                    }
                    else
                    {
                        //Debug.LogError("Null");
                    }
                }
                else
                {
                    //Debug.LogError("Not selecting");
                    promptAnim.SetInteger("ShowHide", 0);
                }
                //-----------------------

                //----------Changing Weapons/Items-------------

                int dir = 0;
                int lastIndex = currentItemIndex;

                if(Input.mouseScrollDelta.y < 0)
                {
                    currentItemIndex++;
                    dir = 1;
                }
                else if(Input.mouseScrollDelta.y > 0)
                {
                    currentItemIndex--;
                    dir = -1;
                }

                if(Mathf.Abs(dir) > 0 && (item1 != null || item2 != null || item3 || item4)) //If scrolling AND have any Items to scroll
                {
                    currentItemIndex = MathHelper.LoopInt(currentItemIndex, items.Count);

                    while(items[currentItemIndex].Equals("")) //Skip to the next available Item if current one is null
                    {
                        currentItemIndex += dir * 1;
                        currentItemIndex = MathHelper.LoopInt(currentItemIndex, items.Count);
                    }

                    switch(lastIndex)
                    {
                        case 0:
                            item1.gameObject.SetActive(false);
                            break;
                        case 1:
                            item2.gameObject.SetActive(false);
                            break;
                        case 2:
                            item3.gameObject.SetActive(false);
                            break;
                        case 3:
                            item4.gameObject.SetActive(false);
                            break;
                    }

                    switch(currentItemIndex)
                    {
                        case 0:
                            item1.gameObject.SetActive(true);
                            item = item1;
                            break;
                        case 1:
                            item2.gameObject.SetActive(true);
                            item = item2;
                            break;
                        case 2:
                            item3.gameObject.SetActive(true);
                            break;
                        case 3:
                            item4.gameObject.SetActive(true);
                            break;
                    }

                    UpdateAnimatorOverrideController(item);

                    UpdateAmmo();
                }

                //-----------------------

                //----Moving----
                movementInput.forward = data.axes[0].input; //Input.GetAxis("Vertical");
                movementInput.strafe = data.axes[1].input; //Input.GetAxis("Horizontal");

                if(data.buttons[3].key) //if(Input.GetKey(KeyCode.LeftShift))
                {
                    if(!data.buttons[9].key) //if(!Input.GetKey(KeyCode.Mouse1))
                    {
                        SetHeadBobValues(runHeadBob);
                        speed = runSpeed;

                        //For the first frame player starts running, if they are crouched force them out of crouch;
                        //TODO: Implement a running while crouched feature (not quite as fast as standing running but faster than just walking while crouched to allow players to be stealthy but fast at certain times)
                        if(!isRunning)
                        {
                            if(isCrouching)
                            {
                                isCrouching = false;

                                StartCoroutine(Crouch());
                            }
                        }

                        isRunning = true;
                    }
                }
                else
                {
                    if(!data.buttons[9].key) //if (!Input.GetKey(KeyCode.Mouse1))
                        SetHeadBobValues(walkHeadBob);

                    speed = walkSpeed;
                    isRunning = false;
                }

                Vector3 move = transform.right * movementInput.strafe + transform.forward * movementInput.forward;
                groundedMovement = move;

                characterController.Move(move * Time.deltaTime * speed);
                //--------------

                //----Crouching----

                if(data.buttons[6].keyDown) //if (Input.GetKeyDown(KeyCode.C))
                {
                    isCrouching = !isCrouching;

                    StartCoroutine(Crouch());
                }
                else
                {
                    //characterController.height = standHeight;
                    //isCrouching = false;
                }

                //-----------------

                //----Jumping----
                if(data.buttons[0].keyDown && isGrounded) //if(Input.GetKeyDown(KeyCode.Space) && isGrounded)
                    velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);//Formula for jumping to a certain heigh: velocity = sqrt(height * -2 * gravity)
                                                                       //---------------

                //----Gravity----
                isGrounded = Physics.CheckBox(transform.position + groundCheckOrigin, groundCheckHalfExtents, new Quaternion(), LayerMasks.obstacle);

                if(isGrounded && velocity.y < 0)
                    velocity.y = -2f;

                velocity.y += gravity * Time.deltaTime;
                //Mutliply by Time.deltaTime again b/c deltaY = .5 * gravity * deltaTime ^ 2
                characterController.Move(velocity * Time.deltaTime);
                //---------------

                //----Shooting----
                if(data.buttons[9].key) //if(Input.GetKey(KeyCode.Mouse1))
                {
                    if(aimValue < 1f)
                        aimInterpolator += aimSpeed * Time.deltaTime;

                    anim.SetBool("IsAiming", true);

                    speed = walkSpeed;

                    SetHeadBobValues(aimHeadBob);

                    //anim.SetFloat("IdleType", aimValue); //Going to cache current IdleType float val. so if stops pressing aim before fully aiming, it can start lerping back to idle from where it currently is instead
                    //of snapping to aim and then going to idle
                }
                else
                {
                    if(aimValue > 0f)
                        aimInterpolator -= aimRelaxSpeed * Time.deltaTime;

                    anim.SetBool("IsAiming", false);

                    //anim.SetFloat("IdleType", Mathf.Lerp(0f, 1f, aimSpeed * Time.deltaTime));
                }

                aimInterpolator = Mathf.Clamp(aimInterpolator, -1f, 1f);
                aimValue = Mathf.Lerp(0f, 1f, aimInterpolator);
                anim.SetFloat("IdleType", aimValue);
                //aimFOV = Mathf.Lerp(40f, 60f, aimInterpolator);

                //camera.fieldOfView = Mathf.Lerp(fovUnaimedAimed.x, fovUnaimedAimed.y, aimInterpolator);
                vcam.m_Lens.FieldOfView = Mathf.Lerp(fovUnaimedAimed.x, fovUnaimedAimed.y, aimInterpolator);
                //Make the flashlight range further and smaller angle when aimed and wider and less range when not aimed
                flashLight.range = Mathf.Lerp(flashLightRangeUnaimedAimed.x, flashLightRangeUnaimedAimed.y, aimInterpolator);
                flashLight.spotAngle = Mathf.Lerp(flashLightAngleUnaimedAimed.x, flashLightAngleUnaimedAimed.y, aimInterpolator);

                //if((Input.GetKeyDown(KeyCode.Mouse0) && (item.u_useMode == Item.UseMode.SEMIAUTOMATIC || item.u_useMode == Item.UseMode.BURST)) || (Input.GetKey(KeyCode.Mouse0) && item.u_useMode == Item.UseMode.AUTOMATIC))
                if((data.buttons[8].keyDown            && (item.u_useMode == Item.UseMode.SEMIAUTOMATIC || item.u_useMode == Item.UseMode.BURST)) || (data.buttons[8].key          && item.u_useMode == Item.UseMode.AUTOMATIC))
                {
                    //If Gun doesn't have rechamber or doesn't need to rechamber now AND Gun has enough ammo
                    if((!item.u_hasRechamber || !item.u_needsRechamber) && item.u_usesLeft > 0)
                    {
                        if(item.u_useMode == Item.UseMode.BURST)
                        {
                            //For burst and auto gun shot, see (https://forum.unity.com/threads/coroutines-and-lag-low-fps.336689/) and search for comment containing "If you have precise, game-relevant mechanics, time them according to Fixed Timestep...."
                            StartCoroutine(BurstShot()); //temp


                            //IF GUN'S FIREMODE IS BURST, USE FIRERATE TO CHECK FOR WHEN THEY CAN START A NEW BURST SHOT
                        }
                        else if(Time.time - item.u_lastTimeUsed > 1 / item.u_useRate)
                        {
                            Debug.LogError("For burst and auto gun shot, see (https://forum.unity.com/threads/coroutines-and-lag-low-fps.336689/) and search for comment containing \"If you have precise, game - relevant mechanics, time them according to Fixed Timestep....\"");

                            item.u_rechambered = false;

                            if(anim.GetBool("IsAiming"))
                                anim.Play("AimShoot", 0, 0f);
                            else
                                anim.Play("Shoot", 0, 0f);
                            
                            PlayUseEffects();

                            if(item.u_useType == Item.UseType.BULLET)
                            {
                                Ray ray = new Ray();
                                RaycastHit hit;

                                ray.origin = item.u_shootOrigin;

                                if(useGunBoneBackward)
                                    ray.direction = -gunBone.forward;
                                else
                                    ray.direction = item.u_shootDirection;

                                //temp.
                                if(aimValue > 0)
                                {
                                    //ray = camera.ScreenPointToRay(camera.WorldToScreenPoint(sightDot.transform.position));
                                    ray.origin = sightDot.position;
                                    ray.direction = scopeCamera.transform.forward;
                                    //ray.direction = camera.transform.forward;
                                }

                                //////////////////////////if(Physics.Raycast(ray, out hit, 500f, LayerMasks.everythingButPlayer, QueryTriggerInteraction.Collide))
                                if(Physics.Raycast(ray, out hit, 500f, 1 << LayerMask.NameToLayer("BodyPart"), QueryTriggerInteraction.Collide))
                                {
                                    ProcessHit(hit);
                                }
                                else if(Physics.Raycast(ray, out hit, 500f, ~(1 << LayerMask.NameToLayer("BodyPart")), QueryTriggerInteraction.Ignore))
                                {
                                    ProcessHit(hit);
                                }
                            }
                            else if(item.u_useType == Item.UseType.RAY)
                            {
                                //Replace RayBlasterShooter with an actual Gun that is set up and has its FireType (and also UseType) set to RAY
                                //Then it can actually be its own weapon and not just a separate script
                                //Eventually going to have to make it work and damage in the same way as the code for UseType.BULLET so that it is consistent (i.e. you can still damage things, earn points,
                                //etc. by shooting a ray weapon as well (either make the ray handle that when it hits something and send back some info OR make it send back the hit info and make this
                                //script handle the actual damaging, adding score, etc.))

                                item.gameObject.GetComponent<Gun.QueuedHitSender>().SendHit(this);
                            }

                            item.u_usesLeft--;
                            item.u_lastTimeUsed = Time.time;

                            if(item.u_needsRechamber)
                            {
                                //if(/*rightGun.components.anim && */!rightGun.stats.needsRechamber2)
                                //    ((FirstPersonPlayer)player).rightArmAnim.Play("Interactable_Rechamber", 1, 0f);

                                //rightGun.stats.needsRechamber2 = true;

                                //StartCoroutine(Rechamber(rightGun));

                                Debug.LogError("Needs rechamber");
                                ////temp.
                                //if(gun.rechamberSound)
                                //    StartCoroutine(WaitThenRechamber());

                                //anim.Play("Rechamber", 0, 0f);
                                anim.SetTrigger("Rechamber");
                            }

                            UpdateAmmo();
                        }
                    }
                    else
                    {
                        //Play empty gun click
                    }
                }

                if(data.buttons[4].keyDown) //if(Input.GetKeyDown(KeyCode.R))
                {
                    if (item.u_hasRechamber && item.u_needsRechamber && item.u_canManualIncrementalReload)
                    {
                        item.u_willManualIncrementalReload = true;
                        Debug.LogError("Set Manual Incremental Reload");
                    }
                    else if (item.u_usesLeft < item.u_maxUses) //If not already full magazine
                    {
                        if (item.u_usesReserved > 0) //If have ammo to reload with
                        {
                            anim.Play("Reload");
                            item.u_anim.Play("Reload");
                            //anim.SetBool("IsReloading", true);

                            //Play reload animation (for guns w/o clips and need to be reloaded bullet-by-bullet, can interrupt reload process if need to shoot before completely done reloading)
                            //Decrement ammoReserved by the appropriate number
                        }
                    }
                }
                //----------------

                //Throwing Explosives
                if(data.buttons[10].key && grenadesLeft > 0) //if(Input.GetKey(KeyCode.Mouse2))
                {
                    if(!currentExplosive)
                    {
                        anim.Play("Grenade_StartThrow");

                        GameObject g = Instantiate(explosivePrefab);
                        currentExplosive = g.GetComponent<Explosive>();

                        g.transform.position = explosivePostionStart.position;
                        g.transform.parent = explosivePostionStart;
                        g.transform.forward = explosivePostionStart.forward;

                        currentExplosive.SetFuse();
                        grenadesLeft--;

                        Gun.QueuedHit hit = new Gun.QueuedHit(this);
                        queuedHits.Add(hit);
                    }
                }
                else if(data.buttons[10].keyUp) //if(Input.GetKeyUp(KeyCode.Mouse2))
                {
                    if(currentExplosive)
                    {
                        currentExplosive.transform.parent = null;

                        anim.Play("Grenade_EndThrow"); //This Animation has an AnimationEvent that will call ThrowExplosive() so that the grenade will be thrown
                    }
                }

                //-------------------


                //----Getting Hit----
                lastHitColCount = currentHitColliders.Count;
                currentHitColliders = ToList(GetCollisions(bodyHalfExtents, LayerMasks.bodyPart));

                BodyPart part = null;
                foreach(Collider col in currentHitColliders)
                {
                    part = col.GetComponent<BodyPart>();
                    if(part && part.body && part.body.isAttacking)
                    {
                        //currentAttackingParts.Add(part);
                        TakeDamage(part.body);
                        part.body.isAttacking = false;

                        audioSource.PlayOneShot(gettHitSound);
                    }
                }

                if(health < maxHealth && Time.time - lastTimeHit >= healthRegainDelay)
                {
                    health = Mathf.Lerp(regainFromHealth, maxHealth, healthRegainT);
                    healthRegainT += healthRegainSpeed * Time.deltaTime;
                }
                else
                {
                    healthRegainT = 0;
                }

                deadAnim.SetFloat("Damage", Mathf.Clamp01(1 - (health / maxHealth)));

                //if(lastHitColCount != currentHitColliders.Count)
                //{
                //    foreach(Collider col in currentHitColliders)
                //    {

                //    }
                //}
                //-------------------

            }

            hitMarker.alpha -= 0.5f * Time.deltaTime;

            if(addScoreTest)
            {
                GameObject g = Instantiate(addedScoreLine.transform.GetChild(0).gameObject);

                g.GetComponent<Text>().text = "+" + scoreAddedtest;
                if(Perk.doublePointsActive) g.GetComponent<Text>().color = successGreen;
                g.transform.parent = addedScoreLine.transform;


                addScoreTest = false;
            }
        }

        //if(Input.GetKeyDown(KeyCode.Escape))
        //{
        //    TogglePause();
        //}
        //else if(Input.GetKeyDown(KeyCode.Tab))

        if(data.buttons[2].keyDown) //if(Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePause();

            inventoryScreen.SetActive(paused);

            LoadInventoryScreen();
        }

        //DecodeInputStream(inputStream);
    }

    private void DecodeInputStream(InputStream input)
    {
        if(controllable)
        {
            if(input.key_E == KeyState.PRESSED)
                controllable.ControlEnd();
        }

    }

    private void TogglePause(bool paused)
    {
        TogglePause();

        cameraController.camera.GetComponent<PostProcessingHandler>().SetSaturation(paused ? -100f : -12.75f);
    }

    private void TogglePause()
    {
        paused = !paused;

        if(paused)
        {
            cameraController.Pause();
            GetComponent<CamRotate>().enabled = false;

            CheckItemsCraftable();
        }
        else
        {
            cameraController.Resume();
            GetComponent<CamRotate>().enabled = true;
        }
    }

    private void LoadInventoryScreen()
    {
        int currentValue = inventoryDropdown.value;
        inventoryDropdown.ClearOptions();
        inventoryDropdown.AddOptions(GetInventory());
        inventoryDropdown.value = currentValue;

        currentValue = recipesDropdown.value;
        recipesDropdown.ClearOptions();
        recipesDropdown.AddOptions(GetAllRecipes());
        recipesDropdown.value = currentValue;

        UpdateInventoryScreen();
    }

    private void UpdateInventoryScreen()
    {
        UpdateSelectedRecipeIngredients();
    }

    public void UpdateSelectedRecipeIngredients()
    {
        Text ingredientText;

        foreach(Transform t in ingredientsLayoutGroup)
            Destroy(t.gameObject);

        string ingredientName;
        int neededIngredientQuantity;
        int playerIngredientQuantity;
        int playerIngredientIndex;

        for(int i = 0; i < Item.allRecipes[recipesDropdown.value].ingredients.Length; i++)
        {
            ingredientName = Item.allRecipes[recipesDropdown.value].ingredients[i].itemName;
            neededIngredientQuantity = Item.allRecipes[recipesDropdown.value].quantities[i];
            playerIngredientIndex = inventory2.names.IndexOf(Item.allRecipes[recipesDropdown.value].ingredients[i].itemName);
            playerIngredientQuantity = playerIngredientIndex > -1 ? inventory2.counts[playerIngredientIndex] : 0;

            ingredientText = Instantiate(ingredientTextPrefab).GetComponent<Text>();

            ingredientText.transform.parent = ingredientsLayoutGroup;
            ingredientText.text = $"{ingredientName} ({playerIngredientQuantity} / {neededIngredientQuantity})";
            
            ingredientText.color = playerIngredientQuantity >= neededIngredientQuantity ? successGreen : failureRed;
        }
    }

    private void CheckItemsCraftable()
    {

    }

    /// <summary>
    /// Returns the amount of a given Recipe that this PlayerInput can craft, returns zero if the PlayerInput cannot craft any.
    /// </summary>
    /// <param name="recipe"></param>
    /// <returns></returns>
    private int AmountCraftable(Recipe recipe)
    {
        //Eventually make it so there is a panel next to the recipes dropdown where the player can see how much of each ingredient they have and how much of what ingredient they need, 
        //but for now just display the amount craftable for each recipe on the dropdown and display the recipe's ingredients next to it

        int amountCraftable = 0, ingredientRatio, quantityHeld, quantityNeeded;

        int[] ingredientIndices = FindIngredients(recipe);

        //ingredientIndices and recipe.quantities should have the same length b/c FindIngredients here is called with recipe
        if(ingredientIndices != null && ingredientIndices.Length > 0 && ingredientIndices.Length == recipe.quantities.Length) //check last part!!!!!
        {
            for(int i = 0; i < recipe.quantities.Length; i++)
            {
                quantityHeld = inventory2.counts[ingredientIndices[i]];
                quantityNeeded = recipe.quantities[i];

                ingredientRatio = quantityHeld / quantityNeeded;

                //On the first ingredient, need to set amountCraftable to = ingredientRatio in order to eventually find the least amountCraftable
                if(i == 0)
                    amountCraftable = ingredientRatio;

                if(ingredientRatio > 0)
                {
                    if(ingredientRatio < amountCraftable)
                        amountCraftable = ingredientRatio;
                }
                //If don't have enough of an ingredient, can't craft at all
                else
                {
                    return 0;
                }
            }
        }

        return amountCraftable;
    }

    private int AmountCraftable(ItemID itemID)
    {
        return AmountCraftable(itemID.recipe);
    }

    /// <summary>
    /// Attempts to find the indices of the required ingredients in this PlayerInput's inventory to craft the given Recipe. Returns null if not all ingredients are found.
    /// </summary>
    /// <param name="recipe"></param>
    /// <returns></returns>
    private int[] FindIngredients(Recipe recipe)
    {
        int[] indices = new int[recipe.ingredients.Length];
        int ingredientIndex;

        for(int j = 0; j < recipe.ingredients.Length; j++)
        {
            ingredientIndex = inventory2.names.IndexOf(recipe.ingredients[j].itemName);

            if(ingredientIndex > -1)
            {
                indices[j] = ingredientIndex;
            }
            else
            {
                return null;
            }
        }


        return indices;
    }

    private void Craft(Recipe recipe, int amount)
    {
        if(amount > 0 && recipe.hasItemID)
        {
            int amountCraftable = AmountCraftable(recipe);

            if(amountCraftable > 0)
            {
                Debug.LogError($"Can craft {recipe.GetItemID().itemName}");

                //If trying to craft more than the amount can craft, set the amount to the maximum amount craftable
                if(amount > amountCraftable)
                    amount = amountCraftable;

                //Use/Consume the ingredients from the inventory (delete them in the quantites from the given Recipe)
                for(int i = 0; i < recipe.ingredients.Length; i++)
                {
                    DeleteFromInventory(recipe.ingredients[i], amount * recipe.quantities[i]);
                }

                AddToInventory(recipe.GetItemID(), amount);


                LoadInventoryScreen();
            }
            else
            {
                Debug.LogError($"Cannot craft {recipe.GetItemID().itemName}");
            }
        }
    }

    private void Craft(Recipe recipe)
    {
        Craft(recipe, 1);

        //if(recipe.hasItemID)
        //{
        //    if(AmountCraftable(recipe) > 0)
        //    {
        //        Debug.LogError($"Can craft {recipe.GetItemID().itemName}");

        //        //Use/Consume the ingredients from the inventory (delete them in the quantites from the given Recipe)
        //        for(int i = 0; i < recipe.ingredients.Length; i++)
        //        {
        //            DeleteFromInventory(recipe.ingredients[i], recipe.quantities[i]);
        //        }

        //        AddToInventory(recipe.GetItemID());
        //    }
        //    else
        //    {
        //        Debug.LogError($"Cannot craft {recipe.GetItemID().itemName}");
        //    }
        //}

        //LoadInventoryScreen();
    }

    public void CraftCurrentRecipe()
    {
        Craft(Item.allRecipes[recipesDropdown.value]);
        //Debug.LogError(recipesDropdown.options[recipesDropdown.value].text);
        //Debug.LogError(recipesDropdown.value);
    }

    /// <summary>
    /// EVENTUALLY COMBINE THIS WITH PickupPartItem() (b/c this is currently copied from there)
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="amount"></param>
    public int AddToInventory(ItemID itemID, int amount = 1, int price = 0)
    {
        if(itemID)
        {
            //If it is a Usable (usables usually have prices), then BuyItem first before adding it to the inventory
            //MERGE THIS PART WITH AddToInventory SO IT IS BETTER
            //MERGE THIS PART WITH AddToInventory SO IT IS BETTER
            //MERGE THIS PART WITH AddToInventory SO IT IS BETTER
            switch(itemID.item_type)
            {
                case ItemID.Item_Type.PART:
                    break;

                case ItemID.Item_Type.USABLE:
                    BuyItem(itemID.itemName, price);
                    break;

                case ItemID.Item_Type.PART_AND_USABLE:
                    break;

                case ItemID.Item_Type.AMMO:
                    break;
            }

            bool hasItem, canPickupAll;

            int index = inventory2.names.IndexOf(itemID.itemName);
            int amountPickable;

            hasItem = index > -1;

            //If don't have the item, the amountPickable would be the maxCount, else if have the item, the amountPickable is the difference between maxCount and the amount held currently
            amountPickable = !hasItem ? itemID.maxCount : itemID.maxCount - inventory2.counts[index];

            canPickupAll = amountPickable >= amount;

            //If can pickup all of the item without going over the maxCount limit, then pick up all, else if can't pickup all, then partial pickup
            int amountTaken = canPickupAll ? amount : amountPickable;

            //If already have this ItemID, add the amountTaken
            if(hasItem)
                inventory2.counts[index] += amountTaken;
            //Else if don't have this ItemID, add the ItemID details and the amountTaken
            else
            {
                inventory2.counts.Add(amountTaken);
                inventory2.names.Add(itemID.itemName);
                inventory2.itemIDs.Add(itemID);
            }

            ////////PrintInventory();

            return amountTaken;
        }

        return 0;
    }

    private void DeleteFromInventory(ItemID itemID, int amount = 1)
    {
        int index = inventory2.names.IndexOf(itemID.itemName);

        //Remove the given amount of the ItemID from the inventory
        if(amount <= inventory2.counts[index])
            inventory2.counts[index] -= amount;

        //If the count of the ItemID is zero after removing the amount, remove the ItemID from the inventory
        if(inventory2.counts[index] == 0)
        {
            inventory2.counts.RemoveAt(index);
            inventory2.names.RemoveAt(index);
            inventory2.itemIDs.RemoveAt(index);
        }
    }

    private void PrintInventory()
    {
        string str = "";
        for(int i = 0; i < inventory2.counts.Count; i++)
        {
            str += $"({inventory2.itemIDs[i].itemName}, {inventory2.names[i]}, {inventory2.counts[i]})\n";
        }

        Debug.LogError(str);
    }

    private void tempShoot()
    {
        item.u_rechambered = false;

        if(anim.GetBool("IsAiming"))
            anim.Play("AimShoot", 0, 0f);
        else
            anim.Play("Shoot", 0, 0f);

        item.u_useEvent.Invoke();

        //if(item.u_shootClip)
        //    PlayOneShot(item.u_shootClip, minMaxShootPitch);

        //if(item.u_anim)
        //    item.u_anim.Play("Shoot", 0, 0f);

        Ray ray = new Ray();
        RaycastHit hit;
        ray.origin = item.u_shootOrigin;
        ray.direction = item.u_shootDirection;

        //temp.
        if(aimValue > 0)
        {
            //ray = camera.ScreenPointToRay(camera.WorldToScreenPoint(sightDot.transform.position));
            ray.origin = sightDot.position;
            ray.direction = scopeCamera.transform.forward;
            Debug.LogError("Setting");
            //ray.direction = camera.transform.forward;
        }

        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //Destroy(cube.GetComponent<Collider>());

        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //Destroy(sphere.GetComponent<Collider>());

        //if(Physics.Raycast(ray, out hit, 500f, LayerMasks.everything, QueryTriggerInteraction.Ignore))
        if(Physics.Raycast(ray, out hit, 500f, LayerMasks.everythingButPlayer, QueryTriggerInteraction.Ignore))
        {
            BodyPart hitBodyPart = hit.transform.GetComponent<BodyPart>();

            if(hitBodyPart && hitBodyPart.body)
            {
                hitBodyPart.TakeDamage(null, item.u_damage);
            }

            Transform hitEffect = Instantiate(bulletHit).transform;

            hitEffect.position = hit.point;
            hitEffect.forward = hit.normal;

            //cube.transform.forward = hit.point - gun.shootOrigin;
            //cube.transform.position = (hit.point + gun.shootOrigin) / 2f;
            //cube.transform.localScale = new Vector3(0.025f, 0.025f, (hit.point - gun.shootOrigin).magnitude);
            //cube.GetComponent<Renderer>().material.color = Color.green;

            //sphere.transform.position = hit.point;
            //sphere.transform.localScale = new Vector3(0.075f, 0.075f, 0.075f);
            //sphere.GetComponent<MeshRenderer>().material = null;

            //Debug.LogError("Hit " + hit.collider.name);
        }
        else
        {
            //cube.transform.forward = ray.direction;
            //cube.transform.position = ray.origin + ray.direction * 500f;
            //cube.transform.localScale = new Vector3(0.025f, 0.025f, 1000f);
            //cube.GetComponent<Renderer>().material.color = Color.red;
        }

        item.u_usesLeft--;
        item.u_lastTimeUsed = Time.time;

        if(item.u_needsRechamber)
        {
            //if(/*rightGun.components.anim && */!rightGun.stats.needsRechamber2)
            //    ((FirstPersonPlayer)player).rightArmAnim.Play("Interactable_Rechamber", 1, 0f);

            //rightGun.stats.needsRechamber2 = true;

            //StartCoroutine(Rechamber(rightGun));

            Debug.LogError("Needs rechamber");
            ////temp.
            //if(gun.rechamberSound)
            //    StartCoroutine(WaitThenRechamber());

            //anim.Play("Rechamber", 0, 0f);
            anim.SetTrigger("Rechamber");
        }

        UpdateAmmo();
    }

    public IEnumerator BurstShot()
    {
        int i = 0;

        while(true)
        {
            tempShoot();

            yield return new WaitForSeconds(item.u_burstInterval);

            i++;

            if(i >= item.u_burstCount)
                break;
        }
    }

    //public void CartridgeEject()
    //{
    //    if(item.u_cartridgeEject)
    //        item.u_cartridgeEject.Play();
    //}

    public void ProcessHit(Gun.QueuedHit hit)
    {
        //queuedHits.Remove(hit);

        Debug.LogError($"Queued hit from instigator {hit.instigator} landed");
    }

    private void ProcessHit(Gun.Hit hit)
    {

    }

    private void ProcessHit(RaycastHit hit)
    {
        BodyPart hitBodyPart = hit.transform.GetComponent<BodyPart>();
        Car c;

        if(hitBodyPart && hitBodyPart.body)
        {
            bool isDeadBeforeHit = hitBodyPart.body.isDead;
            bool isDeadAfterHit;
            bool isKillShot;
            hitBodyPart.TakeDamage(null, item.u_damage);

            isDeadAfterHit = hitBodyPart.body.isDead;

            isKillShot = !isDeadBeforeHit && isDeadAfterHit;

            audioSource.PlayOneShot(hitmarkerSound);
            hitMarker.alpha = 1f;

            int addedScore = GetHitScore(hitBodyPart.bodyPart, isKillShot) * (Perk.doublePointsActive ? 2 : 1);
            scoreManager.AddScore(addedScore);
        }
        else if(c = hit.collider.GetComponent<Car>())
        {
            c.stats.Damage(item.u_damage);
        }
        else if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Explosive"))
        {
            hit.transform.GetComponent<Explosive>().set = true; //temp.; way of setting off explosives from other classes
            Debug.LogError("Hit Explosive: " + hit.transform.name);
        }
        else if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Target"))
        {
            hit.collider.GetComponent<Target>().Hit(hit);
        }

        Transform hitEffect = Instantiate(bulletHit).transform;

        hitEffect.gameObject.SetActive(true);
        hitEffect.position = hit.point;
        hitEffect.forward = hit.normal;
    }

    public void PlayUseEffects()
    {
        //Using UnityEvents for these so don't have to have variables for each of them
                //Gun's AudioSource.Play([the clip])
                //Gun's muzzle flash ParticleSystem.Play()
                //Set Gun's cartridgeEjectDelay as the ParticleSystem.startDelay so can just call ParticleSystem.Play() and it will handle delay itself
                //Gun's Animator.Play([the shoot animation])
                //BarrelRotate.set (make method for it and make better name)

        item.u_useEvent.Invoke();

        //if(item.u_shootClip)
        //    PlayOneShot(item.u_shootClip, minMaxShootPitch);

        //if(item.u_muzzleFlash)
        //    item.u_muzzleFlash.Play();

        //if(!item.u_hasRechamber)
        //    Invoke("CartridgeEject", item.u_cartridgeEjectDelay);

        //if(item.u_hasRotatingBarrels)
        //    item.u_barrelRotate.set = true;

        if(item.u_anim)
            item.u_anim.Play("Shoot", 0, 0f);
    }

    public void CanManualRechamberStart()
    {
        if(item.u_hasRechamber && item.u_needsRechamber)
            item.u_canManualIncrementalReload = true;
    }

    public void CanManualRechamberEnd()
    {
        if(item.u_willManualIncrementalReload)
        {
            //IncrementalReload();
            Debug.LogError("Manually Incremental Reload");
            anim.Play("Reload", 0, item.u_manualIncrementalReloadTime);
        }

        item.u_willManualIncrementalReload = false;
        item.u_canManualIncrementalReload = false;
    }

    public void RechamberStart()
    {

    }

    public void RechamberEnd()
    {
        item.u_rechambered = true;
    }

    public void IncrementalReload()
    {
        //Need to check if gun.u_usesLeft < gun.magazineSize b/c there was glitch where on the last incremental reload, it will just continue to add bullets until ammoReserved == 0 (this glitch only happened when there were many Enemies spawned; this 
        //is possibly a glitch with AnimationEvents b/c there were so many Animators for each Enemy (which also used AnimationEvents)
        if(item.u_usesLeft < item.u_usesLeft && item.u_usesReserved > 0)
        {
            item.u_usesReserved -= item.u_IncrementalReload();

            PlayOneShot(incrementalReloadClip, minMaxIncrementalReloadPitch);

            if(item.u_usesLeft >= item.u_usesLeft || item.u_usesReserved <= 0)
            {
                //anim.SetBool("IsReloading", false); //If reloaded when ammoLeft != 0 and filled the magazine before reaching the end of the incremental reload animation, stop the animation to avoid overfilling the magazine
                anim.Play("Reload", 0, item.u_reloadEndTime); //Play reload animation at the end where character stops reloading
                item.u_anim.Play("ReloadEnd", 0, 0f);
            }
        }


        UpdateAmmo();
    }

    public void ReloadEnd()
    {
        item.u_Reload();

        //PlayOneShot(incrementalReloadClip, minMaxIncrementalReloadPitch);


        UpdateAmmo();
    }

    //public IEnumerator Rechamber(Gun gun)
    //{
    //    yield return new WaitForSeconds(gun.stats.rechamberTime);

    //    if(rightGun.effects.gunScreen)
    //        rightGun.effects.gunScreen.UpdateAmmoScreen(rightGun.stats.usesLeft);

    //    isRecovering = false;
    //    gun.stats.needsRechamber = false;
    //    gun.stats.rechambered = true;
    //    //((FirstPersonPlayer)player).rightArmAnim.SetBool("Rechamber", false);
    //}

    public void UpdateAmmo()
    {
        ammoText.text = $"{item.u_usesLeft} | {item.u_usesReserved}";
    }

    private void PlayOneShot(AudioClip audioClip, Vector2 minMaxPitch)
    {
        float pitch = Random.Range(minMaxPitch.x, minMaxPitch.y);

        audioSource.pitch = pitch;

        audioSource.PlayOneShot(audioClip);
    }

    public void PlayOneShot(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }

    private void SetHeadBobValues(HeadBobValues values)
    {
        headBob.motionBob.HorizontalBobRange = values.horizontalBobRange;
        headBob.motionBob.VerticalBobRange = values.verticalBobRange;
        headBob.SetStrideInterval(values.strideInterval);
    }

    private IEnumerator Crouch()
    {
        //Version 1: Only crouch
        //crouchT = 0f;

        //while(crouchT < crouchTime)
        //{
        //    float lastHeight = characterController.height;
        //    characterController.height = Mathf.Lerp(standHeight, crouchHeight, crouchT / crouchTime);
        //    characterController.center = new Vector3(0f, (characterController.height) / 2f, 0f);
        //    cameraController.transform.localPosition = Vector3.up * (characterController.height - 0.05f); //Make CameraController slightly lower than CharacterController height so that Camera cannot clip through a ceiling that CharacterController can 
        //                                                                                                  //just barely pass under for example

        //    crouchT += Time.deltaTime;
        //    yield return new WaitForFixedUpdate();
        //}

        //Version 2: Crouch and Stand
        //crouchT = 0f;

        //while(crouchT < crouchTime)
        //{
        //    if(isCrouching)
        //        characterController.height = Mathf.Lerp(standHeight, crouchHeight, crouchT / crouchTime);
        //    else
        //        characterController.height = Mathf.Lerp(crouchHeight, standHeight, crouchT / crouchTime);

        //    characterController.center = new Vector3(0f, (characterController.height) / 2f, 0f);
        //    cameraController.transform.localPosition = Vector3.up * (characterController.height - 0.05f); //Make CameraController slightly lower than CharacterController height so that Camera cannot clip through ceilings

        //    crouchT += Time.deltaTime;
        //    yield return new WaitForFixedUpdate();
        //}

        //Version 3: Crouch and stand based on AnimationCurve
        //Time-related variables; AnimationCurve TIME MUST BE [0, desiredCrouchDuration] AND VALUES MUST BE [0, 1]
        crouchInterpolator = crouchCurveT = 0f; //crouchInterpolator is the t used to lerp from standing to crouching/v.v. ; crouchCurveT is the time to evaluate the crouchCurve at (whose output (curve value) will determine crouchInterpolator)

        if(isCrouching)
        {
            while(crouchInterpolator < standToCrouchCurve[standToCrouchCurve.length - 1].time) //While evaluation time is less than curve length (which is the last key's time)
            {
                characterController.height = Mathf.Lerp(standHeight, crouchHeight, crouchInterpolator / standToCrouchCurve[standToCrouchCurve.length - 1].time);

                characterController.center = new Vector3(0f, (characterController.height) / 2f, 0f);
                cameraController.transform.localPosition = Vector3.up * (characterController.height - 0.05f); //Make CameraController slightly lower than CharacterController height so that Camera cannot clip through ceilings

                crouchCurveT += crouchSpeed * Time.deltaTime;
                crouchInterpolator = standToCrouchCurve.Evaluate(crouchCurveT);
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            while(crouchInterpolator < crouchToStandCurve[crouchToStandCurve.length - 1].time) //While evaluation time is less than curve length (which is the last key's time)
            {
                characterController.height = Mathf.Lerp(crouchHeight, standHeight, crouchInterpolator / crouchToStandCurve[crouchToStandCurve.length - 1].time);

                characterController.center = new Vector3(0f, (characterController.height) / 2f, 0f);
                cameraController.transform.localPosition = Vector3.up * (characterController.height - 0.05f); //Make CameraController slightly lower than CharacterController height so that Camera cannot clip through ceilings

                crouchCurveT += crouchSpeed * Time.deltaTime;
                crouchInterpolator = crouchToStandCurve.Evaluate(crouchCurveT);
                yield return new WaitForFixedUpdate();
            }
        }
    }

    //Items
    public bool CanUse(Item item, out bool canUseSecondary)
    {
        Debug.LogError("Don't use CanUse(Item item, out bool canUseSecondary)!!!!!!!!!!!!!!");

        /*int index = inventory2.names.IndexOf(item.itemID.itemName);
        bool hasItemID = index > -1;

        canUseSecondary = false;

        if(item is BuyableItem)
        {
            //Old
            //bool canUsePrimary = score >= ((BuyableItem)item).price && !inventoryOLD.Contains(((BuyableItem)item).itemName);

            //New (check if works)
            //bool canUsePrimary = score >= ((BuyableItem)item).price && !inventory2.names.Contains(((BuyableItem)item).itemName);

            //New 2 (check if works)
            bool canUsePrimary = score >= ((BuyableItem)item).price && !inventory2.names.Contains(((BuyableItem)item).itemID.itemName);

            canUseSecondary = !canUsePrimary; //If can use primary, can't use secondary & v.v.

            return canUsePrimary;
        }
        else if(!hasItemID || (hasItemID && inventory2.counts[index] < inventory2.itemIDs[index].maxCount))
            return true;*/
        canUseSecondary = false; //temp.

        return false;
    }

    public bool CanUse(ItemDispenser itemDispenser, out bool canUseSecondary)
    {
        bool hasItemID = inventory2.names.Contains(itemDispenser.itemID.itemName);

        canUseSecondary = false;
        
        bool canUsePrimary = score >= itemDispenser.price && !hasItemID;

        canUseSecondary = !canUsePrimary; //If can use primary, can't use secondary & v.v.

        return canUsePrimary;

        //------------------------------------------------------------------------------------------------
        //If can't use primary, then see if itemDispenser.dispensesMutuallyExclusiveItems is true, and if so, get the next ItemDispenser Component on the itemDispenser GameObject (see ItemDispenser for info about this)
        //Then set canUseSecondary accordingly OR get rid of this variable b/c ItemDispenser GameObject can have many ItemDispenser Components and is not limied to only two (so going to have to traverse through ItemDispenser Components until 
        //can find one that this player can use)
        //------------------------------------------------------------------------------------------------

        //////else if(!hasItemID || (hasItemID && inventory2.counts[index] < inventory2.itemIDs[index].maxCount))
        //////    return true;

        //////return false;
    }

    public void SetPromptText(string prompt)
    {
        if(promptText)
            promptText.text = prompt;
    }


    private void PrintInventoryItems()
    {
        string inventoryString = "Inventory: ";

        //foreach(string s in inventory.Keys)
        for(int i = 0; i < inventory2.counts.Count; i++)
        {
            inventoryString += $"{inventory2.names[i]} ({inventory2.counts[i]}), ";
        }

        Debug.LogError(inventoryString);
    }

    public void UseAmmoItem(int ammoCount)
    {
        item.u_usesReserved += ammoCount;

        UpdateAmmo();
    }

    //public bool PickupPartItem(Pickup pickupItem, int count)
    //{
    //    bool hasItem, canPickupAll;

    //    int index = inventory2.names.IndexOf(pickupItem.itemID.itemName);
    //    int amountPickable;

    //    hasItem = index > -1;

    //    if(hasItem)
    //        amountPickable = inventory2.itemIDs[index].maxCount - inventory2.counts[index]; //If have the item, the amountPickable is the difference between maxCount and the amount held currently
    //    else
    //        amountPickable = pickupItem.itemID.maxCount; //If don't have the item, the amountPickable would be the maxCount

    //    canPickupAll = amountPickable > count;

    //    //If already have this ItemID
    //    if(hasItem)
    //    {
    //        //If can pickup all of the item without going over the maxCount limit, then pick up all
    //        if(canPickupAll)
    //        {
    //            inventory2.counts[index] += count;


    //            return true;
    //        }
    //        //Else if can't pickup all, then partial pickup
    //        else
    //        {
    //            inventory2.counts[index] += amountPickable;
    //            pickupItem.count -= amountPickable;


    //            return false;
    //        }
    //    }
    //    //Else if don't have this ItemID
    //    else
    //    {
    //        //If can pickup all of the item without going over the maxCount limit, then pick up all
    //        if(canPickupAll)
    //        {
    //            inventory2.counts.Add(count);
    //            inventory2.names.Add(pickupItem.itemID.itemName);
    //            inventory2.itemIDs.Add(pickupItem.itemID);


    //            return true;
    //        }
    //        //Else if can't pickup all, then partial pickup
    //        else
    //        {
    //            inventory2.names.Add(pickupItem.itemID.itemName);
    //            inventory2.itemIDs.Add(pickupItem.itemID);

    //            inventory2.counts.Add(amountPickable);
    //            pickupItem.count -= amountPickable;


    //            return false;
    //        }
    //    }

    //    //PrintInventoryItems();
    //}

    public bool BuyItem(string itemName, int price)
    {
        if(score >= price)
        {
            if(PickUp(itemName))
            {
                score -= price;

                return true;
            }
        }

        return false;
    }

    public bool Pay(int price)
    {
        if(score >= price)
        {
            score -= price;
            return true;
        }

        return false;
    }

    public string GetItem1()
    {
        if(item1 != null)
            return item1.name;

        return "";
    }

    public string GetItem2()
    {
        if(item2 != null)
            return item2.name;

        return "";
    }

    public string GetItem3()
    {
        if(item3)
            return item3.name; //Eventually add name and displayName vars. to Explosive class (right now Exlposive.name is just Object.name so add a new public string name;)

        return "";
    }

    public string GetItem4()
    {
        if(item4)
            return item4.name; //Eventually add name and displayName vars. to Explosive class (right now Exlposive.name is just Object.name so add a new public string name;)

        return "";
    }

    public List<string> GetItems()
    {
        return items;
    }

    public bool PickUp(string itemName)
    {
        bool pickedUp = false;

        //If don't already have this Item
        if(!items.Contains(itemName))
        {
            IUsableItem newItem;
            itemDictionary.TryGetValue(itemName, out newItem);

            if(newItem != null)
            {
                if(currentItemIndex == 0)
                {
                    item1.gameObject.SetActive(false);

                    //If there is no item2, then move item1 to item2 before repacing item1 (so if you only have 1 item, you don't just replace it with the new one)
                    if(item2 == null)
                        item2 = item1;

                    item1 = newItem;
                    item = item1;
                }
                else if(currentItemIndex == 1)
                {
                    item2.gameObject.SetActive(false);

                    //If there is no item2, then move item1 to item2 before repacing item1 (so if you only have 1 item, you don't just replace it with the new one)
                    if(item1 == null)
                        item1 = item2;

                    item2 = newItem;
                    item = item2;
                }

                item.gameObject.SetActive(true);

                pickedUp = true;
            }
        }
        //Else (already have this Item) if ammo is not replenished
        else if(item is Gun && !((Gun)item).ammoReplenished)
        {
            ((Gun)item).ReplenishAmmo();

            pickedUp = true;
        }

        UpdateAnimatorOverrideController((Gun)item);

        UpdateAmmo();

        return pickedUp;
    }

    public /*KeyValuePair<int, int>*/ List<string> GetInventory()
    {
        //Old
        //return inventoryOLD;

        //New
        //List<string> inventoryList = new List<string>();
        //foreach(string s in inventory.Keys)
        //    inventoryList.Add(s);

        //return inventoryList;

        //New2
        return FormattedInventory();
    }

    private List<string> GetAllRecipes()
    {
        List<string> formattedRecipes = new List<string>();

        foreach(Recipe r in Item.allRecipes)
        {
            formattedRecipes.Add($"{r.GetItemID().itemName} ({AmountCraftable(r)})");
        }


        return formattedRecipes;
    }

    private List<string> FormattedInventory()
    {
        List<string> formattedInventory = new List<string>();

        for(int i = 0; i < inventory2.counts.Count; i++)
        {
            formattedInventory.Add($"{inventory2.names[i]} ({inventory2.counts[i]})");
        }


        return formattedInventory;
    }

    private Collider[] GetCollisions(HalfExtents halfExtents, int layerMask, Quaternion rotation = new Quaternion())
    {
        return Physics.OverlapBox(halfExtents.origin, halfExtents.halfExtents, rotation, layerMask, QueryTriggerInteraction.Collide);
        //return Physics.OverlapBox(transform.position + halfExtents.m_origin, halfExtents.halfExtents, transform.rotation, layerMask, QueryTriggerInteraction.Collide);
    }

    private List<T> ToList<T>(T[] array)
    {
        List<T> list = new List<T>();

        foreach(T t in array)
            list.Add(t);


        return list;
    }

    public bool IsOutOfBounds()
    {
        return isOutOfBounds;
    }

    public void NotifyOutOfBounds(bool outOfBounds)
    {
        //If just went out of bounds (if was not out of bounds and is now out of bounds)
        if(!isOutOfBounds && outOfBounds)
            OnOutOfBounds();

        //If just went back into bounds (if was out of bounds and is now in bounds)
        else if(isOutOfBounds && !outOfBounds)
            OnInBounds();
        
        isOutOfBounds = outOfBounds;
    }

    private void OnOutOfBounds()
    {
        outOfBoundsImage.gameObject.SetActive(true);
    }

    private void OnInBounds()
    {
        outOfBoundsImage.gameObject.SetActive(false);
    }

    /*private*/
    public void TakeDamage(Body body)
    {
        if(health > 0)
        {
            health -= body.fightingSkills.strength;

            lastTimeHit = Time.time;
            regainFromHealth = health;
            healthRegainT = 0;

            //deadAnim.SetFloat("Damage", Mathf.Clamp01(1 - (health / maxHealth)));

            if(health <= 0)
                Die();
        }
    }

    public void TakeDamage(Explosive explosive)
    {
        if(health > 0)
        {
            health -= explosive.damage;

            lastTimeHit = Time.time;
            regainFromHealth = health;
            healthRegainT = 0;

            if(health <= 0)
                Die();
        }
    }

    private void Die()
    {
        if(!isDead)
        {
            Enemy.playerIsDead = true;

            //deadAnim.Play("Die_TypewriterStyle", 0, -1f);
            deadAnim.Play("Die_TypewriterStyle");


            isDead = true;
        }
    }

    private int GetHitScore(HumanBodyBones hitBodyPart, bool isKillShot)
    {
        int score = 10;

        if(hitBodyPart == HumanBodyBones.Head && isKillShot)
            score *= 10;

        //Debug.LogError("isKillShot = " + isKillShot);

        return score;
    }

    private void DrawHalfExtents(HalfExtents halfExtents, Color color)
    {
        Gizmos.color = color;
        if(halfExtents.parent)
            Gizmos.DrawWireMesh(cubeMesh, 0, halfExtents.origin, halfExtents.parent.rotation, halfExtents.halfExtents * 2);

        //if(halfExtents.parent)
        //    Gizmos.DrawWireMesh(cubeMesh, 0, halfExtents.parent.TransformPoint(halfExtents.origin), halfExtents.parent.rotation, halfExtents.halfExtents * 2);
        //else
        //    Gizmos.DrawWireMesh(cubeMesh, 0, transform.TransformPoint(halfExtents.origin), transform.rotation, halfExtents.halfExtents * 2);
    }

    //For Guns
    public void UpdateAnimatorOverrideController(IUsableItem i)
    {
        if(i is Gun)
        {
            Gun g = (Gun)i;

            clipOverrides[/*name + "_*/"Idle"] = g.animations.idle;
            clipOverrides[/*name + "_*/"Aim"] = g.animations.aim;
            clipOverrides[/*name + "_*/"AimShoot"] = g.animations.aimUse;
            clipOverrides[/*name + "_*/"Reload"] = g.animations.reload;
            clipOverrides[/*name + "_*/"Rechamber"] = g.animations.rechamber;
            clipOverrides[/*name + "_*/"Shoot"] = g.animations.use;
            clipOverrides[/*name + "_*/"SwapStart"] = g.animations.swapStart;
            clipOverrides[/*name + "_*/"SwapEnd"] = g.animations.swapEnd;
        }
        else
        {
            clipOverrides[/*name + "_*/"Idle"] = i.idle;
            clipOverrides[/*name + "_*/"Aim"] = i.aim;
            clipOverrides[/*name + "_*/"AimShoot"] = i.aimUse;
            clipOverrides[/*name + "_*/"Reload"] = i.reload;
            clipOverrides[/*name + "_*/"Rechamber"] = i.rechamber;
            clipOverrides[/*name + "_*/"Shoot"] = i.use;
            clipOverrides[/*name + "_*/"SwapStart"] = i.swapStart;
            clipOverrides[/*name + "_*/"SwapEnd"] = i.swapEnd;
        }

        animatorOverrideController.ApplyOverrides(clipOverrides);
    }

    //Sounds & Animations
    public void ShowGun()
    {
        if(item != null)
            item.gameObject.SetActive(true);
    }

    public void HideGun()
    {
        if(item != null)
        {
            item.gameObject.SetActive(false);
        }
    }

    public void GunRechamber()
    {
        Debug.LogError("GunRechamber()");

        item.u_anim.Play("Rechamber2", 0, 0f);
    }

    public void RechamberStartSound()
    {
        audioSourcePlayer.Play("RechamberStartSound");
    }

    public void RechamberEndSound()
    {
        audioSourcePlayer.Play("RechamberEndSound");
    }

    public void ThrowExplosive()
    {
        if(currentExplosive)
        {
            currentExplosive.transform.parent = null;

            currentExplosive.rb.isKinematic = false;
            currentExplosive.rb.AddForce(camera.transform.forward * throwForce);
            //Old
            //This doesn't work great b/c depending on when the Explosive is thrown, the rotation of the Explosive affects the relative torque and so it sometimes doesn't look like the character threw it realistically
            //currentExplosive.rb.AddRelativeTorque(explosiveTorque);

            //New
            //currentExplosive.rb.AddTorque(camera.transform.right * explosiveTorque.magnitude); //Temp.; going to make torque into a float
            currentExplosive.rb.AddRelativeTorque(explosiveTorque);

            currentExplosive = null;
        }
    }

    private void OnDrawGizmos/*Selected*/()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + groundCheckOrigin, groundCheckHalfExtents * 2);

        DrawHalfExtents(bodyHalfExtents, Color.red);
        DrawHalfExtents(selectHalfExtents, Color.blue);
    }

    public int GetScore()
    {
        return score;
    }

    public void AddScore(int addedScore)
    {
        score += addedScore;
    }
}
