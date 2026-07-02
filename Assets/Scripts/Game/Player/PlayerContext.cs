using MMORPG.Framework.StateMachine;
using MMORPG.Game.Player.Motor;

namespace MMORPG.Game.Player
{
    public sealed class PlayerContext
    {
        public PlayerContext(PlayerController controller, ICharacterMotor motor, PlayerAnimationDriver animation)
        {
            Controller = controller;
            Motor = motor;
            Animation = animation;
            StateMachine = new StateMachine();
        }

        public PlayerController Controller { get; }
        public ICharacterMotor Motor { get; }
        public PlayerAnimationDriver Animation { get; }
        public StateMachine StateMachine { get; }
        public PlayerInputSnapshot Input => Controller.CurrentInput;
    }
}
