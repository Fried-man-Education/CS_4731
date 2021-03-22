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

   
        protected override void Awake()
        {
            base.Awake();

            StudentName = "George P. Burdell";

            // Only the AI can control. No humans allowed!
            IsPlayer = false;

            // TODO: You can initialize a bunch of Fuzzy stuff here


        }


        override protected void Update()
        {

            // TODO Do all your Fuzzy stuff here and pass the defuzzified values to 
            // the car like so:
            // Throttle = someValue; //[-1f, 1f] -1 is full brake, 0 is neutral, 1 is full throttle
            // Steering = someValue; // [-1f, 1f] -1 if full left, 0 is neutral, 1 is full right



            // recommend you keep the base call at the end, after all your FuzzyVehicle code so that
            // control inputs can be processed properly (Throttle, Steering)
            base.Update();
        }

    }
}
