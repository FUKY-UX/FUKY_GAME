using UnityEngine;
using System.Collections;


/// <summary>
/// UI��������������
/// </summary>
public class UIBreathingAnimator : MonoBehaviour
{
    [Header("������������")]
    [Tooltip("���������ٶ�")]
    [Range(0.1f, 5f)]
    public float breathingSpeed = 1f;

    [Tooltip("λ�ò�������")]
    [Range(1f, 50f)]
    public float positionAmplitude = 10f;

    [Tooltip("��ת��������(��)")]
    [Range(1f, 30f)]
    public float rotationAmplitude = 5f;

    [Tooltip("��������")]
    public AnimationCurve breathingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Ŀ��UIԪ��
    public RectTransform _targetElement;

    // ԭʼλ�ú���ת
    public Vector2 _originalPosition;
    public Quaternion _originalRotation;

    // ��ǰĿ��λ�ú���ת
    private Vector2 _targetPosition;
    private Quaternion _targetRotation;

    // ������ʱ��
    private float _animationTimer;

    // ��ǰ��������ʱ��
    private float _currentDuration;

    // �Ƿ����ڲ��Ŷ���
    private bool _isAnimating = false;

    /// <summary>
    /// ��ʼ��������
    /// </summary>
    /// <param name="element">Ŀ��UIԪ��</param>
    public IEnumerator StartBreathingAnimation(RectTransform element)
    {

        if (element == null)
        {
            Debug.LogWarning("�޷���ʼ����������UIԪ��Ϊ��");
            yield break;
        }
        _targetElement = element;

        // ����ԭʼ״̬
        _originalPosition = element.anchoredPosition;
        _originalRotation = element.rotation;

        // ���ó�ʼĿ��
        SetNewRandomTarget();

        // ���ü�ʱ��
        _animationTimer = 0f;

        // ��ʼ����Э��
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
    /// ֹͣ��������
    /// </summary>
    public void StopBreathingAnimation()
    {
        _isAnimating = false;
        StopCoroutine(BreathingAnimationRoutine());

        // ���õ�ԭʼλ��
        if (_targetElement != null)
        {
            _targetElement.anchoredPosition = _originalPosition;
            _targetElement.rotation = _originalRotation;
        }
    }

    /// <summary>
    /// ��������Э��
    /// </summary>
    private IEnumerator BreathingAnimationRoutine()
    {
        while (_isAnimating && _targetElement != null)
        {
            // ���¼�ʱ��
            _animationTimer += Time.deltaTime;

            // ���㶯������ (0-1)
            float progress = Mathf.Clamp01(_animationTimer / _currentDuration);

            // Ӧ�ö�������
            float curvedProgress = breathingCurve.Evaluate(progress);

            // ���㵱ǰλ�ú���ת
            Vector2 currentPosition = Vector2.Lerp(_originalPosition, _targetPosition, curvedProgress);
            Quaternion currentRotation = Quaternion.Lerp(_originalRotation, _targetRotation, curvedProgress);

            // Ӧ�ñ任
            _targetElement.anchoredPosition = currentPosition;
            _targetElement.rotation = currentRotation;

            // ����Ƿ���ɵ�ǰ�ζ���
            if (progress >= 1f)
            {
                // ������Ŀ��
                SetNewRandomTarget();
            }

            yield return null;
        }
    }

    /// <summary>
    /// �����µ����Ŀ��λ�ú���ת
    /// </summary>
    private void SetNewRandomTarget()
    {
        // ���ü�ʱ��
        _animationTimer = 0f;

        // ������ɶ�������ʱ�� (0.5-2��)
        _currentDuration = Random.Range(0.5f, 2f) / breathingSpeed;

        // �������λ��ƫ��
        Vector2 positionOffset = new Vector2(
            Random.Range(-positionAmplitude, positionAmplitude),
            Random.Range(-positionAmplitude, positionAmplitude)
        );

        // ���������תƫ�� (��)
        float rotationOffset = Random.Range(-rotationAmplitude, rotationAmplitude);

        // ����Ŀ��λ�ú���ת
        _targetPosition = _originalPosition + positionOffset;
        _targetRotation = Quaternion.Euler(0, 0, rotationOffset) * _originalRotation;
    }

    /// <summary>
    /// ���������ʱ��ʼ����
    /// </summary>
    private void OnEnable()
    {
        if (_targetElement != null)
        {
            StartBreathingAnimation(_targetElement);
        }
    }

    /// <summary>
    /// ���������ʱֹͣ����
    /// </summary>
    private void OnDisable()
    {
        StopBreathingAnimation();
    }
}