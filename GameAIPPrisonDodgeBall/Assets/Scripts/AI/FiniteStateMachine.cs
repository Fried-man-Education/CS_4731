using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAI
{

    public class FiniteStateMachine<T> : IFiniteStateMachine<T>
    {
        public IStateBase<T> CurrentState { get; private set; }

        public IState<T> InitialState { get; private set; }

        // array indexer. Example fsm["attack_state"]
        public IStateBase<T> this[string n]
        {
            get
            {
                if (States.TryGetValue(n, out IStateBase<T> state))
                    return state;
                else
                    throw new IndexOutOfRangeException();
            }
        }

        Dictionary<string, IStateBase<T>> States = new Dictionary<string, IStateBase<T>>();

        IState<T> GlobalTransitionState;

        T StateInitializationParameter;

        // constructor
        public FiniteStateMachine(T param)
        {
            StateInitializationParameter = param;
        }

        public DeferredStateTransition<T> CreateStateTransition(string stateName)
        {
            return CreateStateTransition(stateName, false);
        }

        public DeferredStateTransition<T> CreateStateTransition(string stateName, bool updateOnEnter)
        {
            var stateBase = this[stateName];

            var state = stateBase as IState<T>;

            if (state == null)
                throw new ArgumentException("State doesn't match argument signature");

            DeferredStateTransition<T> dst = new DeferredStateTransition<T>(state, updateOnEnter);
            return dst;
        }

        public DeferredStateTransition<T, S0> CreateStateTransition<S0>(string stateName, S0 arg0)
        {
            return CreateStateTransition<S0>(stateName, arg0, false);
        }

            public DeferredStateTransition<T, S0> CreateStateTransition<S0>(string stateName, S0 arg0, bool updateOnEnter)
        {
            var stateBase = this[stateName];

            var state = stateBase as IState<T, S0>;

            if (state == null)
                throw new ArgumentException("State doesn't match argument signature");

            DeferredStateTransition<T, S0> dst = new DeferredStateTransition<T, S0>(state, arg0, updateOnEnter);
            return dst;
        }

        public DeferredStateTransition<T, S0, S1> CreateStateTransition<S0, S1>(string stateName, S0 arg0, S1 arg1)
        {
            return CreateStateTransition(stateName, arg0, arg1, false);
        }

        public DeferredStateTransition<T, S0, S1> CreateStateTransition<S0, S1>(string stateName, S0 arg0, S1 arg1, bool updateOnEnter)
        {
            var stateBase = this[stateName];

            var state = stateBase as IState<T, S0, S1>;

            if (state == null)
                throw new ArgumentException("State doesn't match argument signature");

            DeferredStateTransition<T, S0, S1> dst = new DeferredStateTransition<T, S0, S1>(state, arg0, arg1, updateOnEnter);
            return dst;
        }

        public DeferredStateTransition<T, S0, S1, S2> CreateStateTransition<S0, S1, S2>(string stateName, S0 arg0, S1 arg1, S2 arg2)
        {
            return CreateStateTransition(stateName, arg0, arg1, arg2, false);
        }

        public DeferredStateTransition<T, S0, S1, S2> CreateStateTransition<S0, S1, S2>(string stateName, S0 arg0, S1 arg1, S2 arg2, bool updateOnEnter)
        {
            var stateBase = this[stateName];

            var state = stateBase as IState<T, S0, S1, S2>;

            if (state == null)
                throw new ArgumentException("State doesn't match argument signature");

            DeferredStateTransition<T, S0, S1, S2> dst = new DeferredStateTransition<T, S0, S1, S2>(state, arg0, arg1, arg2, updateOnEnter);
            return dst;
        }

        public DeferredStateTransition<T, S0, S1, S2, S3> CreateStateTransition<S0, S1, S2, S3>(string stateName, S0 arg0, S1 arg1, S2 arg2, S3 arg3)
        {
            return CreateStateTransition(stateName, arg0, arg1, arg2, arg3, false);
        }

        public DeferredStateTransition<T, S0, S1, S2, S3> CreateStateTransition<S0, S1, S2, S3>(string stateName, S0 arg0, S1 arg1, S2 arg2, S3 arg3, bool updateOnEnter)
        {
            var stateBase = this[stateName];

            var state = stateBase as IState<T, S0, S1, S2, S3>;

            if (state == null)
                throw new ArgumentException("State doesn't match argument signature");

            DeferredStateTransition<T, S0, S1, S2, S3> dst = new DeferredStateTransition<T, S0, S1, S2, S3>(state, arg0, arg1, arg2, arg3, updateOnEnter);
            return dst;
        }

        public void SetGlobalTransitionState(IState<T> state)
        {
            if (state == null)
                throw new System.ArgumentNullException("state must be non-null");

            GlobalTransitionState = state;

            //state.Init(this, StateInitializationParameter);
        }

        public void SetInitialState(IState<T> state)
        {
            if (state == null)
                throw new System.ArgumentNullException("state must be non-null");

            if (!States.ContainsKey(state.Name))
            {
                throw new System.ArgumentException("state not in finite state machine");
            }

            InitialState = state;
        }

        public void AddState(IStateBase<T> state, bool makeInitialState)
        {
            if (state == null)
                throw new System.ArgumentNullException("state must be non-null");

            States.Add(state.Name, state);

            if (makeInitialState || InitialState == null)
            {
                var stateNoArgs = state as IState<T>;

                if (makeInitialState && stateNoArgs == null)
                    throw new ArgumentException("Initial State must take no args on Enter()");

                if (stateNoArgs != null)
                {
                    InitialState = stateNoArgs;

                    //Debug.Log($"Initial state is: {InitialState.Name}");
                }
            }

            //state.Init(this, StateInitializationParameter);

        }

        public void AddState(IStateBase<T> state)
        {
            AddState(state, false);
        }


        protected void InitializeStates()
        {
            if (GlobalTransitionState != null)
                GlobalTransitionState.Init(this, StateInitializationParameter);

            foreach(var s in States)
            {
                if(s.Value != null)
                    s.Value.Init(this, StateInitializationParameter);
            }
        }

        bool firstTime = true;



        int numConsecutiveImmediateStateUpdates = 0;
        const int maxConsecutiveImmediateStateUpdates = 5;

        public int CurrNumConsecutiveImmediateStateUpdates
        {
            get => numConsecutiveImmediateStateUpdates;
        }

        public int MaxConsecutiveImmediateStateUpdates
        {
            get => maxConsecutiveImmediateStateUpdates;
        }

        public void Update()
        {
            if (firstTime)
            {
                firstTime = false;

                InitializeStates();

                if(GlobalTransitionState != null)
                    GlobalTransitionState.Enter();

                if (CurrentState == null)
                {
                    //Debug.Log($"Setting CurrentState to InitialState: {InitialState.Name}");
                    if (InitialState == null)
                        throw new ApplicationException("InitialState not set");

                    CurrentState = InitialState;
                    InitialState.Enter();
                }
            }

            bool updateOnEnter = false;

            numConsecutiveImmediateStateUpdates = 0;

            do
            {
                updateOnEnter = false;

                if (GlobalTransitionState != null)
                {

                    var stateTransition = GlobalTransitionState.Update();

                    if (stateTransition != null)
                    {
                        if (!States.TryGetValue(stateTransition.DeferredState.Name, out IStateBase<T> state))
                        {
                            throw new System.ArgumentException("state not in finite state machine");
                        }

                        //Debug.Log($"Global/Wildcard: Switching to state {stateName}");

                        if (CurrentState != null)

                        {
                            CurrentState.Exit(true);

                            CurrentState = stateTransition.DeferredState;//States[stateTransition];

                            //CurrentState.Enter(this);
                            stateTransition.Execute();
                        }

                    }
                }


                if (CurrentState == null)
                {
                    //Debug.Log($"Setting CurrentState to InitialState: {InitialState.Name}");
                    CurrentState = InitialState;
                    //CurrentState.Enter(this);
                    InitialState.Enter();
                }
                else
                {

                    var stateTransition = CurrentState.Update();

                    if (stateTransition != null)
                    {
                        if (!States.TryGetValue(stateTransition.DeferredState.Name, out IStateBase<T> state))
                        {
                            throw new System.ArgumentException("state not in finite state machine");
                        }

                        //Debug.Log($"Switching to state {stateTransition.DeferredState.Name}");

                        CurrentState.Exit();

                        //CurrentState = States[stateTransition];
                        CurrentState = stateTransition.DeferredState;

                        //CurrentState.Enter(this);
                        stateTransition.Execute();

                        updateOnEnter = stateTransition.UpdateOnEnter;
                    }
                }

                ++numConsecutiveImmediateStateUpdates;

                if (updateOnEnter && numConsecutiveImmediateStateUpdates >= maxConsecutiveImmediateStateUpdates)
                    Debug.LogWarning($"Number of updates on transition has reached limit ({numConsecutiveImmediateStateUpdates}). Last state transition will be delayed a frame!");

            } while (updateOnEnter && numConsecutiveImmediateStateUpdates < maxConsecutiveImmediateStateUpdates);
        }


    }

}