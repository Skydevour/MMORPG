using MMORPG.Game.Characters;

namespace MMORPG.Game.Player
{
    public sealed class PlayerStateDriver : CharacterStateDriverBase
    {
        private PlayerController2D controller;

        public void Initialize(PlayerController2D playerController, PlayerSpriteAnimator animator)
        {
            controller = playerController;
            RegisterAnimationState("idle", animator.FrameAnimator);
            RegisterAnimationState("run", animator.FrameAnimator);
            RegisterAnimationState("jump", animator.FrameAnimator);
            RegisterAnimationState("dash", animator.FrameAnimator);
            RegisterAnimationState("shoot", animator.FrameAnimator);
            RegisterAnimationState("dead", animator.FrameAnimator);
            SetInitialState("idle");
        }

        protected override string ResolveStateName()
        {
            if (controller == null)
            {
                return "idle";
            }

            if (controller.IsDead)
            {
                return "dead";
            }

            if (controller.IsDashing)
            {
                return "dash";
            }

            if (controller.IsUsingSuper)
            {
                return "shoot";
            }

            if (controller.IsShooting)
            {
                return "shoot";
            }

            if (!controller.IsGrounded)
            {
                return "jump";
            }

            return controller.IsMoving ? "run" : "idle";
        }
    }
}
