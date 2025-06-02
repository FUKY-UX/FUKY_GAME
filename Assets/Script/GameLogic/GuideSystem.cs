using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏指引流程控制器
/// 管理游戏中的步骤指引，提供简单的API调用
/// </summary>
public class GuideSystem : MonoBehaviour
{
    [System.Serializable]
    public class GuideStep
    {
        [Tooltip("当前目标")]
        public string CurrGoal;

        [TextArea(3, 10)]
        [Tooltip("步骤说明文本")]
        public string stepText;

        [Tooltip("步骤完成时触发的事件")]
        public UnityEvent onStepComplete;

        [Tooltip("步骤开始时触发的事件")]
        public UnityEvent onStepStart;

        [Tooltip("目标对象（可选）")]
        public GameObject targetObject;

        [Tooltip("高亮目标对象")]
        public bool highlightTarget = true;

        [Tooltip("步骤自动完成时间（0表示手动完成）")]
        public float autoCompleteTime = 0f;

        [Tooltip("显示步骤提示")]
        public bool showTip = true;
    }

    [Header("UI设置")]
    public GameObject guidePanel;
    public TextMeshProUGUI guideText;
    public TextMeshProUGUI GoalText; 
    public Image highlightFrame;
    public GameObject arrowIndicator;
    public Button skipButton;
    public Button nextButton;
    public Slider progressBar;

    [Header("指引设置")]
    public List<GuideStep> guideSteps = new List<GuideStep>();
    public bool startOnAwake = true;
    public bool saveProgress = true;
    public string saveKey = "GuideProgress";

    [Header("动画设置")]
    public float fadeDuration = 0.5f;
    public float highlightPulseSpeed = 1f;
    public float arrowMoveDistance = 10f;
    public float arrowMoveSpeed = 2f;

    // 当前状态
    private int _currentStepIndex = -1;
    private bool _isGuiding = false;
    private CanvasGroup _canvasGroup;
    private Coroutine _autoCompleteCoroutine;
    private Dictionary<string, int> _stepIndexMap = new Dictionary<string, int>();

    // 单例模式
    public static GuideSystem Instance { get; private set; }

    private void Awake()
    {
        // 单例实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始化UI
        _canvasGroup = guidePanel.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = guidePanel.AddComponent<CanvasGroup>();
        }

        // 隐藏指引UI
        _canvasGroup.alpha = 0;
        guidePanel.SetActive(false);
        highlightFrame.gameObject.SetActive(false);
        arrowIndicator.SetActive(false);

        // 创建步骤索引映射
        for (int i = 0; i < guideSteps.Count; i++)
        {
            if (!string.IsNullOrEmpty(guideSteps[i].CurrGoal))
            {
                _stepIndexMap[guideSteps[i].CurrGoal] = i;
            }
        }

        // 按钮事件
        //skipButton.onClick.AddListener(SkipGuide);
        //nextButton.onClick.AddListener(CompleteCurrentStep);
    }

    private void Start()
    {
        if (startOnAwake)
        {
            StartGuide();
        }
    }

    /// <summary>
    /// 开始指引流程
    /// </summary>
    public void StartGuide()
    {
        if (_isGuiding) return;

        // 加载进度
        if (saveProgress)
        {
            _currentStepIndex = PlayerPrefs.GetInt(saveKey, 0);
        }
        else
        {
            _currentStepIndex = 0;
        }

        // 检查是否已完成所有步骤
        if (_currentStepIndex >= guideSteps.Count)
        {
            Debug.Log("所有指引步骤已完成");
            return;
        }

        _isGuiding = true;
        StartCoroutine(ShowGuidePanel(() => {
            StartStep(_currentStepIndex);
        }));
    }

    /// <summary>
    /// 跳转到指定步骤
    /// </summary>
    /// <param name="stepID">步骤ID</param>
    public void JumpToStep(string stepID)
    {
        if (_stepIndexMap.TryGetValue(stepID, out int index))
        {
            if (index < 0 || index >= guideSteps.Count) return;

            // 完成当前步骤（如果有）
            if (_currentStepIndex >= 0 && _currentStepIndex < guideSteps.Count)
            {
                guideSteps[_currentStepIndex].onStepComplete?.Invoke();
            }

            _currentStepIndex = index;
            StartStep(_currentStepIndex);
        }
        else
        {
            Debug.LogWarning($"找不到步骤ID: {stepID}");
        }
    }

    /// <summary>
    /// 手动完成当前步骤
    /// </summary>
    public void CompleteCurrentStep()
    {
        if (!_isGuiding || _currentStepIndex < 0) return;

        // 停止自动完成协程
        if (_autoCompleteCoroutine != null)
        {
            StopCoroutine(_autoCompleteCoroutine);
            _autoCompleteCoroutine = null;
        }

        // 执行完成事件
        guideSteps[_currentStepIndex].onStepComplete?.Invoke();

        // 移动到下一步
        MoveToNextStep();
    }

    /// <summary>
    /// 跳过整个指引
    /// </summary>
    public void SkipGuide()
    {
        if (!_isGuiding) return;

        // 执行当前步骤的完成事件（如果有）
        if (_currentStepIndex >= 0 && _currentStepIndex < guideSteps.Count)
        {
            guideSteps[_currentStepIndex].onStepComplete?.Invoke();
        }

        // 标记所有步骤完成
        _currentStepIndex = guideSteps.Count;

        // 保存进度
        if (saveProgress)
        {
            PlayerPrefs.SetInt(saveKey, _currentStepIndex);
        }

        // 隐藏UI
        StartCoroutine(HideGuidePanel(() => {
            _isGuiding = false;
        }));
    }

    /// <summary>
    /// 重置指引进度
    /// </summary>
    public void ResetGuide()
    {
        _currentStepIndex = 0;
        if (saveProgress)
        {
            PlayerPrefs.SetInt(saveKey, 0);
        }
    }

    /// <summary>
    /// 添加新步骤
    /// </summary>
    public void AddStep(GuideStep newStep)
    {
        guideSteps.Add(newStep);
        if (!string.IsNullOrEmpty(newStep.CurrGoal))
        {
            _stepIndexMap[newStep.CurrGoal] = guideSteps.Count - 1;
        }
    }

    /// <summary>
    /// 插入步骤
    /// </summary>
    public void InsertStep(int index, GuideStep newStep)
    {
        if (index < 0 || index > guideSteps.Count) return;

        guideSteps.Insert(index, newStep);

        // 重建索引映射
        RebuildStepIndexMap();
    }

    /// <summary>
    /// 移除步骤
    /// </summary>
    public void RemoveStep(string stepID)
    {
        if (_stepIndexMap.TryGetValue(stepID, out int index))
        {
            guideSteps.RemoveAt(index);
            RebuildStepIndexMap();
        }
    }

    // 开始指定步骤
    private void StartStep(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= guideSteps.Count) return;

        GuideStep step = guideSteps[stepIndex];

        // 更新UI
        if (step.showTip)
        {
            guideText.text = step.stepText;
            GoalText.text = step.CurrGoal;
        }
        else
        {
            guideText.text = "";
        }

        // 更新进度条
        progressBar.value = (float)stepIndex / (guideSteps.Count - 1);

        // 高亮目标对象
        if (step.targetObject != null && step.highlightTarget)
        {
            HighlightTarget(step.targetObject);
        }
        else
        {
            highlightFrame.gameObject.SetActive(false);
            arrowIndicator.SetActive(false);
        }

        // 执行步骤开始事件
        step.onStepStart?.Invoke();

        // 启动自动完成（如果有）
        if (step.autoCompleteTime > 0)
        {
            _autoCompleteCoroutine = StartCoroutine(AutoCompleteStep(step.autoCompleteTime));
        }
    }

    // 移动到下一步
    private void MoveToNextStep()
    {
        _currentStepIndex++;

        // 保存进度
        if (saveProgress)
        {
            PlayerPrefs.SetInt(saveKey, _currentStepIndex);
        }

        // 检查是否完成
        if (_currentStepIndex >= guideSteps.Count)
        {
            SkipGuide();
            return;
        }

        // 开始下一步
        StartStep(_currentStepIndex);
    }

    // 自动完成步骤
    private IEnumerator AutoCompleteStep(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteCurrentStep();
    }

    // 高亮目标对象
    private void HighlightTarget(GameObject target)
    {
        highlightFrame.gameObject.SetActive(true);
        arrowIndicator.SetActive(true);

        // 获取目标在屏幕上的位置
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);

        // 设置高亮框位置
        highlightFrame.rectTransform.position = screenPos;

        // 设置箭头位置（在目标上方）
        arrowIndicator.transform.position = screenPos + Vector3.up * 50f;

        // 开始动画
        StartCoroutine(PulseHighlight());
        StartCoroutine(MoveArrow());
    }

    // 高亮框脉动动画
    private IEnumerator PulseHighlight()
    {
        RectTransform rect = highlightFrame.rectTransform;
        Vector3 originalScale = rect.localScale;

        while (highlightFrame.gameObject.activeSelf)
        {
            float pulse = Mathf.PingPong(Time.time * highlightPulseSpeed, 0.2f);
            rect.localScale = originalScale * (1 + pulse);
            yield return null;
        }
    }

    // 箭头移动动画
    private IEnumerator MoveArrow()
    {
        RectTransform rect = arrowIndicator.GetComponent<RectTransform>();
        Vector3 originalPos = rect.anchoredPosition;

        while (arrowIndicator.activeSelf)
        {
            float offset = Mathf.Sin(Time.time * arrowMoveSpeed) * arrowMoveDistance;
            rect.anchoredPosition = originalPos + Vector3.up * offset;
            yield return null;
        }
    }

    // 显示指引面板
    private IEnumerator ShowGuidePanel(Action onComplete)
    {
        guidePanel.SetActive(true);
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 1;
        onComplete?.Invoke();
    }

    // 隐藏指引面板
    private IEnumerator HideGuidePanel(Action onComplete)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 0;
        guidePanel.SetActive(false);
        highlightFrame.gameObject.SetActive(false);
        arrowIndicator.SetActive(false);

        onComplete?.Invoke();
    }

    // 重建步骤索引映射
    private void RebuildStepIndexMap()
    {
        _stepIndexMap.Clear();
        for (int i = 0; i < guideSteps.Count; i++)
        {
            if (!string.IsNullOrEmpty(guideSteps[i].CurrGoal))
            {
                _stepIndexMap[guideSteps[i].CurrGoal] = i;
            }
        }
    }
}
