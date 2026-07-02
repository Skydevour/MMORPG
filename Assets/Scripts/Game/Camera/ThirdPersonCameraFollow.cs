using UnityEngine;

namespace MMORPG.Game.Camera
{
    public sealed class ThirdPersonCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 7.2f;
        [SerializeField] private float height = 2.6f;
        [SerializeField] private float shoulderOffset = 0.7f;
        [SerializeField] private float lookHeight = 1.45f;
        [SerializeField] private float mouseSensitivity = 2.4f;
        [SerializeField] private float minPitch = -18f;
        [SerializeField] private float maxPitch = 46f;
        [SerializeField] private float positionSmoothTime = 0.055f;
        [SerializeField] private float rotationSmoothSpeed = 18f;
        [SerializeField] private bool lockCursorOnPlay = true;

        private Vector3 followVelocity;
        private float yaw;
        private float pitch = 18f;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
            if (target == null)
            {
                return;
            }

            yaw = target.eulerAngles.y;
            ApplyCursorLock();
            transform.position = GetDesiredPosition();
            transform.rotation = GetDesiredRotation();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            UpdateCameraInput();

            Vector3 desiredPosition = GetDesiredPosition();
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref followVelocity, positionSmoothTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, GetDesiredRotation(), rotationSmoothSpeed * Time.deltaTime);
        }

        /// <summary>
        /// 鼠标控制镜头绕角色旋转。按 Escape 释放鼠标，再点击游戏窗口重新锁定。
        /// </summary>
        private void UpdateCameraInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (lockCursorOnPlay && Input.GetMouseButtonDown(0))
            {
                ApplyCursorLock();
            }

            if (Cursor.lockState != CursorLockMode.Locked && lockCursorOnPlay)
            {
                return;
            }

            yaw += Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        private Vector3 GetDesiredPosition()
        {
            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focusPoint = target.position + Vector3.up * height;
            Vector3 offset = orbitRotation * new Vector3(shoulderOffset, 0f, -distance);
            return focusPoint + offset;
        }

        private Quaternion GetDesiredRotation()
        {
            Vector3 lookPoint = target.position + Vector3.up * lookHeight;
            Vector3 direction = lookPoint - transform.position;
            return direction.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(direction.normalized, Vector3.up) : transform.rotation;
        }

        private void ApplyCursorLock()
        {
            if (!Application.isPlaying || !lockCursorOnPlay)
            {
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
