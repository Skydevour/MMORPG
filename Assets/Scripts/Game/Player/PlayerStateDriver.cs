using System.Collections.Generic;
using MMORPG.Framework.StateMachine;
using UnityEngine;
using StateMachineRunner = MMORPG.Framework.StateMachine.StateMachine;

namespace MMORPG.Game.Player
{
    public sealed class PlayerStateDriver : MonoBehaviour
    {
        private readonly StateMachineRunner stateMachine = new StateMachineRunner();
        private readonly Dictionary<string, IState> states = new Dictionary<string, IState>();

        private PlayerController2D controller;

        public void Initialize(PlayerController2D playerController, PlayerSpriteAnimator animator)
        {
            controller = playerController;
            states["idle"] = new PlayerAnimationState("idle", animator);
            states["run"] = new PlayerAnimationState("run", animator);
            states["jump"] = new PlayerAnimationState("jump", animator);
            states["dash"] = new PlayerAnimationState("dash", animator);
            states["shoot"] = new PlayerAnimationState("shoot", animator);
            stateMachine.ChangeState(states["idle"]);
        }

        private void Update()
        {
            if (controller == null)
            {
                return;
            }

            stateMachine.ChangeState(states[ResolveState()]);
            stateMachine.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            stateMachine.FixedTick(Time.fixedDeltaTime);
        }

        private string ResolveState()
        {
            if (controller.IsDashing)
            {
                return "dash";
            }

            if (controller.IsShooting)
            {
                return "shoot";
            }

            if (!controller.IsGrounded)
            {
                return "jump";
            }

            return controller.IsMoving ? "run" : "idle";
        }
    }
}
