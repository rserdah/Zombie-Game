using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPlate : Target
{
    public enum TargetPlateType
    {
        BASIC, FLIP, DETACH
    }

    public enum FlipAxis
    {
        LOCALX, LOCALY, LOCALZ
    }

    private TargetSet parentTarget;
    public TargetPlateType targetPlateType;
    public FlipAxis flipAxis;
    private Vector3 flipDirection
    {
        get
        {
            switch(flipAxis)
            {
                case FlipAxis.LOCALX:
                    return transform.right;

                case FlipAxis.LOCALY:
                    return transform.up;

                case FlipAxis.LOCALZ:
                    return transform.forward;

                default:
                    return transform.right;
            }
        }
    }

    private bool hitOnce;
    //BASIC variables
    [Header("BASIC")]
    public int hi;

    //FLIP variables
    [Header("FLIP")]
    public float flipSpeed = 5f;
    public float flipAngle;
    public float addToFlipAngleThreshold = 0.005f;
    public float reachedAngleThreshold = 0f;
    public bool invertFlipQuatSignsOnInit = false;
    private float flip, flipTime;
    public bool flipped;
    [Tooltip("Should this TargetPlate flip every time it is hit or should it just flip once?")]
    public bool flipFlop = true;
    public bool flipX, flipY, flipZ;
    public bool autoSetRotationOnInit = true;
    public Quaternion startQuat, flipQuat;
    private Quaternion slerpStart, slerpEnd;

    //DETACH variables
    [Header("DETACH")]
    public float detachForce = 50f;
    public float detachTorque = 10f;


    public override void Init(TargetSet parent)
    {
        base.Init(parent);


        parentTarget = parent;

        startQuat = transform.localRotation;
        //Add a small amount to the flip angle so that the rotations are correct (sometimes the magnitude of the Quaternion X and Y are correct but their sign is sometimes flipped. This threshold
        //fixes this)
        if(autoSetRotationOnInit)
        {
            Vector3 dir = flipDirection;
            
            flipQuat = Quaternion.Inverse(transform.parent.rotation) * Quaternion.AngleAxis(flipAngle + addToFlipAngleThreshold, dir);
        }

        if(invertFlipQuatSignsOnInit)
            flipQuat = InvertSigns(flipQuat);
    }

    public override void Hit(RaycastHit hit)
    {
        base.Hit(hit);

        
        if(parentTarget)
            parentTarget.OnHit();

        switch(targetPlateType)
        {
            case TargetPlateType.BASIC:
                break;

            case TargetPlateType.FLIP:
            {
                Debug.LogError("To fix the DuelTree plates not rotating in correct direction, try these:" +
                    "https://answers.unity.com/questions/478617/not-interpolate-quaternion-on-shortest-path.html" +
                    " and " +
                    "https://forum.unity.com/threads/by-pass-the-shortest-route-aspect-of-quaternion-slerp.459429/");

                //If this TargetPlate should continuously flipflop or if it should only do it once and has not done so yet
                if(flipFlop || !hitOnce)
                {
                    StopCoroutine(Flip());
                    StartCoroutine(Flip());
                }

                break;
            }

            case TargetPlateType.DETACH:
            {
                if(!hitOnce)
                {
                    transform.parent = null;
                    Rigidbody rb = gameObject.AddComponent<Rigidbody>();
                    rb.AddForceAtPosition(detachForce * -hit.normal, hit.point);
                    rb.AddTorque(detachTorque * transform.right, ForceMode.Impulse);
                }

                break;
            }
        }
    }

    public IEnumerator Flip()
    {
        flip = 0;
        flipTime = 0;

        while(true)
        {
            if(!flipped)
            {
                slerpStart = startQuat;
                slerpEnd = flipQuat;
            }
            else
            {
                slerpStart = flipQuat;
                slerpEnd = startQuat;
            }

            transform.localRotation = Quaternion.Slerp(slerpStart, slerpEnd, flipTime);
            flipTime += flipSpeed * Time.deltaTime;

            if(Mathf.Approximately(Quaternion.Angle(transform.localRotation, slerpEnd), reachedAngleThreshold))
            {
                flipped = !flipped;
                hitOnce = true;
                break;
            }


            yield return new WaitForFixedUpdate();
        }
    }

    private Quaternion InvertSigns(Quaternion q)
    {
        //Sometimes the Quaternions will give the same rotation but have different signs, so we can invert the signs before comparing to get the right results
        q.x *= -1;
        q.y *= -1;
        q.z *= -1;
        q.w *= -1;

        return q;
    }



}
