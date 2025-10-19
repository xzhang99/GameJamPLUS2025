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
    [SerializeField] private Animator characterAnimator; // ���ý�ɫģ���ϵ�Animator

    private bool _isGrounded;
    private bool _hasDoubleJump;
    private bool _isJumping;
    private float _kayoteTime;
    private PlayerInputActions _controls;
    private Rigidbody _rigidbody;
    private bool hasAnimator;
    private float _jumpStartTime; // ��¼��Ծ��ʼʱ��

    private void Awake()
    {
        _isGrounded = false;
        _hasDoubleJump = true;
        _kayoteTime = f_maxKayoteTimeMS;
        _isJumping = false;
    }

    private void Start()
    {
        _rigidbody = this.GetComponent<Rigidbody>();

        // �����ҵ�Animator�����Ӷ����У�
        if (characterAnimator == null)
        {
            characterAnimator = GetComponentInChildren<Animator>();
        }

        // ����Ƿ���Animator��Animator Controller
        hasAnimator = characterAnimator != null && characterAnimator.runtimeAnimatorController != null;

        if (!hasAnimator)
        {
            Debug.LogWarning("No Animator or Animator Controller found. Animation controls will be disabled.");
        }
        else
        {
            Debug.Log("Animator found and ready: " + characterAnimator.gameObject.name);
        }

        _controls = new PlayerInputActions();
        _controls.Enable();

        _controls.PlayerInputs.Jump.performed += Jump;

        // ��ʼ������״̬�������Animator��
        if (hasAnimator)
        {
            characterAnimator.SetBool("IsWalking", false);
            characterAnimator.SetBool("IsJumping", false);
        }
    }

    private void Update()
    {
        _isGrounded = CheckIfGrounded();

        // ���¶���״̬
        UpdateAnimations();

        // �����ƶ��߼�
        HandleMovement();

        // ������Ծ״̬
        HandleJumpState();
    }

    private void UpdateAnimations()
    {
        // ���û��Animator��ֱ�ӷ���
        if (!hasAnimator) return;

        // �������߶���
        Vector2 moveInput = _controls.PlayerInputs.Move.ReadValue<Vector2>();
        bool isMoving = moveInput.magnitude > 0.1f;

        // ���߶������ڵ������ƶ��Ҳ�����Ծ״̬
        characterAnimator.SetBool("IsWalking", isMoving && _isGrounded && !_isJumping);

        // ��Ծ����������Ծ״̬��
        characterAnimator.SetBool("IsJumping", _isJumping);

        // ������Ϣ
        if (Input.GetKeyDown(KeyCode.P)) // ��P����ӡ״̬
        {
            Debug.Log($"Grounded: {_isGrounded}, Jumping: {_isJumping}, Moving: {isMoving}");
        }
    }

    private void HandleMovement()
    {
        if (_isGrounded)
        {
            _hasDoubleJump = true;

            float maxSpeed = _controls.PlayerInputs.Sprint.IsPressed() ? f_sprintSpeed : f_maxSpeed;

            _rigidbody.drag = f_drag;

            _rigidbody.AddForce(GetMovementInput() * f_acceleration * Time.deltaTime, ForceMode.Force);

            Vector3 flatvel = _rigidbody.velocity;
            flatvel.y = 0;

            if (flatvel.magnitude > maxSpeed)
            {
                flatvel = flatvel.normalized * maxSpeed;
                _rigidbody.velocity = new(flatvel.x, _rigidbody.velocity.y, flatvel.z);
            }
        }
        else
        {
            _rigidbody.drag = 0;

            _rigidbody.AddForce(GetMovementInput() * f_acceleration * f_airControlMultiplier * Time.deltaTime, ForceMode.Force);

            Vector3 flatvel = _rigidbody.velocity;
            flatvel.y = 0;
        }

        if (!_isGrounded)
        {
            _rigidbody.AddForce(new(0, -1 * f_freeFallAcceleration * Time.deltaTime, 0), ForceMode.Force);

            if (_rigidbody.velocity.y < -f_maxFallSpeed)
                _rigidbody.velocity = new(_rigidbody.velocity.x, -f_maxFallSpeed, _rigidbody.velocity.z);

            if (_kayoteTime > 0)
                _kayoteTime -= Time.deltaTime;
        }
        else
        {
            _rigidbody.velocity = new(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
        }
    }

    private void HandleJumpState()
    {
        // ���������Ծ���Ѿ��ص����棬������Ծ״̬
        // ���һ��С���ӳ٣�ȷ����Ծ�������㹻ʱ�䲥��
        if (_isJumping && _isGrounded && _rigidbody.velocity.y <= 0 && Time.time - _jumpStartTime > 0.2f)
        {
            _isJumping = false;
        }

        // ����ڵ����ϣ�����Kayoteʱ��
        if (_isGrounded)
        {
            _kayoteTime = f_maxKayoteTimeMS;
        }
    }

    private bool CheckIfGrounded()
        => Physics.Raycast(f_feet.position, Vector3.down, f_rayDistance, f_groundLM);

    private Vector3 GetMovementInput()
    {
        Vector2 input = _controls.PlayerInputs.Move.ReadValue<Vector2>();
        return f_orientation.forward * input.y + f_orientation.right * input.x;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (_isGrounded || _kayoteTime > 0)
        {
            _rigidbody.velocity = new(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            _rigidbody.AddForce(Vector3.up * f_jumpHeight, ForceMode.Impulse);
            _isJumping = true;
            _jumpStartTime = Time.time; // ��¼��Ծ��ʼʱ��
            return;
        }

        if (_hasDoubleJump)
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.AddForce(GetMovementInput().normalized * (f_maxSpeed / 4) + Vector3.up * f_jumpHeight, ForceMode.Impulse);
            _isJumping = true;
            _jumpStartTime = Time.time; // ��¼��Ծ��ʼʱ��
            _hasDoubleJump = false;
        }
    }
}