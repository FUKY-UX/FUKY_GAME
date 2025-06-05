using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;

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
    [SerializeField] public AudioSource UniversalAudioSoure;

    public SerializableDictionary<string, AudioClip> soundLibrary = new SerializableDictionary<string, AudioClip>();
    private AudioSource CamAudioSoure;

    private float lastPlayTime;
    private string lastplayName;

    private float soundCooldown = 0.1f; // 防止连续播放的最小间隔

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

        // 建立音效库字典
        foreach (var clip in soundEffects)
        {
            soundLibrary.Add(clip.name, clip);
        }


    }
    private void Start()
    {
        CamAudioSoure = Camera.main.gameObject.GetComponent<AudioSource>();
       // UniversalAudioSoure.PlayOneShot(bgm, globalVolume);

    }

    /// <summary>
    /// 播放指定名称的音效
    /// </summary>
    public void PlaySound(string soundName)
    {
        // 防止高频重复播放
        if (Time.time - lastPlayTime < soundCooldown && lastplayName == soundName) return;

        if (soundLibrary.TryGetValue(soundName, out AudioClip clip))
        {
            CamAudioSoure.PlayOneShot(clip, globalVolume);
            lastPlayTime = Time.time;
            lastplayName = soundName;
        }
        else
        {
            Debug.LogWarning($"音效不存在: {soundName}");
        }
    }

    /// <summary>
    /// 在指定的音源播放指定名称的音效
    /// </summary>
    public void PlaySound( AudioSource SoundSource, string soundName)
    {

        // 防止高频重复播放
        if (Time.time - lastPlayTime < soundCooldown && lastplayName == soundName) return;

        if (soundLibrary.TryGetValue(soundName, out AudioClip clip) && SoundSource!=null)
        {
            Debug.LogWarning($"找到了: {soundName}");

            SoundSource.PlayOneShot(clip, globalVolume);
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
    /// 随机播放一组音效中的一个
    /// </summary>
    public void PlayRandomSound(AudioSource SoundSource,params string[] soundNames)
    {
        if (soundNames.Length == 0) return;

        string selectedSound = soundNames[Random.Range(0, soundNames.Length)];
        PlaySound(SoundSource,selectedSound);
    }


    /// <summary>
    /// 停止所有音效
    /// </summary>
    public void StopAllSounds()
    {
        CamAudioSoure.Stop();
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