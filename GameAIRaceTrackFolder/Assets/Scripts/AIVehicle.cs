// Unfortunately Agent is a class rather than an interface, which makes things a pain
//#define USE_MLAGENTS
#undef USE_MLAGENTS

/*
 * This code is part of Arcade Car Physics for Unity by Saarg (2018)
 * 
 * This is distributed under the MIT Licence (see LICENSE.md for details)
 * 
 * AIVehicle is based on WheelVehicle
 */
using System;
using System.Collections;
using System.Collections.Generic;

using System.Diagnostics;

using UnityEngine;

using Unity.MLAgents;

using PathCreation.Examples;



#if MULTIOSCONTROLS
    using MOSC;
#endif

namespace GameAI
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PathTracker))]
    public class AIVehicle :

#if USE_MLAGENTS
        Agent
#else
        MonoBehaviour
#endif

    {


        protected string StudentName { get; set; }

        [Header("Inputs")]
#if MULTIOSCONTROLS
        [SerializeField] PlayerNumber playerId;
#endif
        // If isPlayer is false inputs are ignored
        [SerializeField] bool isPlayer = true;
        public bool IsPlayer { get { return isPlayer; } set { isPlayer = value; } }

        // Input names to read using GetAxis
        [SerializeField] string throttleInput = "Throttle";
        [SerializeField] string brakeInput = "Brake";
        [SerializeField] string turnInput = "Horizontal";
        [SerializeField] string jumpInput = "Jump";
        [SerializeField] string driftInput = "Drift";
        [SerializeField] string boostInput = "Boost";

        /* 
         *  Turn input curve: x real input, y value used
         *  My advice (-1, -1) tangent x, (0, 0) tangent 0 and (1, 1) tangent x
         */
        [SerializeField] AnimationCurve turnInputCurve = AnimationCurve.Linear(-1.0f, -1.0f, 1.0f, 1.0f);

        [Header("Wheels")]
        [SerializeField] WheelCollider[] driveWheel;
        public WheelCollider[] DriveWheel { get { return driveWheel; } }
        [SerializeField] WheelCollider[] turnWheel;

        public WheelCollider[] TurnWheel { get { return turnWheel; } }

        // This code checks if the car is grounded only when needed and the data is old enough
        bool isGrounded = false;
        int lastGroundCheck = 0;
        public bool IsGrounded
        {
            get
            {
                if (lastGroundCheck == Time.frameCount)
                    return isGrounded;

                lastGroundCheck = Time.frameCount;
                isGrounded = true;
                foreach (WheelCollider wheel in wheels)
                {
                    if (!wheel.gameObject.activeSelf || !wheel.isGrounded)
                        isGrounded = false;
                }
                return isGrounded;
            }
        }

        [Header("Behaviour")]
        /*
         *  Motor torque represent the torque sent to the wheels by the motor with x: speed in km/h and y: torque
         *  The curve should start at x=0 and y>0 and should end with x>topspeed and y<0
         *  The higher the torque the faster it accelerate
         *  the longer the curve the faster it gets
         */
        [SerializeField] AnimationCurve motorTorque = new AnimationCurve(new Keyframe(0, 200), new Keyframe(50, 300), new Keyframe(200, 0));

        // Differential gearing ratio
        [Range(2, 16)]
        [SerializeField] float diffGearing = 4.0f;
        public float DiffGearing { get { return diffGearing; } set { diffGearing = value; } }

        // Basicaly how hard it brakes
        [SerializeField] float brakeForce = 1500.0f;
        public float BrakeForce { get { return brakeForce; } set { brakeForce = value; } }

        // Max steering hangle, usualy higher for drift car
        [Range(0f, 50.0f)]
        [SerializeField] float steerAngle = 30.0f;
        public float SteerAngle { get { return steerAngle; } set { steerAngle = Mathf.Clamp(value, 0.0f, 50.0f); } }

        // The value used in the steering Lerp, 1 is instant (Strong power steering), and 0 is not turning at all
        [Range(0.001f, 1.0f)]
        [SerializeField] float steerSpeed = 0.2f;
        public float SteerSpeed { get { return steerSpeed; } set { steerSpeed = Mathf.Clamp(value, 0.001f, 1.0f); } }

        // How hight do you want to jump?
        [Range(1f, 1.5f)]
        [SerializeField] float jumpVel = 1.3f;
        public float JumpVel { get { return jumpVel; } set { jumpVel = Mathf.Clamp(value, 1.0f, 1.5f); } }

        // How hard do you want to drift?
        [Range(0.0f, 2f)]
        [SerializeField] float driftIntensity = 1f;
        public float DriftIntensity { get { return driftIntensity; } set { driftIntensity = Mathf.Clamp(value, 0.0f, 2.0f); } }

        // Reset Values
        Vector3 spawnPosition;
        Quaternion spawnRotation;

        /*
         *  The center of mass is set at the start and changes the car behavior A LOT
         *  I recomment having it between the center of the wheels and the bottom of the car's body
         *  Move it a bit to the from or bottom according to where the engine is
         */
        [SerializeField] Transform centerOfMass;

        // Force aplied downwards on the car, proportional to the car speed
        [Range(0.5f, 10f)]
        [SerializeField] float downforce = 1.0f;
        public float Downforce { get { return downforce; } set { downforce = Mathf.Clamp(value, 0, 5); } }

        // When IsPlayer is false you can use this to control the steering
        float steering;
        public float Steering { get { return steering; } set { steering = Mathf.Clamp(value, -1f, 1f); } }

        // When IsPlayer is false you can use this to control the throttle
        float throttle;
        public float Throttle { get { return throttle; } set { throttle = Mathf.Clamp(value, -1f, 1f); } }

        // Like your own car handbrake, if it's true the car will not move
        [SerializeField] bool handbrake;
        public bool Handbrake { get { return handbrake; } set { handbrake = value; } }

        // Use this to disable drifting
        [HideInInspector] public bool allowDrift = true;
        bool drift;
        public bool Drift { get { return drift; } set { drift = value; } }

        // Use this to read the current car speed (you'll need this to make a speedometer)
        [SerializeField] float speed = 0.0f;
        public float Speed { get { return speed; } }

        [SerializeField] float averageSpeed = 0.0f;
        public float AverageSpeed { get => averageSpeed; }

        [Header("Particles")]
        // Exhaust fumes
        [SerializeField] ParticleSystem[] gasParticles;

        [Header("Boost")]
        // Disable boost
        [HideInInspector] public bool allowBoost = true;

        // Maximum boost available
        [SerializeField] float maxBoost = 10f;
        public float MaxBoost { get { return maxBoost; } set { maxBoost = value; } }

        // Current boost available
        [SerializeField] float boost = 10f;
        public float Boost { get { return boost; } set { boost = Mathf.Clamp(value, 0f, maxBoost); } }

        // Regen boostRegen per second until it's back to maxBoost
        [Range(0f, 1f)]
        [SerializeField] float boostRegen = 0.2f;
        public float BoostRegen { get { return boostRegen; } set { boostRegen = Mathf.Clamp01(value); } }

        /*
         *  The force applied to the car when boosting
         *  NOTE: the boost does not care if the car is grounded or not
         */
        [SerializeField] float boostForce = 5000;
        public float BoostForce { get { return boostForce; } set { boostForce = value; } }

        // Use this to boost when IsPlayer is set to false
        public bool boosting = false;
        // Use this to jump when IsPlayer is set to false
        public bool jumping = false;

        // Boost particles and sound
        [SerializeField] ParticleSystem[] boostParticles;
        [SerializeField] AudioClip boostClip;
        [SerializeField] AudioSource boostSource;


        [Header("HUD")]
        [SerializeField] protected bool OutputToHUD;

        [Header("Death and Dismemberment")]
        [SerializeField] float FallYPos = -5f;

        [SerializeField] float SpawnYPos = 1.5f;

        [Header("DEBUG")]


        // Private variables set at the start
        Rigidbody _rb;
        WheelCollider[] wheels;

        protected Vector3 AngularVelocity { get => _rb.angularVelocity; }

        protected PathTracker pathTracker;

        float startDist = 0f;

        private bool _isResetting = false;

        //Stopwatch stopwatch;
        float startTime;

        virtual protected void Awake()
        {
            pathTracker = GetComponent<PathTracker>();

            if (pathTracker == null) UnityEngine.Debug.LogError("No path tracker");

            //stopwatch = new Stopwatch();
        }


        // Init rigidbody, center of mass, wheels and more
        virtual protected void Start()
        {

            //stopwatch.Start();
            startTime = Time.timeSinceLevelLoad;

            startDist = pathTracker.totalDistanceTravelled;

            GameManager.Instance.StudentNameTMP.text += StudentName + System.Environment.NewLine;

#if MULTIOSCONTROLS
            Debug.Log("[ACP] Using MultiOSControls");
#endif
            if (boostClip != null)
            {
                boostSource.clip = boostClip;
            }

            boost = maxBoost;

            _rb = GetComponent<Rigidbody>();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;

            if (_rb != null && centerOfMass != null)
            {
                _rb.centerOfMass = centerOfMass.localPosition;
            }

            wheels = GetComponentsInChildren<WheelCollider>();

            // Set the motor torque to a non null value because 0 means the wheels won't turn no matter what
            foreach (WheelCollider wheel in wheels)
            {
                wheel.motorTorque = 0.0001f;
            }
        }


        //public float DB_Throttle;
        //public float DB_Steering;
        //public float DB_internal_steering;

        // Visual feedbacks and boost regen
        virtual protected void Update()
        {
            //DB_Throttle = Throttle;
            //DB_Steering = Steering;

            foreach (ParticleSystem gasParticle in gasParticles)
            {
                gasParticle.Play();
                ParticleSystem.EmissionModule em = gasParticle.emission;
                em.rateOverTime = handbrake ? 0 : Mathf.Lerp(em.rateOverTime.constant, Mathf.Clamp(150.0f * throttle, 30.0f, 100.0f), 0.1f);
            }

            if (isPlayer && allowBoost)
            {
                boost += Time.deltaTime * boostRegen;
                if (boost > maxBoost) { boost = maxBoost; }
            }


            // Get all the inputs!
            if (isPlayer)
            {
                // Accelerate & brake
                if (throttleInput != "" && throttleInput != null)
                {
                    throttle = GetInput(throttleInput) - GetInput(brakeInput);

                    throttle = Mathf.Clamp(throttle, -1f, 1f);

                    //UnityEngine.Debug.Log($"throttle: {GetInput(throttleInput)} brake: {GetInput(brakeInput)}");
                }
                // Boost
                boosting = (GetInput(boostInput) > 0.5f);
                // Turn
                steering = turnInputCurve.Evaluate(GetInput(turnInput));
                // Dirft
                drift = GetInput(driftInput) > 0 && _rb.velocity.sqrMagnitude > 100;
                // Jump
                jumping = GetInput(jumpInput) != 0;
            }

            steering *= steerAngle;

            //DB_internal_steering = steering;

            // Handle the oops #1 (fall) 
            if (_rb.transform.position.y < FallYPos)
            {
                if (!_isResetting)
                {
                    _isResetting = true;
                    UnityEngine.Debug.Log("OOPS:Fall");
                    ResetCar();
                }
            }
            else
            {
                _isResetting = false;
            }

            // Handle the oops #2 (truck flipped, possibly caught on edge)
            var tiltAngle = Vector3.Angle(transform.up, Vector3.up);
            if (tiltAngle > 20f)
            {
                if (!tilted)
                {
                    tilted = true;
                    timeOfTilt = Time.timeSinceLevelLoad;
                }

                //Debug.Log($"tilted by: {tiltAngle} for: {Time.timeSinceLevelLoad - timeOfTilt}");

                if (Time.timeSinceLevelLoad - timeOfTilt > tiltTimeout)
                {
                    tilted = false;
                    UnityEngine.Debug.Log("OOPS:Tilted");
                    ResetCar();
                }
            }
            else
            {
                tilted = false;
            }


            // Handle the oops #3 (truck not moving)

            var distTravelled = pathTracker.totalDistanceTravelled - prevDist;
            prevDist = pathTracker.totalDistanceTravelled;

            //Debug.Log($"distTrav: {distTravelled}");
            //distFakeSlidingAvg = distTravelled / fakeTimeLen +
            //    distFakeSlidingAvg * (fakeTimeLen - Time.deltaTime) / fakeTimeLen;

            //Debug.Log($"distfakeslidingavg: {distFakeSlidingAvg}");

            if (Speed < 0.5f)
            {
                if (!stopped)
                {
                    timeOfStop = Time.timeSinceLevelLoad;
                    stopped = true;
                }

                if (Time.timeSinceLevelLoad - timeOfStop > timeOfStopTimeout)
                {
                    stopped = false;
                    UnityEngine.Debug.Log("OOPS:Stopped");
                    ResetCar();
                }
            }
            else
            {
                stopped = false;
            }

            //if(distFakeSlidingAvg < 0.5f)
            //{
            //    if(!stopped)
            //    {
            //        timeOfStop = Time.timeSinceLevelLoad;
            //        stopped = true;
            //    }

            //    if (Time.timeSinceLevelLoad - timeOfStop > timeOfStopTimeout)
            //    {
            //        stopped = false;
            //        UnityEngine.Debug.Log("OOPS:Stopped");
            //        ResetCar();
            //    }
            //}
            //else
            //{
            //    stopped = false;
            //}


            // Handle oops #4 (turned around backwards)

            if (Vector3.Angle(transform.forward, pathTracker.closestPointDirectionOnPath) > 90f)
            {
                if (!isTurnedBackwards)
                {
                    timeOfBkwds = Time.timeSinceLevelLoad;
                    isTurnedBackwards = true;
                }

                //Debug.Log($"Backwards for: {Time.timeSinceLevelLoad - timeOfBkwds}");

                if (Time.timeSinceLevelLoad - timeOfBkwds > timeOfBkwdsTimeout)
                {
                    isTurnedBackwards = false;
                    UnityEngine.Debug.Log("OOPS:Backwards");
                    ResetCar();
                }
            }
            else
            {
                isTurnedBackwards = false;
            }

            var elpsSec = Time.timeSinceLevelLoad - startTime;//stopwatch.Elapsed.TotalSeconds;
            averageSpeed = (float)(3.6 * (pathTracker.totalDistanceTravelled - startDist) / elpsSec);


            var gm = GameManager.Instance;

            gm.Wipeouts = numResets;
            gm.KpHLTA = averageSpeed;
            gm.MetersTravelled = pathTracker.totalDistanceTravelled;

            if (OutputToHUD)
            {
                gm.ElapsedTMP.text = TimeSpan.FromSeconds((double)elpsSec).ToString(@"hh\:mm\:ss\.fff");//stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                gm.MetersPerSecTMP.text = Speed.ToString("0.0");

                //var elpsSec = stopwatch.Elapsed.TotalSeconds;
                //var avgSpd = 3.6 * pathTracker.totalDistanceTravelled / elpsSec;
                gm.MetersPerSecLTATMP.text = averageSpeed.ToString("0.0");
                gm.TotalMetersTMP.text = pathTracker.totalDistanceTravelled.ToString("0.0");
                gm.WipeoutsTMP.text = numResets.ToString();


            }

        }

        protected int numResets = 0;

        protected bool tilted = false;
        protected float timeOfTilt = 0f;
        protected float tiltTimeout = 5f;


        protected bool stopped = false;
        protected float timeOfStop = 0f;
        protected float timeOfStopTimeout = 5f;

        protected float prevDist = 0;

        // public float distFakeSlidingAvg = 10f;

        protected float fakeTimeLen = 5f;

        protected bool isTurnedBackwards = false;
        protected float timeOfBkwds = 0f;
        protected float timeOfBkwdsTimeout = 5f;

        protected virtual void ResetCar()
        {

            ++numResets;

            tilted = false;
            stopped = false;
            isTurnedBackwards = false;

            // distFakeSlidingAvg = 10f;

            // make room for the car chassis to fit on road
            float minAllowedDist = 3f;

            if (pathTracker.distanceTravelled < minAllowedDist)
            {
                pathTracker.ResetToDistance(minAllowedDist);
            }

            if (pathTracker.distanceTravelled > (pathTracker.MaxPathDistance - minAllowedDist))
            {
                pathTracker.ResetToDistance(pathTracker.MaxPathDistance - minAllowedDist);
            }

            _rb.MovePosition(pathTracker.closestPointOnPath + Vector3.up * SpawnYPos);

            var rotPose = Quaternion.LookRotation(pathTracker.closestPointDirectionOnPath, Vector3.up);

            _rb.MoveRotation(rotPose);

            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.ResetInertiaTensor();

        }



        // Update everything
        virtual protected void FixedUpdate()
        {
            // Mesure current speed
            speed = transform.InverseTransformDirection(_rb.velocity).z * 3.6f;



            // Direction
            foreach (WheelCollider wheel in turnWheel)
            {
                wheel.steerAngle = Mathf.Lerp(wheel.steerAngle, steering, steerSpeed * 100f * Time.fixedDeltaTime);
            }

            foreach (WheelCollider wheel in wheels)
            {
                wheel.brakeTorque = 0;
            }

            // Handbrake
            if (handbrake)
            {
                foreach (WheelCollider wheel in wheels)
                {
                    // Don't zero out this value or the wheel completly lock up
                    wheel.motorTorque = 0.0001f;
                    wheel.brakeTorque = brakeForce;
                }
            }
            else if (Mathf.Abs(speed) < 4 || Mathf.Sign(speed) == Mathf.Sign(throttle))
            {
                foreach (WheelCollider wheel in driveWheel)
                {
                    wheel.motorTorque = throttle * motorTorque.Evaluate(speed) * diffGearing / driveWheel.Length;
                }
            }
            else
            {
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.brakeTorque = Mathf.Abs(throttle) * brakeForce;
                }
            }

            // Jump
            if (jumping && isPlayer)
            {
                if (!IsGrounded)
                    return;

                _rb.velocity += transform.up * jumpVel;
            }

            // Boost
            if (boosting && allowBoost && boost > 0.1f)
            {
                _rb.AddForce(transform.forward * boostForce);

                boost -= Time.fixedDeltaTime;
                if (boost < 0f) { boost = 0f; }

                if (boostParticles.Length > 0 && !boostParticles[0].isPlaying)
                {
                    foreach (ParticleSystem boostParticle in boostParticles)
                    {
                        boostParticle.Play();
                    }
                }

                if (boostSource != null && !boostSource.isPlaying)
                {
                    boostSource.Play();
                }
            }
            else
            {
                if (boostParticles.Length > 0 && boostParticles[0].isPlaying)
                {
                    foreach (ParticleSystem boostParticle in boostParticles)
                    {
                        boostParticle.Stop();
                    }
                }

                if (boostSource != null && boostSource.isPlaying)
                {
                    boostSource.Stop();
                }
            }

            // Drift
            if (drift && allowDrift)
            {
                Vector3 driftForce = -transform.right;
                driftForce.y = 0.0f;
                driftForce.Normalize();

                if (steering != 0)
                    driftForce *= _rb.mass * speed / 7f * throttle * steering / steerAngle;
                Vector3 driftTorque = transform.up * 0.1f * steering / steerAngle;


                _rb.AddForce(driftForce * driftIntensity, ForceMode.Force);
                _rb.AddTorque(driftTorque * driftIntensity, ForceMode.VelocityChange);
            }

            // Downforce
            _rb.AddForce(-transform.up * speed * downforce);
        }

        // Reposition the car to the start position
        public void ResetPos()
        {
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;

            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        public void toogleHandbrake(bool h)
        {
            handbrake = h;
        }

        // MULTIOSCONTROLS is another package I'm working on ignore it I don't know if it will get a release.
#if MULTIOSCONTROLS
        private static MultiOSControls _controls;
#endif

        // Use this method if you want to use your own input manager
        private float GetInput(string input)
        {
#if MULTIOSCONTROLS
        return MultiOSControls.GetValue(input, playerId);
#else
            return Input.GetAxis(input);
#endif
        }

#if USE_MLAGENTS
        // impl to make warning shut up
        public override void Heuristic(float[] actionsOut)
        {
            // This is just for testing with a human
        }
#endif

    }
}
