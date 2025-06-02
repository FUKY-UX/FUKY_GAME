using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// �Ի��������
/// ������Ϸ�еĶԻ�ϵͳ
/// </summary>
public class DialogController : SingletonMono<DialogController>
{
    [System.Serializable]
    public class DialogEvent : UnityEvent { }

    [Header("UI ���")]
    public GameObject dialogPanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;
    public Image characterAvatar;
    public Image dialogBackground;
    public GameObject nextPrompt;
    public GameObject choicePanel;
    public Button[] choiceButtons;

    [Header("��������")]
    public float typewriterSpeed = 0.05f;
    public float fadeDuration = 0.3f;
    public float shakeIntensity = 3f;
    public float shakeDuration = 0.2f;

    public const float UIAnimDuration = 0.5f;


    [Header("��������")]
    public AudioClip dialogOpenSound;
    public AudioClip dialogCloseSound;
    public AudioClip textTypeSound;
    public AudioClip choiceSelectSound;

    [Header("�¼�")]
    public DialogEvent onDialogStart;
    public DialogEvent onDialogEnd;
    public DialogEvent onSentenceComplete;

    // ��ǰ�Ի�����
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
        // ��ʼ�����
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

        // ���ضԻ���
        _dialogCanvasGroup.alpha = 0;
        choicePanel.SetActive(false);
        dialogPanel.SetActive(false);

        // ����ԭʼ�ı�λ��
        _originalTextPosition = dialogText.rectTransform.localPosition;
    }

    private void Update()
    {
        // ����������
        if (_isDialogActive && !_isChoicePending)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (_isTyping)
                {
                    // ������ڴ��֣���ɵ�ǰ����
                    CompleteSentence();
                }
                else
                {
                    // ������ʾ��һ��
                    ShowNextSentence();
                }
            }
        }
    }

    /// <summary>
    /// ��ʼ�¶Ի�
    /// </summary>
    /// <param name="dialogData">�Ի�����</param>
    public void StartDialog(DialogData dialogData)
    {
        if (_isDialogActive) return;

        _currentDialog = dialogData;
        _currentSentenceIndex = 0;
        _isDialogActive = true;

        // ���Ŵ���Ч
        PlaySound(dialogOpenSound);

        // ��ʾ�Ի���
        dialogPanel.SetActive(true);
        StartCoroutine(FadeDialog(0, 1));

        // �����¼�
        onDialogStart.Invoke();

        // ��ʾ��һ��Ի�
        ShowSentence(_currentDialog.sentences[_currentSentenceIndex]);
    }

    /// <summary>
    /// ������ǰ�Ի�
    /// </summary>
    public void EndDialog()
    {
        if (!_isDialogActive) return;

        // ���Źر���Ч
        PlaySound(dialogCloseSound);

        // �����¼�
        onDialogEnd.Invoke();

        // ���ضԻ���
        StartCoroutine(FadeDialog(1, 0, () => {
            dialogPanel.SetActive(false);
            _isDialogActive = false;
        }));
    }

    /// <summary>
    /// ��ʾ��һ��Ի�
    /// </summary>
    public void ShowNextSentence()
    {
        _currentSentenceIndex++;

        if (_currentSentenceIndex < _currentDialog.sentences.Length)
        {
            // ��ʾ��һ��
            ShowSentence(_currentDialog.sentences[_currentSentenceIndex]);
        }
        else
        {
            // �Ի�����
            if (_currentDialog.choices.Length > 0)
            {
                // ��ʾѡ��
                ShowChoices();
            }
            else
            {
                // �����Ի�
                EndDialog();
            }
        }
    }

    /// <summary>
    /// ��ʾ��ǰ����
    /// </summary>
    /// <param name="sentence">��������</param>
    private void ShowSentence(DialogSentence sentence)
    {
        // ֹͣ��ǰ����Ч��
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        // ����UI
        nameText.text = sentence.characterName;
        dialogText.text = "";

        // ���öԻ��򱳾�
        if (sentence.dialogBackground != null )
        {
            dialogBackground.sprite = sentence.dialogBackground;
        }

        // ���ý�ɫͷ��
        if (sentence.characterAvatar.Length > 0)
        {
            characterAvatar.sprite = sentence.characterAvatar[0];
            characterAvatar.gameObject.SetActive(true);
        }
        else
        {
            characterAvatar.gameObject.SetActive(false);
        }

        // ����"����"��ʾ
        nextPrompt.SetActive(false);

        // ��ʼ����Ч��
        _isTyping = true;
        _typingCoroutine = StartCoroutine(TypeSentence(sentence.text, sentence));
    }

    /// <summary>
    /// ���ֻ�Ч����ʾ����
    /// </summary>
    private IEnumerator TypeSentence(string sentence, DialogSentence CurrSentence)
    {
        dialogText.text = "";
        char[] chars = sentence.ToCharArray();
        int AnimIndex = 0;
        float _LastUpdateTime = 0;
        for (int i = 0; i < chars.Length; i++)
        {
            // ����ַ�
            dialogText.text += chars[i];

            // ���Ŵ�����Ч
            if (i % 4 == 0) // ÿ3���ַ�����һ��
            {
                PlaySound(textTypeSound);
            }
            
            if (CurrSentence.characterAvatar.Length > 1 && Time.time - _LastUpdateTime > UIAnimDuration)
            {
                characterAvatar.sprite = CurrSentence.characterAvatar[AnimIndex];
                AnimIndex++;
                if (AnimIndex > CurrSentence.characterAvatar.Length - 1) { AnimIndex = 0; }
                _LastUpdateTime = Time.time;
            }

            // �����ַ�Ч��
            if (chars[i] == '!' || chars[i] == '?')
            {
                StartCoroutine(ShakeText());
            }


            yield return new WaitForSeconds(typewriterSpeed);
        }

        // �����ʾ
        CompleteSentence();
    }

    /// <summary>
    /// ��ɵ�ǰ������ʾ
    /// </summary>
    private void CompleteSentence()
    {
        _isTyping = false;
        nextPrompt.SetActive(true);

        // �����¼�
        onSentenceComplete.Invoke();
    }

    /// <summary>
    /// ������ɵ�ǰ������ʾ
    /// </summary>
    private void CompleteSentence_NOW()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        // ��ʾ��������
        dialogText.text = _currentDialog.sentences[_currentSentenceIndex].text;
        _isTyping = false;
        nextPrompt.SetActive(true);

        // �����¼�
        onSentenceComplete.Invoke();
    }

    /// <summary>
    /// ��ʾѡ��ѡ��
    /// </summary>
    private void ShowChoices()
    {
        _isChoicePending = true;
        nextPrompt.SetActive(false);

        // ȷ����ť�����㹻
        if (choiceButtons.Length < _currentDialog.choices.Length)
        {
            Debug.LogError("ѡ��ť�������㣡");
            return;
        }

        // ���ð�ť
        for (int i = 0; i < _currentDialog.choices.Length; i++)
        {
            choiceButtons[i].gameObject.SetActive(true);
            TextMeshProUGUI buttonText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = _currentDialog.choices[i].choiceText;

            // �Ƴ��ɼ�����
            choiceButtons[i].onClick.RemoveAllListeners();

            // ����¼�����
            int choiceIndex = i; // �����ֲ�����
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choiceIndex));
        }

        // ���ض��ఴť
        for (int i = _currentDialog.choices.Length; i < choiceButtons.Length; i++)
        {
            choiceButtons[i].gameObject.SetActive(false);
        }

        // ��ʾѡ�����
        choicePanel.SetActive(true);
    }

    /// <summary>
    /// ��ѡ��ѡ��ʱ����
    /// </summary>
    /// <param name="choiceIndex">ѡ������</param>
    private void OnChoiceSelected(int choiceIndex)
    {
        PlaySound(choiceSelectSound);

        // ����ѡ�����
        choicePanel.SetActive(false);
        _isChoicePending = false;

        // ִ��ѡ����
        DialogChoice choice = _currentDialog.choices[choiceIndex];
        choice.onChoiceSelected.Invoke();

        // ����к����Ի�
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
    /// ���뵭���Ի���
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
    /// ������Ч��
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
    /// ��������
    /// </summary>
    /// <param name="clip">����Ƭ��</param>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// ������ǰ�Ի�
    /// </summary>
    public void SkipDialog()
    {
        if (!_isDialogActive) return;

        CompleteSentence();
        EndDialog();
    }
}

/// <summary>
/// �Ի�����
/// </summary>
[System.Serializable]
public class DialogData
{
    public string dialogID;
    public DialogSentence[] sentences;
    public DialogChoice[] choices = new DialogChoice[0];
}

/// <summary>
/// �Ի�����
/// </summary>
[System.Serializable]
public class DialogSentence
{
    public string characterName;
    [TextArea(3, 10)] public string text;
    public Sprite[] characterAvatar; 
    public UnityEvent onSentenceDisplayed;
    public Sprite dialogBackground;
}

/// <summary>
/// �Ի�ѡ��
/// </summary>
[System.Serializable]
public class DialogChoice
{
    public string choiceText;
    public DialogData nextDialog;
    public UnityEvent onChoiceSelected;
}
