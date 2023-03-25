using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : UsableItem
{
    //Instead of trying to handle MeleeWeapon hits as collisions, try using raycasting (either have rays coming out of the weapon or have a ray coming out of the camera) so can reuse the stuff for how gunshot damage is calculated
}
