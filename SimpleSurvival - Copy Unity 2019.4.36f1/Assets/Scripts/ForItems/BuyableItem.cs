using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyableItem : /*Item*/ ItemDispenser
{
    //public int itemID;
    //public string itemName;
    //public string itemDisplayName;

    public enum BuyableItemType
    {
        GUN, AMMO
    }

    public bool hasCustomPrompt;
    public string customPrompt;

    public bool oneTimeUse;

    public BuyableItemType buyableItemType;

    //public int price = 10;
    public int secondaryPrice = 10;

    public bool replacesCurrentItemOfSameType;


    //public /*override*/ void Use(IItemUser iItemUser)
    public override void UseItemDispenser(IItemUser iItemUser)
    {
        base.Use(iItemUser);

        //Old
        //bool canUse2 = false;

        //if(iItemUser != null && iItemUser.CanUse(this, out canUse2))
        //{
        //    Debug.LogError("Bought " + itemName);

        //    iItemUser.BuyItem(price);
        //}
        //else if(canUse2)
        //{
        //    if(itemType == ItemType.GUN)
        //        Debug.LogError("Bought ammo for " + itemName);
        //}
        //else
        //{
        //    Debug.LogError("Cannot buy " + itemName);
        //}


        //New
        if(iItemUser != null && !iItemUser.GetItems().Contains(itemID.itemName)) //Change to iItemUser.HasInInventory() (create that method in the IItemUser interface) and/or get rid of this BuyableItem class and just make an ItemDispenser class (so get rid 
                                                                          //of BuyableItem and Pickup (b/c a pickup is essentially just a free ItemDispenser that gets destroyed on dispense)). The goal is to make Item the most general as possible so 
                                                                          //can easily add Items and different kinds of Items without having to add much more functionality manually
        {
            //Debug.LogError("Bought " + itemName);

            iItemUser.BuyItem(itemID.itemName, price);
        }
        else if(iItemUser != null/* && */) //&& check which item is the one that they have (item1, 2, etc.)
        {
            if(buyableItemType == BuyableItemType.GUN)
            {
                //Debug.LogError("Bought ammo for " + itemName);

                iItemUser.BuyItem(itemID.itemName, secondaryPrice);
            }
        }
        else
        {
            //Debug.LogError("Cannot buy " + itemName);
        }

        if(oneTimeUse)
            gameObject.SetActive(false);
    }

    public /*override*/ string GetPrimaryPrompt()
    {
        //return itemDisplayName + " $" + price;

        if(!hasCustomPrompt)
            return $"{itemID.itemDisplayName} ${price} [Ammo ${secondaryPrice}]";
        else
            return customPrompt;
    }

    //public override string GetSecondaryPrompt()
    //{
    //    return itemDisplayName + " $" + secondaryPrice;
    //}
}
