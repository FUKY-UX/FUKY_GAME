using UnityEngine;

public class RoleController : MonoBehaviour
{
    [Header("角色组件")]
    public Rigidbody BodyPos;
    public CapsuleCollider BodyCol;
    public Transform Cam;
    public AudioSource HeadSoundSource;

    [Header("移动设置")]
    [Range(0.1f, 20f)] public float MoveSpeed = 10f;
    [Range(0.1f, 2f)] public float Acceleration = 0.5f;
    [Range(0.1f, 2f)] public float Deceleration = 0.8f;
    [Range(0.1f, 10f)] public float AirControlFactor = 0.5f;

    [Header("视角控制")]
    [Range(50f, 500f)] public float MouseSensitivity = 200f;
    [Range(-90f, 0f)] public float MinVerticalAngle = -85f;
    [Range(0f, 90f)] public float MaxVerticalAngle = 85f;
    private float verticalRotation = 0f;

    [Header("跳跃设置")]
    [Range(1f, 15f)] public float JumpForce = 8f;
    [Range(0.1f, 1f)] public float JumpCooldown = 0.2f;
    [Range(0.1f, 2f)] public float GroundCheckDistance = 0.2f;
    [Range(0.1f, 5f)] public float CoyoteTime = 0.15f;
    private float lastJumpTime;
    private float lastGroundedTime;

    [Header("下蹲设置")]
    [Range(0.1f, 1f)] public float SquatSpeed = 0.2f;
    [Range(0.1f, 1f)] public float StandingHeight = 2f;
    [Range(0.1f, 1f)] public float CrouchingHeight = 1f;
    private bool isCrouching;

    [Header("状态")]
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 targetMoveDirection = Vector3.zero;
    private bool isGrounded;
    private bool lockMouse = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        verticalRotation = Cam.localEulerAngles.x;
        if (verticalRotation > 180) verticalRotation -= 360;
    }

    void Update()
    {
        HandleMouseLock();
        HandleRotation();
        HandleMovementInput();
        HandleJumpInput();
        HandleCrouchInput();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        ApplyMovement();
        ApplyCrouch();
    }

    private void HandleMouseLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            lockMouse = !lockMouse;
            Cursor.lockState = lockMouse ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockMouse;
        }
    }

    private void HandleRotation()
    {
        if (!lockMouse) return;

        // 水平旋转（角色）
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        // 垂直旋转（摄像机）
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, MinVerticalAngle, MaxVerticalAngle);
        Cam.localEulerAngles = new Vector3(verticalRotation, 0f, 0f);
    }

    private void HandleMovementInput()
    {
        // 使用平滑输入而非直接按键检测
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 获取摄像机方向（忽略Y轴）
        Vector3 camForward = Cam.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = Cam.right;
        camRight.y = 0;
        camRight.Normalize();

        // 计算基于摄像机方向的目标移动方向
        targetMoveDirection = (camForward * vertical + camRight * horizontal).normalized;

        // 在地面时平滑过渡方向
        if (isGrounded)
        {
            moveDirection = Vector3.Lerp(moveDirection, targetMoveDirection, Acceleration * Time.deltaTime * 10f);
        }
        // 在空中时保留部分控制
        else
        {
            moveDirection = Vector3.Lerp(moveDirection, targetMoveDirection, AirControlFactor * Time.deltaTime * 5f);
        }
    }

    private void CheckGrounded()
    {
        // 使用球形检测更可靠的地面检测
        bool wasGrounded = isGrounded;
        isGrounded = Physics.SphereCast(
            transform.position + Vector3.up * (BodyCol.radius - 0.01f),
            BodyCol.radius - 0.01f,
            Vector3.down,
            out RaycastHit hit,
            GroundCheckDistance
        );

        // 保留最后接地时间（用于Coyote Time）
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }
        else if (wasGrounded)
        {
            // 刚离开地面时提供短暂时间仍可跳跃
            lastGroundedTime = Time.time;
        }
    }

    private void ApplyMovement()
    {
        // 计算当前速度（忽略Y轴）
        Vector3 currentVelocity = BodyPos.velocity;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        // 计算目标速度
        Vector3 targetVelocity = moveDirection * MoveSpeed;

        // 在地面时应用加速度/减速度
        if (isGrounded)
        {
            // 向目标速度加速
            if (targetVelocity.magnitude > 0.1f)
            {
                horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, Acceleration * Time.fixedDeltaTime * 10f);
            }
            // 减速停止
            else
            {
                horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Deceleration * Time.fixedDeltaTime * 10f);
            }
        }

        // 应用最终速度（保留垂直速度）
        BodyPos.velocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);
    }

    private void HandleJumpInput()
    {
        // 检查是否可跳跃（Coyote Time 允许离地后短暂时间内仍可跳跃）
        bool canJump = (Time.time - lastGroundedTime <= CoyoteTime) && (Time.time - lastJumpTime > JumpCooldown);

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            // 跳跃时取消垂直速度（允许从斜坡跳跃）
            Vector3 velocity = BodyPos.velocity;
            velocity.y = 0;
            BodyPos.velocity = velocity;

            BodyPos.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time;
        }
    }

    private void HandleCrouchInput()
    {
        // 切换下蹲状态
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
        }

        // 跳跃时自动取消下蹲
        if (isCrouching && Input.GetKeyDown(KeyCode.Space))
        {
            isCrouching = false;
        }
    }

    private void ApplyCrouch()
    {
        float targetHeight = isCrouching ? CrouchingHeight : StandingHeight;

        // 检查头顶是否有障碍物（防止站起时卡住）
        if (!isCrouching && Physics.Raycast(transform.position, Vector3.up, StandingHeight - CrouchingHeight + 0.1f))
        {
            targetHeight = CrouchingHeight;
            isCrouching = true;
        }

        // 平滑过渡高度
        if (Mathf.Abs(BodyCol.height - targetHeight) > 0.01f)
        {
            BodyCol.height = Mathf.Lerp(BodyCol.height, targetHeight, SquatSpeed * 10f * Time.fixedDeltaTime);

            // 调整碰撞体位置使底部保持在地面
            float heightDifference = targetHeight - BodyCol.height;
            BodyCol.center = new Vector3(0, BodyCol.height / 2, 0);
        }
    }

    // 可视化地面检测
    private void OnDrawGizmosSelected()
    {
        if (BodyCol != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 sphereCenter = transform.position + Vector3.up * (BodyCol.radius - 0.01f) - Vector3.up * GroundCheckDistance;
            Gizmos.DrawWireSphere(sphereCenter, BodyCol.radius - 0.01f);
        }
    }
}