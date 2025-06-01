using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ������Ч������������ģʽ��
/// ���ܣ�����/ֹͣ��Ч���������ơ���ֹ�ظ�����
/// </summary>
public class AudioManager2025 : MonoBehaviour
{
    public static AudioManager2025 Instance { get; private set; }

    [Header("��Ч����")]
    [Range(0, 1)] public float globalVolume = 1f;
    [SerializeField] private AudioClip[] soundEffects;
    [SerializeField] private AudioClip bgm;
    private Dictionary<string, AudioClip> soundLibrary = new Dictionary<string, AudioClip>();
    private AudioSource audioSource;
    private float lastPlayTime;
    private float soundCooldown = 0.1f; // ��ֹ�������ŵ���С���

    private Camera CurrMainCam;

    private void Awake()
    {

        // ������ʼ��
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ������ƵԴ
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // ������Ч���ֵ�
        foreach (var clip in soundEffects)
        {
            soundLibrary.Add(clip.name, clip);
        }


    }
    private void Start()
    {
        CurrMainCam = Camera.main;
        CurrMainCam.gameObject.GetComponent<AudioSource>().PlayOneShot(bgm, globalVolume);

    }

    /// <summary>
    /// ����ָ�����Ƶ���Ч
    /// </summary>
    public void PlaySound(string soundName)
    {
        // ��ֹ��Ƶ�ظ�����
        if (Time.time - lastPlayTime < soundCooldown) return;

        if (soundLibrary.TryGetValue(soundName, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip, globalVolume);
            lastPlayTime = Time.time;
        }
        else
        {
            Debug.LogWarning($"��Ч������: {soundName}");
        }
    }

    /// <summary>
    /// �������һ����Ч�е�һ��
    /// </summary>
    public void PlayRandomSound(params string[] soundNames)
    {
        if (soundNames.Length == 0) return;

        string selectedSound = soundNames[Random.Range(0, soundNames.Length)];
        PlaySound(selectedSound);
    }

    /// <summary>
    /// ֹͣ������Ч
    /// </summary>
    public void StopAllSounds()
    {
        audioSource.Stop();
    }

    /// <summary>
    /// ����ȫ������
    /// </summary>
    public void SetVolume(float volume)
    {
        globalVolume = Mathf.Clamp01(volume);
    }

    // �༭����ݷ���
    [ContextMenu("Test Play Sound")]
    private void TestPlay()
    {
        if (soundEffects.Length > 0)
            PlaySound(soundEffects[0].name);
    }
}