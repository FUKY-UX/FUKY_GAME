using UnityEngine;

public class RoleController : MonoBehaviour
{
    [Header("��ɫ���")]
    public Rigidbody BodyPos;
    public CapsuleCollider BodyCol;
    public Transform Cam;
    public AudioSource HeadSoundSource;

    [Header("�ƶ�����")]
    [Range(0.1f, 20f)] public float MoveSpeed = 10f;
    [Range(0.1f, 2f)] public float Acceleration = 0.5f;
    [Range(0.1f, 2f)] public float Deceleration = 0.8f;
    [Range(0.1f, 10f)] public float AirControlFactor = 0.5f;

    [Header("�ӽǿ���")]
    [Range(50f, 500f)] public float MouseSensitivity = 200f;
    [Range(-90f, 0f)] public float MinVerticalAngle = -85f;
    [Range(0f, 90f)] public float MaxVerticalAngle = 85f;
    private float verticalRotation = 0f;

    [Header("��Ծ����")]
    [Range(1f, 15f)] public float JumpForce = 8f;
    [Range(0.1f, 1f)] public float JumpCooldown = 0.2f;
    [Range(0.1f, 2f)] public float GroundCheckDistance = 0.2f;
    [Range(0.1f, 5f)] public float CoyoteTime = 0.15f;
    private float lastJumpTime;
    private float lastGroundedTime;

    [Header("�¶�����")]
    [Range(0.1f, 1f)] public float SquatSpeed = 0.2f;
    [Range(0.1f, 1f)] public float StandingHeight = 2f;
    [Range(0.1f, 1f)] public float CrouchingHeight = 1f;
    private bool isCrouching;

    [Header("״̬")]
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

        // ˮƽ��ת����ɫ��
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        // ��ֱ��ת���������
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, MinVerticalAngle, MaxVerticalAngle);
        Cam.localEulerAngles = new Vector3(verticalRotation, 0f, 0f);
    }

    private void HandleMovementInput()
    {
        // ʹ��ƽ���������ֱ�Ӱ������
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // ��ȡ��������򣨺���Y�ᣩ
        Vector3 camForward = Cam.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = Cam.right;
        camRight.y = 0;
        camRight.Normalize();

        // �����������������Ŀ���ƶ�����
        targetMoveDirection = (camForward * vertical + camRight * horizontal).normalized;

        // �ڵ���ʱƽ�����ɷ���
        if (isGrounded)
        {
            moveDirection = Vector3.Lerp(moveDirection, targetMoveDirection, Acceleration * Time.deltaTime * 10f);
        }
        // �ڿ���ʱ�������ֿ���
        else
        {
            moveDirection = Vector3.Lerp(moveDirection, targetMoveDirection, AirControlFactor * Time.deltaTime * 5f);
        }
    }

    private void CheckGrounded()
    {
        // ʹ�����μ����ɿ��ĵ�����
        bool wasGrounded = isGrounded;
        isGrounded = Physics.SphereCast(
            transform.position + Vector3.up * (BodyCol.radius - 0.01f),
            BodyCol.radius - 0.01f,
            Vector3.down,
            out RaycastHit hit,
            GroundCheckDistance
        );

        // �������ӵ�ʱ�䣨����Coyote Time��
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }
        else if (wasGrounded)
        {
            // ���뿪����ʱ�ṩ����ʱ���Կ���Ծ
            lastGroundedTime = Time.time;
        }
    }

    private void ApplyMovement()
    {
        // ���㵱ǰ�ٶȣ�����Y�ᣩ
        Vector3 currentVelocity = BodyPos.velocity;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        // ����Ŀ���ٶ�
        Vector3 targetVelocity = moveDirection * MoveSpeed;

        // �ڵ���ʱӦ�ü��ٶ�/���ٶ�
        if (isGrounded)
        {
            // ��Ŀ���ٶȼ���
            if (targetVelocity.magnitude > 0.1f)
            {
                horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, Acceleration * Time.fixedDeltaTime * 10f);
            }
            // ����ֹͣ
            else
            {
                horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Deceleration * Time.fixedDeltaTime * 10f);
            }
        }

        // Ӧ�������ٶȣ�������ֱ�ٶȣ�
        BodyPos.velocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);
    }

    private void HandleJumpInput()
    {
        // ����Ƿ����Ծ��Coyote Time ������غ����ʱ�����Կ���Ծ��
        bool canJump = (Time.time - lastGroundedTime <= CoyoteTime) && (Time.time - lastJumpTime > JumpCooldown);

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            // ��Ծʱȡ����ֱ�ٶȣ������б����Ծ��
            Vector3 velocity = BodyPos.velocity;
            velocity.y = 0;
            BodyPos.velocity = velocity;

            BodyPos.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time;
        }
    }

    private void HandleCrouchInput()
    {
        // �л��¶�״̬
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
        }

        // ��Ծʱ�Զ�ȡ���¶�
        if (isCrouching && Input.GetKeyDown(KeyCode.Space))
        {
            isCrouching = false;
        }
    }

    private void ApplyCrouch()
    {
        float targetHeight = isCrouching ? CrouchingHeight : StandingHeight;

        // ���ͷ���Ƿ����ϰ����ֹվ��ʱ��ס��
        if (!isCrouching && Physics.Raycast(transform.position, Vector3.up, StandingHeight - CrouchingHeight + 0.1f))
        {
            targetHeight = CrouchingHeight;
            isCrouching = true;
        }

        // ƽ�����ɸ߶�
        if (Mathf.Abs(BodyCol.height - targetHeight) > 0.01f)
        {
            BodyCol.height = Mathf.Lerp(BodyCol.height, targetHeight, SquatSpeed * 10f * Time.fixedDeltaTime);

            // ������ײ��λ��ʹ�ײ������ڵ���
            float heightDifference = targetHeight - BodyCol.height;
            BodyCol.center = new Vector3(0, BodyCol.height / 2, 0);
        }
    }

    // ���ӻ�������
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