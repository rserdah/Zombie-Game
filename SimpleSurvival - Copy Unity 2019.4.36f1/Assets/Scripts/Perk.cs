using System.Collections;
using UnityEngine;

public class Perk : MonoBehaviour
{
    protected static string resourcesFolder = "GameMode/HordeMode/Perks";
    protected static GameObject perkPrefab;
    protected static Perk instance;
    protected static Transform t_perkdroptags;

    //Perk states

    public class PerkTimer
    {
        public bool active { get; protected set; }
        public float timer { get; protected set; }
        public Coroutine coroutine { get; protected set; }

        private Transform tag;
        private Transform tagFlasher;

        public PerkTimer(float time, Transform tag = null, Transform tagFlasher = null)
        {
            timer = time;
            this.tag = tag;
            this.tagFlasher = tagFlasher;
            coroutine = instance.StartCoroutine(PerkTimerCoroutine());
        }

        /// <summary>
        /// Creates (if renewTimer is null) or renews a timer (if renewTimer is not null). The tag Transform is the holder for the perk drop tag that can hide the whole perk tag and allow other tags 
        /// to take its place (the HorizontalLayoutGroup takes care of that), and the tagFlasher Transform is the Transform that allows the perk tag to flash (without shifting the other tags because
        /// if the whole transform is disabled, then the HorizontalLayoutGroup would shift all tags even though we just want to flash the tag)
        /// </summary>
        /// <param name="time"></param>
        /// <param name="renewTimer"></param>
        /// <param name="tag"></param>
        /// <param name="tagFlasher"></param>
        /// <returns></returns>
        public static PerkTimer CreatePerkTimer(float time, PerkTimer renewTimer = null, Transform tag = null, Transform tagFlasher = null)
        {
            if(renewTimer != null)
            {
                instance.StopCoroutine(renewTimer.coroutine);
                renewTimer.timer = time;
                renewTimer.coroutine = instance.StartCoroutine(renewTimer.PerkTimerCoroutine());
                if(tag) tag.gameObject.SetActive(true);
                if(tagFlasher) tagFlasher.gameObject.SetActive(true);

                return renewTimer;
            }


            PerkTimer p = new PerkTimer(time, tag, tagFlasher);

            ///////////Debug.LogError($"{nameof(p)}'s tag: {(p.tag != null ? p.tag.name : "{none}")}; flasher: {(p.tag != null ? p.tagFlasher.name : "{none}")}");
            
            return p;
        }

        private IEnumerator PerkTimerCoroutine()
        {
            active = true;
            if(tag) tag.gameObject.SetActive(true);
            if(tagFlasher) tagFlasher.gameObject.SetActive(true);

            while(timer >= 0f)
            {
                yield return new WaitForFixedUpdate();
                timer -= Time.fixedDeltaTime;
            }

            active = false;
            if(tag) tag.gameObject.SetActive(false);

            yield return null;
        }
    }

    //public static bool cola { get; protected set; }
    //public static bool instaKill { get; protected set; }
    ////On update of enemies, check if Perk.nuke, and if so set a timer with small random delay (for variety) to die after timer ends (may also need a bool
    ////to check if already set the timer so dont set it multiple times while nuke is true, also need some way of telling when either the last enemy died OR have a timer for nuke to turn itself off)
    //public static bool nuke { get; protected set; }
    //public static bool maxAmmo { get; protected set; }

    //protected static float instaKillTimer;
    //protected static float nukeTimer;
    //protected static float doublePointsTimer;


    protected static PerkTimer cola { get; set; }
    protected static PerkTimer instaKill { get; set; }
    //On update of enemies, check if Perk.nuke, and if so set a timer with small random delay (for variety) to die after timer ends (may also need a bool
    //to check if already set the timer so dont set it multiple times while nuke is true, also need some way of telling when either the last enemy died OR have a timer for nuke to turn itself off)
    protected static PerkTimer nuke { get; set; }
    protected static PerkTimer maxAmmo { get; set; }
    protected static PerkTimer doublePoints { get; set; }

    public static bool colaActive { get => cola != null && cola.active; }
    public static bool instaKillActive { get => instaKill != null && instaKill.active; }
    public static bool nukeActive { get => nuke != null && nuke.active; }
    public static bool maxAmmoActive { get => maxAmmo != null && maxAmmo.active; }
    public static bool doublePointsActive { get => doublePoints != null && doublePoints.active; }


    private static float rotSpeed = 45f;
    private static float amplitude = 0.1f;
    private static float frequency = 1f;
    /// <summary>
    /// The height above ground that the Perk should be floating (this should make startPos = {groundHeight} + floatHeight where groundHeight is the height of the ground where the Perk was spawned)
    /// </summary>
    private static float floatHeight = 1.3f;

    private Vector3 startPos;
    private const float mult = 4f;
    private Transform t_perkicon;
    private ParticleSystem ps_perkflash;


    public enum PerkType
    {
        NONE, //NONE must always be first
        COLA, 
        INSTAKILL, 
        NUKE, 
        MAXAMMO,
        DOUBLEPOINTS,
        

        RANDOM //RANDOM must always be last; Also, use the auto-numbering and don't set any values manually
    }

    [SerializeField]
    private PerkType m_perkType = PerkType.RANDOM;
    public PerkType perkType { get => m_perkType; set { if(m_perkType != PerkType.NONE && m_perkType != PerkType.RANDOM) lastPerkType = m_perkType; m_perkType = value; SetPerkIcon(); } }
    private PerkType lastPerkType = PerkType.NONE;

    public float lifeTime = 5f;
    public bool rand;

    private bool initialized;


    protected static void staticpreinit()
    {
        if(!instance) instance = new GameObject("perkinstance").AddComponent<Perk>();

        if(!perkPrefab) perkPrefab = Resources.Load<GameObject>($"{resourcesFolder}/Prefabs/perk");

        if(!t_perkdroptags) t_perkdroptags = TransformHelper.FindRecursive(UI.instance.transform, "t_perkdroptags");

        doublePoints = PerkTimer.CreatePerkTimer(15f, doublePoints);
    }

    public void preinit()
    {
        t_perkicon = TransformHelper.FindRecursive(transform, "t_perkicon");
        ps_perkflash = TransformHelper.FindRecursive(transform, "ps_perkflash").GetComponent<ParticleSystem>();
    }

    public void init()
    {
        staticpreinit();


        float rayDist = 2f;
        Ray ray = new Ray(transform.position + Vector3.up * rayDist / 2f, Vector3.down);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, rayDist, ~0, QueryTriggerInteraction.Ignore))
            startPos = hit.point + Vector3.up * floatHeight;
        else
            startPos = transform.position;

        SetPerkIcon();
    }

    private void Update()
    {
        transform.position = startPos + Vector3.up * (amplitude * Mathf.Sin(frequency * Time.time));
        transform.eulerAngles += Vector3.up * Time.deltaTime * rotSpeed;

        if(rand)
        {
            perkType = PerkType.RANDOM;
            rand = false;
        }
    }

    public static GameObject CreatePerk(PerkType? type = null, Vector3? position = null)
    {
        staticpreinit();

        GameObject g = Instantiate(perkPrefab);
        Perk p = g.GetComponent<Perk>();

        p.preinit();

        if(type != null) p.perkType = (PerkType)type;
        if(position != null) g.transform.position = (Vector3)position;

        p.init();

        return g;
    }

    private void SetPerkIcon()
    {
        if(perkType == PerkType.RANDOM)
            perkType = GetRandomPerk(lastPerkType);

        for(int i = 0; i < t_perkicon.childCount; i++)
            t_perkicon.GetChild(i).gameObject.SetActive(i == (int)perkType);
    }

    /// <summary>
    /// Pass in a PerkType to exclude if want to not land on the same type after randomizing the PerkType. Else leave default param as null 
    /// if does not matter if lands on the same type as before randomizing.
    /// </summary>
    /// <param name="exclude"></param>
    /// <returns></returns>
    private PerkType GetRandomPerk(PerkType? exclude = null)
    {
        PerkType randType;

        do
        {
            //Start at 1 to exclude PerkType.NONE & end at PerkType.RANDOM to exclude PerkType.RANDOM (Random.Range() for int's is exclusive max)
            randType = (PerkType)Random.Range(1, (int)PerkType.RANDOM);
            if(exclude == null) break;
        }
        while(exclude == randType);

        return randType;
    }
    
    public IEnumerator Blink()
    {
        GameObject g = transform.GetChild(0).gameObject;
        GameObject g1 = transform.GetChild(1).gameObject;

        yield return new WaitForSeconds(lifeTime);

        float blinkTime = 3f;
        float blinkDivide = 1.05f;

        while(blinkTime >= 0.01f)
        {
            g.SetActive(!g.activeSelf);
            g1.SetActive(!g1.activeSelf);

            yield return new WaitForSeconds(blinkTime);
            blinkTime /= blinkDivide;
            blinkDivide += 0.05f;
        }

        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            SwitchPerkType(other.gameObject);

            Destroy(gameObject);
        }
    }

    private void SwitchPerkType(GameObject g)
    {
        Transform tag, tagFlasher;

        switch(perkType)
        {
            case PerkType.COLA:
                GetPerkTagTransforms("t_cola", out tag, out tagFlasher);
                cola = PerkTimer.CreatePerkTimer(3, cola, tag, tagFlasher);
                g.GetComponentInChildren<Animator>().SetFloat("ReloadSpeed", mult);
                break;

            case PerkType.INSTAKILL:
                GetPerkTagTransforms("t_instakill", out tag, out tagFlasher);
                instaKill = PerkTimer.CreatePerkTimer(15f, instaKill, tag, tagFlasher);
                break;

            case PerkType.NUKE:
                GetPerkTagTransforms("t_nuke", out tag, out tagFlasher);
                nuke = PerkTimer.CreatePerkTimer(0.1f, nuke, tag, tagFlasher);
                break;

            case PerkType.MAXAMMO:
                GetPerkTagTransforms("t_maxammo", out tag, out tagFlasher);
                maxAmmo = PerkTimer.CreatePerkTimer(1f, maxAmmo, tag, tagFlasher);
                PlayerInput p = g.GetComponent<PlayerInput>();
                if(p.item != null) ((Gun)p.item).ReplenishReservedAmmo();
                if(p.item1 != null) ((Gun)p.item1).ReplenishReservedAmmo();
                if(p.item2 != null) ((Gun)p.item2).ReplenishReservedAmmo();
                p.grenadesLeft = 4;
                p.UpdateAmmo();
                break;

            case PerkType.DOUBLEPOINTS:
                GetPerkTagTransforms("t_doublepoints", out tag, out tagFlasher);
                doublePoints = PerkTimer.CreatePerkTimer(15f, doublePoints, tag, tagFlasher);
                break;

            //If it somehow stayed on PerkType.RANDOM without randomizing itself, force randomization and re-call this function
            case PerkType.RANDOM:
                perkType = PerkType.RANDOM;
                SwitchPerkType(g);
                break;

            case PerkType.NONE:
            default:
                break;
        }
    }

    private static void GetPerkTagTransforms(string tagName, out Transform tag, out Transform tagFlasher)
    {
        tag = TransformHelper.FindRecursive(t_perkdroptags, tagName);
        if(tag) tagFlasher = TransformHelper.FindRecursive(tag, "t_flasher");
        else tagFlasher = null;
    }
}
