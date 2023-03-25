using System;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [Serializable]
    public class Settings
    {
        [Header("Stats")]
        public float distance = 2.0f;
        public float xSensitivity = 2;
        public float ySensitivity = 2;
        public float cameraSpeed = 20.0f;
        public float yMinLimit = -90f;
        public float yMaxLimit = 90f;
        public float xMinLimit = -360f; //Use 360 to not have any clamps on rotation
        public float xMaxLimit = 360f; //Use 360 to not have any clamps on rotation
        public float distanceMin = 10f;
        public float distanceMax = 10f;
        public float distanceChangeSpeed = 2f;
        [Tooltip("When horizontal mouse input is less than this, the camera (really just this.transform) will return to its base rotation.")]
        public float cameraReturnDeadZone = 0.5f;
        public float smoothTime = 2f;
        public float normalFOV = 60f;
        public float zoomFOV = 30f;
        public float zoomSpeed = 200f;
        public float defaultZoomFOV = 30f;


        [Header("Bools")]
        [Tooltip("You have to click in order to change your view (as opposed to the camera rotating with the player and always keeping the camera showing what's ahead).")]
        public bool mustClickToRotate;
        [Tooltip("You can click to change your view (to look behind you while running for example), but the camera returns to following the player rotation when you release the click.")]
        public bool canClickToRotate; //Should probably just change it to always orbit the camera around player and just smoothly reset to look forward when the player starts moving
        public bool invertX;
        public bool invertY;
        public bool dontUseHorizontal;
        public bool dontUseVertical;
        public bool isUnderwater;
    }

    [Serializable]
    public class Components
    {
        public Animator anim;
    }

    [Serializable]
    public struct Pose
    {
        public Transform parent;
        public Vector3 localPosition;
        public Quaternion localRotation;


        public Pose(Transform _parent, Vector3 _localPosition, Quaternion _localRotation)
        {
            parent = _parent;
            localPosition = _localPosition;
            localRotation = _localRotation;
        }
    }


    //=== References ===//
    //public PlayerInput playerInput; //Currently assumes player with PlayerInput is the direct parent if this GameObject
    public CameraHolder cameraHolder;
    private CameraHolder originalCameraHolder;
    private Pose cameraHolderPose;
    public bool resetPose;
    //public Camera camera; //!!! Here, it is assumed that camera is a child that is facing in the same direction as this.transform (b/c when we modify the distance, we are going to move camera in its forward direction) !!!
    public CinemachineVirtualCamera camera;
    public Settings settings = new Settings();
    public Components components = new Components();





    //============================================================================================== Temp./Holder Variables ==============================================================================================//
    /**/
    Quaternion fromRotation;                                                                                                                                                                                       /**/
    /**/ Quaternion toRotation;                                                                                                                                                                                         /**/
    /**/ float rotationYAxis = 0.0f;                                                                                                                                                                                    /**/
    /**/ float rotationXAxis = 0.0f;                                                                                                                                                                                    /**/
    /**/ float velocityX = 0.0f;                                                                                                                                                                                        /**/
    /**/ float velocityY = 0.0f;                                                                                                                                                                                        /**/
    /**/ bool pauseRotation;                                                                                                                                                                                            /**/
    /**/ bool isClickingToRotate;                                                                                                                                                                                       /**/
    /**/ private int zoomDirectionMultiplier = 1;                                                                                                                                                                       /**/
    /**/ public bool zoomed;                                                                                                                                                                                            /**/
    /**/ private bool setUnderwater;                                                                                                                                                                                     /**/
    //====================================================================================================================================================================================================================//


    public virtual void Awake()
    {
        //Set references
        originalCameraHolder = GetComponentInChildren<CameraHolder>();
        if(originalCameraHolder)
        {
            cameraHolderPose = new Pose(originalCameraHolder.transform.parent, originalCameraHolder.transform.localPosition, originalCameraHolder.transform.localRotation);
            cameraHolder = originalCameraHolder;
        }

        //camera = GetComponentInChildren<Camera>(); //GetComponentInChildren<Type T>() actually returns the FIRST Component of Type T in children
        //camera.fieldOfView = settings.normalFOV;
        camera = GetComponentInChildren<CinemachineVirtualCamera>();
        camera.m_Lens.FieldOfView = settings.normalFOV;

        //playerInput = transform.parent.gameObject.GetComponent<PlayerInput>();
        components.anim = GetComponent<Animator>();


        Vector3 angles = transform.eulerAngles;
        rotationYAxis = angles.y;
        rotationXAxis = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    } //Awake()

    public void LateUpdate()
    {
        if(resetPose)
        {
            ResetCameraHolder();

            resetPose = false;
        }

        if(!pauseRotation) //Check; if want to be able to click and drag while pauseRotation is true, must change that part b/c it is in this if statement
        {
                /*if(Mathf.Abs(Input.GetAxis("Mouse X")) > cameraReturnDeadZone || Mathf.Abs(Input.GetAxis("Mouse Y")) > cameraReturnDeadZone)
                {*/
                //================== Get mouse input for rotation ==================
            if(settings.mustClickToRotate && !settings.canClickToRotate)
            {
                if(Input.GetMouseButton(0))
                {
                    velocityX += settings.xSensitivity * Input.GetAxis("Mouse X");
                    velocityY += settings.ySensitivity * Input.GetAxis("Mouse Y");
                }
            }
            else
            {
                velocityX += settings.xSensitivity * Input.GetAxis("Mouse X");
                velocityY += settings.ySensitivity * Input.GetAxis("Mouse Y");
            }
            //==================================================================

            if(settings.canClickToRotate && !settings.mustClickToRotate)
            {
                if(Input.GetMouseButton(0))
                {
                    //playerInput.settings.pauseHorizontalRotation = true;
                    isClickingToRotate = true;

                    velocityX += settings.xSensitivity * Input.GetAxis("Mouse X");
                    velocityY += settings.ySensitivity * Input.GetAxis("Mouse Y");
                }
                if(Input.GetMouseButtonUp(0))
                {
                    //playerInput.settings.pauseHorizontalRotation = false;
                    isClickingToRotate = false;
                }
            }

            if(!settings.dontUseHorizontal || isClickingToRotate)
            {
                if(!settings.invertX)
                {
                    rotationYAxis += velocityX;
                }
                else
                {
                    rotationYAxis -= velocityX;
                }
            }

            if(!settings.dontUseVertical || isClickingToRotate)
            {
                if(!settings.invertY)
                {
                    rotationXAxis -= velocityY;
                }
                else
                {
                    rotationXAxis += velocityY;
                }
            }

            rotationXAxis = ClampAngle(rotationXAxis, settings.yMinLimit, settings.yMaxLimit);
            rotationYAxis = ClampAngle(rotationYAxis, settings.xMinLimit, settings.xMaxLimit);

            fromRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0);

            if(settings.dontUseHorizontal && !isClickingToRotate)
                rotationYAxis = transform.localRotation.eulerAngles.y;
            if(settings.dontUseVertical && !isClickingToRotate)
                rotationXAxis = transform.localRotation.eulerAngles.x;

            toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);

            /*if(!pauseRotation) */transform.localRotation = toRotation;

            velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * settings.smoothTime);
            velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * settings.smoothTime);
        }

        if(Input.GetKey(KeyCode.LeftShift))
            ModifyCameraDistance();
    //}
    } //LateUpdate()

    public void Pause()
    {
        PauseRotation();
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        ResumeRotation();
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void PauseRotation()
    {
        pauseRotation = true;
    }

    public void ResumeRotation()
    {
        pauseRotation = false;
    }

    public bool RotationPaused()
    {
        return pauseRotation;
    }

    private void ModifyCameraDistance()
    {
        //Note that Input.mouseScrollDelta is a Vector2 but the x value is NOT used. Only the y value is used.
        camera.transform.position = Vector3.MoveTowards(camera.transform.position, transform.position, Time.deltaTime * settings.distanceChangeSpeed * Input.mouseScrollDelta.y);
    }

    //Not finished yet
    private void ReturnCamera() //Maybe also try putting localRotation to zero !!!!!!!!!!!!!!!!!!!!!!!!
    {
        //Not finished yet
        //Problem is that when you move the mouse as the camera is returning to normal rotation, it can snap and then continue if there is no more mouse input
        //Possible problems:
        //- The Z rotation is set straight to zero when you interrupt the rotation returning with a mouse movement (causing a quick, harsh movement of the camera)
        //- The velocityX and velocityY need to be recalculated as the camera is returning to normal so it can be smoothly picked up from its current speed when 
        //  it is interrupted with a mouse movement

        Debug.Log(string.Format("rotationYAxis: {0}; rotationXAxis: {1}; velocityX: {2}; velocityY: {3}", rotationYAxis, rotationXAxis, velocityX, velocityY));
        //rotationYAxis = 0.0f;
        //rotationXAxis = 0.0f;
        //velocityX = 0.0f;
        //velocityY = 0.0f;
        transform.localRotation = Quaternion.SlerpUnclamped(transform.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * settings.cameraSpeed);
        rotationYAxis = transform.localRotation.eulerAngles.y;
        rotationXAxis = transform.localRotation.eulerAngles.x;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if(angle < -360F)
            angle += 360F;
        if(angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    } //ClampAngle(float angle, float min, float max)

    public void Zoom(bool zoomIn)
    {
        //Only do something if we want to zoom in and we are not already zoomed OR if we want to zoom out AND we are zoomed OR in the middle of zooming in/out
        if((zoomIn && !zoomed) || (!zoomIn && (zoomed || ZoomingInOrOut())))
        {
            if(zoomIn)
                zoomDirectionMultiplier = -1; //B/c you need to DECREASE Camera.fieldOfView to zoom in
            else
                zoomDirectionMultiplier = 1;

            //camera.fieldOfView = Mathf.Clamp(camera.fieldOfView + zoomDirectionMultiplier * settings.zoomSpeed * Time.deltaTime, settings.zoomFOV, settings.normalFOV); //zoomFOV is minumum b/c DECRESE Camera.fieldOfView to zoom in
            camera.m_Lens.FieldOfView = Mathf.Clamp(camera.m_Lens.FieldOfView + zoomDirectionMultiplier * settings.zoomSpeed * Time.deltaTime, settings.zoomFOV, settings.normalFOV); //zoomFOV is minumum b/c DECRESE Camera.fieldOfView to zoom in

            if(camera.m_Lens.FieldOfView <= settings.zoomFOV)
                zoomed = true;
            else if(camera.m_Lens.FieldOfView >= settings.normalFOV)
                zoomed = false;
        }
    }

    private bool ZoomingInOrOut()
    {
        return camera.m_Lens.FieldOfView > settings.zoomFOV && camera.m_Lens.FieldOfView < settings.normalFOV;
    }

    public void ResetCameraHolder()
    {
        cameraHolder = originalCameraHolder;

        ResetCameraHolderPose();
    }

    public void ResetCameraHolderPose()
    {
        cameraHolder.transform.parent = cameraHolderPose.parent;
        cameraHolder.transform.localPosition = cameraHolderPose.localPosition;
        cameraHolder.transform.localRotation = cameraHolderPose.localRotation;
    }

    public void SetCameraHolder(CameraHolder newCameraHolder)
    {
        newCameraHolder.transform.parent = cameraHolderPose.parent;
        cameraHolder = newCameraHolder;

        ResetCameraHolderPose();
    }
} //CameraController
