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
    private bool _wasGrounded; // ��һ֡�ĵ���״̬
    private bool _hasDoubleJump;
    private bool _isJumping;
    private float _kayoteTime;
    private PlayerInputActions _controls;
    private Rigidbody _rigidbody;
    private bool hasAnimator;
    private float _jumpStartTime;
    private Vector3 _lastVelocity; // ��¼��һ֡���ٶ�

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
        _wasGrounded = _isGrounded; // ������һ֡�ĵ���״̬
        _isGrounded = CheckIfGrounded();

        // ֻ���������ӿ����䵽����ʱ��������Ծ״̬
        if (!_wasGrounded && _isGrounded)
        {
            // ���һ��С���ӳ�ȷ������ϵͳ�ȶ�
            Invoke(nameof(ResetJumpState), 0.1f);
        }

        UpdateAnimations();
        HandleMovement();
        HandleJumpState();
    }

    private void FixedUpdate()
    {
        _lastVelocity = _rigidbody.velocity; // ��¼�ٶ����ڵ���
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

        // ������Ϣ
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

            // Ӧ������
            _rigidbody.AddForce(Vector3.down * f_freeFallAcceleration * Time.deltaTime, ForceMode.Force);

            // ������������ٶ�
            if (_rigidbody.velocity.y < -f_maxFallSpeed)
            {
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, -f_maxFallSpeed, _rigidbody.velocity.z);
            }

            // ����Kayoteʱ��
            if (_kayoteTime > 0)
                _kayoteTime -= Time.deltaTime;
        }
    }

    private void HandleJumpState()
    {
        // ���������Ծ���ٶ��Ѿ���ʼ�½������Ѿ�������С��Ծʱ�䣬���������Ծ״̬
        if (_isJumping && _rigidbody.velocity.y <= 0 && Time.time - _jumpStartTime > 0.3f)
        {
            // ���ﲻֱ������_isJumping = false����Ϊ������ᴦ�����
        }
    }

    private bool CheckIfGrounded()
    {
        // ʹ�ö�����߼�����߼�⾫��
        bool groundCheck = Physics.Raycast(f_feet.position, Vector3.down, f_rayDistance, f_groundLM);

        // ����ļ���
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
            // ˫��ʱ��������ˮƽ����
            Vector3 horizontalMomentum = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z) * 0.5f;
            Vector3 jumpForce = horizontalMomentum + Vector3.up * f_jumpHeight;
            PerformJump(jumpForce);
            _hasDoubleJump = false;
        }
    }

    private void PerformJump(Vector3 jumpForce)
    {
        // ȷ��y�ٶ�����Ϊ0������������Ծ
        if (_rigidbody.velocity.y < 0)
        {
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
        }

        _rigidbody.AddForce(jumpForce, ForceMode.Impulse);
        _isJumping = true;
        _jumpStartTime = Time.time;
        _kayoteTime = 0; // ʹ��Kayoteʱ�������
    }

    private void OnDrawGizmos()
    {
        // ���ӻ�����������
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