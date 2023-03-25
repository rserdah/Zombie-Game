using UnityEngine;

public class ModularInteractionHandler : InteractionHandler
{
    public string actionName;


    public override void Use(IItemUser iItemUser)
    {
        if(actionName.Length > 0)
            InteractionHandler.actions[actionName]?.Invoke(iItemUser, this);
    }
}
