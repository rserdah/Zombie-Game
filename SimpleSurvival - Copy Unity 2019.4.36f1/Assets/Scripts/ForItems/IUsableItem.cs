using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IUsableItem
{
    //-----------------------------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------------------------
    //REORDER THESE TO THE ORDER THAT THEY APPEAR IN Gun.cs
    //REORDER THESE TO THE ORDER THAT THEY APPEAR IN Gun.cs
    //REORDER THESE TO THE ORDER THAT THEY APPEAR IN Gun.cs
    //REORDER THESE TO THE ORDER THAT THEY APPEAR IN Gun.cs
    //REORDER THESE TO THE ORDER THAT THEY APPEAR IN Gun.cs
    //-----------------------------------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------------------------------

    ItemID u_itemID { get; }

    Item.UseMode u_useMode { get; set; }

    Item.UseType u_useType { get; set; }

    GameObject gameObject { get; }

    string name { get; }

    float u_damage { get; set; }

    int u_usesLeft { get; set; }

    int u_maxUses { get; set; }

    int u_usesReserved { get; set; }

    int u_maxUsesReserved { get; set; }

    float u_useRate { get; }

    float u_lastTimeUsed { get; set; }

    bool u_usesReplenished { get; }

    bool u_hasRechamber { get; }
    bool u_rechambered { get; set; }
    int u_rechamberRounds { get; }
    bool u_needsRechamber { get; }

    float u_reloadEndTime { get; }

    bool u_canManualIncrementalReload { get; set; }
    bool u_willManualIncrementalReload { get; set; }
    float u_manualIncrementalReloadTime { get; set; }

    UnityEvent u_useEvent { get; }

    UnityEvent u_onEnableEvent { get; }

    UnityEvent u_onDisableEvent { get; }

    Animator u_anim { get; }

    Vector3 u_shootOrigin { get; }

    Vector3 u_shootDirection { get; }

    float u_burstInterval { get; }

    int u_burstCount { get; }


    AnimationClip idle { get; }
    AnimationClip aim { get; }
    AnimationClip aimUse { get; }
    AnimationClip reload { get; }
    AnimationClip rechamber { get; }
    AnimationClip use { get; }
    AnimationClip swapStart { get; }
    AnimationClip swapEnd { get; }

    int u_IncrementalReload();

    int u_Reload();
}

