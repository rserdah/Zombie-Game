using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemUser
{
    GameObject iGameObject { get; }

    bool CanUse(Item item, out bool canUseSecondary);

    bool CanUse(ItemDispenser itemDispenser, out bool canUseSecondary);

    void UseAmmoItem(int ammoCount);

    // /// <summary>
    // /// Picks up a Pickup Item. Returns true if all of the item is picked up and false if only picked up some of the given amount of the item
    // /// </summary>
    // /// <param name="pickupItem"></param>
    // /// <param name="count"></param>
    // /// <returns></returns>
    //bool PickupPartItem(Pickup pickupItem, int count);

    
    bool BuyItem(string itemName, int price);

    /// <summary>
    /// Pays for an item/action/etc. If player can afford the price, it subtracts the price from the score and returns true, if cannot afford it, it returns false. Different from BuyItem because Pay()
    /// is for generic items/actions like turning on a trap for example where no item is actually picked up but the player still pays points.
    /// </summary>
    /// <param name="price"></param>
    /// <returns></returns>
    bool Pay(int price);

    //string GetPrimaryItem();

    //string GetSecondaryItem();

    //string GetTertiaryItem();

    //string GetQuaternaryItem();

    string GetItem1();

    string GetItem2();

    string GetItem3();

    string GetItem4();

    List<string> GetItems();

    /*KeyValuePair<int, int>*/ List<string> GetInventory();

    /// <summary>
    /// Adds the ItemID to the IItemUser's inventory of the amount given. If the IItemUser cannot accept all of the amount, it returns the excess, else it returns zero.
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    int AddToInventory(ItemID itemID, int amount = 1, int price = 0);
}
