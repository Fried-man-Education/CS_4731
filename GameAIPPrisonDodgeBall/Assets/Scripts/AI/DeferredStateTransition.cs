namespace GameAI
{

    public sealed class DeferredStateTransition<T> : DeferredStateTransitionBase<T>
    {
        public IState<T> DeferredStateWithArgs { get; private set; }

        public DeferredStateTransition(IState<T> state, bool updateOnEnter) : base(state, updateOnEnter)
        {
            DeferredStateWithArgs = state;
        }

        public DeferredStateTransition(IState<T> state) : this(state, false)
        { }

        public override void Execute()
        {
            DeferredStateWithArgs.Enter();
        }
    }


    public sealed class DeferredStateTransition<T, S0> : DeferredStateTransitionBase<T>
    {
        public IState<T, S0> DeferredStateWithArgs { get; private set; }
        public S0 Arg0 { get; set; }

        public DeferredStateTransition(IState<T, S0> state, S0 arg0, bool updateOnEnter) : base(state, updateOnEnter)
        {
            DeferredStateWithArgs = state;
            Arg0 = arg0;
        }

        public DeferredStateTransition(IState<T, S0> state, S0 arg0) : this(state, arg0, false)
        { }

        public override void Execute()
        {
            DeferredStateWithArgs.Enter(Arg0);
        }
    }

    public sealed class DeferredStateTransition<T, S0, S1> : DeferredStateTransitionBase<T>
    {
        public IState<T, S0, S1> DeferredStateWithArgs { get; private set; }
        public S0 Arg0 { get; set; }
        public S1 Arg1 { get; set; }

        public DeferredStateTransition(IState<T, S0, S1> state, S0 arg0, S1 arg1, bool updateOnEnter) : base(state, updateOnEnter)
        {
            DeferredStateWithArgs = state;
            Arg0 = arg0;
            Arg1 = arg1;
        }

        public DeferredStateTransition(IState<T, S0, S1> state, S0 arg0, S1 arg1) : this(state, arg0, arg1, false)
        { }

        public override void Execute()
        {
            DeferredStateWithArgs.Enter(Arg0, Arg1);
        }
    }

    public sealed class DeferredStateTransition<T, S0, S1, S2> : DeferredStateTransitionBase<T>
    {
        public IState<T, S0, S1, S2> DeferredStateWithArgs { get; private set; }
        public S0 Arg0 { get; set; }
        public S1 Arg1 { get; set; }
        public S2 Arg2 { get; set; }

        public DeferredStateTransition(IState<T, S0, S1, S2> state, S0 arg0, S1 arg1, S2 arg2, bool updateOnEnter) : base(state, updateOnEnter)
        {
            DeferredStateWithArgs = state;
            Arg0 = arg0;
            Arg1 = arg1;
            Arg2 = arg2;
        }

        public DeferredStateTransition(IState<T, S0, S1, S2> state, S0 arg0, S1 arg1, S2 arg2) : this(state, arg0, arg1, arg2, false)
        { }


        public override void Execute()
        {
            DeferredStateWithArgs.Enter(Arg0, Arg1, Arg2);
        }
    }

    public sealed class DeferredStateTransition<T, S0, S1, S2, S3> : DeferredStateTransitionBase<T>
    {
        public IState<T, S0, S1, S2, S3> DeferredStateWithArgs { get; private set; }
        public S0 Arg0 { get; set; }
        public S1 Arg1 { get; set; }
        public S2 Arg2 { get; set; }
        public S3 Arg3 { get; set; }

        public DeferredStateTransition(IState<T, S0, S1, S2, S3> state, S0 arg0, S1 arg1, S2 arg2, S3 arg3, bool updateOnEnter) : base(state, updateOnEnter)
        {
            DeferredStateWithArgs = state;
            Arg0 = arg0;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        public DeferredStateTransition(IState<T, S0, S1, S2, S3> state, S0 arg0, S1 arg1, S2 arg2, S3 arg3) : this(state, arg0, arg1, arg2, arg3, false)
        { }


        public override void Execute()
        {
            DeferredStateWithArgs.Enter(Arg0, Arg1, Arg2, Arg3);
        }
    }
}