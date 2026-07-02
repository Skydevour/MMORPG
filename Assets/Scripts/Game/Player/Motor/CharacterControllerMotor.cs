using UnityEngine;

namespace MMORPG.Game.Player.Motor
{
    public sealed class CharacterControllerMotor : MonoBehaviour, ICharacterMotor
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 2.2f;
        [SerializeField] private float runSpeed = 7.4f;
        [SerializeField] private float rotationSpeed = 20f;
        [SerializeField] private float acceleration = 24f;
        [SerializeField] private float deceleration = 30f;
        [SerializeField] private float jumpHeight = 4.2f;
        [SerializeField] private float gravity = -32f;
        [SerializeField] private float groundedStickVelocity = -2f;
        [SerializeField] private float externalVelocityDamping = 8f;

        private CharacterController characterController;
        private Transform cameraTransform;
        private Vector3 currentHorizontalVelocity;
        private Vector3 currentVelocity;
        private Vector3 externalVelocity;
        private float verticalVelocity;

        public bool IsGrounded => characterController != null && characterController.isGrounded;
        public Vector3 Velocity => currentVelocity;
        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;

        /// <summary>
        /// 配置基础移动参数。角色预制体生成时会写入，用于让位移速度贴近当前动作资源的步频。
        /// </summary>
        public void ConfigureMovement(float walk, float run, float accelerate, float decelerate, float rotateSpeed)
        {
            walkSpeed = Mathf.Max(0.1f, walk);
            runSpeed = Mathf.Max(walkSpeed, run);
            acceleration = Mathf.Max(0.1f, accelerate);
            deceleration = Mathf.Max(0.1f, decelerate);
            rotationSpeed = Mathf.Max(0.1f, rotateSpeed);
        }

        /// <summary>
        /// 配置跳跃参数。高度决定起跳表现，重力决定滞空和落地速度，用来贴近动作游戏更干脆的跳跃手感。
        /// </summary>
        public void ConfigureJump(float height, float gravityValue)
        {
            jumpHeight = Mathf.Max(0.1f, height);
            gravity = Mathf.Min(-0.1f, gravityValue);
        }

        /// <summary>
        /// 初始化底层移动组件和摄像机引用。后续切换移动实现时，状态机无需变化。
        /// </summary>
        public void Initialize(CharacterController controller, Transform followCamera)
        {
            characterController = controller;
            cameraTransform = followCamera;
        }

        /// <summary>
        /// 根据摄像机朝向转换 WASD 输入，并通过 CharacterController 执行水平移动、转向和重力。
        /// </summary>
        public void Move(Vector2 input, bool runHeld, float deltaTime)
        {
            Vector3 direction = GetCameraRelativeDirection(input);
            float speed = runHeld ? runSpeed : walkSpeed;
            Vector3 targetHorizontalVelocity = direction * speed;
            float speedChange = targetHorizontalVelocity.sqrMagnitude > currentHorizontalVelocity.sqrMagnitude ? acceleration : deceleration;

            RotateTowards(direction, deltaTime);
            ApplyGravity(deltaTime);
            ApplyExternalDamping(deltaTime);

            currentHorizontalVelocity = Vector3.MoveTowards(currentHorizontalVelocity, targetHorizontalVelocity, speedChange * deltaTime);

            Vector3 velocity = currentHorizontalVelocity + externalVelocity;
            velocity.y = verticalVelocity;
            MoveController(velocity, deltaTime);
        }

        /// <summary>
        /// 无输入时继续处理重力和外力，避免跳跃落地、击退衰减这类状态中断。
        /// </summary>
        public void MoveWithoutInput(float deltaTime)
        {
            ApplyGravity(deltaTime);
            ApplyExternalDamping(deltaTime);
            currentHorizontalVelocity = Vector3.MoveTowards(currentHorizontalVelocity, Vector3.zero, deceleration * deltaTime);

            Vector3 velocity = currentHorizontalVelocity + externalVelocity;
            velocity.y = verticalVelocity;
            MoveController(velocity, deltaTime);
        }

        /// <summary>
        /// 触发跳跃速度。当前实现只允许地面起跳，后续可扩展二段跳或受击浮空。
        /// </summary>
        public void Jump()
        {
            if (!IsGrounded)
            {
                return;
            }

            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        /// <summary>
        /// 写入外部速度。攻击位移、受击击退、冲刺等都可以先走这个入口。
        /// </summary>
        public void SetExternalVelocity(Vector3 velocity)
        {
            externalVelocity = velocity;
        }

        /// <summary>
        /// 传送角色时临时关闭 CharacterController，避免 Unity 因碰撞体重叠阻止位置更新。
        /// </summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            bool wasEnabled = characterController != null && characterController.enabled;
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            transform.SetPositionAndRotation(position, rotation);
            verticalVelocity = 0f;
            currentHorizontalVelocity = Vector3.zero;
            currentVelocity = Vector3.zero;
            externalVelocity = Vector3.zero;

            if (characterController != null)
            {
                characterController.enabled = wasEnabled;
            }
        }

        /// <summary>
        /// 将输入方向从屏幕/摄像机空间转换为世界空间移动方向。
        /// </summary>
        private Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (input.sqrMagnitude < 0.0001f)
            {
                return Vector3.zero;
            }

            Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 direction = forward * input.y + right * input.x;
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        /// <summary>
        /// 只在有移动输入时转向，避免待机或攻击状态被无效方向拉偏。
        /// </summary>
        private void RotateTowards(Vector3 direction, float deltaTime)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * 60f * deltaTime);
        }

        /// <summary>
        /// 统一处理重力和贴地速度，让 CharacterController 在坡面和地面上保持稳定接触。
        /// </summary>
        private void ApplyGravity(float deltaTime)
        {
            if (IsGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickVelocity;
            }

            verticalVelocity += gravity * deltaTime;
        }

        /// <summary>
        /// 让外部速度自然衰减，方便后续击退/冲刺不用单独写计时器也能先跑起来。
        /// </summary>
        private void ApplyExternalDamping(float deltaTime)
        {
            externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, externalVelocityDamping * deltaTime);
        }

        /// <summary>
        /// 真正调用 Unity CharacterController.Move，并缓存本帧速度供动画或战斗系统查询。
        /// </summary>
        private void MoveController(Vector3 velocity, float deltaTime)
        {
            if (characterController == null)
            {
                currentVelocity = Vector3.zero;
                return;
            }

            CollisionFlags flags = characterController.Move(velocity * deltaTime);
            currentVelocity = velocity;
            if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            {
                verticalVelocity = 0f;
            }
        }
    }

    public static class CharacterControllerSetup
    {
        /// <summary>
        /// 根据角色身体网格配置胶囊碰撞体。只统计绑定了躯干/腿部骨骼的 SkinnedMeshRenderer，避免武器把中心和半径拉偏。
        /// </summary>
        public static void FitToModel(CharacterController characterController, GameObject model)
        {
            if (characterController == null || model == null)
            {
                return;
            }

            Bounds bounds = CalculateCharacterBodyBounds(characterController.transform, model);
            float height = Mathf.Clamp(bounds.size.y * 1.02f, 1.4f, 2.4f);
            float radius = Mathf.Clamp(Mathf.Max(bounds.extents.x, bounds.extents.z) * 0.75f, 0.22f, 0.5f);
            float bottom = bounds.center.y - bounds.extents.y;

            characterController.center = new Vector3(0f, bottom + height * 0.5f, 0f);
            characterController.height = height;
            characterController.radius = radius;
            characterController.stepOffset = Mathf.Min(0.35f, height * 0.2f);
            characterController.slopeLimit = 50f;
        }

        /// <summary>
        /// 计算角色身体本地包围盒。优先使用身体蒙皮网格；找不到时才回退到全部蒙皮网格或默认体型。
        /// </summary>
        public static Bounds CalculateCharacterBodyBounds(Transform characterRoot, GameObject model)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            Renderer[] bodyRenderers = System.Array.FindAll(renderers, IsBodyRenderer);
            if (bodyRenderers.Length == 0)
            {
                bodyRenderers = System.Array.FindAll(renderers, renderer => renderer is SkinnedMeshRenderer);
            }

            if (bodyRenderers.Length == 0)
            {
                return new Bounds(new Vector3(0f, 1f, 0f), new Vector3(0.8f, 2f, 0.8f));
            }

            Bounds worldBounds = bodyRenderers[0].bounds;
            for (int i = 1; i < bodyRenderers.Length; i++)
            {
                worldBounds.Encapsulate(bodyRenderers[i].bounds);
            }

            Vector3 localCenter = characterRoot.InverseTransformPoint(worldBounds.center);
            Vector3 localSize = characterRoot.InverseTransformVector(worldBounds.size);
            localSize = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
            return new Bounds(localCenter, localSize);
        }

        /// <summary>
        /// 通过骨骼名判断 Renderer 是否属于身体，过滤手持武器、盾牌等会明显拉大包围盒的部件。
        /// </summary>
        private static bool IsBodyRenderer(Renderer renderer)
        {
            if (renderer is not SkinnedMeshRenderer skinnedMeshRenderer || skinnedMeshRenderer.bones == null)
            {
                return false;
            }

            foreach (Transform bone in skinnedMeshRenderer.bones)
            {
                if (bone == null)
                {
                    continue;
                }

                string boneName = bone.name;
                if (boneName.Contains("Hips") || boneName.Contains("Spine") || boneName.Contains("Head") || boneName.Contains("Leg"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
