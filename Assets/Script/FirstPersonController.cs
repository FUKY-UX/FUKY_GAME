using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("禁用功能")]
    public bool IsDisAbleMovementAndLook = false;
    public CharacterController controller;
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 7f;
    public float gravity = 20f;
    public float airControl = 0.5f;

    [Header("视角设置")]
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -90f;
    public float maxVerticalAngle = 90f;

    [Header("蹲伏设置")]
    public float crouchHeight = 1.0f;
    public float crouchSpeed = 2.5f;
    public float crouchTransitionSpeed = 10f;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("视角摇晃")]
    public float walkBobFrequency = 1.5f;
    public float walkBobHeight = 0.1f;
    public float walkBobSide = 0.05f;
    public float sprintMultiplier = 1.5f;
    public float landBobAmount = 0.2f;
    public float bobSmooth = 5f;
    public float minMoveThreshold = 0.1f;


    private Camera playerCamera;
    private Transform cameraTransform;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private float rotationY = 0;

    // 视角摇晃相关
    private float defaultCameraY;
    private float bobTimer = 0;
    private Vector3 cameraOriginalPosition;
    private Vector3 targetCameraPosition;
    private bool isGrounded = false;
    private bool wasGrounded = false;
    private float verticalVelocity = 0;
    private bool isMoving = false;
    private float standHeight;
    private Vector3 standCenter;
    private Vector3 crouchCenter;
    private bool isCrouching = false;
    private float currentHeight; // 当前实际高度
    private Vector3 currentCenter; // 当前实际中心点

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        cameraTransform = playerCamera.transform;
        cameraOriginalPosition = cameraTransform.localPosition;
        defaultCameraY = cameraOriginalPosition.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 初始化高度参数
        standHeight = controller.height;
        standCenter = controller.center;
        crouchCenter = standCenter - new Vector3(0, (standHeight - crouchHeight) / 2, 0);

        // 设置当前高度和中心点为站立状态
        currentHeight = standHeight;
        currentCenter = standCenter;
    }

    void Update()
    {
        if (IsDisAbleMovementAndLook)
        {
            // 保留最小物理逻辑
            isGrounded = controller.isGrounded;
            controller.Move(Vector3.zero); // 触发碰撞检测
            return;
        }

        HandleCrouch();
        HandleMovement();
        HandleMouseLook();
        HandleHeadBob();
        HandleLandingEffect();

        // 应用平滑过渡
        ApplyCrouchTransition();
    }

    void HandleCrouch()
    {
        bool wantToCrouch = Input.GetKey(crouchKey);

        // 从站立切换到蹲下
        if (wantToCrouch && !isCrouching)
        {
            isCrouching = true;
        }
        // 从蹲下切换到站立（需要检查头顶空间）
        else if (!wantToCrouch && isCrouching)
        {
            if (CanStandUp())
            {
                isCrouching = false;
            }
        }
    }

    void ApplyCrouchTransition()
    {
        // 确定目标高度和中心点
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        Vector3 targetCenter = isCrouching ? crouchCenter : standCenter;

        // 使用Lerp平滑过渡高度和中心点
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        currentCenter = Vector3.Lerp(currentCenter, targetCenter, crouchTransitionSpeed * Time.deltaTime);

        // 应用过渡后的值
        controller.height = currentHeight;
        controller.center = currentCenter;

        // 调整摄像机高度（可选）
        AdjustCameraHeight();
    }

    void AdjustCameraHeight()
    {
        // 计算摄像机应下降的高度差
        float heightDifference = standHeight - currentHeight;

        // 创建新的摄像机位置（保持原始位置减去高度差）
        Vector3 adjustedPosition = cameraOriginalPosition;
        adjustedPosition.y -= heightDifference;

        // 应用平滑过渡到新位置
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            adjustedPosition,
            crouchTransitionSpeed * Time.deltaTime
        );
    }

    // 检查头顶是否有足够空间站起
    private bool CanStandUp()
    {
        float checkDistance = standHeight - crouchHeight;
        Vector3 rayStart = transform.position + currentCenter + Vector3.up * (currentHeight / 2);

        // 向上发射射线检测障碍物
        if (Physics.Raycast(rayStart, Vector3.up, checkDistance))
        {
            return false;
        }
        return true;
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        // 获取输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        bool jumping = Input.GetButton("Jump");

        // 检测是否在移动
        isMoving = (Mathf.Abs(horizontal) > minMoveThreshold ||
                   (Mathf.Abs(vertical) > minMoveThreshold));

        // 计算移动方向
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // 修改速度计算部分（考虑蹲下状态）
        float currentSpeed = isCrouching ? crouchSpeed :
            (sprinting ? sprintSpeed : moveSpeed);

        // 垂直速度计算
        if (isGrounded)
        {
            verticalVelocity = -gravity * Time.deltaTime;
            if (jumping)
            {
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // 计算水平移动向量
        Vector3 horizontalMove = Vector3.zero;

        // 地面移动
        if (isGrounded && isMoving)
        {
            horizontalMove = (forward * vertical + right * horizontal) * currentSpeed;
        }
        // 空中控制
        else if (!isGrounded && isMoving)
        {
            horizontalMove = (forward * vertical + right * horizontal) * (currentSpeed * airControl);
        }

        // 组合水平和垂直移动
        moveDirection = horizontalMove;
        moveDirection.y = verticalVelocity;

        // 应用移动
        controller.Move(moveDirection * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        // 鼠标输入
        rotationY += Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 限制垂直角度
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        // 应用旋转
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

    void HandleHeadBob()
    {
        if (isMoving && isGrounded)
        {
            // 根据是否奔跑调整参数
            float bobMultiplier = Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f;

            // 计算摇晃位置
            bobTimer += Time.deltaTime * walkBobFrequency * bobMultiplier;
            float bobHeight = Mathf.Sin(bobTimer) * walkBobHeight * bobMultiplier;
            float bobSide = Mathf.Cos(bobTimer * 0.5f) * walkBobSide * bobMultiplier;

            // 设置目标位置（加上蹲伏偏移）
            Vector3 bobOffset = new Vector3(bobSide, bobHeight, 0);
            targetCameraPosition = cameraOriginalPosition - new Vector3(0, standHeight - currentHeight, 0) + bobOffset;
        }
        else
        {
            // 平滑回到原始位置（考虑蹲伏偏移）
            targetCameraPosition = cameraOriginalPosition - new Vector3(0, standHeight - currentHeight, 0);

            // 重置摇晃计时器
            if (!isMoving)
            {
                bobTimer = 0;
            }
        }

        // 应用位置插值
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            targetCameraPosition,
            Time.deltaTime * bobSmooth
        );
    }

    void HandleLandingEffect()
    {
        // 落地效果（可选实现）
    }
}