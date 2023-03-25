using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//[ExecuteInEditMode]
public class ItemDispenser : MonoBehaviour, ISelectable
{
    public enum DispenseType
    {
        LIMITED, INFINITE
    }

    public GameObject iGameObject { get => gameObject; }

    public ItemID itemID;

    /// <summary>
    /// The price of one dispense (for a regular Item pickup, this would be zero or the player could buy a gun or buy some ammo)
    /// </summary>
    [Tooltip("The price of one dispense (for a regular Item pickup, this would be zero or the player could buy a gun or buy some ammo)")]
    public int price = 500;

    [SerializeField]
    private string m_prompt;

    /// <summary>
    /// Should the price of this ItemDispenser be appended to the m_prompt?
    /// </summary>
    [Tooltip("Should the price of this ItemDispenser be appended to the m_prompt?")]
    public bool addPriceToPrompt;

    public string prompt
    {
        get
        {
            if(!addPriceToPrompt)
                return m_prompt;
            else
                return $"{m_prompt} ${price}";
        }
    }

    public StringMaker stringMaker;

    /// <summary>
    /// How many of itemID's are given when dispensed (for example, if it is a gun, only one gun should be dispensed at a time, but if it is bullets (for an ammo dispenser) 250 bullets could be dispensed at a time)
    /// </summary>
    [Tooltip("How many of itemID's are given when dispensed (for example, if it is a gun, only one gun should be dispensed at a time, but if it is bullets (for an ammo dispenser) 250 bullets could be dispensed at a time)")]
    public int items = 1;

    [Tooltip("Is this a limited resource (like something scavenged) or is it an infinite resource (like a buyable weapon)? (If limited resource, items is the number of items left, if infinite " +
        "resource, then items is the number of items dispensed each time)")]
    public DispenseType dispenseType;

    /// <summary>
    /// Does this ItemDispenser dispense Items that once an IItemUser has ItemID.maxCount in their inventory, the next ItemDispenser Component on this GameObject is used as the main ItemDispenser. (For example, an ItemDispenser of guns could sell guns and ammo, but once an IItemUser has the maxCount of the gun (usually 1 is maxCount of guns) they can buy ammo (the ammo ItemDispenser could be the next ItemDispenser Component on that GameObject))
    /// </summary>
    [Tooltip("Does this ItemDispenser dispense Items that once an IItemUser has ItemID.maxCount in their inventory, the next ItemDispenser Component on this GameObject is used as the main ItemDispenser. (For example, an ItemDispenser of guns could sell guns and ammo, but once an IItemUser has the maxCount of the gun (usually 1 is maxCount of guns) they can buy ammo (the ammo ItemDispenser could be the next ItemDispenser Component on that GameObject))")]
    public bool dispensesMutuallyExclusiveItems;

    /// <summary>
    /// Does this ItemDispenser get destroyed when dispenseCount reaches zero (and infiniteDispenses is false)?
    /// </summary>
    [Tooltip("Does this ItemDispenser get destroyed when dispenseCount reaches zero (and infiniteDispenses is false)?")]
    public bool destroyOnEmpty;


    /*
     * For replacing Pickup
            * See Pickup.UpdatePrimaryPrompt() and implement the same kind of method in ItemDispenser (since this method is used to update the prompt when a certain number of Items are picked up, going to have to implement limited number of 
            * dispenses in ItemDispenser)
     * 
     * For replacing BuyableItem
     * 
     * When checking if this ItemDispenser can dispense to an IItemUser, make method in IItemUser interface called HasMaxCount(ItemID itemID) b/c if the IItemUser has the maxCount of an ItemDispenser's ItemID, then that ItemDispenser cannot dispense 
     * to that IItemUser
            * For this example, the GameObject would have two ItemDispenser Components, the first one for the gun and the second for the ammo
            * *First work on making only one ItemDispenser work at a time, then make secondary and following ItemDispensers work
     *
     * When handling usable ItemIDs (when dealing with adding a dispensed ItemID to a player's inventory) that are embedded in the rig of an IItemUser, make method in IItemUser called GetEmbeddedItems() that returns Item[] (to handle guns, going 
     * to have to make Gun inherit Item)
     * 
     * 
     */

    
    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Item");

        if(stringMaker)
            m_prompt = stringMaker.FormatString();

        //If the prompt is not null, don't change it, if it is null, replace it with itemID.itemName, if itemID is null, then the prompt will be null
        m_prompt = !m_prompt.Equals("") ? m_prompt : (itemID != null ? (!itemID.itemDisplayName.Equals("") ? itemID.itemDisplayName : itemID.itemName) : "");
    }

    //public string GetPrompt()
    //{
    //    return prompt;
    //}

    public void Use(IItemUser iItemUser)
    {
        bool canUse2 = false;

        if(iItemUser != null && iItemUser.CanUse(this, out canUse2))
        {
            UseItemDispenser(iItemUser);
        }
    }

    public virtual void UseItemDispenser(IItemUser iItemUser)
    {
        int amountTaken = iItemUser.AddToInventory(itemID, items, price);

        switch(dispenseType)
        {
            case DispenseType.LIMITED:
                //If this is a limited resource, update the item count to reflect what the IItemUser took
                items-= amountTaken;
                break;

            case DispenseType.INFINITE:
                //If this is an infinite resource, the items count never changes so no action here
                break;
        }

        if(destroyOnEmpty && items <= 0)
            Destroy(gameObject);
    }

    //From Pickup
    //public void Use(IItemUser iItemUser)
    //{
    //    base.Use(iItemUser);



    //    bool canUse2 = false;

    //    if(iItemUser != null && iItemUser.CanUse(this, out canUse2))
    //    {
    //        switch(type)
    //        {
    //            case PickupType.AMMO:
    //                iItemUser.UseAmmoItem(stats.ammo);
    //                gameObject.SetActive(false);
    //                break;
    //            case PickupType.PART:
    //                if(iItemUser.PickupPartItem(this, count))
    //                    gameObject.SetActive(false);
    //                break;
    //        }
    //    }

    //    if(!dontUpdatePromptOnUse)
    //        UpdatePrimaryPrompt();
    //}

    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    //From BuyableItem
    //public override void Use(IItemUser iItemUser)
    //{
    //    base.Use(iItemUser);



    //    if(iItemUser != null && !iItemUser.GetItems().Contains(itemID.itemName)) //Change to iItemUser.HasInInventory() (create that method in the IItemUser interface) and/or get rid of this BuyableItem class and just make an ItemDispenser class (so get rid 
    //                                                                             //of BuyableItem and Pickup (b/c a pickup is essentially just a free ItemDispenser that gets destroyed on dispense)). The goal is to make Item the most general as possible so 
    //                                                                             //can easily add Items and different kinds of Items without having to add much more functionality manually
    //    {
    //        iItemUser.BuyItem(itemID.itemName, price);
    //    }
    //    else if(iItemUser != null/* && */) //&& check which item is the one that they have (item1, 2, etc.)
    //    {
    //        if(buyableItemType == BuyableItemType.GUN)
    //        {
    //            iItemUser.BuyItem(itemID.itemName, secondaryPrice);
    //        }
    //    }
    //    else
    //    {

    //    }

    //    if(oneTimeUse)
    //        gameObject.SetActive(false);
    //}

    private void UpdatePromptOnUse()
    {

    }

    public string GetItemName()
    {
        if(itemID)
            return itemID.itemName;
        else
            return "";
    }

    public string GetItemDisplayName()
    {
        if(itemID)
            return itemID.itemDisplayName;
        else
            return "";
    }

    public string GetRawPrompt()
    {
        return m_prompt;
    }

    public string GetFormattedPrompt()
    {
        return prompt;
    }

    public int GetPrice()
    {
        return price;
    }

    //public int GetDispensesLeft()
    //{
    //    return dispensesLeft;
    //}

    //For StringMaker------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void StringMaker_ItemName()
    {
        if(stringMaker)
            if(itemID)
                stringMaker.Add(itemID.itemName);
            else
                stringMaker.Add("");
    }

    public void StringMaker_ItemDisplayName()
    {
        if(stringMaker)
            if(itemID)
                stringMaker.Add(itemID.itemDisplayName);
            else
                stringMaker.Add("");
    }

    public void StringMaker_RawPrompt()
    {
        if(stringMaker)
            stringMaker.Add(m_prompt);
    }

    public void StringMaker_FormattedPrompt()
    {
        if(stringMaker)
            stringMaker.Add(prompt);
    }

    public void StringMaker_Price()
    {
        if(stringMaker)
            stringMaker.Add(price);
    }

    //public void StringMaker_DispensesLeft()
    //{
    //    if(stringMaker)
    //        stringMaker.Add(dispensesLeft);
    //}
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
}
