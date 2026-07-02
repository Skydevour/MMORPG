namespace MMORPG.Game.Player.States
{
    public sealed class PlayerAttackState : PlayerStateBase
    {
        private readonly float duration;
        private float elapsedTime;

        public PlayerAttackState(PlayerContext context, float duration) : base(context)
        {
            this.duration = duration;
        }

        public override void Enter()
        {
            elapsedTime = 0f;
            Context.Animation.PlayAttack();
        }

        public override void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;
            if (elapsedTime >= duration)
            {
                Context.Controller.EnterLocomotionByInput();
            }
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            Context.Motor.MoveWithoutInput(fixedDeltaTime);
        }
    }
}
