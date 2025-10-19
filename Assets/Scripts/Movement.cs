using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float f_jumpHeight;
    [SerializeField] private float f_maxSpeed;
    [SerializeField] private float f_sprintSpeed;
    [SerializeField] private float f_acceleration;
    [SerializeField] private float f_freeFallAcceleration;
    [SerializeField] private float f_drag;
    [SerializeField] private float f_maxFallSpeed;
    [SerializeField, Range(0, 1)] private float f_airControlMultiplier;
    [Space]
    [SerializeField, Range(0, 1)] private float f_maxKayoteTimeMS;
    [Space]
    [SerializeField] private LayerMask f_groundLM;
    [SerializeField] private Transform f_orientation;
    [SerializeField] private Transform f_feet;
    [SerializeField] private Transform f_head;
    [SerializeField, Min(0)] private float f_rayDistance;

    [Header("Animation")]
    [SerializeField] private Animator characterAnimator;

    private bool _isGrounded;
    private bool _wasGrounded; // 上一帧的地面状态
    private bool _hasDoubleJump;
    private bool _isJumping;
    private float _kayoteTime;
    private PlayerInputActions _controls;
    private Rigidbody _rigidbody;
    private bool hasAnimator;
    private float _jumpStartTime;
    private Vector3 _lastVelocity; // 记录上一帧的速度

    private void Awake()
    {
        _isGrounded = false;
        _wasGrounded = false;
        _hasDoubleJump = true;
        _kayoteTime = f_maxKayoteTimeMS;
        _isJumping = false;
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        if (characterAnimator == null)
        {
            characterAnimator = GetComponentInChildren<Animator>();
        }

        hasAnimator = characterAnimator != null && characterAnimator.runtimeAnimatorController != null;

        if (!hasAnimator)
        {
            Debug.LogWarning("No Animator or Animator Controller found. Animation controls will be disabled.");
        }

        _controls = new PlayerInputActions();
        _controls.Enable();
        _controls.PlayerInputs.Jump.performed += Jump;

        if (hasAnimator)
        {
            characterAnimator.SetBool("IsWalking", false);
            characterAnimator.SetBool("IsJumping", false);
        }
    }

    private void Update()
    {
        _wasGrounded = _isGrounded; // 保存上一帧的地面状态
        _isGrounded = CheckIfGrounded();

        // 只有在真正从空中落到地面时才重置跳跃状态
        if (!_wasGrounded && _isGrounded)
        {
            // 添加一个小的延迟确保物理系统稳定
            Invoke(nameof(ResetJumpState), 0.1f);
        }

        UpdateAnimations();
        HandleMovement();
        HandleJumpState();
    }

    private void FixedUpdate()
    {
        _lastVelocity = _rigidbody.velocity; // 记录速度用于调试
    }

    private void ResetJumpState()
    {
        _isJumping = false;
        _hasDoubleJump = true;
        _kayoteTime = f_maxKayoteTimeMS;
    }

    private void UpdateAnimations()
    {
        if (!hasAnimator) return;

        Vector2 moveInput = _controls.PlayerInputs.Move.ReadValue<Vector2>();
        bool isMoving = moveInput.magnitude > 0.1f;

        characterAnimator.SetBool("IsWalking", isMoving && _isGrounded && !_isJumping);
        characterAnimator.SetBool("IsJumping", _isJumping);

        // 调试信息
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"Grounded: {_isGrounded}, Jumping: {_isJumping}, Moving: {isMoving}, Velocity: {_rigidbody.velocity}");
        }
    }

    private void HandleMovement()
    {
        Vector2 moveInput = _controls.PlayerInputs.Move.ReadValue<Vector2>();
        bool isTryingToMove = moveInput.magnitude > 0.1f;

        if (_isGrounded)
        {
            _hasDoubleJump = true;

            float maxSpeed = _controls.PlayerInputs.Sprint.IsPressed() ? f_sprintSpeed : f_maxSpeed;
            _rigidbody.drag = f_drag;

            if (isTryingToMove)
            {
                Vector3 movement = GetMovementInput() * f_acceleration * Time.deltaTime;
                _rigidbody.AddForce(movement, ForceMode.Force);
            }

            Vector3 flatVel = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            if (flatVel.magnitude > maxSpeed)
            {
                flatVel = flatVel.normalized * maxSpeed;
                _rigidbody.velocity = new Vector3(flatVel.x, _rigidbody.velocity.y, flatVel.z);
            }
        }
        else
        {
            _rigidbody.drag = 0;

            if (isTryingToMove)
            {
                Vector3 movement = GetMovementInput() * f_acceleration * f_airControlMultiplier * Time.deltaTime;
                _rigidbody.AddForce(movement, ForceMode.Force);
            }

            // 应用重力
            _rigidbody.AddForce(Vector3.down * f_freeFallAcceleration * Time.deltaTime, ForceMode.Force);

            // 限制最大下落速度
            if (_rigidbody.velocity.y < -f_maxFallSpeed)
            {
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, -f_maxFallSpeed, _rigidbody.velocity.z);
            }

            // 更新Kayote时间
            if (_kayoteTime > 0)
                _kayoteTime -= Time.deltaTime;
        }
    }

    private void HandleJumpState()
    {
        // 如果正在跳跃但速度已经开始下降，且已经过了最小跳跃时间，允许结束跳跃状态
        if (_isJumping && _rigidbody.velocity.y <= 0 && Time.time - _jumpStartTime > 0.3f)
        {
            // 这里不直接设置_isJumping = false，因为地面检测会处理这个
        }
    }

    private bool CheckIfGrounded()
    {
        // 使用多个射线检测点提高检测精度
        bool groundCheck = Physics.Raycast(f_feet.position, Vector3.down, f_rayDistance, f_groundLM);

        // 额外的检测点
        float checkOffset = 0.2f;
        bool frontCheck = Physics.Raycast(f_feet.position + f_orientation.forward * checkOffset, Vector3.down, f_rayDistance, f_groundLM);
        bool backCheck = Physics.Raycast(f_feet.position - f_orientation.forward * checkOffset, Vector3.down, f_rayDistance, f_groundLM);
        bool leftCheck = Physics.Raycast(f_feet.position - f_orientation.right * checkOffset, Vector3.down, f_rayDistance, f_groundLM);
        bool rightCheck = Physics.Raycast(f_feet.position + f_orientation.right * checkOffset, Vector3.down, f_rayDistance, f_groundLM);

        return groundCheck || frontCheck || backCheck || leftCheck || rightCheck;
    }

    private Vector3 GetMovementInput()
    {
        Vector2 input = _controls.PlayerInputs.Move.ReadValue<Vector2>();
        return f_orientation.forward * input.y + f_orientation.right * input.x;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (_isGrounded || _kayoteTime > 0)
        {
            PerformJump(Vector3.up * f_jumpHeight);
            return;
        }

        if (_hasDoubleJump)
        {
            // 双跳时保留部分水平动量
            Vector3 horizontalMomentum = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z) * 0.5f;
            Vector3 jumpForce = horizontalMomentum + Vector3.up * f_jumpHeight;
            PerformJump(jumpForce);
            _hasDoubleJump = false;
        }
    }

    private void PerformJump(Vector3 jumpForce)
    {
        // 确保y速度至少为0，避免向下跳跃
        if (_rigidbody.velocity.y < 0)
        {
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
        }

        _rigidbody.AddForce(jumpForce, ForceMode.Impulse);
        _isJumping = true;
        _jumpStartTime = Time.time;
        _kayoteTime = 0; // 使用Kayote时间后重置
    }

    private void OnDrawGizmos()
    {
        // 可视化地面检测射线
        if (f_feet != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(f_feet.position, Vector3.down * f_rayDistance);

            float checkOffset = 0.2f;
            Gizmos.DrawRay(f_feet.position + f_orientation.forward * checkOffset, Vector3.down * f_rayDistance);
            Gizmos.DrawRay(f_feet.position - f_orientation.forward * checkOffset, Vector3.down * f_rayDistance);
            Gizmos.DrawRay(f_feet.position - f_orientation.right * checkOffset, Vector3.down * f_rayDistance);
            Gizmos.DrawRay(f_feet.position + f_orientation.right * checkOffset, Vector3.down * f_rayDistance);
        }
    }
}