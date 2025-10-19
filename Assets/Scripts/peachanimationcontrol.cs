using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class peachanimationcontrol : MonoBehaviour
{
    Animator animator;

    [Header("Movement Settings")]
    public float walkSpeed = 2.0f;
    public float rotationSpeed = 100.0f;
    public float jumpForce = 8.0f;
    public float gravityMultiplier = 3.0f;

    [Header("Input Settings")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public string mouseXAxis = "Mouse X";

    // ���ڿ��ƽ�ɫ�ƶ������
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    private Vector3 movementDirection;
    private bool isJumping = false;
    private bool isGrounded = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        // ȷ���б�Ҫ�����
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        if (capsuleCollider == null)
        {
            capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            capsuleCollider.height = 2.0f;
            capsuleCollider.center = new Vector3(0, 1.0f, 0);
        }

        // ����Rigidbody
        rb.freezeRotation = true;
        rb.mass = 1.0f;
        rb.drag = 0f;
        rb.angularDrag = 0.05f;

        // �Ƴ�����ó�ͻ��Movement�ű�
        Movement movementScript = GetComponent<Movement>();
        if (movementScript != null)
        {
            movementScript.enabled = false;
            Debug.Log("�ѽ��ó�ͻ��Movement�ű�");
        }

        // ȷ������״̬��ȷ��ʼ��
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsWalking", false);
    }

    // Update is called once per frame
    void Update()
    {
        CheckGrounded();
        HandleMovement();
        HandleRotation();

        // ��Ծ���� - �ڵ������Ҳ�����Ծ��
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping && isGrounded)
        {
            StartJump();
        }

        // �����Ծ�����Ƿ���Ȼ����
        if (isJumping && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            EndJump();
        }
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyGravity();
    }

    void CheckGrounded()
    {
        // ʹ�����߼�����
        float rayDistance = 0.2f;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(rayStart, Vector3.down, rayDistance);
    }

    void HandleMovement()
    {
        bool forwardPressed = Input.GetKey("w");
        bool leftPressed = Input.GetKey("a");
        bool rightPressed = Input.GetKey("d");
        bool backwardPressed = Input.GetKey("s");
        bool isMoving = forwardPressed || backwardPressed || leftPressed || rightPressed;

        // ֻ���ڲ���Ծʱ�Ÿ������߶���״̬
        if (!isJumping)
        {
            animator.SetBool("IsWalking", isMoving);

            if (isMoving)
            {
                // �����ƶ�����
                movementDirection = CalculateMovementDirection(forwardPressed, backwardPressed, leftPressed, rightPressed);
            }
            else
            {
                movementDirection = Vector3.zero;
            }
        }
        else
        {
            // ��Ծʱ��Ȼ�����ƶ����򣬵����������߶���
            if (isMoving)
            {
                movementDirection = CalculateMovementDirection(forwardPressed, backwardPressed, leftPressed, rightPressed);
            }
            else
            {
                movementDirection = Vector3.zero;
            }
        }
    }

    void HandleRotation()
    {
        // ��ȡ���ˮƽ�ƶ�
        float mouseX = Input.GetAxis(mouseXAxis);

        // ��������ƶ���ת��ɫ
        if (Mathf.Abs(mouseX) > 0.01f)
        {
            // ������ת�Ƕ�
            float rotationAmount = mouseX * rotationSpeed * Time.deltaTime;

            // Ӧ����ת
            transform.Rotate(0, rotationAmount, 0);
        }
    }

    Vector3 CalculateMovementDirection(bool forward, bool backward, bool left, bool right)
    {
        Vector3 direction = Vector3.zero;

        // ���ݰ������㷽�򣨻��ڽ�ɫ��ǰ����
        if (forward) direction += transform.forward;
        if (backward) direction -= transform.forward;
        if (right) direction += transform.right;
        if (left) direction -= transform.right;

        // ��һ��������������ֹ�Խ����ƶ�����
        if (direction.magnitude > 1.0f)
        {
            direction.Normalize();
        }

        return direction;
    }

    void ApplyMovement()
    {
        // ���û���ƶ�������ִ���ƶ�
        if (movementDirection == Vector3.zero) return;

        // ʹ��Rigidbody�ƶ�
        Vector3 newVelocity = movementDirection * walkSpeed;
        newVelocity.y = rb.velocity.y; // ����ԭ�еĴ�ֱ�ٶȣ�����/��Ծ��
        rb.velocity = newVelocity;
    }

    void ApplyGravity()
    {
        // ���������Ծ״̬�Ҳ��ڵ����ϣ�Ӧ������
        if (!isGrounded && !isJumping)
        {
            rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    void StartJump()
    {
        isJumping = true;
        animator.SetBool("IsJumping", true);

        // Ӧ����Ծ��
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // ������Ծ
    void EndJump()
    {
        isJumping = false;
        animator.SetBool("IsJumping", false);
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // �����ƶ�����
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, movementDirection * 2);

            // ���Ƶ�������
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 rayStart = transform.position + Vector3.up * 0.1f;
            Gizmos.DrawLine(rayStart, rayStart + Vector3.down * 0.2f);
        }
    }
}