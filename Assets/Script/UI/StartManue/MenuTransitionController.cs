using System.Collections;
using UnityEngine;

/// <summary>
/// 增强版主菜单过渡控制器
/// 处理点击任意键后主菜单UI元素的动画过渡效果
/// 并控制相机沿着贝塞尔曲线移动到目标位置
/// </summary>
public class MenuTransitionController : MonoBehaviour
{
    [Header("开始UI引用")]
    [Tooltip("标志UI")]
    public RectTransform StartLogo;
    [Tooltip("点击任意键开始文字UI")]
    public GameObject startPrompt;
    [Tooltip("开始下划装饰UI")]
    public RectTransform StartDecoration;

    [Header("主菜单UI引用")]
    public GameObject Menue;
    [Tooltip("主菜单标志UI")]
    public RectTransform MenuLogo;
    [Tooltip("点击任意键开始文字UI")]
    public RectTransform[] Button;
    [Tooltip("主菜单下划装饰UI")]
    public RectTransform MenuDecoration;


    [Header("相机控制")]
    [Tooltip("贝塞尔曲线控制点")]
    public Transform[] bezierControlPoints;

    [Tooltip("相机移动持续时间(秒)")]
    [Range(1.0f, 10.0f)]
    public float cameraMoveDuration = 3.0f;

    [Tooltip("相机移动动画曲线")]
    public AnimationCurve cameraMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("动画设置")]
    [Tooltip("UI动画持续时间(秒)")]
    [Range(0.5f, 3.0f)]
    public float animationDuration = 1.0f;
    [Tooltip("相机旋转速度")]
    [Range(-3.0f, 3.0f)]
    public float RotationSpeed = 1.0f;
    [Header("按钮呼吸动画设置")]
    public float BTN_breathingSpeed = 0.8f;
    public float BTN_positionAmplitude = 15f;
    public float BTN_rotationAmplitude = 3f;
    [Header("标题大图案呼吸动画设置")]
    public float LOGO_breathingSpeed = 0.8f;
    public float LOGO_positionAmplitude = 15f;
    public float LOGO_rotationAmplitude = 3f;
    [Tooltip("UI动画曲线")]
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("标志向上移动距离(相对于屏幕高度)")]
    [Range(1.0f, 3.0f)]
    public float logoMoveDistance = 1.5f;

    [Tooltip("装饰向下移动距离(相对于屏幕高度)")]
    [Range(1.0f, 3.0f)]
    public float decorationMoveDistance = 1.5f;

    // 私有变量
    private bool _transitionTriggered = false;
    private Vector2 _logoInitialPosition;
    private Vector2 _decorationInitialPosition;
    private float _screenHeight;
    private Camera _mainCamera;
    private Vector3 _cameraStartPosition;
    private Quaternion _cameraStartRotation;

    /// <summary>
    /// 初始化组件
    /// </summary>
    private void Start()
    {
        // 检查必要的引用
        if (StartLogo == null || startPrompt == null || StartDecoration == null)
        {
            Debug.LogError("MenuTransitionController: 缺少必要的UI引用，请在Inspector中设置。");
            enabled = false;
            return;
        }

        // 获取主相机
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("MenuTransitionController: 未找到主相机。");
            enabled = false;
            return;
        }

        // 保存相机初始状态
        _cameraStartPosition = _mainCamera.transform.position;
        _cameraStartRotation = _mainCamera.transform.rotation;

        // 保存初始位置
        _logoInitialPosition = StartLogo.anchoredPosition;
        _decorationInitialPosition = StartDecoration.anchoredPosition;

        // 获取屏幕高度
        _screenHeight = Screen.height;

        // 检查贝塞尔曲线控制点
        if (bezierControlPoints == null || bezierControlPoints.Length < 2)
        {
            Debug.LogWarning("贝塞尔曲线控制点不足，相机移动功能将禁用。");
        }
    }

    /// <summary>
    /// 每帧检测按键输入
    /// </summary>
    private void Update()
    {
        // 如果已经触发过渡，则不再检测按键
        if (_transitionTriggered)
            return;

        // 检测任意按键输入
        if (Input.anyKeyDown)
        {
            // 触发过渡动画
            StartCoroutine(FullTransitionRoutine());
        }

        _mainCamera.transform.rotation *= Quaternion.Euler(0, RotationSpeed * Time.deltaTime, 0);
    }

    /// <summary>
    /// 开始完整过渡流程
    /// </summary>
    private IEnumerator FullTransitionRoutine()
    {
        // 标记已触发过渡
        _transitionTriggered = true;

        // 立即禁用开始提示文字
        if (startPrompt != null)
        {
            startPrompt.SetActive(false);
        }

        // 同时启动UI元素动画
        StartCoroutine(AnimateLogoUp());
        StartCoroutine(AnimateDecorationDown());


        // 启动相机移动动画
        Coroutine decoAnim =StartCoroutine(MoveCameraAlongBezierCurve());
        
        yield return decoAnim;

        UIBreathingAnimator _Animator;
        //图案LOGO
        _Animator = MenuLogo.gameObject.AddComponent<UIBreathingAnimator>();
        _Animator.breathingSpeed = LOGO_breathingSpeed;
        _Animator.positionAmplitude = LOGO_positionAmplitude;
        _Animator.rotationAmplitude = LOGO_rotationAmplitude;
        StartCoroutine(_Animator.StartBreathingAnimation(MenuLogo));
        Vector2 MenuLogo_startPos = new Vector2(MenuLogo.anchoredPosition.x, MenuLogo.anchoredPosition.y + 350f);
        StartCoroutine(AnimateUIElement(_Animator, MenuLogo_startPos, MenuLogo.anchoredPosition, 2f, animationCurve, false));


        Vector2 MenuDe_startPos = new Vector2(MenuDecoration.anchoredPosition.x, MenuDecoration.anchoredPosition.y - 220f);
        StartCoroutine(AnimateUIElement(MenuDecoration, MenuDe_startPos, MenuDecoration.anchoredPosition, 2f, animationCurve, false));
        foreach (var btn in Button) 
        { 
            _Animator = btn.gameObject.AddComponent<UIBreathingAnimator>();
            _Animator.breathingSpeed = BTN_breathingSpeed;
            _Animator.positionAmplitude = BTN_positionAmplitude;
            _Animator.rotationAmplitude = BTN_rotationAmplitude;
            StartCoroutine(_Animator.StartBreathingAnimation(btn));
            Vector2 startPos = new Vector2(btn.anchoredPosition.x - 460, btn.anchoredPosition.y);
            Coroutine BtnAnim = StartCoroutine(AnimateUIElement(_Animator, startPos, btn.anchoredPosition, 0.5f, animationCurve, false));

            yield return BtnAnim;
        }

    }

    /// <summary>
    /// 通用的UI元素动画协程
    /// </summary>
    /// <param name="element">要动画的UI元素</param>
    /// <param name="startPos">起始位置</param>
    /// <param name="endPos">目标位置</param>
    /// <param name="duration">动画持续时间</param>
    /// <param name="curve">动画曲线</param>
    /// <param name="disableAfterAnimation">动画完成后是否禁用元素</param>
    /// <returns>IEnumerator协程</returns>
    private IEnumerator AnimateUIElement(RectTransform element, Vector2 startPos, Vector2 endPos,
        float duration, AnimationCurve curve, bool disableAfterAnimation = true)
    {
        if (element == null)
        {
            Debug.LogWarning("尝试动画一个空UI元素");
            yield break;
        }

        // 确保元素激活
        element.gameObject.SetActive(true);
        element.anchoredPosition = startPos;

        // 动画计时器
        float timer = 0;

        // 动画循环
        while (timer < duration)
        {
            // 更新计时器
            timer += Time.deltaTime;

            // 计算动画进度（0-1）
            float progress = Mathf.Clamp01(timer / duration);

            // 应用动画曲线
            float curvedProgress = curve.Evaluate(progress);

            // 计算当前位置
            Vector2 currentPosition = Vector2.Lerp(startPos, endPos, curvedProgress);

            // 应用位置
            element.anchoredPosition = currentPosition;

            // 等待下一帧
            yield return null;
        }

        // 确保最终位置精确
        element.anchoredPosition = endPos;
        // 动画完成后禁用对象（可选）
        if (disableAfterAnimation)
        {
            element.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 通用的UI元素,带呼吸动画协程
    /// </summary>
    /// <param name="Breathingelement">要动画的UI元素</param>
    /// <param name="startPos">起始位置</param>
    /// <param name="endPos">目标位置</param>
    /// <param name="duration">动画持续时间</param>
    /// <param name="curve">动画曲线</param>
    /// <param name="disableAfterAnimation">动画完成后是否禁用元素</param>
    /// <returns>IEnumerator协程</returns>
    private IEnumerator AnimateUIElement(UIBreathingAnimator Breathingelement, Vector2 startPos, Vector2 endPos,
        float duration, AnimationCurve curve, bool disableAfterAnimation = true)
    {
        if (Breathingelement == null)
        {
            Debug.LogWarning("尝试动画一个空UI元素");
            yield break;
        }

        // 确保元素激活
        Breathingelement._originalPosition = startPos;
        Breathingelement._targetElement.anchoredPosition = startPos;
        Breathingelement.gameObject.SetActive(true);

        // 动画计时器
        float timer = 0;

        // 动画循环
        while (timer < duration)
        {
            // 更新计时器
            timer += Time.deltaTime;

            // 计算动画进度（0-1）
            float progress = Mathf.Clamp01(timer / duration);

            // 应用动画曲线
            float curvedProgress = curve.Evaluate(progress);

            // 计算当前位置
            Vector2 currentPosition = Vector2.Lerp(startPos, endPos, curvedProgress);

            // 应用位置
            Breathingelement._originalPosition = currentPosition;

            // 等待下一帧
            yield return null;
        }

        // 确保最终位置精确
        Breathingelement._originalPosition = endPos;
        // 动画完成后禁用对象（可选）
        if (disableAfterAnimation)
        {
            Breathingelement.gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// 标志向上飞出动画
    /// </summary>
    private IEnumerator AnimateLogoUp()
    {
        // 计算目标位置（向上移动屏幕高度的logoMoveDistance倍）
        Vector2 targetPosition = _logoInitialPosition + new Vector2(0, _screenHeight * logoMoveDistance);

        // 动画计时器
        float timer = 0;

        // 动画循环
        while (timer < animationDuration)
        {
            // 更新计时器
            timer += Time.deltaTime;

            // 计算动画进度（0-1）
            float progress = timer / animationDuration;

            // 应用动画曲线
            float curvedProgress = animationCurve.Evaluate(progress);

            // 计算当前位置
            Vector2 currentPosition = Vector2.Lerp(_logoInitialPosition, targetPosition, curvedProgress);

            // 应用位置
            StartLogo.anchoredPosition = currentPosition;

            // 等待下一帧
            yield return null;
        }

        // 确保最终位置精确
        StartLogo.anchoredPosition = targetPosition;

        // 动画完成后禁用对象
        StartLogo.gameObject.SetActive(false);
    }

    /// <summary>
    /// 装饰向下飞出动画
    /// </summary>
    private IEnumerator AnimateDecorationDown()
    {
        // 计算目标位置（向下移动屏幕高度的decorationMoveDistance倍）
        Vector2 targetPosition = _decorationInitialPosition - new Vector2(0, _screenHeight * decorationMoveDistance);

        // 动画计时器
        float timer = 0;

        // 动画循环
        while (timer < animationDuration)
        {
            // 更新计时器
            timer += Time.deltaTime;

            // 计算动画进度（0-1）
            float progress = timer / animationDuration;

            // 应用动画曲线
            float curvedProgress = animationCurve.Evaluate(progress);

            // 计算当前位置
            Vector2 currentPosition = Vector2.Lerp(_decorationInitialPosition, targetPosition, curvedProgress);

            // 应用位置
            StartDecoration.anchoredPosition = currentPosition;

            // 等待下一帧
            yield return null;
        }

        // 确保最终位置精确
        StartDecoration.anchoredPosition = targetPosition;

        // 动画完成后禁用对象
        StartDecoration.gameObject.SetActive(false);
    }

    /// <summary>
    /// 沿着贝塞尔曲线移动相机
    /// </summary>
    private IEnumerator MoveCameraAlongBezierCurve()
    {
        // 检查贝塞尔曲线控制点是否有效
        if (bezierControlPoints == null || bezierControlPoints.Length < 2)
        {
            Debug.LogWarning("相机贝塞尔曲线控制点不足，跳过相机移动。");
            yield break;
        }

        // 记录相机初始状态
        Vector3 startPosition = _mainCamera.transform.position;
        Quaternion startRotation = _mainCamera.transform.rotation;

        // 获取目标位置和旋转
        Vector3 targetPosition = bezierControlPoints[bezierControlPoints.Length - 1].position;
        Quaternion targetRotation = bezierControlPoints[bezierControlPoints.Length - 1].rotation;

        // 相机移动计时器
        float timer = 0;

        while (timer < cameraMoveDuration)
        {
            // 更新计时器
            timer += Time.deltaTime;

            // 计算动画进度（0-1）
            float progress = Mathf.Clamp01(timer / cameraMoveDuration);

            // 应用相机移动曲线
            float curvedProgress = cameraMoveCurve.Evaluate(progress);

            // 计算贝塞尔曲线上的位置
            Vector3 bezierPosition = CalculateBezierPoint(curvedProgress, bezierControlPoints);

            // 计算旋转插值
            Quaternion interpolatedRotation = Quaternion.Slerp(startRotation, targetRotation, curvedProgress);

            // 应用位置和旋转
            _mainCamera.transform.position = bezierPosition;
            _mainCamera.transform.rotation = interpolatedRotation;

            // 等待下一帧
            yield return null;
        }

        // 确保最终位置和旋转精确
        _mainCamera.transform.position = targetPosition;
        _mainCamera.transform.rotation = targetRotation;
    }

    /// <summary>
    /// 计算贝塞尔曲线上的点
    /// </summary>
    /// <param name="t">曲线参数 (0-1)</param>
    /// <param name="points">控制点数组</param>
    /// <returns>贝塞尔曲线上的点</returns>
    private Vector3 CalculateBezierPoint(float t, Transform[] points)
    {
        // 递归计算贝塞尔曲线点
        return CalculateBezierPointRecursive(t, points, 0, points.Length - 1);
    }

    /// <summary>
    /// 递归计算贝塞尔曲线点
    /// </summary>
    private Vector3 CalculateBezierPointRecursive(float t, Transform[] points, int startIndex, int endIndex)
    {
        // 基础情况：只有一个点时直接返回
        if (startIndex == endIndex)
            return points[startIndex].position;

        // 递归计算中间点
        Vector3 p1 = CalculateBezierPointRecursive(t, points, startIndex, endIndex - 1);
        Vector3 p2 = CalculateBezierPointRecursive(t, points, startIndex + 1, endIndex);

        // 线性插值
        return Vector3.Lerp(p1, p2, t);
    }

    /// <summary>
    /// 重置所有UI元素到初始状态
    /// </summary>
    public void ResetMenu()
    {
        // 重置标记
        _transitionTriggered = false;

        // 重置位置
        if (StartLogo != null)
        {
            StartLogo.anchoredPosition = _logoInitialPosition;
            StartLogo.gameObject.SetActive(true);
        }

        if (StartDecoration != null)
        {
            StartDecoration.anchoredPosition = _decorationInitialPosition;
            StartDecoration.gameObject.SetActive(true);
        }

        if (startPrompt != null)
        {
            startPrompt.SetActive(true);
        }

        // 重置相机位置
        if (_mainCamera != null)
        {
            _mainCamera.transform.position = _cameraStartPosition;
            _mainCamera.transform.rotation = _cameraStartRotation;
        }
    }

    /// <summary>
    /// 在场景中绘制贝塞尔曲线预览
    /// </summary>
    private void OnDrawGizmos()
    {
        if (bezierControlPoints == null || bezierControlPoints.Length < 2)
            return;

        // 设置曲线颜色
        Gizmos.color = Color.green;

        // 绘制控制点连线
        for (int i = 0; i < bezierControlPoints.Length - 1; i++)
        {
            if (bezierControlPoints[i] != null && bezierControlPoints[i + 1] != null)
            {
                Gizmos.DrawLine(bezierControlPoints[i].position, bezierControlPoints[i + 1].position);
            }
        }

        // 绘制贝塞尔曲线预览
        Gizmos.color = Color.cyan;
        int segments = 20;
        Vector3 prevPoint = bezierControlPoints[0].position;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 point = CalculateBezierPoint(t, bezierControlPoints);
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}