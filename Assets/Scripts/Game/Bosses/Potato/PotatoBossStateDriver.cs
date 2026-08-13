using MMORPG.Framework.Animation;
using MMORPG.Game.Characters;

namespace MMORPG.Game.Bosses.Potato
{
    public sealed class PotatoBossStateDriver : CharacterStateDriverBase
    {
        private PotatoBossController controller;

        public void Initialize(PotatoBossController bossController, FrameAnimator animator)
        {
            controller = bossController;
            RegisterAnimationState("idle", animator);
            RegisterAnimationState("attack_spit", animator);
            RegisterAnimationState("dead", animator);
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

            return controller.IsAttacking ? "attack_spit" : "idle";
        }
    }
}