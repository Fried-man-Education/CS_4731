namespace GameAI
{
    public interface IFiniteStateMachine<T>
    {

        IStateBase<T> CurrentState { get;  }
        IState<T> InitialState { get;  }
        DeferredStateTransition<T> CreateStateTransition(string stateName, bool updateOnEnter);
        DeferredStateTransition<T, S0> CreateStateTransition<S0>(string stateName, S0 arg0, bool updateOnEnter);
        DeferredStateTransition<T, S0, S1> CreateStateTransition<S0, S1>(string stateName, S0 arg0, S1 arg1, bool updateOnEnter);
        DeferredStateTransition<T, S0, S1, S2> CreateStateTransition<S0, S1, S2>(string stateName, S0 arg0, S1 arg1, S2 arg2, bool updateOnEnter);
        DeferredStateTransition<T, S0, S1, S2, S3> CreateStateTransition<S0, S1, S2, S3>(string stateName, S0 arg0, S1 arg1, S2 arg2, S3 arg3, bool updateOnEnter);
        DeferredStateTransition<T> CreateStateTransition(string stateName);
        DeferredStateTransition<T, S0> CreateStateTransition<S0>(string stateName, S0 arg0);
        DeferredStateTransition<T, S0, S1> CreateStateTransition<S0, S1>(string stateName, S0 arg0, S1 arg1);
        DeferredStateTransition<T, S0, S1, S2> CreateStateTransition<S0, S1, S2>(string stateName, S0 arg0, S1 arg1, S2 arg2);
        DeferredStateTransition<T, S0, S1, S2, S3> CreateStateTransition<S0, S1, S2, S3>(string stateName, S0 arg0, S1 arg1, S2 arg2, S3 arg3);

    }
}
