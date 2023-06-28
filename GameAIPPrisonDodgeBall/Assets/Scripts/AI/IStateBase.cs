namespace GameAI
{
    public interface IStateBase<T>
    {
        string Name { get; }
        void Init(IFiniteStateMachine<T> fsm, T param);
        DeferredStateTransitionBase<T> Update();
        void Exit(bool globalTransition);
        void Exit();
    }
}