using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ��Ϸָ�����̿�����
/// ������Ϸ�еĲ���ָ�����ṩ�򵥵�API����
/// </summary>
public class GuideSystem : MonoBehaviour
{
    [System.Serializable]
    public class GuideStep
    {
        [Tooltip("��ǰĿ��")]
        public string CurrGoal;

        [TextArea(3, 10)]
        [Tooltip("����˵���ı�")]
        public string stepText;

        [Tooltip("�������ʱ�������¼�")]
        public UnityEvent onStepComplete;

        [Tooltip("���迪ʼʱ�������¼�")]
        public UnityEvent onStepStart;

        [Tooltip("Ŀ����󣨿�ѡ��")]
        public GameObject targetObject;

        [Tooltip("����Ŀ�����")]
        public bool highlightTarget = true;

        [Tooltip("�����Զ����ʱ�䣨0��ʾ�ֶ���ɣ�")]
        public float autoCompleteTime = 0f;

        [Tooltip("��ʾ������ʾ")]
        public bool showTip = true;
    }

    [Header("UI����")]
    public GameObject guidePanel;
    public TextMeshProUGUI guideText;
    public TextMeshProUGUI GoalText; 
    public Image highlightFrame;
    public GameObject arrowIndicator;
    public Button skipButton;
    public Button nextButton;
    public Slider progressBar;

    [Header("ָ������")]
    public List<GuideStep> guideSteps = new List<GuideStep>();
    public bool startOnAwake = true;
    public bool saveProgress = true;
    public string saveKey = "GuideProgress";

    [Header("��������")]
    public float fadeDuration = 0.5f;
    public float highlightPulseSpeed = 1f;
    public float arrowMoveDistance = 10f;
    public float arrowMoveSpeed = 2f;

    // ��ǰ״̬
    private int _currentStepIndex = -1;
    private bool _isGuiding = false;
    private CanvasGroup _canvasGroup;
    private Coroutine _autoCompleteCoroutine;
    private Dictionary<string, int> _stepIndexMap = new Dictionary<string, int>();

    // ����ģʽ
    public static GuideSystem Instance { get; private set; }

    private void Awake()
    {
        // ����ʵ��
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

        // ��ʼ��UI
        _canvasGroup = guidePanel.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = guidePanel.AddComponent<CanvasGroup>();
        }

        // ����ָ��UI
        _canvasGroup.alpha = 0;
        guidePanel.SetActive(false);
        highlightFrame.gameObject.SetActive(false);
        arrowIndicator.SetActive(false);

        // ������������ӳ��
        for (int i = 0; i < guideSteps.Count; i++)
        {
            if (!string.IsNullOrEmpty(guideSteps[i].CurrGoal))
            {
                _stepIndexMap[guideSteps[i].CurrGoal] = i;
            }
        }

        // ��ť�¼�
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
    /// ��ʼָ������
    /// </summary>
    public void StartGuide()
    {
        if (_isGuiding) return;

        // ���ؽ���
        if (saveProgress)
        {
            _currentStepIndex = PlayerPrefs.GetInt(saveKey, 0);
        }
        else
        {
            _currentStepIndex = 0;
        }

        // ����Ƿ���������в���
        if (_currentStepIndex >= guideSteps.Count)
        {
            Debug.Log("����ָ�����������");
            return;
        }

        _isGuiding = true;
        StartCoroutine(ShowGuidePanel(() => {
            StartStep(_currentStepIndex);
        }));
    }

    /// <summary>
    /// ��ת��ָ������
    /// </summary>
    /// <param name="stepID">����ID</param>
    public void JumpToStep(string stepID)
    {
        if (_stepIndexMap.TryGetValue(stepID, out int index))
        {
            if (index < 0 || index >= guideSteps.Count) return;

            // ��ɵ�ǰ���裨����У�
            if (_currentStepIndex >= 0 && _currentStepIndex < guideSteps.Count)
            {
                guideSteps[_currentStepIndex].onStepComplete?.Invoke();
            }

            _currentStepIndex = index;
            StartStep(_currentStepIndex);
        }
        else
        {
            Debug.LogWarning($"�Ҳ�������ID: {stepID}");
        }
    }

    /// <summary>
    /// �ֶ���ɵ�ǰ����
    /// </summary>
    public void CompleteCurrentStep()
    {
        if (!_isGuiding || _currentStepIndex < 0) return;

        // ֹͣ�Զ����Э��
        if (_autoCompleteCoroutine != null)
        {
            StopCoroutine(_autoCompleteCoroutine);
            _autoCompleteCoroutine = null;
        }

        // ִ������¼�
        guideSteps[_currentStepIndex].onStepComplete?.Invoke();

        // �ƶ�����һ��
        MoveToNextStep();
    }

    /// <summary>
    /// ��������ָ��
    /// </summary>
    public void SkipGuide()
    {
        if (!_isGuiding) return;

        // ִ�е�ǰ���������¼�������У�
        if (_currentStepIndex >= 0 && _currentStepIndex < guideSteps.Count)
        {
            guideSteps[_currentStepIndex].onStepComplete?.Invoke();
        }

        // ������в������
        _currentStepIndex = guideSteps.Count;

        // �������
        if (saveProgress)
        {
            PlayerPrefs.SetInt(saveKey, _currentStepIndex);
        }

        // ����UI
        StartCoroutine(HideGuidePanel(() => {
            _isGuiding = false;
        }));
    }

    /// <summary>
    /// ����ָ������
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
    /// ����²���
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
    /// ���벽��
    /// </summary>
    public void InsertStep(int index, GuideStep newStep)
    {
        if (index < 0 || index > guideSteps.Count) return;

        guideSteps.Insert(index, newStep);

        // �ؽ�����ӳ��
        RebuildStepIndexMap();
    }

    /// <summary>
    /// �Ƴ�����
    /// </summary>
    public void RemoveStep(string stepID)
    {
        if (_stepIndexMap.TryGetValue(stepID, out int index))
        {
            guideSteps.RemoveAt(index);
            RebuildStepIndexMap();
        }
    }

    // ��ʼָ������
    private void StartStep(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= guideSteps.Count) return;

        GuideStep step = guideSteps[stepIndex];

        // ����UI
        if (step.showTip)
        {
            guideText.text = step.stepText;
            GoalText.text = step.CurrGoal;
        }
        else
        {
            guideText.text = "";
        }

        // ���½�����
        progressBar.value = (float)stepIndex / (guideSteps.Count - 1);

        // ����Ŀ�����
        if (step.targetObject != null && step.highlightTarget)
        {
            HighlightTarget(step.targetObject);
        }
        else
        {
            highlightFrame.gameObject.SetActive(false);
            arrowIndicator.SetActive(false);
        }

        // ִ�в��迪ʼ�¼�
        step.onStepStart?.Invoke();

        // �����Զ���ɣ�����У�
        if (step.autoCompleteTime > 0)
        {
            _autoCompleteCoroutine = StartCoroutine(AutoCompleteStep(step.autoCompleteTime));
        }
    }

    // �ƶ�����һ��
    private void MoveToNextStep()
    {
        _currentStepIndex++;

        // �������
        if (saveProgress)
        {
            PlayerPrefs.SetInt(saveKey, _currentStepIndex);
        }

        // ����Ƿ����
        if (_currentStepIndex >= guideSteps.Count)
        {
            SkipGuide();
            return;
        }

        // ��ʼ��һ��
        StartStep(_currentStepIndex);
    }

    // �Զ���ɲ���
    private IEnumerator AutoCompleteStep(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteCurrentStep();
    }

    // ����Ŀ�����
    private void HighlightTarget(GameObject target)
    {
        highlightFrame.gameObject.SetActive(true);
        arrowIndicator.SetActive(true);

        // ��ȡĿ������Ļ�ϵ�λ��
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);

        // ���ø�����λ��
        highlightFrame.rectTransform.position = screenPos;

        // ���ü�ͷλ�ã���Ŀ���Ϸ���
        arrowIndicator.transform.position = screenPos + Vector3.up * 50f;

        // ��ʼ����
        StartCoroutine(PulseHighlight());
        StartCoroutine(MoveArrow());
    }

    // ��������������
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

    // ��ͷ�ƶ�����
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

    // ��ʾָ�����
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

    // ����ָ�����
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

    // �ؽ���������ӳ��
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
