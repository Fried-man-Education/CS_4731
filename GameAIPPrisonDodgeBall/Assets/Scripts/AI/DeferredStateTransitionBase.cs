using System;
namespace GameAI
{
    public abstract class DeferredStateTransitionBase<T>
    {
        public IStateBase<T> DeferredState { get; private set; }
        public bool UpdateOnEnter { get; set; }

        public DeferredStateTransitionBase(IStateBase<T> state, bool updateOnEnter)
        {
            if (state == null)
                throw new ArgumentNullException("state cannot be null");

            DeferredState = state;
            UpdateOnEnter = updateOnEnter;
        }

        public DeferredStateTransitionBase(IStateBase<T> state):this(state, false)
        {}

        public abstract void Execute();
    }
}
