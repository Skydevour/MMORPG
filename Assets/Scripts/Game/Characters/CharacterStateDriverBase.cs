using System.Collections.Generic;
using MMORPG.Framework.Animation;
using MMORPG.Framework.StateMachine;
using UnityEngine;
using StateMachineRunner = MMORPG.Framework.StateMachine.StateMachine;

namespace MMORPG.Game.Characters
{
    public abstract class CharacterStateDriverBase : MonoBehaviour
    {
        private readonly StateMachineRunner stateMachine = new StateMachineRunner();
        private readonly Dictionary<string, IState> states = new Dictionary<string, IState>();
        private bool initialized;

        protected void RegisterAnimationState(string stateName, FrameAnimator animator)
        {
            states[stateName] = new CharacterAnimationState(stateName, animator);
        }

        protected void SetInitialState(string stateName)
        {
            if (states.TryGetValue(stateName, out IState state))
            {
                stateMachine.ChangeState(state);
                initialized = true;
                return;
            }

            Debug.LogWarning($"角色状态未注册，无法进入初始状态：{stateName}");
        }

        protected abstract string ResolveStateName();

        protected virtual void TickState(float deltaTime)
        {
        }

        protected virtual void FixedTickState(float fixedDeltaTime)
        {
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            string nextStateName = ResolveStateName();
            if (states.TryGetValue(nextStateName, out IState nextState))
            {
                stateMachine.ChangeState(nextState);
            }

            stateMachine.Tick(Time.deltaTime);
            TickState(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (!initialized)
            {
                return;
            }

            stateMachine.FixedTick(Time.fixedDeltaTime);
            FixedTickState(Time.fixedDeltaTime);
        }
    }
}
