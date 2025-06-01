using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 对话框控制器
/// 管理游戏中的对话系统
/// </summary>
public class DialogController : SingletonMono<DialogController>
{
    [System.Serializable]
    public class DialogEvent : UnityEvent { }

    [Header("UI 组件")]
    public GameObject dialogPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;
    public Image dialogBackground;
    public GameObject nextPrompt;
    public GameObject choicePanel;
    public Button[] choiceButtons;

    [Header("动画设置")]
    public float typewriterSpeed = 0.05f;
    public float fadeDuration = 0.3f;
    public float shakeIntensity = 3f;
    public float shakeDuration = 0.2f;

    [Header("声音设置")]
    public AudioClip dialogOpenSound;
    public AudioClip dialogCloseSound;
    public AudioClip textTypeSound;
    public AudioClip choiceSelectSound;

    [Header("事件")]
    public DialogEvent onDialogStart;
    public DialogEvent onDialogEnd;
    public DialogEvent onSentenceComplete;

    // 当前对话数据
    private DialogData _currentDialog;
    private int _currentSentenceIndex = 0;
    private Coroutine _typingCoroutine;
    private bool _isTyping = false;
    private bool _isDialogActive = false;
    private bool _isChoicePending = false;
    private CanvasGroup _dialogCanvasGroup;
    private Vector3 _originalTextPosition;
    private AudioSource _audioSource;

    private void Awake()
    {
        // 初始化组件
        _dialogCanvasGroup = dialogPanel.GetComponentInParent<CanvasGroup>();
        if (_dialogCanvasGroup == null)
        {
            _dialogCanvasGroup = dialogPanel.AddComponent<CanvasGroup>();
        }

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        // 隐藏对话框
        _dialogCanvasGroup.alpha = 0;
        choicePanel.SetActive(false);
        dialogPanel.SetActive(false);

        // 保存原始文本位置
        _originalTextPosition = dialogText.rectTransform.localPosition;
    }

    private void Update()
    {
        // 检测玩家输入
        if (_isDialogActive && !_isChoicePending)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                if (_isTyping)
                {
                    // 如果正在打字，完成当前句子
                    CompleteSentence();
                }
                else
                {
                    // 否则显示下一句
                    ShowNextSentence();
                }
            }
        }
    }

    /// <summary>
    /// 开始新对话
    /// </summary>
    /// <param name="dialogData">对话数据</param>
    public void StartDialog(DialogData dialogData)
    {
        if (_isDialogActive) return;

        _currentDialog = dialogData;
        _currentSentenceIndex = 0;
        _isDialogActive = true;

        // 播放打开音效
        PlaySound(dialogOpenSound);

        // 显示对话框
        dialogPanel.SetActive(true);
        StartCoroutine(FadeDialog(0, 1));

        // 触发事件
        onDialogStart.Invoke();

        // 显示第一句对话
        ShowSentence(_currentDialog.sentences[_currentSentenceIndex]);
    }

    /// <summary>
    /// 结束当前对话
    /// </summary>
    public void EndDialog()
    {
        if (!_isDialogActive) return;

        // 播放关闭音效
        PlaySound(dialogCloseSound);

        // 触发事件
        onDialogEnd.Invoke();

        // 隐藏对话框
        StartCoroutine(FadeDialog(1, 0, () => {
            dialogPanel.SetActive(false);
            _isDialogActive = false;
        }));
    }

    /// <summary>
    /// 显示下一句对话
    /// </summary>
    public void ShowNextSentence()
    {
        _currentSentenceIndex++;

        if (_currentSentenceIndex < _currentDialog.sentences.Length)
        {
            // 显示下一句
            ShowSentence(_currentDialog.sentences[_currentSentenceIndex]);
        }
        else
        {
            // 对话结束
            if (_currentDialog.choices.Length > 0)
            {
                // 显示选择
                ShowChoices();
            }
            else
            {
                // 结束对话
                EndDialog();
            }
        }
    }

    /// <summary>
    /// 显示当前句子
    /// </summary>
    /// <param name="sentence">句子数据</param>
    private void ShowSentence(DialogSentence sentence)
    {
        // 停止当前打字效果
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        // 更新UI
        nameText.text = sentence.characterName;
        dialogText.text = "";

        // 设置对话框背景
        if (sentence.dialogBackground != null)
        {
            dialogBackground.sprite = sentence.dialogBackground;
        }

        // 隐藏"继续"提示
        nextPrompt.SetActive(false);

        // 开始打字效果
        _isTyping = true;
        _typingCoroutine = StartCoroutine(TypeSentence(sentence.text));
    }

    /// <summary>
    /// 打字机效果显示句子
    /// </summary>
    private IEnumerator TypeSentence(string sentence)
    {
        dialogText.text = "";
        char[] chars = sentence.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            // 添加字符
            dialogText.text += chars[i];

            // 播放打字音效
            if (i % 3 == 0) // 每3个字符播放一次
            {
                PlaySound(textTypeSound);
            }

            // 特殊字符效果
            if (chars[i] == '!' || chars[i] == '?')
            {
                StartCoroutine(ShakeText());
            }

            yield return new WaitForSeconds(typewriterSpeed);
        }

        // 完成显示
        CompleteSentence();
    }

    /// <summary>
    /// 完成当前句子显示
    /// </summary>
    private void CompleteSentence()
    {
        _isTyping = false;
        nextPrompt.SetActive(true);

        // 触发事件
        onSentenceComplete.Invoke();
    }

    /// <summary>
    /// 立即完成当前句子显示
    /// </summary>
    private void CompleteSentence_NOW()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        // 显示完整句子
        dialogText.text = _currentDialog.sentences[_currentSentenceIndex].text;
        _isTyping = false;
        nextPrompt.SetActive(true);

        // 触发事件
        onSentenceComplete.Invoke();
    }

    /// <summary>
    /// 显示选择选项
    /// </summary>
    private void ShowChoices()
    {
        _isChoicePending = true;
        nextPrompt.SetActive(false);

        // 确保按钮数量足够
        if (choiceButtons.Length < _currentDialog.choices.Length)
        {
            Debug.LogError("选择按钮数量不足！");
            return;
        }

        // 设置按钮
        for (int i = 0; i < _currentDialog.choices.Length; i++)
        {
            choiceButtons[i].gameObject.SetActive(true);
            TextMeshProUGUI buttonText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = _currentDialog.choices[i].choiceText;

            // 移除旧监听器
            choiceButtons[i].onClick.RemoveAllListeners();

            // 添加新监听器
            int choiceIndex = i; // 创建局部变量
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));
        }

        // 隐藏多余按钮
        for (int i = _currentDialog.choices.Length; i < choiceButtons.Length; i++)
        {
            choiceButtons[i].gameObject.SetActive(false);
        }

        // 显示选择面板
        choicePanel.SetActive(true);
    }

    /// <summary>
    /// 当选择被选中时调用
    /// </summary>
    /// <param name="choiceIndex">选择索引</param>
    private void OnChoiceSelected(int choiceIndex)
    {
        PlaySound(choiceSelectSound);

        // 隐藏选择面板
        choicePanel.SetActive(false);
        _isChoicePending = false;

        // 执行选择结果
        DialogChoice choice = _currentDialog.choices[choiceIndex];
        choice.onChoiceSelected.Invoke();

        // 如果有后续对话
        if (choice.nextDialog != null)
        {
            StartDialog(choice.nextDialog);
        }
        else
        {
            EndDialog();
        }
    }

    /// <summary>
    /// 淡入淡出对话框
    /// </summary>
    private IEnumerator FadeDialog(float startAlpha, float targetAlpha, Action onComplete = null)
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            _dialogCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            yield return null;
        }

        _dialogCanvasGroup.alpha = targetAlpha;
        onComplete?.Invoke();
    }

    /// <summary>
    /// 文字震动效果
    /// </summary>
    private IEnumerator ShakeText()
    {
        Vector3 originalPos = dialogText.rectTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float offset = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
            dialogText.rectTransform.localPosition = new Vector3(
                originalPos.x + offset,
                originalPos.y + offset,
                originalPos.z
            );
            yield return null;
        }

        dialogText.rectTransform.localPosition = originalPos;
    }

    /// <summary>
    /// 播放声音
    /// </summary>
    /// <param name="clip">声音片段</param>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// 跳过当前对话
    /// </summary>
    public void SkipDialog()
    {
        if (!_isDialogActive) return;

        CompleteSentence();
        EndDialog();
    }
}

/// <summary>
/// 对话数据
/// </summary>
[System.Serializable]
public class DialogData
{
    public string dialogID;
    public DialogSentence[] sentences;
    public DialogChoice[] choices = new DialogChoice[0];
}

/// <summary>
/// 对话句子
/// </summary>
[System.Serializable]
public class DialogSentence
{
    public string characterName;
    [TextArea(3, 10)] public string text;
    public Sprite dialogBackground;
    public UnityEvent onSentenceDisplayed;
}

/// <summary>
/// 对话选择
/// </summary>
[System.Serializable]
public class DialogChoice
{
    public string choiceText;
    public DialogData nextDialog;
    public UnityEvent onChoiceSelected;
}
