namespace MMORPG.Game.Player.States
{
    public sealed class PlayerMoveState : PlayerStateBase
    {
        public PlayerMoveState(PlayerContext context) : base(context)
        {
        }

        public override void Enter()
        {
            PlayLocomotion();
        }

        public override void Tick(float deltaTime)
        {
            if (TryEnterHighPriorityState())
            {
                return;
            }

            if (!Context.Input.HasMoveInput)
            {
                Context.Controller.EnterIdleState();
                return;
            }

            PlayLocomotion();
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            Context.Motor.Move(Context.Input.Move, Context.Input.RunHeld, fixedDeltaTime);
        }

        private void PlayLocomotion()
        {
            if (Context.Input.RunHeld)
            {
                Context.Animation.PlayRun();
            }
            else
            {
                Context.Animation.PlayWalk();
            }
        }
    }
}
