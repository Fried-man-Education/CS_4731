namespace GameAI
{
    public interface IState<T> : IStateBase<T>
    {
        void Enter();
    }

    public interface IState<T, S0> : IStateBase<T>
    {
        void Enter(S0 s0);
    }

    public interface IState<T, S0, S1> : IStateBase<T>
    {
        void Enter(S0 s0, S1 s1);
    }

    public interface IState<T, S0, S1, S2> : IStateBase<T>
    {
        void Enter(S0 s0, S1 s1, S2 s2);
    }

    public interface IState<T, S0, S1, S2, S3> : IStateBase<T>
    {
        void Enter(S0 s0, S1 s1, S2 s2, S3 s3);
    }
}
