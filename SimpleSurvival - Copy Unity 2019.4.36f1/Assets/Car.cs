using System;
using UnityEngine;
using Cinemachine;


public class Car : Controller
{
    public Rigidbody rb;

    [Serializable]
    public struct VehicleStats
    {
        //Engine
        [SerializeField]
        private bool m_engineOn;
        public bool engineOn { get => m_engineOn; set { m_engineOn = value; onEngineStateChange?.Invoke(m_engineOn); } }
        public Action<bool> onEngineStateChange;

        //Health
        /// <summary>
        /// Current amount of health in units of generic health points (HP)
        /// </summary>
        [Space(7.5f)]
        public float health;
        /// <summary>
        /// Max amount of health in units of generic health points (HP)
        /// </summary>
        public float healthMax;

        //Gasoline
        /// <summary>
        /// Current amount of gasoline in units of gallons (gal)
        /// </summary>
        [Space(7.5f)]
        [SerializeField]
        private float m_gas;
        public float gas { get => m_gas; set { m_gas = value; if(m_gas <= 0) engineOn = false; } }
        /// <summary>
        /// Max amount of gasoline in units of gallons (gal). Average vehicle's max is ~15 gal.
        /// </summary>
        public float gasMax;
        /// <summary>
        /// How much gasoline is used idling in units of gallons per hour (gal/hr). Average is ~1/2 gal per hour.
        /// </summary>
        public float gasDrainIdle;
        //Maybe instead have stat for miles per gallon (mpg = mi/gal) in order to be able to accurately drain the gas based off of how far the vehicle is traveling
        /// <summary>
        /// How much gasoline is used driving in units of gallons per mile (gpm = gal/mi) (reciprocal of the standard miles per gallon (mpg)). This should be the drain rate when input is detected, 
        /// otherwise, gasDrainIdle should be used (when coasting or just idling). Average is ~0.025 gal/mi = ~40 mi/gal (mpg)
        /// Should probably increase/multiply this value for more reasonable in game purposes (i.e. player would not have to refill gas very often because it would take too long to actually drive 40 miles)
        /// </summary>
        public float gasDrainNormal;

        //Battery
        /// <summary>
        /// Current amount of battery in units of generic battery points (BP). This is mostly an arbitrary scale and value, will default to simply percentage points
        /// </summary>
        [Space(7.5f)]
        public float battery;
        /// <summary>
        /// Max amount of battery in units of generic battery points (BP). This is mostly an arbitrary scale and value, will default to simply percentage points
        /// </summary>
        public float batteryMax;

        public ParticleSystem engineSmoke;

        public VehicleStats(bool _engineOn, float _health, float _healthMax, float _gas, float _gasMax, float _gasDrainIdle, float _gasDrainNormal, float _battery, float _batteryMax)
        {
            m_engineOn = _engineOn;

            health = _health;
            healthMax = _healthMax;

            m_gas = _gas;
            gasMax = _gasMax;
            gasDrainIdle = _gasDrainIdle;
            gasDrainNormal = _gasDrainNormal;

            battery = _battery;
            batteryMax = _batteryMax;

            engineSmoke = null;
            onEngineStateChange = null;
        }

        public void Damage(float amount)
        {
            if(health > 0)
            {
                health -= amount;
            }

            if(engineSmoke)
            {
                //If health is >= 90% don't play the engine smoke effect
                if(health >= 0.9f * healthMax)
                {
                    engineSmoke.gameObject.SetActive(false);
                }
                //Else if health is <= 90% activate the engine smoke and multiply the smoke rate and speed by an amount relative to how low the health is
                else
                {
                    engineSmoke.gameObject.SetActive(true);

                    float x = health;
                    float rate = -0.75f * x + 75;
                    rate = Mathf.Clamp(rate, 10, 75);
                    float speed = -0.015f * x + 1.5f;
                    speed = Mathf.Clamp(speed, 0.3f, 1.5f);

                    //Accessing a ParticleSystem's Module and setting a variable. You don't need to set back the Module because it is a special type of struct that automatically sets
                    //the value to the ParticleSystem that it belongs to (see https://blog.unity.com/technology/particle-system-modules-faq#:~:text=in%20the%20future.-,Accessing%20Modules,-An%20example)
                    ParticleSystem.EmissionModule emission = engineSmoke.emission;
                    emission.rateOverTime = rate;
                    ParticleSystem.MainModule main = engineSmoke.main;
                    main.simulationSpeed = speed;
                }
            }

            if(health <= 0)
            {
                engineOn = false;
                onEngineStateChange?.Invoke(engineOn);
            }
        }
    }

    public VehicleStats stats = new VehicleStats(false, 100, 100, 15, 15, 0.5f, 0.025f, 100, 100);

    public WheelCollider FL, FR, BL, BR;
    public WheelCollider[] wheels;
    public Transform FLTransform, FRTransform, BLTransform, BRTransform;
    public Transform FrontAxle, BackAxle;

    public Transform centerOfMass;
    private Vector3 centerOfMassRestPosition;
    public float damper = 0.01f;
    public float downwardForce = 50f;

    /// <summary>
    /// When gas pedal is released on a car, the car's momentum is fighting the force of the gear that it currently is in. The idleBrakeTorque is this force, should probably make it gear/speed 
    /// dependent (i.e. if car is in 1st gear, the idleBrakeTorque is very high because 1st gear is a very strong gear, but if car is in 5th gear, idleBrakeTorque is pretty small because 5th gear 
    /// is a weaker gear). However, the car (if automatic transmission) will downshift anyways when gas is released so naturally idleBrakeTorque will respond to the change in gear anyways
    /// </summary>
    public float idleBrakeTorque = 4f;

    public float speed = 10f;
    public float brakeTorque = 10f;
    public float maxSteerAngle = 60f;
    public float burnoutSpeed = 5f;

    public float normalForwardStiffness = 1f;
    public float brakeForwardStiffness = 2f;

    public float normalSideStiffness = 1f;
    public float turnSideStiffness = 0.9f;

    public ParticleSystem[] tireSmokes;


    WheelFrictionCurve forwardCurve;

    bool burningOut;
    public float minTireSmokeRPM;
    public float tireSmokeOffset;
    public float BLSlip, BRSlip;

    //Temp. - used for testing switching controllers
    public InputManager inputManager;
    private InputData data;

    public Material headlightsOn, headlightsOff;



    private void Start()
    {
        if(rb && centerOfMass)
            rb.centerOfMass = centerOfMass.localPosition;

        centerOfMassRestPosition = centerOfMass.localPosition;

        wheels = new WheelCollider[4];

        wheels[0] = FL;
        wheels[1] = FR;
        wheels[2] = BL;
        wheels[3] = BR;

        /*
         * 
         * Maybe make Stabilize() method where if there is no steer input, try to lower the sideways component of velocity (sideways being local right direction) so there is no sideways slipping/drifting when going straight
         * 
         * Instead of limiting steering angle based on speed, change steering speed based on speed (i.e. faster steering at lower speeds and slower steering at higher speeds) so can still turn same amount, it will just take slightly longer to drift at 
         * higher speeds for example
         * 
         * 
         * 
         */

        //We know that the only Controller that is going to interact with a vehicle is going to be the player so we can safely assume some things
        GetComponentInChildren<InteractionHandler>().onInteract += (IItemUser i) => { Debug.LogError("This way works but is very bad design so should fix it. It requires the player offset Vector3 to be public and also the InputManager.standbyController as well. Change these things! Also see Car.cs line 66 for the other part to change!"); inputManager.RequestTransfer(this); GetComponentInChildren<CinemachineFreeLook>().Priority += 1; i.iGameObject.GetComponent<PlayerInput>().localStandbyControlOffset = transform.InverseTransformPoint(i.iGameObject.transform.position); };

        stats.engineSmoke = TransformHelper.FindRecursive(transform, "ps_enginesmoke")?.GetComponent<ParticleSystem>();
        stats.onEngineStateChange += (bool engineOn) => 
        { 
            if(engineOn)
                GetComponent<AudioSource>().Play();
            else
                GetComponent<AudioSource>().Stop();

            if(headlightsOn && headlightsOff)
            {
                Material[] materials;
                Renderer r;

                r = TransformHelper.FindRecursive(transform, "Body").GetComponent<Renderer>();
                materials = r.materials;
                materials[1] = engineOn ? headlightsOn : headlightsOff;
                r.materials = materials;

                r = TransformHelper.FindRecursive(transform, "Spotlight").GetComponent<Renderer>();
                materials = r.materials;
                materials[1] = engineOn ? headlightsOn : headlightsOff;
                r.materials = materials;
            }

            TransformHelper.FindRecursive(transform, "t_headlights")?.gameObject.SetActive(engineOn);
        };

        //Turn off the engine in the start in order to make sure the engine sound is off and the headlights are off too
        stats.engineOn = false;

        stats.engineOn = true; //temp.; so that the engine is already on when you enter so its easier to record gameplay
    }

    private void FixedUpdate()
    {
        //These things should always happen even if the Car is not actively being controlled (for example, always set the wheels' poses)
        if (rb && centerOfMass)
            rb.centerOfMass = centerOfMass.localPosition;

        UpdatePartPoses();

        rb.AddForceAtPosition(-transform.up * downwardForce * rb.velocity.magnitude, rb.worldCenterOfMass);

        if(active)
        {
            //Temp. - used for testing switching controllers
            //if(Input.GetKeyDown(KeyCode.E))
            //if(data.buttons[1])
            if (data.buttons[1].keyDown)
            {
                //This way works but is very bad design so should fix it. It requires the player offset Vector3 to be public and also the InputManager.standbyController as well. Change these things!!!!!!!
                //This way works but is very bad design so should fix it. It requires the player offset Vector3 to be public and also the InputManager.standbyController as well. Change these things!!!!!!!
                //This way works but is very bad design so should fix it. It requires the player offset Vector3 to be public and also the InputManager.standbyController as well. Change these things!!!!!!!
                //This way works but is very bad design so should fix it. It requires the player offset Vector3 to be public and also the InputManager.standbyController as well. Change these things!!!!!!!
                //This way works but is very bad design so should fix it. It requires the player offset Vector3 to be public and also the InputManager.standbyController as well. Change these things!!!!!!!
                //also see line 66 for the other part to change!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //also see line 66 for the other part to change!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //also see line 66 for the other part to change!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //also see line 66 for the other part to change!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //also see line 66 for the other part to change!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //also see line 66 for the other part to change!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //also see line 66 for the other part to change!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //also see line 66 for the other part to change!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                //see above; also for vehicle headlights make it find a child transform with a general/reusable name (like a naming convention; also like naming of bones for animation) such as
                //t_headlight or something so that it can find this headlight transform in order to toggle it when player presses the headlight button (same as flashlight key, which is F)
                Debug.LogError("This way works but is very bad design so should fix it. It requires the player offset Vector3 to be public and also the InputManager.standbyController as well. Change these things! Also see Car.cs line 66 for the other part to change!");
                inputManager.standbyController.transform.position = transform.TransformPoint(inputManager.standbyController.GetComponent<PlayerInput>().localStandbyControlOffset);
                inputManager.standbyController.transform.forward = transform.forward;

                //Since we are expecting the player to be the standby Controller, we can pass in null and the InputManager will know to reactivate the player when they exit
                inputManager.RequestTransfer(null);
                GetComponentInChildren<CinemachineFreeLook>().Priority -= 1;
                Debug.LogError("Exitting Vehicle");
            }

            if(Input.GetKeyDown(KeyCode.LeftControl))
            {
                stats.engineOn = !stats.engineOn;
            }

            WheelHit tireSmokeHit;
            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i].GetGroundHit(out tireSmokeHit))
                {
                    if (tireSmokeHit.forwardSlip > 0.99f && wheels[i].rpm >= minTireSmokeRPM)
                        tireSmokes[i].transform.position = tireSmokeHit.point + transform.forward * tireSmokeOffset;
                }
            }

            if (!burningOut)
            {
                //If engine is on and is able to drive (has enough gas, etc.)
                if(stats.engineOn && stats.health > 0 && stats.gas > 0 && stats.battery > 0)
                {
                    //Need to set the wheel motorTorque no matter if player is pressing gas or not
                    if (!isAirborne())
                    {
                        Debug.LogError("Not airborne");
                        foreach (WheelCollider w in wheels)
                        {
                            //w.motorTorque = Input.GetAxis("Vertical") * speed;
                            //New input handling system
                            w.motorTorque = data.axes[0].input * speed;

                            //If player releases gas pedal, make the car momentum fight the force of the current gear so that it will slow down more realistically
                            if(Mathf.Abs(data.axes[0].input) < 0.025f)
                            {
                                //Fix!!!! The idleBrakeTorque is not being applied or is being overridden only when the player is not pressing the gas. It works fine when they exit the vehicle though
                                //It possibly is the fact that the car sets the brakeforce to 0 if they are not pressing the brakes, but after uncommenting that part, the car wouldn't move
                                //maybe look at that part again
                                //If that is the problem, then maybe have booleans for 'hasInput' and 'isIdle' in order to check if idleBrakeForce should be applied
                                //For example
                                //if the car is idle (no gas pressed) AND there is no brakes applied, then apply idleBrakeForce
                                //if the car is idle AND there is brakes applied, then apply regular brake force
                                //if car is NOT idle AND brakes, then apply regular brake force
                                //if car is NOT idle AND NO brakes, then apply 0 brake force (because actively driving and pressing gas)

                                w.brakeTorque = idleBrakeTorque;
                                //Debug.LogError($"Idle; Input is { Mathf.Abs(data.axes[0].input) }");
                                Debug.LogError("Fix!!!! The idleBrakeTorque is not being applied or is being overridden only when the player is not pressing the gas. It works fine when they exit the vehicle though");
                            }
                            else
                            {
                                Debug.LogError($"NOT idle; Input is { Mathf.Abs(data.axes[0].input) }");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Airborne");
                        centerOfMass.localPosition = centerOfMassRestPosition + transform.right * data.axes[1].input * damper + transform.forward * data.axes[0].input * damper;
                    }

                    //Vehicle driving
                    if (Mathf.Abs(data.axes[0].input) > 0.01f)
                    {
                        //Does not really reflect the actual units of gallons per mile of the gas drain. This is just for testing purposes. Also, should probably multiply the gallons per mile value because
                        //currently is is equal to the real life average which is going to be too much in game (i.e. player would actually have to drive 40 miles until they have to refill gas)
                        stats.gas -= stats.gasDrainNormal * Time.fixedDeltaTime;
                    }
                    //Vehicle coasting or idling
                    else
                    {
                        stats.gas -= stats.gasDrainIdle * Time.fixedDeltaTime;
                    }
                }
                //Else is not able to drive so set motorTorque to 0 (or else the motorTorque will continue with its last value and never stop if just ran out of gas for example)
                else
                {
                    foreach (WheelCollider w in wheels)
                    {
                        w.motorTorque = 0f;
                        w.brakeTorque = idleBrakeTorque;
                    }
                }
            }

            //FL.steerAngle = Input.GetAxis("Horizontal") * maxSteerAngle;
            //FR.steerAngle = Input.GetAxis("Horizontal") * maxSteerAngle;

            //New input handling system
            FL.steerAngle = data.axes[1].input * maxSteerAngle;
            FR.steerAngle = data.axes[1].input * maxSteerAngle;

            //if(data.buttons[0] /*Input.GetKey(KeyCode.Space)*/ && !burningOut)
            if (data.buttons[0].key && !burningOut)
            {
                foreach (WheelCollider w in wheels)
                {
                    w.brakeTorque = brakeTorque;
                    forwardCurve = w.forwardFriction;
                    forwardCurve.stiffness = brakeForwardStiffness;
                    w.forwardFriction = forwardCurve;
                }
            }
            else if (!burningOut)
            {
                foreach (WheelCollider w in wheels)
                {
                    w.brakeTorque = 0f;
                    forwardCurve = w.forwardFriction;
                    forwardCurve.stiffness = normalForwardStiffness;
                    w.forwardFriction = forwardCurve;
                }
            }

            //If speed is slow enough AND holding brake AND holding forward, then Player is trying to burnout
            //if(rb.velocity.sqrMagnitude < burnoutSpeed * burnoutSpeed && data.buttons[0] /*Input.GetKey(KeyCode.Space)*/ && data.axes[0].input /*Input.GetAxis("Vertical")*/ > 0.05f)
            if (rb.velocity.sqrMagnitude < burnoutSpeed * burnoutSpeed && data.buttons[0].key && data.axes[0].input > 0.05f)
            {
                burningOut = true;
                FL.brakeTorque = FR.brakeTorque = speed * 2f; //Lock front wheels; set to speed * 2f so back wheels definitely do not overcome front wheels lock while burning out

                BL.brakeTorque = BR.brakeTorque = 0f;

                if(stats.engineOn && stats.health > 0 && stats.gas > 0 && stats.battery > 0)
                    if(!isAirborne())
                        BL.motorTorque = BR.motorTorque = data.axes[0].input /*Input.GetAxis("Vertical")*/ * speed;
                else
                    BL.motorTorque = BR.motorTorque = 0f;


                WheelHit hit;

                BL.GetGroundHit(out hit);
                BLSlip = hit.forwardSlip;

                BR.GetGroundHit(out hit);
                BRSlip = hit.forwardSlip;
            }
            else
            {
                burningOut = false;
            }

            WheelFrictionCurve curve = BL.sidewaysFriction;

            if (Mathf.Abs(data.axes[1].input /*Input.GetAxis("Horizontal")*/) < 0.005f)
                curve.stiffness = normalSideStiffness;
            else
                curve.stiffness = turnSideStiffness;

            BL.sidewaysFriction = BR.sidewaysFriction = curve;
        }
        else
        {
            //Set the motorTorque's to zero in order to make the car slow to a stop after player exits while driving (or else the motorTorque will continue to be the value before exitting)
            foreach(WheelCollider w in wheels)
            { 
                w.motorTorque = 0f;
                w.brakeTorque = idleBrakeTorque;
            }

            //If vehicle is not active but the engine is still on, then drain the gas
            if(stats.engineOn && stats.health > 0 && stats.gas > 0 && stats.battery > 0)
            {
                stats.gas -= stats.gasDrainIdle * Time.fixedDeltaTime;
            }
        }
    }

    private void UpdatePartPoses()
    {
        UpdateWheelPoses();
        UpdateAxelPoses();
    }

    private void UpdateAxelPoses()
    {
        FrontAxle.position = (FRTransform.position + FLTransform.position) / 2f;
        FrontAxle.right = FRTransform.position - FLTransform.position;

        BackAxle.position = (BRTransform.position + BLTransform.position) / 2f;
        BackAxle.right = BRTransform.position - BLTransform.position;
    }

    private void UpdateWheelPoses()
    {
        UpdateWheelPose(FL, FLTransform);
        UpdateWheelPose(FR, FRTransform);
        UpdateWheelPose(BL, BLTransform);
        UpdateWheelPose(BR, BRTransform);
    }

    private void UpdateWheelPose(WheelCollider col, Transform t)
    {
        Vector3 pos = t.position;
        Quaternion quat = t.rotation;

        col.GetWorldPose(out pos, out quat);

        t.position = pos;
        t.rotation = quat;
    }

    private bool isAirborne()
    {
        //If even one of the wheels is still grounded, then the vehicle is not airborne
        foreach(WheelCollider w in wheels)
        {
            if(w.isGrounded)
                return false;
        }

        return true;
    }

    public override void ReadInput(InputData data)
    {
        //Update local data to be used in FixedUpdate()
        this.data = data;
        newInput = true;
    }

    public override void Enable(InputManager manager)
    {
        inputManager = manager;
        active = true;
        cinemachineCamera.Priority = 10;
    }

    public override void Disable()
    {
        active = false;
        newInput = false;
        cinemachineCamera.Priority = 0;
    }
}
