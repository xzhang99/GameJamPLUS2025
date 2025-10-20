using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float f_jumpHeight = 5f;
    [SerializeField] private float f_maxSpeed = 7f;
    [SerializeField] private float f_sprintSpeed = 10f;
    [SerializeField] private float f_acceleration = 50f;
    [SerializeField] private float f_deceleration = 60f; // 新增：减速力
    [SerializeField] private float f_freeFallAcceleration = 20f;
    [SerializeField] private float f_drag = 6f;
    [SerializeField] private float f_maxFallSpeed = 30f;
    [SerializeField, Range(0, 1)] private float f_airControlMultiplier = 0.3f;
    [Space]
    [SerializeField, Range(0, 1)] private float f_maxKayoteTimeMS = 0.2f;
    [Space]
    [SerializeField] private LayerMask f_groundLM;
    [SerializeField] private Transform f_orientation;
    [SerializeField] private Transform f_feet;
    [SerializeField] private Transform f_head;
    [SerializeField, Min(0)] private float f_rayDistance = 0.2f;

    [Header("Animation")]
    [SerializeField] private Animator characterAnimator;

    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _hasDoubleJump;
    private bool _isJumping;
    private float _kayoteTime;
    private PlayerInputActions _controls;
    private Rigidbody _rigidbody;
    private bool hasAnimator;
    private float _jumpStartTime;
    private Vector2 _currentMoveInput;

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

        // 改进刚体设置以减少滑动
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

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
        _wasGrounded = _isGrounded;
        _isGrounded = CheckIfGrounded();

        // 读取输入
        _currentMoveInput = _controls.PlayerInputs.Move.ReadValue<Vector2>();

        if (!_wasGrounded && _isGrounded)
        {
            Invoke(nameof(ResetJumpState), 0.1f);
        }

        UpdateAnimations();
        HandleJumpState();
    }

    private void FixedUpdate()
    {
        HandleMovement();
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

        bool isMoving = _currentMoveInput.magnitude > 0.1f;

        characterAnimator.SetBool("IsWalking", isMoving && _isGrounded && !_isJumping);
        characterAnimator.SetBool("IsJumping", _isJumping);
    }

    private void HandleMovement()
    {
        bool isTryingToMove = _currentMoveInput.magnitude > 0.1f;

        if (_isGrounded)
        {
            _hasDoubleJump = true;

            float maxSpeed = _controls.PlayerInputs.Sprint.IsPressed() ? f_sprintSpeed : f_maxSpeed;
            _rigidbody.drag = f_drag;

            Vector3 moveDirection = GetMovementInput();

            if (isTryingToMove)
            {
                // 加速 - 使用VelocityChange来获得更直接的控制
                Vector3 targetVelocity = moveDirection * maxSpeed;
                Vector3 velocityChange = (targetVelocity - new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z));

                velocityChange.x = Mathf.Clamp(velocityChange.x, -f_acceleration, f_acceleration);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -f_acceleration, f_acceleration);
                velocityChange.y = 0;

                _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
            }
            else
            {
                // 没有输入时应用减速
                Vector3 horizontalVel = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
                if (horizontalVel.magnitude > 0.1f)
                {
                    Vector3 decelerationForce = -horizontalVel.normalized * f_deceleration * Time.fixedDeltaTime;
                    _rigidbody.AddForce(decelerationForce, ForceMode.VelocityChange);
                }
                else
                {
                    // 速度很小时直接停止
                    _rigidbody.velocity = new Vector3(0, _rigidbody.velocity.y, 0);
                }
            }

            // 限制水平速度
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
                Vector3 movement = GetMovementInput() * f_acceleration * f_airControlMultiplier * Time.fixedDeltaTime;
                _rigidbody.AddForce(movement, ForceMode.Force);
            }

            // 应用重力
            _rigidbody.AddForce(Vector3.down * f_freeFallAcceleration * Time.fixedDeltaTime, ForceMode.Force);

            // 限制最大下落速度
            if (_rigidbody.velocity.y < -f_maxFallSpeed)
            {
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, -f_maxFallSpeed, _rigidbody.velocity.z);
            }

            // 更新Kayote时间
            if (_kayoteTime > 0)
                _kayoteTime -= Time.fixedDeltaTime;
        }
    }

    private void HandleJumpState()
    {
        if (_isJumping && _rigidbody.velocity.y <= 0 && Time.time - _jumpStartTime > 0.3f)
        {
            // 跳跃状态由地面检测处理
        }
    }

    private bool CheckIfGrounded()
    {
        bool groundCheck = Physics.Raycast(f_feet.position, Vector3.down, f_rayDistance, f_groundLM);

        float checkOffset = 0.2f;
        bool frontCheck = Physics.Raycast(f_feet.position + f_orientation.forward * checkOffset, Vector3.down, f_rayDistance, f_groundLM);
        bool backCheck = Physics.Raycast(f_feet.position - f_orientation.forward * checkOffset, Vector3.down, f_rayDistance, f_groundLM);
        bool leftCheck = Physics.Raycast(f_feet.position - f_orientation.right * checkOffset, Vector3.down, f_rayDistance, f_groundLM);
        bool rightCheck = Physics.Raycast(f_feet.position + f_orientation.right * checkOffset, Vector3.down, f_rayDistance, f_groundLM);

        return groundCheck || frontCheck || backCheck || leftCheck || rightCheck;
    }

    private Vector3 GetMovementInput()
    {
        return f_orientation.forward * _currentMoveInput.y + f_orientation.right * _currentMoveInput.x;
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
            Vector3 horizontalMomentum = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z) * 0.5f;
            Vector3 jumpForce = horizontalMomentum + Vector3.up * f_jumpHeight;
            PerformJump(jumpForce);
            _hasDoubleJump = false;
        }
    }

    private void PerformJump(Vector3 jumpForce)
    {
        if (_rigidbody.velocity.y < 0)
        {
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
        }

        _rigidbody.AddForce(jumpForce, ForceMode.Impulse);
        _isJumping = true;
        _jumpStartTime = Time.time;
        _kayoteTime = 0;
    }

    private void OnDrawGizmos()
    {
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

    private void OnDisable()
    {
        _controls?.Disable();
    }
}