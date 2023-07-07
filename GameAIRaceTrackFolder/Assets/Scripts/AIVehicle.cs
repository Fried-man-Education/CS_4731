// Unfortunately Agent is a class rather than an interface, which makes things a pain
//#define USE_MLAGENTS
#undef USE_MLAGENTS
#define USE_FANCY_EFFECTS

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
using Unity.MLAgents.Actuators;

using PathCreation.Examples;

// All the Fuzz
using Tochas.FuzzyLogic;
using Tochas.FuzzyLogic.MembershipFunctions;
using Tochas.FuzzyLogic.Evaluators;
using Tochas.FuzzyLogic.Mergers;
using Tochas.FuzzyLogic.Defuzzers;
using Tochas.FuzzyLogic.Expressions;


#if MULTIOSCONTROLS
    using MOSC;
#endif

namespace GameAI
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PathTracker))]
    public partial class AIVehicle :

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
        public float Steering {
            get { return steering; }

#if USE_MLAGENTS
            set { steering = steerAngle * Mathf.Clamp(value, -1f, 1f); }
#else
            set
            {        	
                if(IsPlayer)    
                    steering = Mathf.Clamp(value, -1f, 1f);
            }
        
#endif
		}

        private float InternalSteering
        {
            get { return steering; }
            set { steering = Mathf.Clamp(value, -1f, 1f); }
        }
        
        // When IsPlayer is false you can use this to control the throttle
        float throttle;
        public float Throttle {
            get { return throttle; }

#if USE_MLAGENTS
			set { throttle = Mathf.Clamp(value, -1f, 1f); }
#else		
			set {
                if(IsPlayer)
                    throttle = Mathf.Clamp(value, -1f, 1f);
            } 
#endif
        }

        private float InternalThrottle
        {
            get { return throttle; }
            set
            {
                throttle = Mathf.Clamp(value, -1f, 1f);
            }
        }

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
        [SerializeField] protected bool outputToHUD;

        [SerializeField] protected bool freezeHUDAtTime;

        [SerializeField] protected int freezeHUDSeconds = 5 * 60;

        [SerializeField] protected RectTransform vizInputMarker;

        [SerializeField] protected TMPro.TextMeshProUGUI vizText;


        public bool OutputToHUD { get => outputToHUD; set => outputToHUD = value; }

        public bool FreezeHUDAtTime { get => freezeHUDAtTime; set => freezeHUDAtTime = value; }

        public int FreezeHUDSeconds { get => freezeHUDSeconds; set => freezeHUDSeconds = value; }

        public RectTransform VizInputMarker { get => vizInputMarker;
            set {
                UnityEngine.Debug.Log("SETTING");
                vizInputMarker = value;
            } }

        public TMPro.TextMeshProUGUI VizText { get => vizText; set => vizText = value; }



        [Header("Death and Dismemberment")]
        [SerializeField] float FallYPos = -5f;

        [SerializeField] float SpawnYPos = 1.5f;

        [Header("DEBUG")]


        // Private variables set at the start
        Rigidbody _rb;
        WheelCollider[] wheels;

        protected Vector3 AngularVelocity { get => _rb.angularVelocity; }

        protected Vector3 Velocity { get => _rb.velocity; }


        protected PathTracker pathTracker;

        float startDist = 0f;

        private bool _isResetting = false;

        //Stopwatch stopwatch;
        float startTime;

        [Header("DEBUG")]
        public float DB_Throttle;
        public float DB_Steering;
        //public float DB_internal_steering;

        public void ApplyFuzzyRules<T, S>(
            FuzzyRuleSet<T> throttleFRS, 
            FuzzyRuleSet<S> steerFRS,
            FuzzyValueSet fuzzyValueSet,
            out FuzzyValue<T>[] throttleRuleOutput,
            out FuzzyValue<S>[] steeringRuleOutput,
            ref FuzzyValueSet mergedThrottle,
            ref FuzzyValueSet mergedSteering
            ) 
            where T: struct, IConvertible where S: struct, IConvertible
        {

            // Manually evaluate so we can probe each step for debugging purposes

            if (!hardCodeThrottle)
            {
                throttleRuleOutput = throttleFRS.RuleEvaluator.EvaluateRules(throttleFRS.Rules, fuzzyValueSet);
                var throttleMerger = throttleFRS.OutputsMerger;//new CachedOutputsFuzzyValuesMerger<T>();
                throttleMerger.MergeValues(throttleRuleOutput, mergedThrottle);
                var throttleDefuzz = throttleFRS.Defuzzer;
                InternalThrottle = throttleDefuzz.Defuzze(throttleFRS.OutputVarSet, mergedThrottle);
            }
            else
            {
                throttleRuleOutput = null;
                InternalThrottle = hardCodedThrottleVal;
            }

            if (!hardCodeSteering)
            {
                steeringRuleOutput = steerFRS.RuleEvaluator.EvaluateRules(steerFRS.Rules, fuzzyValueSet);
                var steeringMerger = steerFRS.OutputsMerger;//new CachedOutputsFuzzyValuesMerger<T>();
                steeringMerger.MergeValues(steeringRuleOutput, mergedSteering);
                var steeringDefuzz = steerFRS.Defuzzer;
                InternalSteering = steeringDefuzz.Defuzze(steerFRS.OutputVarSet, mergedSteering);
            }
            else
            {
                steeringRuleOutput = null;
                InternalSteering = hardCodedSteeringVal;
            }

        }


        virtual protected void Awake()
        {
            pathTracker = GetComponent<PathTracker>();

            if (pathTracker == null) UnityEngine.Debug.LogError("No path tracker");

            //stopwatch = new Stopwatch();

            //if (vizInputMarker == null)
            //    UnityEngine.Debug.LogError("No input viz marker");
        }


        private bool wasPlayer = false;

        private bool hardCodeSteering = false;
        private bool reportedHardCodeSteering = false;
        private float hardCodedSteeringVal = 0f;
        private bool hardCodeThrottle = false;
        private bool reportedHardCodeThrottle = false;
        private float hardCodedThrottleVal = 0f;

        public Vector3[] vizInputCorners = new Vector3[4];
       

        // Init rigidbody, center of mass, wheels and more
        virtual protected void Start()
        {
            wasPlayer = IsPlayer;

            if (vizInputMarker != null)
            {
                var vizPar = vizInputMarker.parent.GetComponent<RectTransform>();
                vizPar.GetLocalCorners(vizInputCorners);
            }

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



            ResetCar(false);


            //stopwatch.Start();
            startTime = Time.timeSinceLevelLoad;

            startDist = pathTracker.totalDistanceTravelled;
        }


        protected void HardCodeThrottle(float v)
        {
            hardCodedThrottleVal = v;
            InternalThrottle = v;
            hardCodeThrottle = true;

            if (!reportedHardCodeThrottle)
            {
                reportedHardCodeThrottle = true;
                throw new UnityException("Hard coded throttle only allowed for testing.");
            }
        }

        protected void HardCodeSteering(float v)
        {
            hardCodedSteeringVal = v;
            InternalSteering = v;
            hardCodeSteering = true;

            if (!reportedHardCodeSteering)
            {
                reportedHardCodeSteering = true;
                throw new UnityException("Hard coded steering only allowed for testing.");
            }
        }


        // Visual feedbacks and boost regen
        virtual protected void Update()
        {
            DB_Throttle = Throttle;
            DB_Steering = Steering;

            if( IsPlayer && IsPlayer != wasPlayer)
            {
                throw new UnityException("Cheat detected!");
            }

#if USE_FANCY_EFFECTS
            foreach (ParticleSystem gasParticle in gasParticles)
            {
                gasParticle.Play();
                ParticleSystem.EmissionModule em = gasParticle.emission;
                em.rateOverTime = handbrake ? 0 : Mathf.Lerp(em.rateOverTime.constant, Mathf.Clamp(150.0f * throttle, 30.0f, 100.0f), 0.1f);
            }
#endif
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

#if !USE_MLAGENTS
                    throttle = Mathf.Clamp(throttle, -1f, 1f);

                    //UnityEngine.Debug.Log($"throttle: {GetInput(throttleInput)} brake: {GetInput(brakeInput)}");
#endif
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

#if !USE_MLAGENTS
            steering *= steerAngle;
#endif

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
            gm.MinThrottle = Mathf.Min(gm.MinThrottle, Throttle);
            gm.MaxThrottle = Mathf.Max(gm.MaxThrottle, Throttle);

            if (outputToHUD)
            {

                if (freezeHUDAtTime && (elpsSec > (float)freezeHUDSeconds))
                {
                    gm.ElapsedTMP.text = TimeSpan.FromSeconds((double)freezeHUDSeconds).ToString(@"hh\:mm\:ss\.fff");//stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                }
                else
                {
                    gm.ElapsedTMP.text = TimeSpan.FromSeconds((double)elpsSec).ToString(@"hh\:mm\:ss\.fff");//stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                    gm.MetersPerSecTMP.text = Speed.ToString("0.0");

                    //var elpsSec = stopwatch.Elapsed.TotalSeconds;
                    //var avgSpd = 3.6 * pathTracker.totalDistanceTravelled / elpsSec;
                    gm.MetersPerSecLTATMP.text = averageSpeed.ToString("0.0");
                    gm.TotalMetersTMP.text = pathTracker.totalDistanceTravelled.ToString("0.0");
                    gm.WipeoutsTMP.text = numResets.ToString();
                }


                if (vizInputMarker != null)
                {
                    if(!ContainsNaNorInf(Steering) && !ContainsNaNorInf(Throttle))
                        vizInputMarker.localPosition = new Vector3((vizInputCorners[2].x - vizInputCorners[0].x) * 0.5f * Steering / steerAngle, (vizInputCorners[2].y - vizInputCorners[0].y) * 0.5f * Throttle, 0f);

                }
            }

        }

        private bool ContainsNaNorInf(float v)
        {
            if (
                float.IsNaN(v) || float.IsInfinity(v) 
                )
            {
                return true;
            }

            return false;
        }

        private bool ContainsNaNorInf(Vector3 v)
        {
            if(
                float.IsNaN(v.x) || float.IsInfinity(v.x) ||
                float.IsNaN(v.y) || float.IsInfinity(v.y) ||
                float.IsNaN(v.z) || float.IsInfinity(v.z) 
                )
            {
                return true;
            }

            return false;
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
            ResetCar(true);
        }

        protected virtual void ResetCar(bool trackResetCountAndPreserveDist)
        {
            UnityEngine.Debug.Log("reset car");
            if (trackResetCountAndPreserveDist)
                ++numResets;

            tilted = false;
            stopped = false;
            isTurnedBackwards = false;

            // distFakeSlidingAvg = 10f;

            // make room for the car chassis to fit on road
            float minAllowedDist = 3f;

            if (!trackResetCountAndPreserveDist || pathTracker.distanceTravelled < minAllowedDist)
            {
                UnityEngine.Debug.Log("reset to min");
                pathTracker.ResetToDistance(minAllowedDist);
                pathTracker.ResetTotalDistance();
            }

            if (pathTracker.distanceTravelled > (pathTracker.MaxPathDistance - minAllowedDist))
            {
                UnityEngine.Debug.Log("reset to max");
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

        public void ToggleHandbrake(bool h)
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
        public override void OnActionReceived(ActionBuffers actions)
        {
            // This is just for testing with a human
        }


        // impl to make warning shut up
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            // This is just for testing with a human
        }

#endif

        static public void DiagnosticPrintFuzzyValueSet<T>(FuzzyValueSet fzset, System.Text.StringBuilder sb) where T : struct, IConvertible
        {
            Type typ = typeof(T);
            sb.AppendLine(typ.Name);

            if (fzset == null)
            {
                sb.AppendLine("null");
                return;
            }

            foreach (var e in System.Enum.GetValues(typ))
            {
                var v = fzset.Get<T>((T)e);

                //UnityEngine.Debug.Log($"{v.linguisticVariable}::{v.membershipDegree}");
                sb.AppendLine($"{v.linguisticVariable}::{v.membershipDegree:0.00}");

            }

        }

        static public void DiagnosticPrintRuleSet<T>(FuzzyRuleSet<T> fzRuleSet, FuzzyValue<T>[] fzRuleOutpts, System.Text.StringBuilder sb) where T : struct, IConvertible
        {
            Type typ = typeof(T);
            sb.AppendLine(typ.Name);

            if (fzRuleSet == null || fzRuleSet.Rules == null || fzRuleOutpts == null)
            {
                sb.AppendLine("null");
                return;
            }

            for (int i = 0; i < fzRuleOutpts.Length; ++i)// (var ro in ruleOutputs)
            {
                var ro = fzRuleOutpts[i];
                var r = fzRuleSet.Rules[i];
                sb.AppendLine($"{r.ToString()}::{ro.membershipDegree:0.00}::{ro.Confidence:0.00}");
            }

        }

    }
}
