using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
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

    [Header("视角摇晃")]
    public float walkBobFrequency = 1.5f;
    public float walkBobHeight = 0.1f;
    public float walkBobSide = 0.05f;
    public float sprintMultiplier = 1.5f;
    public float landBobAmount = 0.2f;
    public float bobSmooth = 5f;
    public float minMoveThreshold = 0.1f; // 新增：最小移动阈值

    private CharacterController controller;
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
    private bool isMoving = false; // 新增：移动状态检测

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        cameraTransform = playerCamera.transform;
        cameraOriginalPosition = cameraTransform.localPosition;
        defaultCameraY = cameraOriginalPosition.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleHeadBob();
        HandleLandingEffect();
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
        float currentSpeed = sprinting ? sprintSpeed : moveSpeed;

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

            // 设置目标位置
            targetCameraPosition = cameraOriginalPosition +
                                  new Vector3(bobSide, bobHeight, 0);
        }
        else
        {
            // 平滑回到原始位置
            targetCameraPosition = cameraOriginalPosition;

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
        // 落地时的额外摇晃效果
        //if (isGrounded && !wasGrounded)
        //{
        //    // 使用临时变量避免直接修改摄像机位置
        //    Vector3 landingPosition = cameraTransform.localPosition;
        //    landingPosition.y -= landBobAmount;

        //    // 直接设置位置确保立即生效
        //    cameraTransform.localPosition = landingPosition;

        //    // 更新目标位置保持一致性
        //    targetCameraPosition = landingPosition;
        //}
        //wasGrounded = isGrounded;
    }
}