using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum SoundTpye
{
    [Tooltip("�������\r\n��ѡ��Slowlyʱ��ֻ�������ڲ��ŵ���Ч������ʧ��Ȼ�����Ч�����̲���\r\n������Ч������ȫ֧�ֽ��뽥��,�����Ҫ���뽥����Ҫ�ⲿ�ϳ�Ȼ����ΪSingle������")]
    RamdomNoise,
    [Tooltip("��������Ч(��Ч��ĵ�һ��),����ָ������������\r\n��ѡ��Slowlyʱ��ֻ�������ڲ��ŵ���Ч������ʧ��Ȼ�����Ч�����̲���\r\n������Ч������ȫ֧�ֽ��뽥��,�����Ҫ���뽥����Ҫ�ⲿ�ϳ�Ȼ����ΪSingle������")]
    MainWithRamdomNoise,
    [Tooltip("��������Ч(��Ч��ĵ�һ��)")]
    Single
}
public enum SwitchStyle
{
    [Tooltip("�޹���")]
    Flash,
    [Tooltip("����")]
    Slowly
}

[Serializable]
public class SoundInf
{
    [Tooltip("��Դ��Ĭ�����ա�\r\n����Ĭ��Ϊ������Ч�µ���Դ")]
    public AudioSource Source;
    [Tooltip("�����Ч������")]
    [Range(0,1)]
    public float Volume = 1f;
    [Tooltip("��Ч�л����")]
    public SwitchStyle Style;
    [Tooltip("��Ч��������")]
    public SoundTpye Type;
    [Tooltip("��Ч��")]
    public AudioClip[] Sounds;
    [Tooltip("�����Ƶ�ڳ���ʱ�õ�ʱ��\r\nֻ���л��������ΪSlowlyʱ��Ч\r\n������������Singleʱ\r\n����Ӱ�쵭�뵭��Ч����ʱ��\r\n����������Ϊ�����������Чʱ\r\n��ֻ��Ӱ�쵭��Ч������ֱ�Ӳ��Ż������Ч")]
    [Range(0, 10)]
    public float Time = 1f;
    [Tooltip("ÿ�β���ʱ�����ϵ���Ƶ��\r\nֻ����Ч���Ͳ���Singleʱ��Ч\r\n��������������Ч�б��е���Ч����������Ϊȫ������")]
    [Range(0, 10)]
    public int SoundsOneTime = 3;
}


public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;
    public AudioSource CamAudioSource;
    private Dictionary<string, AudioClip> dictAudio;
    public Dictionary<(SwitchStyle, SoundTpye), Action<AudioSource, SoundInf>> PlayActions = new Dictionary<(SwitchStyle, SoundTpye), Action<AudioSource, SoundInf>>();

    public SoundInf BGM;
    public float MainVolume;

    public AudioManager()
    {
        PlayActions[(SwitchStyle.Flash, SoundTpye.RamdomNoise)] = (source, soundinf) => PlayRamSound(source, soundinf.Sounds, soundinf.Volume, Mathf.Max(Mathf.Min(soundinf.Sounds.Length, soundinf.SoundsOneTime), 0));
        PlayActions[(SwitchStyle.Flash, SoundTpye.MainWithRamdomNoise)] = (source, soundinf) => PlayRamMixSound(source, soundinf.Sounds, Mathf.Max(Mathf.Min(soundinf.Sounds.Length, soundinf.SoundsOneTime), 0), soundinf.Volume);
        PlayActions[(SwitchStyle.Flash, SoundTpye.Single)] = (source, soundinf) => ImmediatePlaySound(source, soundinf.Sounds[0], soundinf.Volume);
        PlayActions[(SwitchStyle.Slowly, SoundTpye.RamdomNoise)] = (source, soundinf) =>
        {
            if (source.volume >= 0)
            {
                FadeOutVolume(source, soundinf.Time, soundinf.Volume);
            }
            PlayRamSound(source, soundinf.Sounds, soundinf.Volume, Mathf.Max(Mathf.Min(soundinf.Sounds.Length, soundinf.SoundsOneTime), 0));
        };
        PlayActions[(SwitchStyle.Slowly, SoundTpye.MainWithRamdomNoise)] = (source, soundinf) =>
        {
            if (source.volume >= 0)
            {
                FadeOutVolume(source, soundinf.Time, soundinf.Volume);
            }
            PlayRamMixSound(source, soundinf.Sounds, Mathf.Max(Mathf.Min(soundinf.Sounds.Length, soundinf.SoundsOneTime), 0), soundinf.Volume);
        };
        PlayActions[(SwitchStyle.Slowly, SoundTpye.Single)] = (source, soundinf) =>
        {
            if (source.volume >= 0)
            {
                CrossTransSound(source, soundinf.Sounds[0], soundinf.Time, soundinf.Volume);
            }
            else
            {
                MuteAndChangeClip(source, soundinf.Sounds[0]);
                FadeInVolume(source, soundinf.Time, soundinf.Volume);
            }
        };
    }

    private void Awake()
    {
        instance = this;
        dictAudio = new Dictionary<string, AudioClip>();
    }
    private void Start()
    {
        Play(CamAudioSource, BGM);
    }
    private AudioClip LoadAudio(string path)
    {
        //Debug.Log((AudioClip)Resources.Load(path));
        return (AudioClip)Resources.Load(path);
    }
    private AudioClip GetAudio(string path)
    {
        if (!dictAudio.ContainsKey(path))
        {
            dictAudio[path] = LoadAudio(path);
        }
        return dictAudio[path];
    }
    public void Play(AudioSource AudioSource, SoundInf soundinf)
    {
        // ���Դ��ֵ��л�ȡ��Ӧ�Ĳ�����Ϊ����ִ����
        if (PlayActions.TryGetValue((soundinf.Style, soundinf.Type), out var action))
        {
            action(AudioSource, soundinf);
        }
    }
    #region ��������
    public void PlayBGM(string name,float Volume = 1.0f)
        {
            CamAudioSource.Stop();
            CamAudioSource.clip = GetAudio(name);
            CamAudioSource.Play();
            CamAudioSource.volume = Volume;
        }
    public void MuteAndChangeClip(AudioSource _audioSource, string path)
    {
        _audioSource.clip = GetAudio(path);
        _audioSource.volume = 0f;
        _audioSource.Play();
    }
    public void MuteAndChangeClip(AudioSource _audioSource, AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.volume = 0f;
        _audioSource.Play();
    }
    public void ImmediatePlaySound(AudioSource _audioSource,string path,float volume = 1.0f)
    {
        _audioSource.clip = GetAudio(path);
        _audioSource.volume = volume * MainVolume;
        _audioSource.Play();
    }
    public void ImmediatePlaySound(AudioSource _audioSource, AudioClip _clip, float volume = 1.0f)
    {
        _audioSource.clip = _clip;
        _audioSource.volume = volume * MainVolume;
        _audioSource.Play();
    }
    public void NonRepeatPlaySound(AudioSource _audioSource, string path, float volume = 1.0f)
    {
        AudioClip _clip = GetAudio(path);
        if(_audioSource.clip != _clip)
        {
            _audioSource.PlayOneShot(GetAudio(path), volume * MainVolume);
        }
    }
    public void PlayRamMixSound(AudioSource _audioSource, string MainPath, string[] SecPath, float volume = 1.0f)
    {
        _audioSource.PlayOneShot(GetAudio(MainPath), volume);
        for (int i = 0; i < SecPath.Length; i++)
        {
            float SecVolume = UnityEngine.Random.Range(0, volume * 0.5f * MainVolume);
            _audioSource.PlayOneShot(GetAudio(SecPath[i]), SecVolume);
        }

    }
    public void PlayRamMixSound(AudioSource _audioSource, AudioClip[] Sounds,int MixCount, float volume = 1.0f)
    {
        _audioSource.PlayOneShot(Sounds[0]);
        List<AudioClip> TheRestClips = new List<AudioClip>(Sounds);
        TheRestClips.RemoveAt(0);
        for (int i = 0; i < MixCount; i++)
        {
            int RandomIndex = UnityEngine.Random.Range(0, TheRestClips.Count);
            float SecVolume = UnityEngine.Random.Range(0, volume * 0.5f * MainVolume);
            _audioSource.PlayOneShot(TheRestClips[RandomIndex], SecVolume);
        }
    }
    public void PlayRamSound(AudioSource _audioSource ,string[] SecPath, float volume = 1.0f,int Audios = 0)
    {
        int PlayAudios = Mathf.Clamp(Audios, 0, SecPath.Length);
        float lastVolume = 0f;
        List<string> audioList = new List<string>(SecPath);
        for (int i = 0; i < PlayAudios; i++)
        {
            int index = UnityEngine.Random.Range(0,audioList.Count);
            float SecVolume = UnityEngine.Random.Range(0, 
                Mathf.Max(0,volume - lastVolume) * MainVolume);
            lastVolume = SecVolume;
            _audioSource.PlayOneShot(GetAudio(audioList[index]), SecVolume);
            audioList.RemoveAt(index);
        }
    }
    public void PlayRamSound(AudioSource _audioSource, AudioClip[] Sounds, float volume = 1.0f, int AudiosCount = 0)
    {
        int PlayAudios = Mathf.Clamp(AudiosCount, 0, Sounds.Length);
        float lastVolume = 0f;
        List<AudioClip> audioList = new List<AudioClip>(Sounds);
        for (int i = 0; i < PlayAudios; i++)
        {
            int index = UnityEngine.Random.Range(0, audioList.Count);
            float SecVolume = UnityEngine.Random.Range(0,
                Mathf.Max(0, volume - lastVolume) * MainVolume);
            lastVolume = SecVolume;
            _audioSource.PlayOneShot(audioList[index], SecVolume);
            audioList.RemoveAt(index);
        }
    }
    #region ��Ч���뵭��Э��
    private IEnumerator IE_CrossTransSound(AudioSource OnPlayingSource, string Path, float duration = 1.0f, float targetVolume=1.0f)
    {
        if (duration <= 0) { Debug.LogError("�������뵭���Ĺ�����ʱ�����������"); yield break; }
        float UsingTime = 0f;
        AudioSource OnComingSource = Instantiate( OnPlayingSource.gameObject).GetComponent<AudioSource>();
        OnComingSource.transform.position = OnPlayingSource.transform.position;
        OnComingSource.transform.parent = OnPlayingSource.transform;
        MuteAndChangeClip(OnComingSource, Path);
        while (OnPlayingSource.volume > 0 || OnComingSource.volume < targetVolume)
        {
            OnPlayingSource.volume = Mathf.Max(0, OnPlayingSource.volume - UsingTime / duration);
            OnComingSource.volume = Mathf.Min(targetVolume,UsingTime / duration*targetVolume);
            UsingTime += Time.deltaTime;
            yield return null;
        }
        OnPlayingSource.clip = OnComingSource.clip;
        OnPlayingSource.time = OnComingSource.time;
        OnPlayingSource.volume = OnComingSource.volume;
        GameObject.Destroy(OnComingSource);
    }
    private IEnumerator IE_CrossTransSound(AudioSource OnPlayingSource, AudioClip Sound, float duration = 1.0f, float targetVolume = 1.0f)
    {
        if (duration <= 0) { Debug.LogError("�������뵭���Ĺ�����ʱ�����������"); yield break; }
        float UsingTime = 0f;
        float StartVolume = OnPlayingSource.volume;
        AudioSource OnComingSource = Instantiate(OnPlayingSource.gameObject).GetComponent<AudioSource>();
        OnComingSource.transform.position = OnPlayingSource.transform.position;
        OnComingSource.transform.parent = OnPlayingSource.transform;
        MuteAndChangeClip(OnComingSource, Sound);
        while (OnPlayingSource.volume > 0 || OnComingSource.volume < targetVolume)
        {
            OnPlayingSource.volume = Mathf.Max(0, StartVolume - UsingTime / duration * StartVolume);
            OnComingSource.volume = Mathf.Min(targetVolume, UsingTime / duration * targetVolume);
            UsingTime += Time.deltaTime;
            yield return null;
        }
        OnPlayingSource.clip = OnComingSource.clip;
        OnPlayingSource.time = OnComingSource.time;
        OnPlayingSource.pitch = OnComingSource.pitch;
        OnPlayingSource.volume = OnComingSource.volume;
        OnPlayingSource.Play();
        GameObject.Destroy(OnComingSource.gameObject);
    }
    private IEnumerator IE_FadeInVolume(AudioSource OnComingSource, float duration = 1.0f, float targetVolume = 1.0f)
    {
        float UsingTime = 0f;
        while (OnComingSource.volume < targetVolume)
        {
            OnComingSource.volume = Mathf.Min(targetVolume, UsingTime / duration * targetVolume);
            UsingTime += Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator IE_FadeOutVolume(AudioSource OnMeltSource, float duration = 1.0f)
    {
        float UsingTime = 0f;
        while (OnMeltSource.volume > 0)
        {
            OnMeltSource.volume = Mathf.Max(0, OnMeltSource.volume - UsingTime / duration);
            UsingTime += Time.deltaTime;
            yield return null;
        }
    }
    #endregion
    public void CrossTransSound(AudioSource OnPlayingSource, string Path, float duration = 1.0f, float targetVolume = 1.0f)
    {
        StartCoroutine(IE_CrossTransSound(OnPlayingSource, Path, duration, targetVolume));
    }
    public void CrossTransSound(AudioSource OnPlayingSource, AudioClip Sound, float duration = 1.0f, float targetVolume = 1.0f)
    {
        StartCoroutine(IE_CrossTransSound(OnPlayingSource, Sound, duration, targetVolume));
    }
    public void FadeInVolume(AudioSource OnPlayingSource, float fadeInDuration= 1.0f, float targetVolume = 1.0f)
    {
        StartCoroutine(IE_FadeInVolume(OnPlayingSource, fadeInDuration, targetVolume));
    }
    public void FadeOutVolume(AudioSource OnMeltSource, float fadeOutDuration = 1.0f, float targetVolume = 1.0f)
    {
        StartCoroutine(IE_FadeOutVolume(OnMeltSource, fadeOutDuration));
    }
    #endregion
}

