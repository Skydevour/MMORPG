using MMORPG.Framework.StateMachine;

namespace MMORPG.Game.Player
{
    public sealed class PlayerAnimationState : IState
    {
        private readonly PlayerSpriteAnimator animator;

        public PlayerAnimationState(string name, PlayerSpriteAnimator animator)
        {
            Name = name;
            this.animator = animator;
        }

        public string Name { get; }

        public void Enter()
        {
            animator.Play(Name);
        }

        public void Tick(float deltaTime)
        {
        }

        public void FixedTick(float fixedDeltaTime)
        {
        }

        public void Exit()
        {
        }
    }
}
