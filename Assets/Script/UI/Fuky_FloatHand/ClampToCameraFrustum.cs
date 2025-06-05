using UnityEngine;

public class ClampToCameraFrustum : MonoBehaviour
{
    public Camera targetCamera;
    public float distanceFromCamera = 5f;
    public float padding = 0.1f; // 避免物体紧贴边缘
    public bool clampEveryFrame = true;
    public Transform FUKY_Hand;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (clampEveryFrame)
        {
            ClampPositionToFrustum();
        }
    }

    public void ClampPositionToFrustum()
    {
        if (targetCamera == null || FUKY_Hand == null) return;

        // 检查当前位置是否已在视锥体内（有足够的padding）且在最大距离内
        if (IsPositionInFrustum(FUKY_Hand.position))
        {
            return; // 已经在视锥体内，不需要更新
        }

        // 使用世界位置作为起点进行偏移修正
        Vector3 newPosition = FUKY_Hand.position;
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
        newPosition = ClampPositionToPlanes(newPosition, frustumPlanes);

        FUKY_Hand.position = newPosition;
    }

    private bool IsPositionInFrustum(Vector3 position)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(targetCamera);

        // 检查所有平面（除了远平面）
        for (int i = 0; i < 4; i++) // 0:左, 1:右, 2:下, 3:上
        {
            float distance = frustumPlanes[i].GetDistanceToPoint(position);
            if (distance < padding)
            {
                return false;
            }
        }

        // 检查相机前向距离约束
        Vector3 cameraToHand = position - targetCamera.transform.position;
        float forwardDistance = Vector3.Dot(cameraToHand, targetCamera.transform.forward);
        return forwardDistance >= padding && forwardDistance <= distanceFromCamera;
    }
    private Vector3 ClampPositionToPlanes(Vector3 position, Plane[] planes)
    {
        Vector3 camRight = targetCamera.transform.right;
        Vector3 camUp = targetCamera.transform.up;
        Vector3 camForward = targetCamera.transform.forward;

        // 仅处理左右上下平面（不包括近远平面）
        for (int i = 0; i < 4; i++) // 0:左, 1:右, 2:下, 3:上
        {
            float distance = planes[i].GetDistanceToPoint(position);
            if (distance < padding)
            {
                Vector3 adjustmentDirection = Vector3.zero;
                switch (i)
                {
                    case 0: // 左平面：向右调整
                        adjustmentDirection = camRight;
                        break;
                    case 1: // 右平面：向左调整
                        adjustmentDirection = -camRight;
                        break;
                    case 2: // 下平面：向上调整
                        adjustmentDirection = camUp;
                        break;
                    case 3: // 上平面：向下调整
                        adjustmentDirection = -camUp;
                        break;
                }

                // 应用较小幅度的调整
                position += adjustmentDirection * (padding - distance);
            }
        }

        // 单独处理前向距离约束
        Vector3 cameraToHand = position - targetCamera.transform.position;
        float forwardDistance = Vector3.Dot(cameraToHand, camForward);

        // 确保点在相机的近远平面之间
        forwardDistance = Mathf.Clamp(forwardDistance, padding, distanceFromCamera);

        // 计算目标位置：使用投影方法保持点在视锥体内
        Vector3 targetPosition = targetCamera.transform.position + camForward * forwardDistance;

        // 保持原点的水平和垂直位置，仅调整深度
        Vector3 horizontalOffset = Vector3.ProjectOnPlane(position - targetPosition, camForward);
        position = targetPosition + horizontalOffset;

        return position;
    }

    private void OnDrawGizmosSelected()
    {
        if (targetCamera == null) return;

        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(targetCamera);

        Gizmos.color = Color.cyan;
        for (int i = 0; i < frustumPlanes.Length; i++)
        {
            // 跳过远平面（索引5）
            if (i == 5) continue;

            Vector3 planePoint = frustumPlanes[i].ClosestPointOnPlane(targetCamera.transform.position);
            Gizmos.DrawLine(planePoint, planePoint + frustumPlanes[i].normal * 2f);
        }

        // 绘制最大距离指示器
        Gizmos.color = Color.yellow;
        Vector3 maxDistancePos = targetCamera.transform.position + targetCamera.transform.forward * distanceFromCamera;
        Gizmos.DrawSphere(maxDistancePos, 0.1f);
        Gizmos.DrawLine(targetCamera.transform.position, maxDistancePos);
    }
}