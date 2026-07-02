using System;

namespace MMORPG.Framework.StateMachine
{
    public sealed class StateMachine
    {
        public IState CurrentState { get; private set; }

        public event Action<IState, IState> StateChanged;

        public void ChangeState(IState nextState)
        {
            if (nextState == null || ReferenceEquals(CurrentState, nextState))
            {
                return;
            }

            IState previousState = CurrentState;
            previousState?.Exit();
            CurrentState = nextState;
            CurrentState.Enter();
            StateChanged?.Invoke(previousState, CurrentState);
        }

        public void Tick(float deltaTime)
        {
            CurrentState?.Tick(deltaTime);
        }

        public void FixedTick(float fixedDeltaTime)
        {
            CurrentState?.FixedTick(fixedDeltaTime);
        }
    }
}
