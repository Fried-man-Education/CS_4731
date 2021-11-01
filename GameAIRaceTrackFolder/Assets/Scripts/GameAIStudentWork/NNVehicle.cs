using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;

using GameAI;
using Unity.MLAgents.Actuators;

namespace GameAICourse
{


    public class NNVehicle : AIVehicleNN
    {

        // How long does a training episode last?
        [Header("NN Stuff")]
        public float EpisodeLenSec = 360f;
        bool episodeEndPending = false;
        float episodeStart = 0f;

        float normSpdFactor = 1f / 100f;
        float lastDistTraveled = 0f;
        float lastDistSampleTime = 0f;

        public float DB_RewardTotal = 0f;


        protected override void Awake()
        {
            base.Awake();

            StudentName = "George P. Burdell";

            // Only the AI can control. No humans allowed!
            IsPlayer = false;

        }

        protected override void Start()
        {
            base.Start();

            lastDistTraveled = pathTracker.totalDistanceTravelled;
            lastDistSampleTime = Time.timeSinceLevelLoad;

        }

        protected void Rewards()
        {
            // Conditionally add a reward if deserved (negative for punishment)
            // Ex: AddReward(v);

            var episodeTime = Time.timeSinceLevelLoad - episodeStart;

            // If time is up, we reward for finishing and start a new episode
            if (episodeTime > EpisodeLenSec)
            {
                var r = 1f;
                AddReward(r);

                DB_RewardTotal += r;

                EndEpisode();
            }
            else
            {
                // The following is an example of rewarding the NN based 
                // on how fast it is going down the track
                // Note that the reward is normalized so that it is generally 
                // in the range of [0,1]
                // This code can be revised!

                var r = pathTracker.totalDistanceTravelled - lastDistTraveled;

                var dt = Time.timeSinceLevelLoad - lastDistSampleTime;

                if (!Mathf.Approximately(dt, 0f))
                {
                    var normAverageSpeed = 3.6f * r / dt * normSpdFactor;

                    r = normAverageSpeed * 0.001f;

                    AddReward(r);

                    DB_RewardTotal += r;
                }

                lastDistTraveled = pathTracker.totalDistanceTravelled;
                lastDistSampleTime = Time.timeSinceLevelLoad;
            }
        }


        // This is called at the beginning of each episode
        public override void OnEpisodeBegin()
        {
            // Just need to reset some stats
            // Feel free to revise as needed

            episodeStart = Time.timeSinceLevelLoad;

            lastDistTraveled = pathTracker.totalDistanceTravelled;
            lastDistSampleTime = episodeStart;

            DB_RewardTotal = 0f;

            episodeEndPending = false;
        }

        // Sometimes the car needs to be reset but not due to a mistake.
        // This happens when an episode ends
        protected void NoPunishResetCar()
        {
            base.ResetCar();

            // just in case these are still set
            Throttle = 0f;
            Steering = 0f;

            EndEpisode();
        }


        // ResetCar() gets called by the parent class if the
        // car goes off the road, flips over, gets turned around, stuck, etc.
        // The NN agent is punished for this, and the episode ends.
        protected override void ResetCar()
        {
            base.ResetCar();

            Debug.Log("RESET CAR");
            // just in case these are still set
            Throttle = 0f;
            Steering = 0f;

            // Punish the bad agent and end the episode

            var v = -1f;
            AddReward(v);

            DB_RewardTotal += v;

            EndEpisode();
        }



        public override void CollectObservations(VectorSensor sensor)
        {

            // TODO in CollectObservations(), you must pass along game state that 
            // the NN will use for input to decide on an action to take. It is also
            // used for training. The observations that are passed along should generally
            // be normalized to be [-1, 1] range. See the MLAgents documentation for
            // more details.
            // Observations are add like so:
            //sensor.AddObservation(...)
            // Observations added must be exactly the same each frame and match
            // the Behavior Parameters Vector Observation Space Size in the Inspector
            // view of Unity.

            // Also Rewards can be given here. The end of episodes
            // must be deferred (see example Rewards() impl)

            Rewards();


        }


        public override void OnActionReceived(ActionBuffers actions)
        {
            // Apply the inputs
            var vectorAction = actions.ContinuousActions;
            Steering = vectorAction[0];
            Throttle = vectorAction[1];
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            // This is just for testing with a human if Heuristic mode
            // is enabled
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = Input.GetAxis("Horizontal");
            continuousActionsOut[1] = Input.GetAxis("Vertical");
        }

        override protected void Update()
        {

            if(episodeEndPending)
            {
                NoPunishResetCar();
                episodeEndPending = false;
            }

            // recommend you keep the base call at the end, after all your Vehicle code so that
            // control inputs can be processed properly (Throttle, Steering)

            base.Update();
        }

    }

}
