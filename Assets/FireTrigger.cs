using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FireTrigger : MonoBehaviour
{
    [Header("火源设置")]
    [Tooltip("检测火源的层级")]
    public LayerMask fireLayer;          // 在Inspector中选择火源所在的层级
    [Tooltip("检测范围半径")]
    public float detectionRadius = 2f;  // 可调整的检测半径

    [Header("目标物体")]
    [Tooltip("需要控制的GameObject")]
    public GameObject targetObject;     // 需要启用/禁用的物体

    [Header("延迟设置")]
    [Tooltip("离开火源后延迟关闭的时间（秒）")]
    public float delayOffTime = 1f;     // 可调整的延迟关闭时间

    [Header("调试选项")]
    public bool showGizmos = true;      // 是否显示检测范围
    public Color gizmoColor = Color.red; // 检测范围显示颜色

    private bool isNearFire = false;    // 是否在火源附近
    private float delayTimer = 0f;      // 延迟计时器

    private void Update()
    {
        // 检查周围是否有火源
        bool fireNearby = Physics.CheckSphere(transform.position, detectionRadius, fireLayer);

        // 当进入火源范围
        if (fireNearby && !isNearFire)
        {
            isNearFire = true;
            delayTimer = 0f; // 重置计时器
            SetTargetActive(true);
        }
        // 当离开火源范围
        else if (!fireNearby && isNearFire)
        {
            delayTimer += Time.deltaTime;

            // 超过延迟时间后关闭
            if (delayTimer >= delayOffTime)
            {
                isNearFire = false;
                SetTargetActive(false);
            }
        }
        // 当仍在火源中时重置计时器
        else if (fireNearby)
        {
            delayTimer = 0f;
        }
    }

    // 设置目标物体状态
    private void SetTargetActive(bool active)
    {
        if (targetObject != null)
        {
            targetObject.SetActive(active);
        }
    }

    // 在Scene视图中显示检测范围
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
