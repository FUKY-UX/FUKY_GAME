using UnityEngine;

/// <summary>
/// 改进版摄像机控制器 - 实现"拴小狗"式拖拽效果
/// </summary>
[RequireComponent(typeof(Camera))]
public class FUKY_CAM_CTRL : MonoBehaviour
{
    [Header("跟随设置")]
    [Tooltip("要跟随的目标物体")]
    public Transform target;

    [Tooltip("摄像机不移动的跟随范围")]
    public float FollowingRange = 1f;

    [Tooltip("摄像机与目标的偏移量")]
    public Vector3 offset = new Vector3(0f, 2f, -5f);

    [Tooltip("跟随平滑度 (值越小越平滑)")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;

    [Tooltip("跟随延迟 (拖拽效果强度)")]
    [Range(0f, 1f)]
    public float followLag = 0.2f;

    [Header("视角设置")]
    [Tooltip("始终看向目标")]
    public bool lookAtTarget = true;

    [Tooltip("看向目标的偏移点")]
    public Vector3 lookAtOffset = Vector3.zero;

    [Tooltip("视角平滑度")]
    [Range(0.01f, 1f)]
    public float rotationSmoothness = 0.1f;

    [Header("碰撞检测")]
    [Tooltip("启用碰撞避免")]
    public bool avoidObstacles = true;

    [Tooltip("碰撞检测距离")]
    public float collisionDistance = 1f;

    [Tooltip("碰撞检测半径")]
    public float collisionRadius = 0.5f;

    [Tooltip("碰撞层遮罩")]
    public LayerMask collisionMask = -1;

    [Header("高级设置")]
    [Tooltip("摄像机移动时的最大速度")]
    public float maxSpeed = 10f;

    [Tooltip("目标丢失时是否返回默认位置")]
    public bool returnToDefaultOnTargetLost = true;

    [Tooltip("默认位置")]
    public Transform defaultPosition;

    [Tooltip("启用边界限制")]
    public bool useBoundaries = false;

    [Tooltip("游戏边界")]
    public Vector3 boundaryMin = new Vector3(-10f, 0f, -10f);
    public Vector3 boundaryMax = new Vector3(10f, 10f, 10f);

    // 私有变量
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _currentOffset;
    private Vector3 _desiredPosition;
    private Quaternion _desiredRotation;
    private Vector3 _lastTargetPosition;
    private float _distanceToTarget;
    private Camera _camera;
    private bool _isWithinRange = true;
    private Vector3 _leashAnchorPoint; // 绳子锚点位置

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _currentOffset = offset;

        if (target)
        {
            _lastTargetPosition = target.position;
            // 初始化位置
            _desiredPosition = CalculateLeashPosition();
            transform.position = _desiredPosition;

            if (lookAtTarget)
            {
                transform.LookAt(target.position + lookAtOffset);
            }

            // 初始化绳子锚点
            _leashAnchorPoint = transform.position;
        }
    }

    private void Update()
    {
    if (FUKYMouse.Instance.Left_pressed)
    {
        // 获取鼠标移动量
        Vector3 mouseDelta = FUKYMouse.Instance.deltaTranslate * FUKYMouse.Instance.PressureValue;
        
        // 计算目标移动方向：
        // - 左右：相机右方向 (x轴分量)
        // - 上下：世界Y轴
        // - 前后：相机前方向 (z轴分量)
        Vector3 moveOffset = _camera.transform.right * mouseDelta.x   // 左右
                            + Vector3.up * mouseDelta.y               // 上下
                            + _camera.transform.forward * mouseDelta.z; // 前后
        
        target.localPosition += moveOffset;
    }
    
    if (FUKYMouse.Instance.Right_pressed)
    {
        // 获取鼠标移动量
        Vector3 mouseDelta = FUKYMouse.Instance.deltaTranslate * FUKYMouse.Instance.PressureValue;
        
        // 相机移动方向计算（与target逻辑相同）
        Vector3 moveOffset = _camera.transform.right * mouseDelta.x   // 左右
                            + Vector3.up * mouseDelta.y               // 上下
                            + _camera.transform.forward * mouseDelta.z; // 前后
        
        _camera.transform.position += moveOffset;
    }    }

    private void LateUpdate()
    {
        if (!target)
        {
            HandleTargetLost();
            return;
        }

        // 计算到目标的距离
        _distanceToTarget = Vector3.Distance(transform.position, target.position);

        // 检查是否在跟随范围内
        _isWithinRange = _distanceToTarget <= FollowingRange;

        if (_isWithinRange)
        {
            // 在范围内：只旋转，不移动
            _leashAnchorPoint = transform.position; // 更新锚点
        }
        else
        {
            // 超出范围：计算新的期望位置（沿着看向目标的方向）
            _desiredPosition = CalculateLeashPosition();
        }

        // 应用边界限制
        if (useBoundaries)
        {
            _desiredPosition = ApplyBoundaries(_desiredPosition);
        }

        // 应用碰撞避免
        if (avoidObstacles)
        {
            _desiredPosition = AdjustForObstacles(_desiredPosition);
        }

        // 平滑移动摄像机（仅在超出范围时移动）
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

        // 始终更新看向目标
        if (lookAtTarget)
        {
            UpdateLookAtTarget();
        }

        // 更新最后位置
        _lastTargetPosition = target.position;
    }

    /// <summary>
    /// 计算绳子效果位置（沿着看向目标的方向）
    /// </summary>
    private Vector3 CalculateLeashPosition()
    {
        // 1. 计算从锚点到目标的方向
        Vector3 toTarget = target.position - _leashAnchorPoint;

        // 2. 计算绳子长度方向上的点
        Vector3 leashDirection = toTarget.normalized;
        Vector3 leashEndPoint = _leashAnchorPoint + leashDirection * FollowingRange;

        // 3. 添加拖拽效果
        Vector3 lagOffset = Vector3.zero;
        if (followLag > 0)
        {
            Vector3 targetMovement = target.position - _lastTargetPosition;
            lagOffset = targetMovement * followLag;
        }

        // 4. 返回最终位置
        return leashEndPoint - lagOffset;
    }

    /// <summary>
    /// 更新看向目标
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
    /// 调整避免障碍物
    /// </summary>
    private Vector3 AdjustForObstacles(Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - (target.position + lookAtOffset);
        float distance = direction.magnitude;

        // 球形射线检测
        if (Physics.SphereCast(
            target.position + lookAtOffset,
            collisionRadius,
            direction.normalized,
            out RaycastHit hit,
            distance,
            collisionMask))
        {
            // 计算避免障碍物的位置
            float hitDistance = hit.distance - collisionDistance;
            if (hitDistance < 0) hitDistance = 0;

            Vector3 adjustedPosition = target.position + lookAtOffset +
                                       direction.normalized * hitDistance;

            // 从目标位置向摄像机方向轻微偏移
            Vector3 offsetDirection = (adjustedPosition - (target.position + lookAtOffset)).normalized;
            return adjustedPosition - offsetDirection * 0.2f;
        }

        return desiredPosition;
    }

    /// <summary>
    /// 应用边界限制
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
    /// 处理目标丢失
    /// </summary>
    private void HandleTargetLost()
    {
        if (!returnToDefaultOnTargetLost || !defaultPosition)
        {
            enabled = false;
            return;
        }

        // 平滑返回默认位置
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
    /// 设置新目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target)
        {
            _lastTargetPosition = target.position;
            _leashAnchorPoint = transform.position; // 重置绳子锚点
            enabled = true;
        }
    }

    /// <summary>
    /// 设置偏移量
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        _currentOffset = offset;
    }

    /// <summary>
    /// 在编辑器中绘制辅助线
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!target) return;

        // 绘制跟随范围
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_leashAnchorPoint, FollowingRange);

        // 绘制绳子锚点
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(_leashAnchorPoint, 0.2f);
        Gizmos.DrawLine(_leashAnchorPoint, target.position);

        // 绘制目标位置
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);

        // 绘制看向目标
        if (lookAtTarget)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position + lookAtOffset);
            Gizmos.DrawWireSphere(target.position + lookAtOffset, 0.3f);
        }

        // 绘制碰撞检测范围
        if (avoidObstacles)
        {
            Gizmos.color = Color.yellow;
            Vector3 direction = (transform.position - (target.position + lookAtOffset)).normalized;
            Gizmos.DrawLine(target.position + lookAtOffset, target.position + lookAtOffset + direction * collisionDistance);
            Gizmos.DrawWireSphere(target.position + lookAtOffset + direction * collisionDistance, collisionRadius);
        }

        // 绘制边界
        if (useBoundaries)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Vector3 center = (boundaryMin + boundaryMax) / 2f;
            Vector3 size = boundaryMax - boundaryMin;
            Gizmos.DrawWireCube(center, size);
        }
    }
}