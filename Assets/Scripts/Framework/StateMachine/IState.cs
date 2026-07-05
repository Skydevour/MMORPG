namespace MMORPG.Framework.StateMachine
{
    public interface IState
    {
        string Name { get; }

        void Enter();

        void Tick(float deltaTime);

        void FixedTick(float fixedDeltaTime);

        void Exit();
    }
}
