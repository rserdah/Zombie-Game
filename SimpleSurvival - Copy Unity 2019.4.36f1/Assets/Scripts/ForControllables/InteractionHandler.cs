using System;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethodsTest;

public class InteractionHandler : MonoBehaviour, ISelectable
{
    /// <summary>
    /// These are used by the so called ModularInteractionHandler's. They perform relatively basic tasks/actions so it was presumed better to have a static array of Actions rather than having to write 
    /// an entirely new script every time another interactable item is created. Instead, just add the ModularInteractionHandler script to an object, give it the correct actionIndex and add the 
    /// Action that the item should perform when it is interacted with. They can also use the same Action's if desired, just make sure to assign the correct actionIndex.
    /// </summary>
    protected static Dictionary<string, Action<IItemUser, ISelectable>> actions = new Dictionary<string, Action<IItemUser, ISelectable>>();
    protected static bool actionsInitialized;


    public GameObject iGameObject { get => gameObject; }

    //private Controllable controllable;
    public string m_prompt = null;

    //Return m_prompt if it is not null, else return default prompt
    public string prompt => m_prompt != "" ? m_prompt : "Press E to interact";
    public Action<IItemUser> onInteract { get; set; }


    protected static void actionsInit()
    {
        if(!actionsInitialized)
        {
            actions.Add("mystery_box_buy", (IItemUser iItemUser, ISelectable iSelectable) => 
            {
                MysteryBox box = iSelectable.iGameObject.GetComponent<MysteryBox>();

                if(box && box.available && iItemUser.Pay(950))
                {
                    box.Open();
                }
            });

            actions.Add("set_fire_trap", (IItemUser iItemUser, ISelectable iSelectable) =>
            {
                ParticleSystem p = iSelectable.iGameObject.transform.GetChild(2).GetComponent<ParticleSystem>();
                if(p && !p.isPlaying && iItemUser.Pay(1250))
                {
                    Action<bool> startfiretrap = (bool start) => { if(start) p.Play(); else p.Stop(); };
                    startfiretrap(true);
                    iSelectable.iGameObject.WaitAndDo(10f, () => { startfiretrap(false); });
                }
            });


            actionsInitialized = true;
        }
    }

    private void Start()
    {
        if(!actionsInitialized) actionsInit();

        gameObject.layer = LayerMask.NameToLayer("Item");

        //controllable = transform.GetComponentInParent<Controllable>();
        //TransformHelper.DisableIfNull(transform.parent, controllable, "No Controllable found in parent, disabling", gameObject);
    }

    public virtual void Use(IItemUser iItemUser)
    {
        //controllable?.ControlBegin((PlayerInput)iItemUser);
        onInteract?.Invoke(iItemUser);
    }
}
