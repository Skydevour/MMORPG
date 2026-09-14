using MMORPG.Framework.Animation;
using MMORPG.Game.Characters;

namespace MMORPG.Game.Bosses.Potato
{
    // 控制器先推进并派发，状态驱动随后采样，最后由动画器显示同一时钟。
    [UnityEngine.DefaultExecutionOrder(50)]
    public sealed class PotatoBossStateDriver : CharacterStateDriverBase
    {
        private PotatoBossController controller;
        private FrameAnimator frameAnimator;

        public void Initialize(PotatoBossController bossController, FrameAnimator animator)
        {
            controller = bossController;
            frameAnimator = animator;
            RegisterAnimationState("idle", animator);
            RegisterAnimationState("attack_spit", animator);
            RegisterAnimationState("dead", animator);
            SetInitialState("idle");
        }

        protected override string ResolveStateName()
        {
            return ResolveAnimation();
        }

        protected override void TickState(float deltaTime)
        {
            frameAnimator.ExternalClock = controller.IsAttacking;
            if (controller.IsAttacking) frameAnimator.SampleNormalized(controller.AttackVisualProgress);
        }

        private string ResolveAnimation()
        {
            if (controller == null)
            {
                return "idle";
            }

            if (controller.IsDead)
            {
                return "dead";
            }

            return controller.IsAttacking ? "attack_spit" : "idle";
        }
    }
}
