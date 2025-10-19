using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class peachanimationcontrol : MonoBehaviour
{
    Animator animator;

    [Header("Movement Settings")]
    public float walkSpeed = 2.0f;
    public float rotationSpeed = 100.0f;
    public float jumpForce = 12.0f;
    public float gravityMultiplier = 3.0f; // ��������������

    [Header("Input Settings")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public string mouseXAxis = "Mouse X";

    // ���ڿ��ƽ�ɫ�ƶ������
    private CharacterController characterController;
    private Rigidbody rb;

    private Vector3 movementDirection;
    private bool isJumping = false;
    private Vector3 velocity; // ����������CharacterController���ٶ�

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        // ���Ի�ȡ��ɫ��������������
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        // ���û���ҵ��κ��ƶ���������һ������
        if (characterController == null && rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.freezeRotation = true; // ��ֹ��ת
            // ���һ��������ײ��
            CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.height = 2.0f;
            capsule.center = new Vector3(0, 1.0f, 0);
        }

        // ȷ������״̬��ȷ��ʼ��
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsWalking", false);
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleRotation();

        // ����Ծ���� - ֻ����Ƿ��Ѿ�����Ծ
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            StartJump();
        }

        // �����Ծ�����Ƿ���Ȼ����
        if (isJumping && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            EndJump();
        }

        // Ӧ��������������CharacterController��
        if (characterController != null)
        {
            ApplyGravity();
        }
    }

    void FixedUpdate()
    {
        ApplyMovement();
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
            float rotationAmount = mouseX * 2.0f * rotationSpeed * Time.deltaTime;

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

        // ����ʹ�õ��������Ӧ���ƶ�
        if (characterController != null && characterController.enabled)
        {
            // ʹ��CharacterController�ƶ�
            Vector3 move = movementDirection * walkSpeed;
            move.y = velocity.y; // ������ֱ�ٶ�
            characterController.Move(move * Time.deltaTime);
        }
        else if (rb != null)
        {
            // ʹ��Rigidbody�ƶ�
            // ע�⣺����ʹ���ٶȿ��ƣ������ڷ�����ȷ�ĳ���
            Vector3 newVelocity = movementDirection * walkSpeed;
            newVelocity.y = rb.velocity.y; // ����ԭ�еĴ�ֱ�ٶȣ�����/��Ծ��
            rb.velocity = newVelocity;
        }
        else
        {
            // ���û�����������ֱ���ƶ�Transform
            transform.position += movementDirection * walkSpeed * Time.deltaTime;
        }
    }

    // ������Ӧ������
    void ApplyGravity()
    {
        if (characterController != null)
        {
            // �����ɫ�ڵ����ϣ����ô�ֱ�ٶ�
            if (characterController.isGrounded && velocity.y < 0)
            {
                velocity.y = -0.5f; // С�ĸ�ֵȷ����ɫ�����ڵ�����
            }
            else
            {
                // Ӧ������
                velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            }
        }
    }

    void StartJump()
    {
        isJumping = true;
        animator.SetBool("IsJumping", true);

        if (rb != null)
        {
            // ʹ�ø������Ծ��
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // ΪRigidbody������������
            if (rb.useGravity)
            {
                // ���ʹ��������������������
                rb.mass *= gravityMultiplier;
            }
        }
        else if (characterController != null)
        {
            // ���ʹ��CharacterController��Ӧ����Ծ�ٶ�
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
        }
    }

    // ������Ծ
    void EndJump()
    {
        isJumping = false;
        animator.SetBool("IsJumping", false);

        // ����Rigidbody�����������֮ǰ�޸Ĺ���
        if (rb != null)
        {
            rb.mass = 1.0f; // ����ΪĬ������
        }
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, movementDirection * 2);
        }
    }
}