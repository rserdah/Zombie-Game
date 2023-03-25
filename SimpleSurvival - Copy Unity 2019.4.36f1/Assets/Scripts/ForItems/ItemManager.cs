using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private static bool initialized;


    private void Awake()
    {
        if(!initialized)
        {
            SetAllRecipes();


            initialized = true;
        }
    }

    private void SetAllRecipes()
    {
        Object[] itemsIDs = Resources.LoadAll("ItemIDs");

        Item.allItemIDs = new ItemID[itemsIDs.Length];

        for(int i = 0; i < itemsIDs.Length; i++)
        {
            Item.allItemIDs[i] = (ItemID)itemsIDs[i];
        }

        foreach(ItemID i in Item.allItemIDs)
        {
            if(i.recipe)
                i.recipe.SetItemID(i);
        }

        Object[] recipes = Resources.LoadAll("Recipes");

        Item.allRecipes = new Recipe[recipes.Length];

        for(int i = 0; i < recipes.Length; i++)
        {
            Item.allRecipes[i] = (Recipe)recipes[i];
            ////////Debug.LogError(Item.allRecipes[i].GetItemID().itemName);
        }

        //foreach(ItemID i in Item.allItemIDs)
        //    Debug.LogError(i.itemName);

        //foreach(Recipe r in Item.allRecipes)
        //    Debug.LogError(r.name);
    }
}
