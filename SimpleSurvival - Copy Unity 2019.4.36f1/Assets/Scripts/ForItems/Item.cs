using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour//, IUsableItem
{
    public static ItemID[] allItemIDs;
    public static Recipe[] allRecipes;

    [System.Serializable]
    public class Animations
    {
        public AnimationClip aim;
        public AnimationClip aimUse;
        public AnimationClip idle;
        public AnimationClip rechamber;
        public AnimationClip reload;
        public AnimationClip use;
        public AnimationClip swapEnd;
        public AnimationClip swapStart;
    }

    public enum UseMode
    {
        SEMIAUTOMATIC, AUTOMATIC, BURST
    }

    public enum UseType
    {
        BULLET, RAY, FLAME
    }

    /// <summary>
    /// This is only used when converting to an ItemID in Item.ConvertToItemID() (for setting the reference to the respective ItemID)
    /// </summary>
    [SerializeField]
    private string itemName;

    public ItemID itemID;

    public UseMode useMode;
    public UseType useType;

    public Animations animations = new Animations();

    public bool dontAutoSetPromptOnStart;
    public bool dontUpdatePromptOnUse;
    public string prompt = "";

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    //////public float u_damage { get { return 0; } set { } }

    //////public int u_usesLeft { get { return 0; } set { } }

    //////public int u_maxUses { get { return 0; } set { } }

    //////public int u_usesReserved { get { return 0; } set { } }

    //////public int u_maxUsesReserved { get { return 0; } set { } }

    //////public bool u_usesReplenished { get { return false; } }

    //////public bool u_hasRechamber { get { return false; } }
    //////public bool u_rechambered { get { return false; } set { } }
    //////public int u_rechamberRounds { get { return 0; } }
    //////public bool u_needsRechamber { get { return false; } }

    //////public UseMode u_useMode { get { return useMode; } set { } }

    //////public float u_lastTimeUsed { get { return 0; } set { } }

    //////public AnimationClip idle { get { return animations.idle; } }
    //////public AnimationClip aim { get { return animations.aim; } }
    //////public AnimationClip aimUse { get { return animations.aimUse; } }
    //////public AnimationClip reload { get { return animations.reload; } }
    //////public AnimationClip rechamber { get { return animations.rechamber; } }
    //////public AnimationClip use { get { return animations.use; } }
    //////public AnimationClip swapStart { get { return animations.swapStart; } }
    //////public AnimationClip swapEnd { get { return animations.swapEnd; } }

    //////public ItemID u_itemID { get { return itemID; } }

    //////public float u_useRate { get { return fireRate; } }

    //////public float u_reloadEndTime { get { return reloadEndTime; } }

    //////public bool u_canManualIncrementalReload { get { return canManualIncrementalReload; } set { canManualIncrementalReload = value; } }
    //////public bool u_willManualIncrementalReload { get { return willManualIncrementalReload; } set { willManualIncrementalReload = value; } }
    //////public float u_manualIncrementalReloadTime { get { return manualIncrementalReloadTime; } set { manualIncrementalReloadTime = value; } }

    //////public AudioClip u_shootClip { get { return shootClip; } }

    //////public ParticleSystem u_muzzleFlash { get { return muzzleFlash; } }

    //////public Animator u_anim { get { return anim; } }

    //////public bool u_hasRotatingBarrels { get { return hasRotatingBarrels; } }

    //////public BarrelRotate u_barrelRotate { get { return barrelRotate; } }

    //////public Vector3 u_shootOrigin { get { return shootOrigin; } }

    //////public Vector3 u_shootDirection { get { return shootDirection; } }

    //////public float u_cartridgeEjectDelay { get { return cartridgeEjectDelay; } }

    //////public float u_burstInterval { get { return burstInterval; } }

    //////public int u_burstCount { get { return burstCount; } }

    //////public ParticleSystem u_cartridgeEject { get { return null; } }


    //////public int u_IncrementalReload()
    //////{
    //////    return IncrementalReload();
    //////}

    //////public int u_Reload()
    //////{
    //////    return Reload();
    //////}

    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


    public virtual void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Item");

        if(!itemID)
            itemID = ConvertToItemID();

        if(!dontAutoSetPromptOnStart && itemID)
            prompt = "Use " + itemID.itemName;
    }

    public virtual int Reload()
    {
        return 0;
    }

    public virtual void Use(IItemUser iItemUser)
    {
        bool canUse2 = false;

        if(iItemUser != null && iItemUser.CanUse(this, out canUse2))
        {

        }
    }

    public ItemID ConvertToItemID()
    {
        if(!itemName.Equals(""))
            return (ItemID)Resources.Load("ItemIDs/" + itemName);
        else
            return null;
    }
}
