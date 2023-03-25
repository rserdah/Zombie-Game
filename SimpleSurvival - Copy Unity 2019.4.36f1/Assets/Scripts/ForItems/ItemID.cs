using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ItemID", order = 1)]
public class ItemID : ScriptableObject
{
    //public enum ItemType
    //{
    //    DEFAULT, GUN, GRENADE, AMMO, PART
    //}

    [System.Flags]
    public enum Item_Type
    {
        PART = 1 << 1,
        USABLE = 1 << 2,
        PART_AND_USABLE = PART | USABLE,
        AMMO = 1 << 3
    }

    public string itemName;
    public string itemDisplayName;
    public Item_Type item_type;
    public int ID;

    /// <summary>
    /// If an ItemID has a recipe, it will be considered craftable.
    /// </summary>
    [Tooltip("If an ItemID has a recipe, it will be considered craftable.")]
    public Recipe recipe;

    /// <summary>
    /// The biproducts of an ItemID are ItemIDs that are obtained from using this ItemID in certain ways (e.g. consuming a canned food will give off a biproduct of tin can which can be used in crafting as its own ItemID)
    /// </summary>
    [Tooltip("The biproducts of an ItemID are ItemIDs that are obtained from using this ItemID in certain ways (e.g. consuming a canned food will give off a biproduct of tin can which can be used in crafting as its own ItemID)")]
    public ItemID[] biproducts;

    public bool hasBiproducts
    {
        get
        {
            return biproducts.Length > 0;
        }
    }

    public bool isStackable = true;
    public int maxCount = 99;

    public int maxSecondaryCount;
}