using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Recipe", order = 1)]
public class Recipe : ScriptableObject
{
    //Each craftable ItemID will have a reference to a Recipe, so Recipe only needs references to the ingredient ItemIDs and their respective quantities

    public ItemID[] ingredients;
    public int[] quantities;

    public bool hasItemID
    {
        get
        {
            return itemID;
        }
    }

    /// <summary>
    /// The ItemID that this Recipe crafts
    /// </summary>
    private ItemID itemID;


    public void SetItemID(ItemID itemID)
    {
        if(itemID && itemID.recipe.Equals(this))
            this.itemID = itemID;
    }

    public ItemID GetItemID()
    {
        return itemID;
    }
}