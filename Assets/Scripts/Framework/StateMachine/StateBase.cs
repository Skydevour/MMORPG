namespace MMORPG.Framework.StateMachine
{
    public abstract class StateBase : IState
    {
        public virtual void Enter()
        {
        }

        public virtual void Tick(float deltaTime)
        {
        }

        public virtual void FixedTick(float fixedDeltaTime)
        {
        }

        public virtual void Exit()
        {
        }
    }
}
