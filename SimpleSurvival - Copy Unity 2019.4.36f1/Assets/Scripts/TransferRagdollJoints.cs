using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;

[ExecuteInEditMode]
public class TransferRagdollJoints : MonoBehaviour
{
    public bool set;

    public Transform[] fromBones;
    public Transform[] toBones;

    public Component fromComponent;
    public GameObject toGameObject;


    private void Update()
    {
        if(set)
        {
            //TransferComponent(fromComponent, toGameObject);
            Transfer();


            set = false;
        }
    }

    public static bool TransferComponent(Component component, GameObject toGameObject)
    {
        ComponentUtility.CopyComponent(component);

        if(toGameObject.GetComponent(component.GetType()))
            return ComponentUtility.PasteComponentValues(toGameObject.GetComponent(component.GetType()));
        else
            return ComponentUtility.PasteComponentAsNew(toGameObject);
    }

    public Transform getNearestParentWithConfigurableJoint(Transform child)
    {
        try
        {
            //If the current child's parent exists and has a ConfigurableJoint OR if it is the hips (b/c the hips bone is the central bone (so is the ConfigurableJoint.connectedBody for many other bones) yet does not have a ConfigurableJoint)
            if((child.parent && child.parent.GetComponent<ConfigurableJoint>())/* || child.parent.Equals(allBodyParts[13])*/)
                return child.parent;
        }
        catch(System.Exception) { }

        if(child.parent)
            return getNearestParentWithConfigurableJoint(child.parent);


        return null;
    }

    public Transform getNearestParentWithRigidbody(Transform child)
    {
        try
        {
            //If the current child's parent exists and has a ConfigurableJoint OR if it is the hips (b/c the hips bone is the central bone (so is the ConfigurableJoint.connectedBody for many other bones) yet does not have a ConfigurableJoint)
            if((child.parent && child.parent.GetComponent<Rigidbody>())/* || child.parent.Equals(allBodyParts[13])*/)
                return child.parent;
        }
        catch(System.Exception) { }

        if(child.parent)
            return getNearestParentWithRigidbody(child.parent);


        return null;
    }

    public bool SetConnectedBody(ConfigurableJoint joint)
    {
        if(joint)
        {
            Transform jointParent = getNearestParentWithRigidbody(joint.transform);
            //In these ragdolls, if it has a ConfigurableJoint, it also has a Rigidbody, so the nearest parent with a ConfigurableJoint should be this current ConfigurableJoint's connectedBody
            if(jointParent && (joint.connectedBody = jointParent.GetComponent<Rigidbody>()))
                return true;
        }

        return false;
    }

    private void Transfer()
    {
        if(fromBones.Length > 0 && fromBones.Length == toBones.Length)
        {
            Rigidbody rb = null;
            Collider col = null;
            ConfigurableJoint joint = null, toJoint = null;

            for(int i = 0; i < fromBones.Length; i++)
            {
                if(fromBones[i] && toBones[i])
                {
                    rb = fromBones[i].GetComponent<Rigidbody>();
                    col = fromBones[i].GetComponent<Collider>();
                    joint = fromBones[i].GetComponent<ConfigurableJoint>();

                    if(rb)
                        TransferComponent(rb, toBones[i].gameObject);

                    if(col)
                        TransferComponent(col, toBones[i].gameObject);

                    if(joint && TransferComponent(joint, toBones[i].gameObject))
                    {
                        if(toJoint = toBones[i].GetComponent<ConfigurableJoint>())
                        {
                            toJoint.connectedBody = null; //Set the current toBone's ConfigurableJoint.connectedBody to null b/c it will reference the connectedBody for the current fromBone
                            SetConnectedBody(toJoint); //Set the current toBone's ConfigurableJoint.connectedBody to its connected Rigidbody (set it to null before just in case this method fails/this bone doesn't hae connected Rigidbody)
                        }
                    }
                }

                rb = null;
                joint = toJoint = null;
            }
        }
        else
        {
            Debug.LogError("fromBones.Length != toBones.Length || fromBones.Length == 0 || toBones.Length == 0", gameObject);
        }
    }

    //This is for runtime so not actually needed for this; Not fully working (things that are selected in dropdown lists line enums are not transfered correctly but number values seem to transfer fine)
    /// <summary>
    /// Modified from Shaffe's answer at Unity Answers (https://answers.unity.com/questions/458207/copy-a-component-at-runtime.html)
    /// </summary>
    /// <param name="original"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    Component CopyComponent(Component original, GameObject destination)
    {
        //Old
        //System.Type type = original.GetType();
        //Component copy = destination.AddComponent(type);
        //// Copied fields can be restricted with BindingFlags
        //System.Reflection.FieldInfo[] fields = type.GetFields();
        //foreach(System.Reflection.FieldInfo field in fields)
        //{
        //    field.SetValue(copy, field.GetValue(original));
        //}
        //return copy;

        //New
        System.Type type = original.GetType();

        //If destination already has the Component, get that Component and just change its values, if not add one and change the new values
        Component copy = null;
        copy = destination.GetComponent(type);
        if(!copy)
            copy = destination.AddComponent(type);

        // Copied fields can be restricted with BindingFlags
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach(System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy;
    }

    //This is for runtime so not actually needed for this; Generic type version; NOT TESTED
    /// <summary>
    /// Modified from Shaffe's answer at Unity Answers (https://answers.unity.com/questions/458207/copy-a-component-at-runtime.html)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="original"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach(System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
    }
}
