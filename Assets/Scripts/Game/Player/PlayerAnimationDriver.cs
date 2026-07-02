using System;
using UnityEngine;

namespace MMORPG.Game.Player
{
    public sealed class PlayerAnimationDriver : MonoBehaviour
    {
        [Header("Animator State Names")]
        [SerializeField] private string idleState = "Great Sword Idle";
        [SerializeField] private string[] idleVariants = Array.Empty<string>();
        [SerializeField] private string walkState = "Great Sword Walk";
        [SerializeField] private string runState = "Great Sword Run";
        [SerializeField] private string jumpState = "Great Sword Jump";
        [SerializeField] private string attackState = "Great Sword Slash";
        [SerializeField] private string turnState = "Great Sword Turn";

        [Header("Blend")]
        [SerializeField] private float locomotionFade = 0.12f;
        [SerializeField] private float actionFade = 0.05f;

        private Animator animator;
        private int currentStateHash;

        public int IdleVariantCount => idleVariants?.Length ?? 0;

        /// <summary>
        /// 初始化动画驱动，并在角色生成后立刻进入待机，避免 Animator 默认状态未起播导致模型静止。
        /// </summary>
        public void Initialize(Animator targetAnimator)
        {
            animator = targetAnimator;
            currentStateHash = 0;
            PlayIdle();
        }

        /// <summary>
        /// 写入角色专属状态名。编辑器生成不同角色 prefab 时会调用，保证同一套状态机代码可复用。
        /// </summary>
        public void ConfigureStateNames(string idle, string[] idles, string walk, string run, string jump, string attack, string turn)
        {
            idleState = idle;
            idleVariants = idles ?? Array.Empty<string>();
            walkState = walk;
            runState = run;
            jumpState = jump;
            attackState = attack;
            turnState = turn;
        }

        public void PlayIdle()
        {
            CrossFade(idleState, locomotionFade);
        }

        public void PlayIdleVariant(int index)
        {
            if (IdleVariantCount == 0)
            {
                PlayIdle();
                return;
            }

            int wrappedIndex = Mathf.Abs(index) % IdleVariantCount;
            CrossFade(idleVariants[wrappedIndex], locomotionFade);
        }

        public void PlayWalk()
        {
            CrossFade(walkState, locomotionFade);
        }

        public void PlayRun()
        {
            CrossFade(runState, locomotionFade);
        }

        public void PlayJump()
        {
            CrossFade(jumpState, actionFade);
        }

        public void PlayAttack()
        {
            CrossFade(attackState, actionFade);
        }

        public void PlayTurn()
        {
            CrossFade(turnState, locomotionFade);
        }

        /// <summary>
        /// 统一切状态前做空值和 HasState 检查，资源命名错了时直接给出可定位的日志。
        /// </summary>
        private void CrossFade(string stateName, float fadeDuration)
        {
            if (animator == null || string.IsNullOrWhiteSpace(stateName))
            {
                return;
            }

            int stateHash = Animator.StringToHash(stateName);
            if (stateHash == currentStateHash)
            {
                return;
            }

            if (!animator.HasState(0, stateHash))
            {
                Debug.LogWarning($"{nameof(PlayerAnimationDriver)} 找不到 Animator 状态：{stateName}。请检查角色 controller 是否已经绑定 Motion。", this);
                return;
            }

            animator.CrossFadeInFixedTime(stateHash, fadeDuration);
            currentStateHash = stateHash;
        }
    }
}
