using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("���ù���")]
    public bool IsDisAbleMovementAndLook = false;
    public CharacterController controller;
    [Header("�ƶ�����")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 7f;
    public float gravity = 20f;
    public float airControl = 0.5f;

    [Header("�ӽ�����")]
    public float mouseSensitivity = 2f;
    public float minVerticalAngle = -90f;
    public float maxVerticalAngle = 90f;

    [Header("�׷�����")]
    public float crouchHeight = 1.0f;
    public float crouchSpeed = 2.5f;
    public float crouchTransitionSpeed = 10f;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("�ӽ�ҡ��")]
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

    // �ӽ�ҡ�����
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
    private float currentHeight; // ��ǰʵ�ʸ߶�
    private Vector3 currentCenter; // ��ǰʵ�����ĵ�

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        cameraTransform = playerCamera.transform;
        cameraOriginalPosition = cameraTransform.localPosition;
        defaultCameraY = cameraOriginalPosition.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ��ʼ���߶Ȳ���
        standHeight = controller.height;
        standCenter = controller.center;
        crouchCenter = standCenter - new Vector3(0, (standHeight - crouchHeight) / 2, 0);

        // ���õ�ǰ�߶Ⱥ����ĵ�Ϊվ��״̬
        currentHeight = standHeight;
        currentCenter = standCenter;
    }

    void Update()
    {
        if (IsDisAbleMovementAndLook)
        {
            // ������С�����߼�
            isGrounded = controller.isGrounded;
            controller.Move(Vector3.zero); // ������ײ���
            return;
        }

        HandleCrouch();
        HandleMovement();
        HandleMouseLook();
        HandleHeadBob();
        HandleLandingEffect();

        // Ӧ��ƽ������
        ApplyCrouchTransition();
    }

    void HandleCrouch()
    {
        bool wantToCrouch = Input.GetKey(crouchKey);

        // ��վ���л�������
        if (wantToCrouch && !isCrouching)
        {
            isCrouching = true;
        }
        // �Ӷ����л���վ������Ҫ���ͷ���ռ䣩
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
        // ȷ��Ŀ��߶Ⱥ����ĵ�
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        Vector3 targetCenter = isCrouching ? crouchCenter : standCenter;

        // ʹ��Lerpƽ�����ɸ߶Ⱥ����ĵ�
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        currentCenter = Vector3.Lerp(currentCenter, targetCenter, crouchTransitionSpeed * Time.deltaTime);

        // Ӧ�ù��ɺ��ֵ
        controller.height = currentHeight;
        controller.center = currentCenter;

        // ����������߶ȣ���ѡ��
        AdjustCameraHeight();
    }

    void AdjustCameraHeight()
    {
        // ���������Ӧ�½��ĸ߶Ȳ�
        float heightDifference = standHeight - currentHeight;

        // �����µ������λ�ã�����ԭʼλ�ü�ȥ�߶Ȳ
        Vector3 adjustedPosition = cameraOriginalPosition;
        adjustedPosition.y -= heightDifference;

        // Ӧ��ƽ�����ɵ���λ��
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            adjustedPosition,
            crouchTransitionSpeed * Time.deltaTime
        );
    }

    // ���ͷ���Ƿ����㹻�ռ�վ��
    private bool CanStandUp()
    {
        float checkDistance = standHeight - crouchHeight;
        Vector3 rayStart = transform.position + currentCenter + Vector3.up * (currentHeight / 2);

        // ���Ϸ������߼���ϰ���
        if (Physics.Raycast(rayStart, Vector3.up, checkDistance))
        {
            return false;
        }
        return true;
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        // ��ȡ����
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        bool jumping = Input.GetButton("Jump");

        // ����Ƿ����ƶ�
        isMoving = (Mathf.Abs(horizontal) > minMoveThreshold ||
                   (Mathf.Abs(vertical) > minMoveThreshold));

        // �����ƶ�����
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // �޸��ٶȼ��㲿�֣����Ƕ���״̬��
        float currentSpeed = isCrouching ? crouchSpeed :
            (sprinting ? sprintSpeed : moveSpeed);

        // ��ֱ�ٶȼ���
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

        // ����ˮƽ�ƶ�����
        Vector3 horizontalMove = Vector3.zero;

        // �����ƶ�
        if (isGrounded && isMoving)
        {
            horizontalMove = (forward * vertical + right * horizontal) * currentSpeed;
        }
        // ���п���
        else if (!isGrounded && isMoving)
        {
            horizontalMove = (forward * vertical + right * horizontal) * (currentSpeed * airControl);
        }

        // ���ˮƽ�ʹ�ֱ�ƶ�
        moveDirection = horizontalMove;
        moveDirection.y = verticalVelocity;

        // Ӧ���ƶ�
        controller.Move(moveDirection * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        // �������
        rotationY += Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // ���ƴ�ֱ�Ƕ�
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        // Ӧ����ת
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

    void HandleHeadBob()
    {
        if (isMoving && isGrounded)
        {
            // �����Ƿ��ܵ�������
            float bobMultiplier = Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f;

            // ����ҡ��λ��
            bobTimer += Time.deltaTime * walkBobFrequency * bobMultiplier;
            float bobHeight = Mathf.Sin(bobTimer) * walkBobHeight * bobMultiplier;
            float bobSide = Mathf.Cos(bobTimer * 0.5f) * walkBobSide * bobMultiplier;

            // ����Ŀ��λ�ã����϶׷�ƫ�ƣ�
            Vector3 bobOffset = new Vector3(bobSide, bobHeight, 0);
            targetCameraPosition = cameraOriginalPosition - new Vector3(0, standHeight - currentHeight, 0) + bobOffset;
        }
        else
        {
            // ƽ���ص�ԭʼλ�ã����Ƕ׷�ƫ�ƣ�
            targetCameraPosition = cameraOriginalPosition - new Vector3(0, standHeight - currentHeight, 0);

            // ����ҡ�μ�ʱ��
            if (!isMoving)
            {
                bobTimer = 0;
            }
        }

        // Ӧ��λ�ò�ֵ
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            targetCameraPosition,
            Time.deltaTime * bobSmooth
        );
    }

    void HandleLandingEffect()
    {
        // ���Ч������ѡʵ�֣�
    }
}