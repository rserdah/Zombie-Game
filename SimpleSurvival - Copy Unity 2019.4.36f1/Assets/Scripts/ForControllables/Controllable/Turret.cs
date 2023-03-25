using System;
using Cinemachine;
using UnityEngine;


/*
 * Useful for doing rotation for cameras and things:
 * https://gamedev.stackexchange.com/questions/136174/im-rotating-an-object-on-two-axes-so-why-does-it-keep-twisting-around-the-thir
 * 
 
 */


public class Turret : Controller//Controllable
{
    [Serializable]
    public struct physicsbone
    {
        public Rigidbody rb;
        public Vector3 m_localforceoffset;
        public Vector3 localforceoffset { get => rb.transform.TransformPoint(m_localforceoffset); }

        public Vector3 m_localforcedir;
        public Vector3 localforcedir { get => rb.transform.TransformDirection(m_localforcedir); }

        public float force;
    }

    public physicsbone[] physicsbones;
    public bool drawphysicsbones;

    public Item.UseMode u_useMode;
    public int u_usesLeft = 99;
    public float u_lastTimeUsed;
    public float u_useRate = 1;

    public float damage = 2.5f;

    public AudioClip shootSound;

    public float sensitivity = 1f;
    public Vector2 horizontalClamp = Vector2.one * 360f;
    public Vector2 verticalClamp = Vector2.one * 360f;

    //public (Transform t_base, Transform t_gun, Transform t_shootOrigin) rig;
    public (Transform t_bipod, Transform t_gun, Transform t_guntip) rig;
    public Vector2 input = Vector2.zero;

    public InputManager inputManager;
    private InputData data;

    private AudioSource audioSource;
    private ParticleSystem muzzleFlash;
    public GameObject bulletImpact;


    private void Start()
    {
        //rig.t_base = TransformHelper.FindRecursive(transform, "t_base");
        //rig.t_gun = TransformHelper.FindRecursive(transform, "t_gun");
        //rig.t_shootOrigin = TransformHelper.FindRecursive(transform, "t_shootOrigin");

        rig.t_bipod = TransformHelper.FindRecursive(transform, "t_bipod");
        rig.t_gun = TransformHelper.FindRecursive(transform, "t_gunpivot");
        rig.t_guntip = TransformHelper.FindRecursive(transform, "t_guntip");

        audioSource = GetComponent<AudioSource>();
        muzzleFlash = GetComponentInChildren<ParticleSystem>();

        GetComponentInChildren<InteractionHandler>().onInteract += (IItemUser i) =>
        {
            inputManager.RequestTransfer(this);
            cinemachineCamera = GetComponentInChildren<CinemachineVirtualCameraBase>();
            cinemachineCamera.Priority += 1;
            i.iGameObject.GetComponent<PlayerInput>().localStandbyControlOffset = transform.InverseTransformPoint(i.iGameObject.transform.position);
        };

        GetComponentInChildren<CameraController>().enabled = false;
    }

    public override void Enable(InputManager manager)
    {
        inputManager = manager;
        active = true;
        GetComponentInChildren<CameraController>().enabled = true;
        //cinemachineCamera.Priority = 10;
    }

    public override void ReadInput(InputData data)
    {
        this.data = data;
    }

    /// <summary>
    /// Found on Unity Forum (https://forum.unity.com/threads/how-do-i-clamp-a-quaternion.370041/)
    /// </summary>
    /// <param name="q"></param>
    /// <param name="bounds"></param>
    /// <returns></returns>
    public static Quaternion ClampRotation(Quaternion q, Vector3 bounds)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, -bounds.x, bounds.x);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
        angleY = Mathf.Clamp(angleY, -bounds.y, bounds.y);
        q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);

        float angleZ = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.z);
        angleZ = Mathf.Clamp(angleZ, -bounds.z, bounds.z);
        q.z = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleZ);

        return q.normalized;
    }

    private void Update()
    {
        if(active)
        {
            //////////Found on Unity answers I think. Don't remember exactly where though.
            ////////{
            ////////    // Yaw happens "over" the current rotation, in global coordinates.
            ////////    Quaternion yaw = Quaternion.Euler(0f, Input.GetAxis("Mouse X") * sensitivity, 0f);
            ////////    rig.t_gun.rotation = yaw * rig.t_gun.rotation; // yaw on the left.

            ////////    //          //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            ////////    //          //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            ////////    //          //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            ////////    //          //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            ////////    //rig.t_gun.rotation = ClampRotation(rig.t_gun.rotation, new Vector3(0, 30, 0));

            ////////    // Pitch happens "under" the current rotation, in local coordinates.
            ////////    Quaternion pitch = Quaternion.Euler(-Input.GetAxis("Mouse Y") * sensitivity, 0f, 0f);
            ////////    rig.t_gun.rotation = (rig.t_gun.rotation * pitch); // pitch on the right.

            ////////    //          /////////////////////Clamps rotation but doesnt specify a starting rotation. Like for 30, it clamps it 15 degrees about the down vector.
            ////////    //          /////////////////////Maybe do a startRotation * clampedRotation in order to do 30 degrees about the starting forward vector.
            ////////    //          /////////////////////Also may need to clamp once after the first quaternion addition in addition to this one here.
            ////////    //rig.t_gun.rotation = ClampRotation(rig.t_gun.rotation, new Vector3(30, 0, 0));
            ////////}

            if(data.buttons[8].key)
            {
                if((data.buttons[8].keyDown && (u_useMode == Item.UseMode.SEMIAUTOMATIC || u_useMode == Item.UseMode.BURST)) || (data.buttons[8].key && u_useMode == Item.UseMode.AUTOMATIC))
                {
                    if(u_usesLeft > 0)
                    {
                        if(Time.time - u_lastTimeUsed > 1 / u_useRate)
                        {
                            Shoot();
                        }
                    }
                }
            }

            if(data.buttons[1].keyDown)
            {
                Debug.LogError("This way works but is very bad design so should fix it. It requires the player offset Vector3 to be public and also the InputManager.standbyController as well. Change these things! Also see Car.cs line 66 for the other part to change!");
                inputManager.standbyController.transform.position = transform.TransformPoint(inputManager.standbyController.GetComponent<PlayerInput>().localStandbyControlOffset);
                inputManager.standbyController.transform.forward = transform.forward;

                //Since we are expecting the player to be the standby Controller, we can pass in null and the InputManager will know to reactivate the player when they exit
                inputManager.RequestTransfer(null);
                GetComponentInChildren<CinemachineFreeLook>().Priority -= 1;
                GetComponentInChildren<CameraController>().enabled = false;
                Debug.LogError("Exitting Turret");
            }
        }
    }

    private void Shoot()
    {
        //Ray ray = new Ray(rig.t_shootOrigin.position, rig.t_shootOrigin.forward);
        Ray ray = new Ray(rig.t_guntip.position, -rig.t_guntip.up);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 500f, 1 << LayerMask.NameToLayer("BodyPart"), QueryTriggerInteraction.Collide) || Physics.Raycast(ray, out hit, 500f, ~(1 << LayerMask.NameToLayer("BodyPart")), QueryTriggerInteraction.Ignore))
        {
            GameObject g;
            Destroy(g = Instantiate(bulletImpact), 1.75f);
            g.transform.position = hit.point;
            g.transform.forward = Vector3.up;

            BodyPart hitBodyPart;
            Car c;

            if(hitBodyPart = hit.transform.GetComponent<BodyPart>())
            {
                hitBodyPart.TakeDamage(null, damage);
            }
            else if(c = hit.collider.GetComponent<Car>())
            {
                c.stats.Damage(damage);
            }
        }

        //new GameObject().AddComponent<AudioSource>().PlayOneShot(shootSound);
        audioSource.PlayOneShot(shootSound);
        cinemachineCamera.GetComponent<CinemachineImpulseSource>().GenerateImpulse();

        muzzleFlash.Play();

        foreach(physicsbone p in physicsbones)
            p.rb.AddForceAtPosition(p.force * p.localforcedir, p.localforceoffset);
        //r.AddRelativeTorque(50000f * Vector3.up, ForceMode.Force);

        u_usesLeft--;
        u_lastTimeUsed = Time.time;
    }

    private void OnDrawGizmos()
    {
        if(drawphysicsbones)
        {
            foreach(physicsbone p in physicsbones)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(p.localforceoffset, 0.01f);
                Gizmos.DrawLine(p.localforceoffset, p.localforceoffset + 0.025f * p.localforcedir);
            }
        }
    }

    public override void Disable()
    {
        active = false;
        newInput = false;
        GetComponentInChildren<CameraController>().enabled = false;
        //cinemachineCamera.Priority = 0;
    }
}
