using MMORPG.Game.Characters;
using MMORPG.Game.Config;
using UnityEngine;

namespace MMORPG.Game.Player
{
    public sealed class PlayerStateDriver : CharacterStateDriverBase
    {
        private PlayerController2D controller;
        private MMORPG.Framework.Animation.FrameAnimator frameAnimator;
        private int lastJumpSequence;
        private float takeoffRemaining, landRemaining;
        private bool wasGrounded;

        public void Initialize(PlayerController2D playerController, PlayerSpriteAnimator animator)
        {
            controller = playerController;
            frameAnimator = animator.FrameAnimator;
            RegisterAnimationState("idle", animator.FrameAnimator);
            RegisterAnimationState("run", animator.FrameAnimator);
            RegisterAnimationState("jump", animator.FrameAnimator);
            foreach (var state in new[] { "takeoff", "rise", "apex", "fall", "land", "air_flip" })
                RegisterAnimationState(state, animator.FrameAnimator);
            RegisterAnimationState("dash", animator.FrameAnimator);
            RegisterAnimationState("shoot", animator.FrameAnimator);
            RegisterAnimationState("dead", animator.FrameAnimator);
            RegisterAnimationState("ghost", animator.FrameAnimator);
            RegisterAnimationState("parry", animator.FrameAnimator);
            RegisterAnimationState("hurt", animator.FrameAnimator);
            SetInitialState("idle");
        }

        protected override string ResolveStateName()
        {
            if (controller == null) return "idle";
            var timing = GameConfigService.Current.animation.hero;
            bool newJump = controller.JumpSequence != lastJumpSequence;
            takeoffRemaining = Mathf.Max(0f, takeoffRemaining - Time.deltaTime);
            landRemaining = Mathf.Max(0f, landRemaining - Time.deltaTime);
            if (controller.JumpSequence != lastJumpSequence)
            {
                lastJumpSequence = controller.JumpSequence;
                takeoffRemaining = controller.IsAirFlipping ? 0f : timing.takeoff;
                landRemaining = 0f;
            }
            if (!wasGrounded && controller.IsGrounded) landRemaining = timing.land;
            wasGrounded = controller.IsGrounded;
            if (controller.IsHurt || controller.IsParrying || controller.IsDashing) takeoffRemaining = 0f;
            string resolved = ResolveAnimation();
            if (newJump && (resolved == "takeoff" || resolved == "air_flip")) frameAnimator.Play(resolved, true);
            return resolved;
        }

        protected override void TickState(float deltaTime)
        {
        }

        private string ResolveAnimation()
        {
            if (controller == null)
            {
                return "idle";
            }

            if (controller.IsDead)
            {
                return controller.IsGhost ? "ghost" : "dead";
            }

            if (controller.IsDashing)
            {
                return "dash";
            }

            if (controller.IsUsingSuper)
            {
                return "shoot";
            }

            if (controller.IsParrying) return "parry";
            if (controller.IsHurt) return "hurt";

            if (!controller.IsGrounded)
            {
                if (controller.IsAirFlipping) return "air_flip";
                if (takeoffRemaining > 0f) return "takeoff";
                float threshold = GameConfigService.Current.animation.hero.apexVelocity;
                return controller.VerticalVelocity > threshold ? "rise" : controller.VerticalVelocity < -threshold ? "fall" : "apex";
            }

            if (landRemaining > 0f && !controller.IsMoving && !controller.IsShooting) return "land";

            return controller.IsMoving ? "run" : controller.IsShooting ? "shoot" : "idle";
        }
    }
}
