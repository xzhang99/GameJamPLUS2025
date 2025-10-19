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
    public float gravityMultiplier = 3.0f; // 新增：重力倍数

    [Header("Input Settings")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public string mouseXAxis = "Mouse X";

    // 用于控制角色移动的组件
    private CharacterController characterController;
    private Rigidbody rb;

    private Vector3 movementDirection;
    private bool isJumping = false;
    private Vector3 velocity; // 新增：用于CharacterController的速度

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        // 尝试获取角色控制器或刚体组件
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        // 如果没有找到任何移动组件，添加一个刚体
        if (characterController == null && rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.freezeRotation = true; // 防止旋转
            // 添加一个胶囊碰撞体
            CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.height = 2.0f;
            capsule.center = new Vector3(0, 1.0f, 0);
        }

        // 确保动画状态正确初始化
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsWalking", false);
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleRotation();

        // 简化跳跃条件 - 只检查是否已经在跳跃
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            StartJump();
        }

        // 检测跳跃动画是否自然结束
        if (isJumping && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            EndJump();
        }

        // 应用重力（适用于CharacterController）
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
            float rotationAmount = mouseX * 2.0f * rotationSpeed * Time.deltaTime;

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

        // 根据使用的物理组件应用移动
        if (characterController != null && characterController.enabled)
        {
            // 使用CharacterController移动
            Vector3 move = movementDirection * walkSpeed;
            move.y = velocity.y; // 包含垂直速度
            characterController.Move(move * Time.deltaTime);
        }
        else if (rb != null)
        {
            // 使用Rigidbody移动
            // 注意：这里使用速度控制，适用于非物理精确的场景
            Vector3 newVelocity = movementDirection * walkSpeed;
            newVelocity.y = rb.velocity.y; // 保持原有的垂直速度（重力/跳跃）
            rb.velocity = newVelocity;
        }
        else
        {
            // 如果没有物理组件，直接移动Transform
            transform.position += movementDirection * walkSpeed * Time.deltaTime;
        }
    }

    // 新增：应用重力
    void ApplyGravity()
    {
        if (characterController != null)
        {
            // 如果角色在地面上，重置垂直速度
            if (characterController.isGrounded && velocity.y < 0)
            {
                velocity.y = -0.5f; // 小的负值确保角色保持在地面上
            }
            else
            {
                // 应用重力
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
            // 使用更大的跳跃力
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // 为Rigidbody增加重力缩放
            if (rb.useGravity)
            {
                // 如果使用重力，增加重力倍数
                rb.mass *= gravityMultiplier;
            }
        }
        else if (characterController != null)
        {
            // 如果使用CharacterController，应用跳跃速度
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
        }
    }

    // 结束跳跃
    void EndJump()
    {
        isJumping = false;
        animator.SetBool("IsJumping", false);

        // 重置Rigidbody的质量（如果之前修改过）
        if (rb != null)
        {
            rb.mass = 1.0f; // 重置为默认质量
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