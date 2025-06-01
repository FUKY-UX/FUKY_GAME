using UnityEngine;
using System.Collections;


/// <summary>
/// UI呼吸动画控制器
/// </summary>
public class UIBreathingAnimator : MonoBehaviour
{
    [Header("呼吸动画设置")]
    [Tooltip("呼吸动画速度")]
    [Range(0.1f, 5f)]
    public float breathingSpeed = 1f;

    [Tooltip("位置波动幅度")]
    [Range(1f, 50f)]
    public float positionAmplitude = 10f;

    [Tooltip("旋转波动幅度(度)")]
    [Range(1f, 30f)]
    public float rotationAmplitude = 5f;

    [Tooltip("动画曲线")]
    public AnimationCurve breathingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // 目标UI元素
    public RectTransform _targetElement;

    // 原始位置和旋转
    public Vector2 _originalPosition;
    public Quaternion _originalRotation;

    // 当前目标位置和旋转
    private Vector2 _targetPosition;
    private Quaternion _targetRotation;

    // 动画计时器
    private float _animationTimer;

    // 当前动画持续时间
    private float _currentDuration;

    // 是否正在播放动画
    private bool _isAnimating = false;

    /// <summary>
    /// 开始呼吸动画
    /// </summary>
    /// <param name="element">目标UI元素</param>
    public IEnumerator StartBreathingAnimation(RectTransform element)
    {

        if (element == null)
        {
            Debug.LogWarning("无法开始呼吸动画：UI元素为空");
            yield break;
        }
        _targetElement = element;

        // 保存原始状态
        _originalPosition = element.anchoredPosition;
        _originalRotation = element.rotation;

        // 设置初始目标
        SetNewRandomTarget();

        // 重置计时器
        _animationTimer = 0f;

        // 开始动画协程
        if (!_isAnimating)
        {
            _isAnimating = true;
            while (!_targetElement.gameObject.activeInHierarchy)
            {
                yield return null;
            }
            StartCoroutine(BreathingAnimationRoutine());
        }
    }

    /// <summary>
    /// 停止呼吸动画
    /// </summary>
    public void StopBreathingAnimation()
    {
        _isAnimating = false;
        StopCoroutine(BreathingAnimationRoutine());

        // 重置到原始位置
        if (_targetElement != null)
        {
            _targetElement.anchoredPosition = _originalPosition;
            _targetElement.rotation = _originalRotation;
        }
    }

    /// <summary>
    /// 呼吸动画协程
    /// </summary>
    private IEnumerator BreathingAnimationRoutine()
    {
        while (_isAnimating && _targetElement != null)
        {
            // 更新计时器
            _animationTimer += Time.deltaTime;

            // 计算动画进度 (0-1)
            float progress = Mathf.Clamp01(_animationTimer / _currentDuration);

            // 应用动画曲线
            float curvedProgress = breathingCurve.Evaluate(progress);

            // 计算当前位置和旋转
            Vector2 currentPosition = Vector2.Lerp(_originalPosition, _targetPosition, curvedProgress);
            Quaternion currentRotation = Quaternion.Lerp(_originalRotation, _targetRotation, curvedProgress);

            // 应用变换
            _targetElement.anchoredPosition = currentPosition;
            _targetElement.rotation = currentRotation;

            // 检查是否完成当前段动画
            if (progress >= 1f)
            {
                // 设置新目标
                SetNewRandomTarget();
            }

            yield return null;
        }
    }

    /// <summary>
    /// 设置新的随机目标位置和旋转
    /// </summary>
    private void SetNewRandomTarget()
    {
        // 重置计时器
        _animationTimer = 0f;

        // 随机生成动画持续时间 (0.5-2秒)
        _currentDuration = Random.Range(0.5f, 2f) / breathingSpeed;

        // 随机生成位置偏移
        Vector2 positionOffset = new Vector2(
            Random.Range(-positionAmplitude, positionAmplitude),
            Random.Range(-positionAmplitude, positionAmplitude)
        );

        // 随机生成旋转偏移 (度)
        float rotationOffset = Random.Range(-rotationAmplitude, rotationAmplitude);

        // 计算目标位置和旋转
        _targetPosition = _originalPosition + positionOffset;
        _targetRotation = Quaternion.Euler(0, 0, rotationOffset) * _originalRotation;
    }

    /// <summary>
    /// 在组件启用时开始动画
    /// </summary>
    private void OnEnable()
    {
        if (_targetElement != null)
        {
            StartBreathingAnimation(_targetElement);
        }
    }

    /// <summary>
    /// 在组件禁用时停止动画
    /// </summary>
    private void OnDisable()
    {
        StopBreathingAnimation();
    }
}