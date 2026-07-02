namespace MMORPG.Game.Player.States
{
    public sealed class PlayerJumpState : PlayerStateBase
    {
        private float elapsedTime;

        public PlayerJumpState(PlayerContext context) : base(context)
        {
        }

        public override void Enter()
        {
            elapsedTime = 0f;
            Context.Motor.Jump();
            Context.Animation.PlayJump();
        }

        public override void Tick(float deltaTime)
        {
            elapsedTime += deltaTime;

            if (Context.Input.AttackPressed)
            {
                Context.Controller.EnterAttackState();
                return;
            }

            if (elapsedTime > 0.2f && Context.Motor.IsGrounded)
            {
                Context.Controller.EnterLocomotionByInput();
            }
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            Context.Motor.Move(Context.Input.Move, Context.Input.RunHeld, fixedDeltaTime);
        }
    }
}
