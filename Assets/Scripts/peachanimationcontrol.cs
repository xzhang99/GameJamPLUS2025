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

    // 用于控制角色移动的组件
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

        // 确保有必要的组件
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

        // 配置Rigidbody
        rb.freezeRotation = true;
        rb.mass = 1.0f;
        rb.drag = 0f;
        rb.angularDrag = 0.05f;

        // 移除或禁用冲突的Movement脚本
        Movement movementScript = GetComponent<Movement>();
        if (movementScript != null)
        {
            movementScript.enabled = false;
            Debug.Log("已禁用冲突的Movement脚本");
        }

        // 确保动画状态正确初始化
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsWalking", false);
    }

    // Update is called once per frame
    void Update()
    {
        CheckGrounded();
        HandleMovement();
        HandleRotation();

        // 跳跃条件 - 在地面上且不在跳跃中
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping && isGrounded)
        {
            StartJump();
        }

        // 检测跳跃动画是否自然结束
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
        // 使用射线检测地面
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

        // 只有在不跳跃时才更新行走动画状态
        if (!isJumping)
        {
            animator.SetBool("IsWalking", isMoving);

            if (isMoving)
            {
                // 计算移动方向
                movementDirection = CalculateMovementDirection(forwardPressed, backwardPressed, leftPressed, rightPressed);
            }
            else
            {
                movementDirection = Vector3.zero;
            }
        }
        else
        {
            // 跳跃时仍然计算移动方向，但不更新行走动画
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
        // 获取鼠标水平移动
        float mouseX = Input.GetAxis(mouseXAxis);

        // 根据鼠标移动旋转角色
        if (Mathf.Abs(mouseX) > 0.01f)
        {
            // 计算旋转角度
            float rotationAmount = mouseX * rotationSpeed * Time.deltaTime;

            // 应用旋转
            transform.Rotate(0, rotationAmount, 0);
        }
    }

    Vector3 CalculateMovementDirection(bool forward, bool backward, bool left, bool right)
    {
        Vector3 direction = Vector3.zero;

        // 根据按键计算方向（基于角色当前朝向）
        if (forward) direction += transform.forward;
        if (backward) direction -= transform.forward;
        if (right) direction += transform.right;
        if (left) direction -= transform.right;

        // 归一化方向向量，防止对角线移动更快
        if (direction.magnitude > 1.0f)
        {
            direction.Normalize();
        }

        return direction;
    }

    void ApplyMovement()
    {
        // 如果没有移动方向，则不执行移动
        if (movementDirection == Vector3.zero) return;

        // 使用Rigidbody移动
        Vector3 newVelocity = movementDirection * walkSpeed;
        newVelocity.y = rb.velocity.y; // 保持原有的垂直速度（重力/跳跃）
        rb.velocity = newVelocity;
    }

    void ApplyGravity()
    {
        // 如果不在跳跃状态且不在地面上，应用重力
        if (!isGrounded && !isJumping)
        {
            rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    void StartJump()
    {
        isJumping = true;
        animator.SetBool("IsJumping", true);

        // 应用跳跃力
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // 结束跳跃
    void EndJump()
    {
        isJumping = false;
        animator.SetBool("IsJumping", false);
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // 绘制移动方向
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, movementDirection * 2);

            // 绘制地面检测线
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 rayStart = transform.position + Vector3.up * 0.1f;
            Gizmos.DrawLine(rayStart, rayStart + Vector3.down * 0.2f);
        }
    }
}