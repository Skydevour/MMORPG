namespace MMORPG.Game.Player.States
{
    public sealed class PlayerIdleState : PlayerStateBase
    {
        private const float MinIdleVariantDelay = 4.5f;
        private const float MaxIdleVariantDelay = 7.5f;

        private float elapsedTime;
        private float nextVariantTime;
        private int nextVariantIndex;

        public PlayerIdleState(PlayerContext context) : base(context)
        {
        }

        public override void Enter()
        {
            elapsedTime = 0f;
            nextVariantTime = UnityEngine.Random.Range(MinIdleVariantDelay, MaxIdleVariantDelay);
            Context.Animation.PlayIdle();
        }

        public override void Tick(float deltaTime)
        {
            if (TryEnterHighPriorityState())
            {
                return;
            }

            if (Context.Input.HasMoveInput)
            {
                Context.Controller.EnterMoveState();
                return;
            }

            elapsedTime += deltaTime;
            if (elapsedTime >= nextVariantTime && Context.Animation.IdleVariantCount > 1)
            {
                // 长时间无输入时轮播 idle 变体，让角色保持有呼吸感，但不打断玩家操作。
                elapsedTime = 0f;
                nextVariantTime = UnityEngine.Random.Range(MinIdleVariantDelay, MaxIdleVariantDelay);
                Context.Animation.PlayIdleVariant(nextVariantIndex++);
            }
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            Context.Motor.MoveWithoutInput(fixedDeltaTime);
        }
    }
}
