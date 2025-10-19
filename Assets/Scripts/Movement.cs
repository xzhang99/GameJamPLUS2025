using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    [SerializeField] private float f_jumpHeight;
    [SerializeField] private float f_maxSpeed;
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

    private bool _isGrounded;
    private bool _hasDoubleJump;
    private float _kayoteTime;
    private PlayerInputActions _controls;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _isGrounded = false;
        _hasDoubleJump = true;
        _kayoteTime = f_maxKayoteTimeMS;
    }

    private void Start()
    {
        _rigidbody = this.GetComponent<Rigidbody>();

        _controls = new PlayerInputActions();
        _controls.Enable();

        _controls.PlayerInputs.Jump.performed += Jump;
    }

    private void Update()
    {
        _isGrounded = CheckIfGrounded();

        if(_isGrounded)
        {
            _rigidbody.drag = f_drag;

            _rigidbody.AddForce(GetMovementInput() * f_acceleration * Time.deltaTime, ForceMode.Force);

            Vector3 flatvel = _rigidbody.velocity;
            flatvel.y = 0;

            if(flatvel.magnitude > f_maxSpeed)
            {
                flatvel = flatvel.normalized * f_maxSpeed;

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

    private bool CheckIfGrounded()
        => Physics.Raycast(f_feet.position, Vector3.down, f_rayDistance, f_groundLM);

    private Vector3 GetMovementInput()
    {
        Vector2 input = _controls.PlayerInputs.Move.ReadValue<Vector2>();

        return f_orientation.forward * input.y + f_orientation.right * input.x;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        _rigidbody.velocity = new(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
        _rigidbody.AddForce(Vector3.up * f_jumpHeight, ForceMode.Impulse);    
    }
}
