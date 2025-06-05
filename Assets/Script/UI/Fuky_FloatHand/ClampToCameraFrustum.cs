using UnityEngine;

public class ClampToCameraFrustum : MonoBehaviour
{
    public Camera targetCamera;
    public float distanceFromCamera = 5f;
    public float padding = 0.1f; // �������������Ե
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

        // ��鵱ǰλ���Ƿ�������׶���ڣ����㹻��padding��������������
        if (IsPositionInFrustum(FUKY_Hand.position))
        {
            return; // �Ѿ�����׶���ڣ�����Ҫ����
        }

        // ʹ������λ����Ϊ������ƫ������
        Vector3 newPosition = FUKY_Hand.position;
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
        newPosition = ClampPositionToPlanes(newPosition, frustumPlanes);

        FUKY_Hand.position = newPosition;
    }

    private bool IsPositionInFrustum(Vector3 position)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(targetCamera);

        // �������ƽ�棨����Զƽ�棩
        for (int i = 0; i < 4; i++) // 0:��, 1:��, 2:��, 3:��
        {
            float distance = frustumPlanes[i].GetDistanceToPoint(position);
            if (distance < padding)
            {
                return false;
            }
        }

        // ������ǰ�����Լ��
        Vector3 cameraToHand = position - targetCamera.transform.position;
        float forwardDistance = Vector3.Dot(cameraToHand, targetCamera.transform.forward);
        return forwardDistance >= padding && forwardDistance <= distanceFromCamera;
    }
    private Vector3 ClampPositionToPlanes(Vector3 position, Plane[] planes)
    {
        Vector3 camRight = targetCamera.transform.right;
        Vector3 camUp = targetCamera.transform.up;
        Vector3 camForward = targetCamera.transform.forward;

        // ��������������ƽ�棨��������Զƽ�棩
        for (int i = 0; i < 4; i++) // 0:��, 1:��, 2:��, 3:��
        {
            float distance = planes[i].GetDistanceToPoint(position);
            if (distance < padding)
            {
                Vector3 adjustmentDirection = Vector3.zero;
                switch (i)
                {
                    case 0: // ��ƽ�棺���ҵ���
                        adjustmentDirection = camRight;
                        break;
                    case 1: // ��ƽ�棺�������
                        adjustmentDirection = -camRight;
                        break;
                    case 2: // ��ƽ�棺���ϵ���
                        adjustmentDirection = camUp;
                        break;
                    case 3: // ��ƽ�棺���µ���
                        adjustmentDirection = -camUp;
                        break;
                }

                // Ӧ�ý�С���ȵĵ���
                position += adjustmentDirection * (padding - distance);
            }
        }

        // ��������ǰ�����Լ��
        Vector3 cameraToHand = position - targetCamera.transform.position;
        float forwardDistance = Vector3.Dot(cameraToHand, camForward);

        // ȷ����������Ľ�Զƽ��֮��
        forwardDistance = Mathf.Clamp(forwardDistance, padding, distanceFromCamera);

        // ����Ŀ��λ�ã�ʹ��ͶӰ�������ֵ�����׶����
        Vector3 targetPosition = targetCamera.transform.position + camForward * forwardDistance;

        // ����ԭ���ˮƽ�ʹ�ֱλ�ã����������
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
            // ����Զƽ�棨����5��
            if (i == 5) continue;

            Vector3 planePoint = frustumPlanes[i].ClosestPointOnPlane(targetCamera.transform.position);
            Gizmos.DrawLine(planePoint, planePoint + frustumPlanes[i].normal * 2f);
        }

        // ����������ָʾ��
        Gizmos.color = Color.yellow;
        Vector3 maxDistancePos = targetCamera.transform.position + targetCamera.transform.forward * distanceFromCamera;
        Gizmos.DrawSphere(maxDistancePos, 0.1f);
        Gizmos.DrawLine(targetCamera.transform.position, maxDistancePos);
    }
}