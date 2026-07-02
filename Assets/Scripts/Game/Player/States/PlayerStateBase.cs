using MMORPG.Framework.StateMachine;

namespace MMORPG.Game.Player.States
{
    public abstract class PlayerStateBase : StateBase
    {
        protected PlayerStateBase(PlayerContext context)
        {
            Context = context;
        }

        protected PlayerContext Context { get; }

        protected bool TryEnterHighPriorityState()
        {
            if (Context.Input.AttackPressed)
            {
                Context.Controller.EnterAttackState();
                return true;
            }

            if (Context.Input.JumpPressed && Context.Motor.IsGrounded)
            {
                Context.Controller.EnterJumpState();
                return true;
            }

            return false;
        }
    }
}
