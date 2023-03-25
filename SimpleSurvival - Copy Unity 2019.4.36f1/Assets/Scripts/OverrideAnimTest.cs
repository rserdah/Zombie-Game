using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverrideAnimTest : MonoBehaviour
{
    [System.Serializable]
    public struct Pose
    {
        public string name; //So can see each bone's name in Inspector

        public Vector3 position;
        public Quaternion rotation;
    }


    public Transform[] bones;
    public Pose[] poses;


    private void Start()
    {
        if(bones.Length > 0 && bones.Length == poses.Length)
        {
            for(int i = 0; i < bones.Length; i++)
            {
                poses[i].name = bones[i].name;
            }
        }
    }

    private void LateUpdate()
    {
        if(bones.Length > 0 && bones.Length == poses.Length)
        {
            for(int i = 0; i < bones.Length; i++)
            {
                bones[i].localPosition = poses[i].position;
                bones[i].localRotation = poses[i].rotation;
            } 
        }
    }
}
