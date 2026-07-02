using UnityEngine;

namespace MMORPG.Game.Player.Motor
{
    public interface ICharacterMotor
    {
        bool IsGrounded { get; }
        Vector3 Velocity { get; }

        /// <summary>
        /// 按输入方向移动角色。状态机只调用这个接口，不关心底层是 CharacterController、Rigidbody 还是 Root Motion。
        /// </summary>
        void Move(Vector2 input, bool runHeld, float deltaTime);

        /// <summary>
        /// 没有操作输入时仍然更新垂直速度，确保重力和贴地逻辑持续生效。
        /// </summary>
        void MoveWithoutInput(float deltaTime);

        /// <summary>
        /// 请求角色跳跃。具体实现负责判断当前是否允许起跳。
        /// </summary>
        void Jump();

        /// <summary>
        /// 设置外部速度，用于后续击退、冲刺、拉拽等非玩家输入产生的位移。
        /// </summary>
        void SetExternalVelocity(Vector3 velocity);

        /// <summary>
        /// 直接移动到指定位置和朝向，用于出生点、传送、复活、切场景等流程。
        /// </summary>
        void Teleport(Vector3 position, Quaternion rotation);
    }
}
