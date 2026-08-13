using MMORPG.Framework.Animation;
using MMORPG.Framework.StateMachine;

namespace MMORPG.Game.Characters
{
    public sealed class CharacterAnimationState : IState
    {
        private readonly FrameAnimator animator;

        public CharacterAnimationState(string name, FrameAnimator animator)
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
