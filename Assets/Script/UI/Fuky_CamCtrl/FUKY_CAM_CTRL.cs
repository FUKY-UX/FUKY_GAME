using UnityEngine;

/// <summary>
/// �Ľ�������������� - ʵ��"˩С��"ʽ��קЧ��
/// </summary>
[RequireComponent(typeof(Camera))]
public class FUKY_CAM_CTRL : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("Ҫ�����Ŀ������")]
    public Transform target;

    [Tooltip("��������ƶ��ĸ��淶Χ")]
    public float FollowingRange = 1f;

    [Tooltip("�������Ŀ���ƫ����")]
    public Vector3 offset = new Vector3(0f, 2f, -5f);

    [Tooltip("����ƽ���� (ֵԽСԽƽ��)")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;

    [Tooltip("�����ӳ� (��קЧ��ǿ��)")]
    [Range(0f, 1f)]
    public float followLag = 0.2f;

    [Header("�ӽ�����")]
    [Tooltip("ʼ�տ���Ŀ��")]
    public bool lookAtTarget = true;

    [Tooltip("����Ŀ���ƫ�Ƶ�")]
    public Vector3 lookAtOffset = Vector3.zero;

    [Tooltip("�ӽ�ƽ����")]
    [Range(0.01f, 1f)]
    public float rotationSmoothness = 0.1f;

    [Header("��ײ���")]
    [Tooltip("������ײ����")]
    public bool avoidObstacles = true;

    [Tooltip("��ײ������")]
    public float collisionDistance = 1f;

    [Tooltip("��ײ���뾶")]
    public float collisionRadius = 0.5f;

    [Tooltip("��ײ������")]
    public LayerMask collisionMask = -1;

    [Header("�߼�����")]
    [Tooltip("������ƶ�ʱ������ٶ�")]
    public float maxSpeed = 10f;

    [Tooltip("Ŀ�궪ʧʱ�Ƿ񷵻�Ĭ��λ��")]
    public bool returnToDefaultOnTargetLost = true;

    [Tooltip("Ĭ��λ��")]
    public Transform defaultPosition;

    [Tooltip("���ñ߽�����")]
    public bool useBoundaries = false;

    [Tooltip("��Ϸ�߽�")]
    public Vector3 boundaryMin = new Vector3(-10f, 0f, -10f);
    public Vector3 boundaryMax = new Vector3(10f, 10f, 10f);

    // ˽�б���
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _currentOffset;
    private Vector3 _desiredPosition;
    private Quaternion _desiredRotation;
    private Vector3 _lastTargetPosition;
    private float _distanceToTarget;
    private Camera _camera;
    private bool _isWithinRange = true;
    private Vector3 _leashAnchorPoint; // ����ê��λ��

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _currentOffset = offset;

        if (target)
        {
            _lastTargetPosition = target.position;
            // ��ʼ��λ��
            _desiredPosition = CalculateLeashPosition();
            transform.position = _desiredPosition;

            if (lookAtTarget)
            {
                transform.LookAt(target.position + lookAtOffset);
            }

            // ��ʼ������ê��
            _leashAnchorPoint = transform.position;
        }
    }

    private void Update()
    {
    if (FUKYMouse.Instance.Left_pressed)
    {
        // ��ȡ����ƶ���
        Vector3 mouseDelta = FUKYMouse.Instance.deltaTranslate * FUKYMouse.Instance.PressureValue;
        
        // ����Ŀ���ƶ�����
        // - ���ң�����ҷ��� (x�����)
        // - ���£�����Y��
        // - ǰ�����ǰ���� (z�����)
        Vector3 moveOffset = _camera.transform.right * mouseDelta.x   // ����
                            + Vector3.up * mouseDelta.y               // ����
                            + _camera.transform.forward * mouseDelta.z; // ǰ��
        
        target.localPosition += moveOffset;
    }
    
    if (FUKYMouse.Instance.Right_pressed)
    {
        // ��ȡ����ƶ���
        Vector3 mouseDelta = FUKYMouse.Instance.deltaTranslate * FUKYMouse.Instance.PressureValue;
        
        // ����ƶ�������㣨��target�߼���ͬ��
        Vector3 moveOffset = _camera.transform.right * mouseDelta.x   // ����
                            + Vector3.up * mouseDelta.y               // ����
                            + _camera.transform.forward * mouseDelta.z; // ǰ��
        
        _camera.transform.position += moveOffset;
    }    }

    private void LateUpdate()
    {
        if (!target)
        {
            HandleTargetLost();
            return;
        }

        // ���㵽Ŀ��ľ���
        _distanceToTarget = Vector3.Distance(transform.position, target.position);

        // ����Ƿ��ڸ��淶Χ��
        _isWithinRange = _distanceToTarget <= FollowingRange;

        if (_isWithinRange)
        {
            // �ڷ�Χ�ڣ�ֻ��ת�����ƶ�
            _leashAnchorPoint = transform.position; // ����ê��
        }
        else
        {
            // ������Χ�������µ�����λ�ã����ſ���Ŀ��ķ���
            _desiredPosition = CalculateLeashPosition();
        }

        // Ӧ�ñ߽�����
        if (useBoundaries)
        {
            _desiredPosition = ApplyBoundaries(_desiredPosition);
        }

        // Ӧ����ײ����
        if (avoidObstacles)
        {
            _desiredPosition = AdjustForObstacles(_desiredPosition);
        }

        // ƽ���ƶ�����������ڳ�����Χʱ�ƶ���
        if (!_isWithinRange)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                _desiredPosition,
                ref _velocity,
                smoothSpeed,
                maxSpeed
            );
        }

        // ʼ�ո��¿���Ŀ��
        if (lookAtTarget)
        {
            UpdateLookAtTarget();
        }

        // �������λ��
        _lastTargetPosition = target.position;
    }

    /// <summary>
    /// ��������Ч��λ�ã����ſ���Ŀ��ķ���
    /// </summary>
    private Vector3 CalculateLeashPosition()
    {
        // 1. �����ê�㵽Ŀ��ķ���
        Vector3 toTarget = target.position - _leashAnchorPoint;

        // 2. �������ӳ��ȷ����ϵĵ�
        Vector3 leashDirection = toTarget.normalized;
        Vector3 leashEndPoint = _leashAnchorPoint + leashDirection * FollowingRange;

        // 3. �����קЧ��
        Vector3 lagOffset = Vector3.zero;
        if (followLag > 0)
        {
            Vector3 targetMovement = target.position - _lastTargetPosition;
            lagOffset = targetMovement * followLag;
        }

        // 4. ��������λ��
        return leashEndPoint - lagOffset;
    }

    /// <summary>
    /// ���¿���Ŀ��
    /// </summary>
    private void UpdateLookAtTarget()
    {
        _desiredRotation = Quaternion.LookRotation(
            (target.position + lookAtOffset) - transform.position
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            _desiredRotation,
            rotationSmoothness
        );
    }

    /// <summary>
    /// ���������ϰ���
    /// </summary>
    private Vector3 AdjustForObstacles(Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - (target.position + lookAtOffset);
        float distance = direction.magnitude;

        // �������߼��
        if (Physics.SphereCast(
            target.position + lookAtOffset,
            collisionRadius,
            direction.normalized,
            out RaycastHit hit,
            distance,
            collisionMask))
        {
            // ��������ϰ����λ��
            float hitDistance = hit.distance - collisionDistance;
            if (hitDistance < 0) hitDistance = 0;

            Vector3 adjustedPosition = target.position + lookAtOffset +
                                       direction.normalized * hitDistance;

            // ��Ŀ��λ���������������΢ƫ��
            Vector3 offsetDirection = (adjustedPosition - (target.position + lookAtOffset)).normalized;
            return adjustedPosition - offsetDirection * 0.2f;
        }

        return desiredPosition;
    }

    /// <summary>
    /// Ӧ�ñ߽�����
    /// </summary>
    private Vector3 ApplyBoundaries(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, boundaryMin.x, boundaryMax.x),
            Mathf.Clamp(position.y, boundaryMin.y, boundaryMax.y),
            Mathf.Clamp(position.z, boundaryMin.z, boundaryMax.z)
        );
    }

    /// <summary>
    /// ����Ŀ�궪ʧ
    /// </summary>
    private void HandleTargetLost()
    {
        if (!returnToDefaultOnTargetLost || !defaultPosition)
        {
            enabled = false;
            return;
        }

        // ƽ������Ĭ��λ��
        transform.position = Vector3.SmoothDamp(
            transform.position,
            defaultPosition.position,
            ref _velocity,
            smoothSpeed * 2f
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            defaultPosition.rotation,
            rotationSmoothness * 2f
        );
    }

    /// <summary>
    /// ������Ŀ��
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target)
        {
            _lastTargetPosition = target.position;
            _leashAnchorPoint = transform.position; // ��������ê��
            enabled = true;
        }
    }

    /// <summary>
    /// ����ƫ����
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        _currentOffset = offset;
    }

    /// <summary>
    /// �ڱ༭���л��Ƹ�����
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!target) return;

        // ���Ƹ��淶Χ
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_leashAnchorPoint, FollowingRange);

        // ��������ê��
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(_leashAnchorPoint, 0.2f);
        Gizmos.DrawLine(_leashAnchorPoint, target.position);

        // ����Ŀ��λ��
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);

        // ���ƿ���Ŀ��
        if (lookAtTarget)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position + lookAtOffset);
            Gizmos.DrawWireSphere(target.position + lookAtOffset, 0.3f);
        }

        // ������ײ��ⷶΧ
        if (avoidObstacles)
        {
            Gizmos.color = Color.yellow;
            Vector3 direction = (transform.position - (target.position + lookAtOffset)).normalized;
            Gizmos.DrawLine(target.position + lookAtOffset, target.position + lookAtOffset + direction * collisionDistance);
            Gizmos.DrawWireSphere(target.position + lookAtOffset + direction * collisionDistance, collisionRadius);
        }

        // ���Ʊ߽�
        if (useBoundaries)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Vector3 center = (boundaryMin + boundaryMax) / 2f;
            Vector3 size = boundaryMax - boundaryMin;
            Gizmos.DrawWireCube(center, size);
        }
    }
}