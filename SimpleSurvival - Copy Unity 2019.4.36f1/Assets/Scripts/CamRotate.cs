using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamRotate : MonoBehaviour
{
    [Header("Running Effect")]
    public CinemachineVirtualCamera cam;
    //public Transform cam;
    public float speed = 1f;
    public float amplitude = 1f;

    public float speedX = 1f;
    public float speedY = 1f;

    public float amplitudeX = 1f;
    public float amplitudeY = 1f;

    [Header("Weapon Sway")]
    public float swaySpeed = 1f;
    public float swayRelaxSpeed = 7f;
    public bool relaxCamera;
    public float swayAmount;
    public float maxX = 0.5f;
    public float maxY = 0.5f;
    public Transform lookAt;
    float y, x;
    Vector3 pos;

    [Header("Camera Shake")]
    public AnimationCurve xShake;
    public AnimationCurve yShake;
    public bool cameraShaking;
    public bool startedCameraShake;
    public float shakeSpeed = 1f;
    public float amplitudeMultiplier = 1f;
    public float t;
    public Vector3 originalLocPos;
    public float stopPlaybackBuffer = 0.0001f;



    private void FixedUpdate()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            //Try syncing this rotation with the camera bob and maybe also have y rotation so camera can rotate diagonally while running; also add this to when player is falling (if not grounded, make camera's rotation.x be set to sine of time * speed)
            //Also can make this rotation last for a couple seconds after stopping running to simulate player being out of breathe temporarily (start a timer after stopping running and then slowly lerp the rotation speed to zero)

            //Also make it so gunshots attract zombies and player can accidentally knock things over, which can create sound and attract zombies

            //Maybe also make feature where player can trip over small/medium sized objects (make half extents and check that box for collision with trippable objects)

            //Old - Only up/down rotation
            //cam.transform.localEulerAngles = (Vector3.right * amplitude * Mathf.Sin(Time.time * speed)) + (Vector3.up * amplitude * Mathf.Cos(Time.time * speed));

            //New - Rotation around local X & Y (with different speeds and amplitudes)
            cam.transform.localEulerAngles = (Vector3.right * amplitudeX * Mathf.Sin(Time.time * speedX)) + (Vector3.up * amplitudeY * Mathf.Cos(Time.time * speedY)); //Use sin() for X rot & cos for Y rot so rotations don't sync and so they look 
                                                                                                                                                                       //more like running footsteps
        }
        else if(cameraShaking)
        {
            if(!startedCameraShake)
            {
                StartCoroutine(CameraShake());


                startedCameraShake = true;
            }
        }
        else
            WeaponSway();
    }

    private void WeaponSway()
    {
        //Move Camera but make it look at one point (so can control rotation AND position at same time (also so can use Vector3.Lerp for position, b/c messes up when used for angles)
        x = Input.GetAxis("Mouse X") * swayAmount;
        y = Input.GetAxis("Mouse Y") * swayAmount;
        x = Mathf.Clamp(x, -maxX, maxX);
        y = Mathf.Clamp(y, -maxY, maxY);

        if(Mathf.Abs(x) > 0 || Mathf.Abs(y) > 0)
        {
            pos = cam.transform.localPosition + new Vector3(x, y, 0f);
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, pos, Time.deltaTime * swaySpeed);
        }

        if(relaxCamera)
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, Vector3.zero, Time.deltaTime * swayRelaxSpeed);
        //else
        //    Debug.LogError("Remember to combine HeadBob and CamRotate (which will then be renamed to CameraEffects) b/c when HeadBob is enabled, it lerps the Camera position back to zero but makes it jittery, when CamRotate does it, it is not.");

        cam.transform.LookAt(lookAt);
    }

    public IEnumerator CameraShake()
    {
        t = 0f;

        while(t < xShake[xShake.length - 1].time)
        {
            if(t + stopPlaybackBuffer >= xShake[xShake.length - 1].time)
            {
                cameraShaking = false;
                startedCameraShake = false;

                break;
            }

            cam.transform.localPosition = originalLocPos + new Vector3(xShake.Evaluate(t) * amplitudeMultiplier, yShake.Evaluate(t) * amplitudeMultiplier, 0f);

            t += shakeSpeed * Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
}
