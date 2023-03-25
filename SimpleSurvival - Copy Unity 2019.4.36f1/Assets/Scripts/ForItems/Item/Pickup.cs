using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : Item
{
    [Flags]
    public enum PickupType
    {
        PART = 1 << 1, USABLE = 1 << 2, PART_AND_USABLE = PART | USABLE, AMMO = 1 << 3
    }

    public PickupType type;
    public int count = 1;

    [Serializable]
    public class Stats
    {
        public int ammo;
    }

    public Stats stats = new Stats();


    public override void Start()
    {
        base.Start();


        if(!dontAutoSetPromptOnStart)
            UpdatePrimaryPrompt();
    }

    public /*override*/ void UpdatePrimaryPrompt()
    {
        prompt = $"Pick up {itemID.itemName} ({count})";
    }

    public override void Use(IItemUser iItemUser)
    {
        base.Use(iItemUser);



        bool canUse2 = false;

        if(iItemUser != null && iItemUser.CanUse(this, out canUse2))
        {
            switch(type)
            {
                case PickupType.AMMO:
                    iItemUser.UseAmmoItem(stats.ammo);
                    gameObject.SetActive(false);
                    break;
                case PickupType.PART:
                    //if(iItemUser.PickupPartItem(this, count))
                        //gameObject.SetActive(false);
                    break;
            }
        }

        if(!dontUpdatePromptOnUse)
            UpdatePrimaryPrompt();
    }
}
