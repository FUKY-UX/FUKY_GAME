using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))] // Ensures this script is attached to a Button
public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Sound Effects")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    [Header("Hover Animation")]
    [SerializeField] private float hoverScale = 1.1f; // 10% larger
    [SerializeField] private float animationDuration = 0.2f;

    private AudioSource audioSource;
    private Button button;
    private Vector3 originalScale;
    private bool isHovering = false;

    private void OnEnable()
    {
        button =gameObject.GetComponent<Button>();
    }

    private void Awake()
    {
        // Get required components
        button = gameObject.GetComponent<Button>();
        audioSource = Camera.main.gameObject.GetComponent<AudioSource>();

        // Add AudioSource if not present
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Store original scale
        originalScale = transform.localScale;

        // Add click sound to button
        button.onClick.AddListener(PlayClickSound);
    }

    // Called when mouse enters the button
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hover Entered"); // 检查是否输出
        isHovering = true;
        PlayHoverSound();
        StartCoroutine(ScaleButton(hoverScale));
    }

    // Called when mouse exits the button
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        StartCoroutine(ScaleButton(1f)); // Return to original scale
    }

    private void PlayHoverSound()
    {
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    private void PlayClickSound()
    {
        Debug.Log("Button Clicked"); // 检查是否输出
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    private System.Collections.IEnumerator ScaleButton(float targetScale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = originalScale * targetScale;
        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.unscaledDeltaTime; // Use unscaledDeltaTime so it works even when game is paused
            float progress = Mathf.Clamp01(time / animationDuration);
            transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            yield return null;
        }

        transform.localScale = endScale;
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }
}