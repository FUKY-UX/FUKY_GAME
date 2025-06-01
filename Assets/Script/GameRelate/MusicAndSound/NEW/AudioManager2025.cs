using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 简易音效播放器（单例模式）
/// 功能：播放/停止音效、音量控制、防止重复播放
/// </summary>
public class AudioManager2025 : MonoBehaviour
{
    public static AudioManager2025 Instance { get; private set; }

    [Header("音效设置")]
    [Range(0, 1)] public float globalVolume = 1f;
    [SerializeField] private AudioClip[] soundEffects;
    [SerializeField] private AudioClip bgm;
    private Dictionary<string, AudioClip> soundLibrary = new Dictionary<string, AudioClip>();
    private AudioSource audioSource;
    private float lastPlayTime;
    private float soundCooldown = 0.1f; // 防止连续播放的最小间隔

    private Camera CurrMainCam;

    private void Awake()
    {

        // 单例初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 创建音频源
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // 建立音效库字典
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
    /// 播放指定名称的音效
    /// </summary>
    public void PlaySound(string soundName)
    {
        // 防止高频重复播放
        if (Time.time - lastPlayTime < soundCooldown) return;

        if (soundLibrary.TryGetValue(soundName, out AudioClip clip))
        {
            audioSource.PlayOneShot(clip, globalVolume);
            lastPlayTime = Time.time;
        }
        else
        {
            Debug.LogWarning($"音效不存在: {soundName}");
        }
    }

    /// <summary>
    /// 随机播放一组音效中的一个
    /// </summary>
    public void PlayRandomSound(params string[] soundNames)
    {
        if (soundNames.Length == 0) return;

        string selectedSound = soundNames[Random.Range(0, soundNames.Length)];
        PlaySound(selectedSound);
    }

    /// <summary>
    /// 停止所有音效
    /// </summary>
    public void StopAllSounds()
    {
        audioSource.Stop();
    }

    /// <summary>
    /// 设置全局音量
    /// </summary>
    public void SetVolume(float volume)
    {
        globalVolume = Mathf.Clamp01(volume);
    }

    // 编辑器快捷方法
    [ContextMenu("Test Play Sound")]
    private void TestPlay()
    {
        if (soundEffects.Length > 0)
            PlaySound(soundEffects[0].name);
    }
}