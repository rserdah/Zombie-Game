using UnityEngine;
using Cinemachine;

public class HeadBob : MonoBehaviour
{
    //public Camera Camera;
    public CinemachineVirtualCamera Camera;
    public CurveControlledBob motionBob = new CurveControlledBob();
    public LerpControlledBob jumpAndLandingBob = new LerpControlledBob();
    //public RigidbodyFirstPersonController rigidbodyFirstPersonController;
    public PlayerInput playerInput;
    public float StrideInterval;
    //[Range(0f, 1f)] public float RunningStrideLengthen;
    public float RunningStrideLengthen;

    //TODO: When aiming, make horizontalBobRange = 0.00005 and make verticalBobRange = 0.0001 (Which is the current values divided by 10) AND multiply strideInterval by 2 OR make horiz bob = 0 and make vertical bob = 0.0001 and multiply stride by 2

    /*
    ** Since going to be setting strideInterval manually, set RunningStrideLengthen to 1 **
    Normal Head Bob: Horiz = 0.00025; Vert = 0.0005; Stride = 0.5
    Aiming Head Bob: Horiz = 0      ; Vert = 0.0001; Stride = 1
    Running Head Bob: 

     */

    public float cameraResetSpeed = 7f;

    // private CameraRefocus m_CameraRefocus;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;


    private void Start()
    {
        motionBob.Setup(Camera, StrideInterval);
        m_OriginalCameraPosition = Camera.transform.localPosition;
        //     m_CameraRefocus = new CameraRefocus(Camera, transform.root.transform, Camera.transform.localPosition);
    }

    /**/private void Update()
    {
        //  m_CameraRefocus.GetFocusPoint();
        Vector3 newCameraPosition = new Vector3();
        //if(rigidbodyFirstPersonController.Velocity.magnitude > 0 && rigidbodyFirstPersonController.Grounded)
        if(playerInput.groundedMovement.magnitude > 0 && playerInput.isGrounded)
        {
            Camera.transform.localPosition = motionBob.DoHeadBob(playerInput.groundedMovement.magnitude * (playerInput.isRunning ? RunningStrideLengthen : 1f));
            newCameraPosition = Camera.transform.localPosition;
            newCameraPosition.y = Camera.transform.localPosition.y - jumpAndLandingBob.Offset();
        }
        else
        {
            newCameraPosition = Camera.transform.localPosition;
            newCameraPosition.y = Camera.transform.localPosition.y - jumpAndLandingBob.Offset();
        }

        if(playerInput.groundedMovement.magnitude <= 0)
        {
            motionBob.ResetCyclePosition();

            newCameraPosition = Vector3.Lerp(Camera.transform.localPosition, m_OriginalCameraPosition, cameraResetSpeed * Time.deltaTime);
        }

        Camera.transform.localPosition = newCameraPosition;

        if(!m_PreviouslyGrounded && playerInput.isGrounded)
        {
            StartCoroutine(jumpAndLandingBob.DoBobCycle());
        }

        m_PreviouslyGrounded = playerInput.isGrounded;
        //  m_CameraRefocus.SetFocusPoint();
    }

    /*private void Update()
    {
        //  m_CameraRefocus.GetFocusPoint();
        Vector3 newCameraPosition;
        if(playerInput.groundedMovement.magnitude > 0 && playerInput.isGrounded)
        {
            Camera.transform.localPosition = motionBob.DoHeadBob(playerInput.groundedMovement.magnitude * (playerInput.isRunning ? RunningStrideLengthen : 1f));
            newCameraPosition = Camera.transform.localPosition;
            newCameraPosition.y = Camera.transform.localPosition.y - jumpAndLandingBob.Offset();
        }
        else
        {
            newCameraPosition = Camera.transform.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y - jumpAndLandingBob.Offset();
        }
        Camera.transform.localPosition = newCameraPosition;

        if(!m_PreviouslyGrounded && playerInput.isGrounded)
        {
            StartCoroutine(jumpAndLandingBob.DoBobCycle());
        }

        m_PreviouslyGrounded = playerInput.isGrounded;
        //  m_CameraRefocus.SetFocusPoint();
    }*/

    public void SetStrideInterval(float strideInterval)
    {
        StrideInterval = strideInterval;

        motionBob.SetBobBaseInterval(strideInterval);
    }

    public void Step()
    {
        //Debug.LogError("Stepped");
    }
}

