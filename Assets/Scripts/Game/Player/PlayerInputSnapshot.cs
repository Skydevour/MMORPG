using UnityEngine;

namespace MMORPG.Game.Player
{
    public readonly struct PlayerInputSnapshot
    {
        public PlayerInputSnapshot(Vector2 move, bool runHeld, bool jumpPressed, bool attackPressed)
        {
            Move = move;
            RunHeld = runHeld;
            JumpPressed = jumpPressed;
            AttackPressed = attackPressed;
        }

        public Vector2 Move { get; }
        public bool RunHeld { get; }
        public bool JumpPressed { get; }
        public bool AttackPressed { get; }
        public bool HasMoveInput => Move.sqrMagnitude > 0.01f;
    }
}
