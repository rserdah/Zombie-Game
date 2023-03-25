using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Randomize : MonoBehaviour
{
    [TextArea(1, 1)]
    public string title;

    [Space(20f)]

    public Vector3 minPositionChange;
    public Vector3 maxPositionChange;

    public Vector3 minRotationChange;
    public Vector3 maxRotationChange;

    public Transform[] transforms;

    public Pose[] originalTransforms;

    public bool makeBackupOnRandomize = true;
    public bool useLocalPosition;
    public bool useLocalRotation;


    public bool restore;
    public bool randomize;

    [System.Serializable]
    public struct Pose
    {
        public Vector3 position;
        public Quaternion rotation;
    }


    private void Start()
    {
        makeBackupOnRandomize = true;
    }

    private void Update()
    {
        if(randomize)
        {
            if(makeBackupOnRandomize)
                BackupOriginalTransforms();

            RandomizeTransforms();


            randomize = false;
            makeBackupOnRandomize = false;
        }

        if(restore)
        {
            RestoreTransforms();


            restore = false;
        }
    }

    private void BackupOriginalTransforms()
    {
        originalTransforms = new Pose[transforms.Length];

        for(int i = 0; i < transforms.Length; i++)
        {
            if(useLocalPosition)
                originalTransforms[i].position = transforms[i].localPosition;
            else
                originalTransforms[i].position = transforms[i].position;

            if(useLocalRotation)
                originalTransforms[i].rotation = transforms[i].localRotation;
            else
                originalTransforms[i].rotation = transforms[i].rotation;
        }
    }

    private void RestoreTransforms()
    {
        if(transforms.Length > 0 && transforms.Length == originalTransforms.Length)
        {
            for(int i = 0; i < originalTransforms.Length; i++)
            {
                if(useLocalPosition)
                    transforms[i].localPosition = originalTransforms[i].position;
                else
                    transforms[i].position = originalTransforms[i].position;

                if(useLocalRotation)
                    transforms[i].localRotation = originalTransforms[i].rotation;
                else
                    transforms[i].rotation = originalTransforms[i].rotation;
            }
        }
    }

    private void RandomizeTransforms()
    {
        float deltaX, deltaY, deltaZ;
        float deltaRotX, deltaRotY, deltaRotZ;
        Vector3 deltaPosition;
        Quaternion deltaRotation;

        for(int i = 0; i < transforms.Length; i++)
        {
            deltaX = Random.Range(minPositionChange.x, maxPositionChange.x);
            deltaY = Random.Range(minPositionChange.y, maxPositionChange.y);
            deltaZ = Random.Range(minPositionChange.z, maxPositionChange.z);

            deltaPosition = new Vector3(deltaX, deltaY, deltaZ);

            if(useLocalPosition)
                transforms[i].localPosition = originalTransforms[i].position + deltaPosition;
            else
                transforms[i].position = originalTransforms[i].position + deltaPosition;


            deltaRotX = Random.Range(minRotationChange.x, maxRotationChange.x);
            deltaRotY = Random.Range(minRotationChange.y, maxRotationChange.y);
            deltaRotZ = Random.Range(minRotationChange.z, maxRotationChange.z);

            //Old
            //deltaRotation = Quaternion.Euler(deltaRotX, deltaRotY, deltaRotZ);

            //if(useLocalRotation)
            //    transforms[i].localRotation = originalTransforms[i].rotation * deltaRotation;
            //else
            //    transforms[i].rotation = originalTransforms[i].rotation * deltaRotation;

            //New
            if(useLocalRotation)
            {
                transforms[i].localRotation = originalTransforms[i].rotation;
                transforms[i].Rotate(deltaRotX, deltaRotY, deltaRotZ, Space.Self);
            }
            else
            {
                transforms[i].rotation = originalTransforms[i].rotation;
                transforms[i].Rotate(deltaRotX, deltaRotY, deltaRotZ, Space.World);
            }
        }
    }
}
