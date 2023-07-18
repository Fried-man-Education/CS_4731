// Remove the line above if you are submitting to GradeScope for a grade. But leave it if you only want to check
// that your code compiles and the autograder can access your public methods.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GameAI;

// All the Fuzz
using Tochas.FuzzyLogic;
using Tochas.FuzzyLogic.MembershipFunctions;
using Tochas.FuzzyLogic.Evaluators;
using Tochas.FuzzyLogic.Mergers;
using Tochas.FuzzyLogic.Defuzzers;
using Tochas.FuzzyLogic.Expressions;

namespace GameAICourse
{

    public class FuzzyVehicle : AIVehicle
    {

        // TODO create some Fuzzy Set enumeration types, and member variables for:
        // Fuzzy Sets (input and output), one or more Fuzzy Value Sets, and Fuzzy
        // Rule Sets for each output.
        // Also, create some methods to instantiate each of the member variables

        // Here are some basic examples to get you started
        enum FzOutputThrottle {Brake, Coast, Accelerate }
        enum FzOutputWheel { TurnLeft, Straight, TurnRight }

        enum FzInputSpeed { Slow, Medium, Fast }
        enum FzVehiclePosition {LeftHard, Left, Middle, Right, RightHard}
        enum FzVehicleDirection {Left, Straight, Right}
        enum FzFuturePoint {Left, Straight, Right}

        FuzzySet<FzInputSpeed> fzSpeedSet;
        FuzzySet<FzVehiclePosition> fzPositionSet;
        FuzzySet<FzVehicleDirection> fzDirectionSet;
        FuzzySet<FzFuturePoint> fzPointSet;

        FuzzySet<FzOutputThrottle> fzThrottleSet;
        FuzzyRuleSet<FzOutputThrottle> fzThrottleRuleSet;

        FuzzySet<FzOutputWheel> fzWheelSet;
        FuzzyRuleSet<FzOutputWheel> fzWheelRuleSet;

        FuzzyValueSet fzInputValueSet = new FuzzyValueSet();

        // These are used for debugging (see ApplyFuzzyRules() call
        // in Update()
        FuzzyValueSet mergedThrottle = new FuzzyValueSet();
        FuzzyValueSet mergedWheel = new FuzzyValueSet();



        private FuzzySet<FzInputSpeed> GetSpeedSet()
        {
            FuzzySet<FzInputSpeed> set = new FuzzySet<FzInputSpeed>();

            set.Set(
                FzInputSpeed.Slow,
                new ShoulderMembershipFunction(
                    0f, 
                    new Coords(0f, 1f), 
                    new Coords(25f, 0f), 
                    100f
                )
            );
            set.Set(
                FzInputSpeed.Medium,
                new TriangularMembershipFunction(
                    new Coords(25f, 0f), 
                    new Coords(50f, 1f), 
                    new Coords(100f, 0f)
                )
            );
            set.Set(
                FzInputSpeed.Fast,
                new ShoulderMembershipFunction(
                    0f, 
                    new Coords(50f, 0f), 
                    new Coords(100f, 1f), 
                    100f
                )
            );

            return set;
        }

        private FuzzySet<FzOutputThrottle> GetThrottleSet()
        {

            FuzzySet<FzOutputThrottle> set = new FuzzySet<FzOutputThrottle>();

            set.Set(
                FzOutputThrottle.Brake,
                new ShoulderMembershipFunction(
                    -1f, 
                    new Coords(-1f, 1f), 
                    new Coords(-0.25f, 0f), 
                    1f
                )
            );
            set.Set(
                FzOutputThrottle.Coast,
                new TriangularMembershipFunction(
                    new Coords(-0.25f, 0f),
                    new Coords(0f, 1f),
                    new Coords(0.25f, 0f)
                )
            );
            set.Set(
                FzOutputThrottle.Accelerate,
                new ShoulderMembershipFunction(
                    -1.0f, 
                    new Coords(0.25f, 0f), 
                    new Coords(1.0f, 1.0f), 
                    1.0f
                )
            );

            return set;
        }

        private FuzzySet<FzOutputWheel> GetWheelSet()
        {

            FuzzySet<FzOutputWheel> set = new FuzzySet<FzOutputWheel>();

            set.Set(
                FzOutputWheel.TurnLeft,
                new ShoulderMembershipFunction(
                    -0.75f, 
                    new Coords(-0.75f, 1f), 
                    new Coords(-0.25f, 0f), 
                    0.75f
                )
            );
            set.Set(
                FzOutputWheel.Straight,
                new TriangularMembershipFunction(
                    new Coords(-0.25f, 0f),
                    new Coords(0f, 1f),
                    new Coords(0.25f, 0f)
                )
            );
            set.Set(
                FzOutputWheel.TurnRight,
                new ShoulderMembershipFunction(
                    -0.75f, 
                    new Coords(0.25f, 0f), 
                    new Coords(0.75f, 1.0f), 
                    0.75f
                )
            );

            return set;
        }


        private FuzzyRule<FzOutputThrottle>[] GetThrottleRules()
        {

            FuzzyRule<FzOutputThrottle>[] rules =
            {
                // TODO: Add some rules. Here is an example
                // (Note: these aren't necessarily good rules)
                // Left
                If(And(
                    FzInputSpeed.Slow, 
                    FzVehicleDirection.Left
                )).Then(FzOutputThrottle.Coast),
                If(And(
                    FzInputSpeed.Medium, 
                    FzVehicleDirection.Left
                )).Then(FzOutputThrottle.Coast),
                If(And(
                    FzInputSpeed.Fast, 
                    FzVehicleDirection.Left
                )).Then(FzOutputThrottle.Brake),
                // Straight
                If(And(
                    FzInputSpeed.Slow, 
                    FzVehicleDirection.Straight
                )).Then(FzOutputThrottle.Accelerate),
                If(And(
                    FzInputSpeed.Medium, 
                    FzVehicleDirection.Straight
                )).Then(FzOutputThrottle.Accelerate),
                If(And(
                    FzInputSpeed.Fast, 
                    FzVehicleDirection.Straight
                )).Then(FzOutputThrottle.Coast),
                // Right
                If(And(
                    FzInputSpeed.Slow, 
                    FzVehicleDirection.Right
                )).Then(FzOutputThrottle.Coast),
                If(And(
                    FzInputSpeed.Medium, 
                    FzVehicleDirection.Right
                )).Then(FzOutputThrottle.Coast),
                If(And(
                    FzInputSpeed.Fast, 
                    FzVehicleDirection.Right
                )).Then(FzOutputThrottle.Brake),
                // More example syntax
                //If(And(FzInputSpeed.Fast, Not(FzFoo.Bar)).Then(FzOutputThrottle.Accelerate),
            };

            return rules;
        }

        private FuzzyRule<FzOutputWheel>[] GetWheelRules()
        {

            FuzzyRule<FzOutputWheel>[] rules =
            {
                If(FzFuturePoint.Left)
                    .Then(FzOutputWheel.TurnLeft),
                If(FzFuturePoint.Straight)
                    .Then(FzOutputWheel.Straight),
                If(FzFuturePoint.Right)
                    .Then(FzOutputWheel.TurnRight),
                If(FzVehicleDirection.Left)
                    .Then(FzOutputWheel.TurnRight),
                If(FzVehicleDirection.Straight)
                    .Then(FzOutputWheel.Straight),
                If(FzVehicleDirection.Right)
                    .Then(FzOutputWheel.TurnLeft),
            };

            return rules;
        }

        private FuzzyRuleSet<FzOutputThrottle> GetThrottleRuleSet(FuzzySet<FzOutputThrottle> throttle)
        {
            var rules = this.GetThrottleRules();
            return new FuzzyRuleSet<FzOutputThrottle>(throttle, rules);
        }

        private FuzzyRuleSet<FzOutputWheel> GetWheelRuleSet(FuzzySet<FzOutputWheel> wheel)
        {
            var rules = this.GetWheelRules();
            return new FuzzyRuleSet<FzOutputWheel>(wheel, rules);
        }


        protected override void Awake()
        {
            base.Awake();

            StudentName = "Andrew Friedman";

            // Only the AI can control. No humans allowed!
            IsPlayer = false;

        }

        protected override void Start()
        {
            base.Start();

            // TODO: You can initialize a bunch of Fuzzy stuff here
            fzSpeedSet = this.GetSpeedSet();

            // Vehicle's Position Set Setup
            fzPositionSet = new FuzzySet<FzVehiclePosition>();
            fzPositionSet.Set(
                FzVehiclePosition.LeftHard,
                new ShoulderMembershipFunction(
                    -2.5f, 
                    new Coords(1.5f, 0f), 
                    new Coords(2.5f, 1f), 
                    2.5f
                )
            );
            fzPositionSet.Set(
                FzVehiclePosition.Left,
                new TriangularMembershipFunction(
                    new Coords(0.5f, 0f), 
                    new Coords(1f, 1f),
                    new Coords(1.5f, 0f)
                )
            );
            fzPositionSet.Set(
                FzVehiclePosition.Middle,
                new TriangularMembershipFunction(
                    new Coords(-1f, 0f), 
                    new Coords(0f, 1f), 
                    new Coords(1, 0f)
                )
            );
            fzPositionSet.Set(
                FzVehiclePosition.Right,
                new TriangularMembershipFunction(
                    new Coords(-1.5f, 0f),
                    new Coords(-1f, 1f),
                    new Coords(0.5f, 0f)
                )
            );
            fzPositionSet.Set(
                FzVehiclePosition.RightHard,
                new ShoulderMembershipFunction(
                    -2.5f, 
                    new Coords(-2.5f, 1f), 
                    new Coords(-1.5f, 0f), 
                    2.5f
                )
            );
            // Vehicle's Direction Set Setup
            fzDirectionSet = new FuzzySet<FzVehicleDirection>();
            fzDirectionSet.Set(
                FzVehicleDirection.Left,
                new ShoulderMembershipFunction(
                    -40f, 
                    new Coords(10f, 0f), 
                    new Coords(40f, 1.0f), 
                    40f
                )
            );
            fzDirectionSet.Set(
                FzVehicleDirection.Straight,
                new TriangularMembershipFunction(
                    new Coords(-10f, 0f), 
                    new Coords(0f, 1f), 
                    new Coords(10f, 0f)
                )
            );
            fzDirectionSet.Set(
                FzVehicleDirection.Right,
                new ShoulderMembershipFunction(
                    -40f, 
                    new Coords(-40f, 1f), 
                    new Coords(-10f, 0f), 
                    40f
                )
            );
            // Vehicle's Point Set Setup
            fzPointSet = new FuzzySet<FzFuturePoint>();
            fzPointSet.Set(
                FzFuturePoint.Left,
                new ShoulderMembershipFunction(
                    -60f, 
                    new Coords(-60f, 1f), 
                    new Coords(-10f, 0f), 
                    60f
                )
            );
            fzPointSet.Set(
                FzFuturePoint.Straight,
                new TriangularMembershipFunction(
                    new Coords(-10f, 0f), 
                    new Coords(0f, 1f), 
                    new Coords(10f, 0f)
                )
            );
            fzPointSet.Set(
                FzFuturePoint.Right,
                new ShoulderMembershipFunction(
                    -60f, 
                    new Coords(10f, 0f), 
                    new Coords(60f, 1f), 
                    60f
                )
            );

            fzThrottleSet = this.GetThrottleSet();
            fzThrottleRuleSet = this.GetThrottleRuleSet(fzThrottleSet);

            fzWheelSet = this.GetWheelSet();
            fzWheelRuleSet = this.GetWheelRuleSet(fzWheelSet);
        }

        System.Text.StringBuilder strBldr = new System.Text.StringBuilder();

        override protected void Update()
        {

            // TODO Do all your Fuzzy stuff here and then
            // pass your fuzzy rule sets to ApplyFuzzyRules()
            
            // Remove these once you get your fuzzy rules working.
            // You can leave one hardcoded while you work on the other.
            // Both steering and throttle must be implemented with variable
            // control and not fixed/hardcoded!

            fzPositionSet.Evaluate(
                Math.Sign(
                    Vector3.SignedAngle(
                        transform.position - pathTracker.closestPointOnPath,
                        pathTracker.closestPointDirectionOnPath,
                        Vector3.up
                    )
                ) * Vector2.Distance(
                    new Vector2(
                        transform.position.x, 
                        transform.position.z
                    ),
                    new Vector2(
                        pathTracker.closestPointOnPath.x, 
                        pathTracker.closestPointOnPath.z
                    )
                ),
                fzInputValueSet
            );
            fzDirectionSet.Evaluate(
                Vector3.SignedAngle(
                    transform.forward,
                    pathTracker.closestPointDirectionOnPath,
                    Vector3.up
                ),
                fzInputValueSet
            );
            fzPointSet.Evaluate(
                Vector3.SignedAngle(
                    transform.position + 5 * transform.forward - transform.position,
                    (
                        pathTracker.pathCreator.path.GetPointAtDistance(
                            pathTracker.distanceTravelled + Speed / 3.6f
                        ) - transform.position
                    ),
                    Vector3.up
                ),
                fzInputValueSet
            );
            // Simple example of fuzzification of vehicle state
            // The Speed is fuzzified and stored in fzInputValueSet
            fzSpeedSet.Evaluate(Speed, fzInputValueSet);

            // ApplyFuzzyRules evaluates your rules and assigns Thottle and Steering accordingly
            // Also, some intermediate values are passed back for debugging purposes
            // Throttle = someValue; //[-1f, 1f] -1 is full brake, 0 is neutral, 1 is full throttle
            // Steering = someValue; // [-1f, 1f] -1 if full left, 0 is neutral, 1 is full right

            ApplyFuzzyRules<FzOutputThrottle, FzOutputWheel>(
                fzThrottleRuleSet,
                fzWheelRuleSet,
                fzInputValueSet,
                // access to intermediate state for debugging
                out var throttleRuleOutput,
                out var wheelRuleOutput,
                ref mergedThrottle,
                ref mergedWheel
                );

            // recommend you keep the base Update call at the end, after all your FuzzyVehicle code so that
            // control inputs can be processed properly (e.g. Throttle, Steering)
            base.Update();
        }

    }
}
