using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
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

    [Header("�ӽ�ҡ��")]
    public float walkBobFrequency = 1.5f;
    public float walkBobHeight = 0.1f;
    public float walkBobSide = 0.05f;
    public float sprintMultiplier = 1.5f;
    public float landBobAmount = 0.2f;
    public float bobSmooth = 5f;
    public float minMoveThreshold = 0.1f; // ��������С�ƶ���ֵ

    private CharacterController controller;
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
    private bool isMoving = false; // �������ƶ�״̬���

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
        float currentSpeed = sprinting ? sprintSpeed : moveSpeed;

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

            // ����Ŀ��λ��
            targetCameraPosition = cameraOriginalPosition +
                                  new Vector3(bobSide, bobHeight, 0);
        }
        else
        {
            // ƽ���ص�ԭʼλ��
            targetCameraPosition = cameraOriginalPosition;

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
        // ���ʱ�Ķ���ҡ��Ч��
        //if (isGrounded && !wasGrounded)
        //{
        //    // ʹ����ʱ��������ֱ���޸������λ��
        //    Vector3 landingPosition = cameraTransform.localPosition;
        //    landingPosition.y -= landBobAmount;

        //    // ֱ������λ��ȷ��������Ч
        //    cameraTransform.localPosition = landingPosition;

        //    // ����Ŀ��λ�ñ���һ����
        //    targetCameraPosition = landingPosition;
        //}
        //wasGrounded = isGrounded;
    }
}