using System.Collections.Generic;
using UnityEngine;

public class FireEffect : MonoBehaviour
{
    [Header("火焰缩放设置")]
    [Tooltip("与火焰交互的锅的Transform组件")]
    public Transform potTransform;  // 锅的Transform

    [Tooltip("完全压下时的最小垂直缩放比例")]
    [Range(0.1f, 1f)]
    public float minVerticalScale = 0.3f;  // 最小垂直缩放

    [Tooltip("无压力时的最大垂直缩放比例")]
    [Range(1f, 2f)]
    public float maxVerticalScale = 1f;  // 最大垂直缩放

    [Tooltip("锅与火焰开始产生缩放效果的高度阈值")]
    public float scalingStartHeight = 0.1f;  // 开始缩放的高度

    [Tooltip("锅需要下压多少距离才能达到最小缩放")]
    public float fullPressHeight = 0.5f;  // 完全按压的高度

    [Tooltip("缩放变化的平滑过渡系数")]
    [Range(0.01f, 1f)]
    public float smoothingFactor = 0.1f;  // 平滑过渡系数

    [Header("视觉效果设置")]
    [Tooltip("火焰的基础缩放比例(宽,高,深)")]
    public Vector3 baseScale = Vector3.one;  // 基础缩放

    [Tooltip("垂直缩放时是否保持宽度/深度比例")]
    public bool maintainProportions = true;  // 是否保持宽深比例

    private Vector3 initialScale;  // 初始缩放
    private float targetHeightScale = 1f;  // 目标高度缩放
    private float currentHeightScale = 1f;  // 当前高度缩放

    private void Start()
    {
        initialScale = transform.localScale;
        if (potTransform == null)
        {
            Debug.LogWarning("未给FireEffect指定锅的Transform!", this);
        }
    }

    private void Update()
    {
        UpdateVerticalScale();
    }

    /// <summary>
    /// 根据锅的位置更新火焰的垂直缩放
    /// </summary>
    private void UpdateVerticalScale()
    {
        if (potTransform == null) return;

        // 计算锅底到火焰顶部的垂直距离
        float potToFireDistance = potTransform.position.y - transform.position.y;

        // 计算当前压力比例(0-1范围)
        float pressRatio = Mathf.InverseLerp(scalingStartHeight, fullPressHeight, potToFireDistance);
        pressRatio = Mathf.Clamp01(pressRatio);

        // 根据压力比例确定目标缩放值
        targetHeightScale = Mathf.Lerp(minVerticalScale, maxVerticalScale, pressRatio);

        // 使用平滑过渡到目标缩放值
        currentHeightScale = Mathf.Lerp(currentHeightScale, targetHeightScale, smoothingFactor * Time.deltaTime * 60f);

        // 应用计算出的缩放值
        ApplyVerticalScale(currentHeightScale);
    }

    /// <summary>
    /// 应用垂直缩放效果，可选是否保持宽深比例
    /// </summary>
    /// <param name="heightScale">高度(Y轴)的缩放系数</param>
    private void ApplyVerticalScale(float heightScale)
    {
        Vector3 newScale = initialScale;

        if (maintainProportions)
        {
            // 保持宽深比例缩放(按高度缩放的一半比例调整宽度和深度)
            float proportionalScale = Mathf.Lerp(1f, heightScale, 0.5f);
            newScale.x = initialScale.x * proportionalScale;
            newScale.z = initialScale.z * proportionalScale;
        }

        newScale.y = initialScale.y * heightScale;
        transform.localScale = Vector3.Scale(baseScale, newScale);
    }

    /// <summary>
    /// 手动设置火焰的垂直缩放比例
    /// </summary>
    /// <param name="scale">缩放系数(0-1, 1为原始大小)</param>
    public void SetVerticalScale(float scale)
    {
        scale = Mathf.Clamp(scale, minVerticalScale, maxVerticalScale);
        targetHeightScale = scale;
        currentHeightScale = scale;
        ApplyVerticalScale(scale);
    }

    /// <summary>
    /// 重置火焰到最大缩放状态
    /// </summary>
    public void ResetScale()
    {
        targetHeightScale = maxVerticalScale;
        currentHeightScale = maxVerticalScale;
        ApplyVerticalScale(maxVerticalScale);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (potTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, potTransform.position);

            // 在场景视图中绘制缩放范围指示器
            Vector3 fireTop = transform.position + Vector3.up * transform.lossyScale.y * 0.5f;
            Vector3 scalingStartPos = fireTop + Vector3.up * scalingStartHeight;
            Vector3 fullPressPos = fireTop + Vector3.up * fullPressHeight;

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(scalingStartPos, 0.05f);  // 开始缩放位置(绿色)
            Gizmos.DrawLine(fireTop, scalingStartPos);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(fullPressPos, 0.05f);  // 完全按压位置(红色)
            Gizmos.DrawLine(scalingStartPos, fullPressPos);
        }
    }
#endif
}