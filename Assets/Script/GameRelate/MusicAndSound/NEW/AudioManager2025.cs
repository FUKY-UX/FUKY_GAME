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

    [Header("短音效设置")]
    [Range(0, 1)] public float globalVolume = 1f;
    [SerializeField] private AudioClip[] soundEffects;
    [SerializeField] private AudioClip bgm;
    [SerializeField] public AudioSource UniversalAudioSoure;


    // 长音效专用音频源
    [Header("长音效设置")]
    private SerializableDictionary<string, AudioClip> soundLibrary = new SerializableDictionary<string, AudioClip>();

    [SerializeField] private AudioSource _longSoundSource; // 专门用于播放长音效的音频源
    private string _currentLongSound; // 当前播放的长音效名称
    private bool _isLongSoundPlaying; // 标记是否有长音效正在播放

    [Header("通用设置")]
    private AudioSource CamAudioSoure;
    public float soundCooldown = 0.5f; // 防止连续播放的最小间隔
    private float lastPlayTime;
    private string lastplayName;

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

        // 确保有专用的长音效音频源
        if (_longSoundSource == null)
        {
            _longSoundSource = gameObject.AddComponent<AudioSource>();
            _longSoundSource.loop = false; // 默认不循环
            _longSoundSource.playOnAwake = false;
        }

    }
    private void Start()
    {
        CamAudioSoure = Camera.main.gameObject.GetComponent<AudioSource>();

    }

    /// <summary>
    /// 播放长音效（不会被打断或重复播放）
    /// </summary>
    /// <param name="soundName">音效名称</param>
    /// <param name="loop">是否循环播放</param>
    /// <param name="interruptible">是否允许被新音效中断</param>
    /// <returns>是否成功开始播放</returns>
    public bool PlayLongSound(string soundName, bool loop = false, bool interruptible = false)
    {
        // 如果已经有长音效在播放且不允许中断
        if (_isLongSoundPlaying && !interruptible && _currentLongSound == soundName)
        {
            Debug.Log($"长音效 {soundName} 已经在播放中，忽略请求");
            return false;
        }
        
        // 如果音效不存在
        if (!soundLibrary.TryGetValue(soundName, out AudioClip clip))
        {
            Debug.LogWarning($"长音效不存在: {soundName}");
            return false;
        }
        
        // 停止当前播放的长音效
        if (_isLongSoundPlaying)
        {
            _longSoundSource.Stop();
        }
        
        // 设置并播放新音效
        _longSoundSource.clip = clip;
        _longSoundSource.loop = loop;
        _longSoundSource.volume = globalVolume;
        _longSoundSource.Play();
        
        _currentLongSound = soundName;
        _isLongSoundPlaying = true;
        
        Debug.Log($"开始播放长音效: {soundName} (循环: {loop}, 可中断: {interruptible})");
        return true;
    }
    
    /// <summary>
    /// 停止当前播放的长音效
    /// </summary>
    public void StopLongSound()
    {
        if (_isLongSoundPlaying)
        {
            _longSoundSource.Stop();
            _isLongSoundPlaying = false;
            Debug.Log($"已停止长音效: {_currentLongSound}");
        }
    }
    
    /// <summary>
    /// 淡出当前长音效
    /// </summary>
    /// <param name="duration">淡出时间（秒）</param>
    public void FadeOutLongSound(float duration = 1.0f)
    {
        if (_isLongSoundPlaying)
        {
            StartCoroutine(FadeOutCoroutine(duration));
        }
    }
    
    private IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = _longSoundSource.volume;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _longSoundSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }
        
        StopLongSound();
        _longSoundSource.volume = globalVolume; // 重置音量
    }
    
    /// <summary>
    /// 检查指定长音效是否正在播放
    /// </summary>
    public bool IsLongSoundPlaying(string soundName = null)
    {
        if (string.IsNullOrEmpty(soundName))
            return _isLongSoundPlaying;
        
        return _isLongSoundPlaying && _currentLongSound == soundName;
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
            //Debug.LogWarning($"找到了: {soundName}");

            SoundSource.PlayOneShot(clip, globalVolume);
            lastPlayTime = Time.time;
            lastplayName = soundName;
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
