using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


public enum SoundTpye
{
    [Tooltip("������ţ���Ȼ��������\r\n�������Ч����ѡ��һ������\r\n��Ϊ����������Ȼ�����εݼ�")]
    RamdomNoise,
    [Tooltip("��������Ч(��Ч��ĵ�һ��),����ָ������������\r\n����������������Ч��һ�����Ҳ���")]
    MainWithRamdomNoise,
    [Tooltip("��������Ч(��Ч��ĵ�һ��)\r\n�������٣�����Ҫʵʱ�ϳ�")]
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
    public SoundInf(Transform transform)
    {
        Source = transform;
        Sounds = new List<AudioClip>();
    }

    [Tooltip("��Դ��Ĭ�����ա�\r\n����Ĭ��Ϊ���ؽű���������Ϊ��Դ")]
    public Transform Source;
    [Tooltip("�����Ч������")]
    [Range(0,1)]
    public float Volume = 1f;
    [Tooltip("��Ч�л����")]
    public SwitchStyle SwitchStyle;
    [Tooltip("��Ч��������")]
    public SoundTpye Type;
    [Tooltip("��Ч��")]
    public List<AudioClip> Sounds;
    [Tooltip("��Ч���뽥��ʱ��")]
    [Range(0, 20)]
    public float Time = 1f;
    [Tooltip("ÿ�β���ʱ��ϵ���Ƶ��\r\n��������������Ч�б��е���Ч����������Ϊȫ������")]
    [Range(1, 10)]
    public int MixCount = 3;
    [Tooltip("����Ƶ�Ƿ���һֱѭ����")]
    public bool Loop = false;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public GameObject DynamicSourceTemplate;
    public Transform MyChildren;
    public Queue<AudioSource> VirtulAudioSources;
    public Dictionary<(SwitchStyle, SoundTpye), Action<SoundInf>> PlayActions = new Dictionary<(SwitchStyle, SoundTpye), Action<SoundInf>>();
    public SerializableDictionary<SoundInf, List<AudioSource>> AudioSourceInSoundInf;
    public SerializableDictionary<AudioSource, Coroutine> OnFadeAudioSource;
    public SoundInf BGM;
    public float MainVolume;
    public AudioManager()
    {
        instance = this;
        AudioSourceInSoundInf = new SerializableDictionary<SoundInf, List<AudioSource>>();
        OnFadeAudioSource = new SerializableDictionary<AudioSource, Coroutine>();
        VirtulAudioSources = new Queue<AudioSource>();
        PlayActions[(SwitchStyle.Flash, SoundTpye.RamdomNoise)] = (soundinf) => PlayRamSound(soundinf, 1.0f);
        PlayActions[(SwitchStyle.Flash, SoundTpye.MainWithRamdomNoise)] = (soundinf) => PlayRamMixSound(soundinf,1.0f);
        PlayActions[(SwitchStyle.Flash, SoundTpye.Single)] = (soundinf) => PlayMainSound(soundinf, 1.0f);
        PlayActions[(SwitchStyle.Slowly, SoundTpye.RamdomNoise)] = (soundinf) =>{CrossTrans_RandomN(soundinf);};
        PlayActions[(SwitchStyle.Slowly, SoundTpye.MainWithRamdomNoise)] = (soundinf) =>{CrossTrans_1plusN(soundinf);};
        PlayActions[(SwitchStyle.Slowly, SoundTpye.Single)] = (soundinf) =>{CrossTrans_Only1(soundinf);};
    }

    private void Start()
    {
        AudioSourceInSoundInf.Add(BGM, new List<AudioSource>());
        Play(BGM);
    }
    private AudioClip LoadAudio(string path)
    {
        //Debug.Log((AudioClip)Resources.Load(path));
        return (AudioClip)Resources.Load(path);
    }
    public void Play(SoundInf soundinf)
    {
        // ���Դ��ֵ��л�ȡ��Ӧ�Ĳ�����Ϊ����ִ����
        if (PlayActions.TryGetValue((soundinf.SwitchStyle, soundinf.Type), out var action))
        {
            action(soundinf);
        }
    }
    #region ��������
    public AudioSource PlayMainSound(SoundInf soundinf, float _Volume = 0)
    {
        AudioSource _SourceToPlay;
        if (VirtulAudioSources.Count == 0) { NewEmptyAudioSource_ToQueue(); }
        _SourceToPlay = GetAndBindAudioSource_FromQueueToSoundinf(soundinf);
        _SourceToPlay.PlayOneShot(soundinf.Sounds[0]);
        _SourceToPlay.volume = _Volume * soundinf.Volume;
        if (!soundinf.Loop) { WaitForRecycle(soundinf, _SourceToPlay, soundinf.Sounds[0].length); }
        return _SourceToPlay;
    }
    public AudioSource PlayRamMixSound(SoundInf soundinf, float _Volume = 0)
    {
        int Count = Mathf.Clamp(soundinf.MixCount, 1, soundinf.Sounds.Count);
        List<AudioClip> TheRestClips = new List<AudioClip>(soundinf.Sounds);
        List<float> PlayingClips = new List<float>();
        AudioSource _SourceToPlay;
        if (VirtulAudioSources.Count == 0) { NewEmptyAudioSource_ToQueue(); }
        _SourceToPlay = GetAndBindAudioSource_FromQueueToSoundinf(soundinf);
        for (int i = 0; i < Count; i++)
        {
            int RandomIndex = UnityEngine.Random.Range(0, TheRestClips.Count);
            float SecVolume = UnityEngine.Random.Range(0, soundinf.Volume * 0.5f * MainVolume);
            _SourceToPlay.PlayOneShot(TheRestClips[RandomIndex], SecVolume);
            PlayingClips.Add(TheRestClips[RandomIndex].length);
        }
        _SourceToPlay.volume = _Volume * soundinf.Volume;
        if (!soundinf.Loop) { WaitForRecycle(soundinf, _SourceToPlay, PlayingClips.Max()); }
        return _SourceToPlay;
    }
    public AudioSource PlayRamSound(SoundInf soundinf, float _Volume = 0)
    {
        int Count = Mathf.Clamp(soundinf.MixCount, 1, soundinf.Sounds.Count);
        float lastVolume = 0f;
        AudioSource _SourceToPlay;
        List<AudioClip> TheRestClips = new List<AudioClip>(soundinf.Sounds);
        List<float> PlayingClips = new List<float>();
        if(VirtulAudioSources.Count == 0){NewEmptyAudioSource_ToQueue();}
        _SourceToPlay= GetAndBindAudioSource_FromQueueToSoundinf(soundinf);
        for (int i = 0; i < Count; i++)
        {
            int RandomIndex = UnityEngine.Random.Range(0, TheRestClips.Count);
            float SecVolume = UnityEngine.Random.Range(0, Mathf.Max(0, soundinf.Volume - lastVolume) * MainVolume);
            lastVolume = SecVolume;
            _SourceToPlay.PlayOneShot(TheRestClips[RandomIndex], SecVolume);
            PlayingClips.Add(TheRestClips[RandomIndex].length);
            TheRestClips.RemoveAt(RandomIndex);
        }
        _SourceToPlay.volume = _Volume * soundinf.Volume;
        if (!soundinf.Loop) { WaitForRecycle(soundinf, _SourceToPlay, PlayingClips.Max()); }
        return _SourceToPlay;
    }
    private void WaitForRecycle(SoundInf soundinf, AudioSource _SourceToPlay)
    {
        OnFadeAudioSource.Add(_SourceToPlay, StartCoroutine(IE_WaitRecycle(_SourceToPlay, _SourceToPlay.clip.length, soundinf)));
    }
    private void WaitForRecycle(SoundInf soundinf, AudioSource _SourceToPlay,float WaitingTime)
    {
        OnFadeAudioSource.Add(_SourceToPlay, StartCoroutine(IE_WaitRecycle(_SourceToPlay, WaitingTime, soundinf)));
    }

    #region ��Ч���뵭��Э��
    private IEnumerator IE_CrossTrans_Only1(SoundInf soundinf)
    {
        AudioSource NewSoundFade;
        NewSoundFade = PlayMainSound(soundinf);
        List<AudioSource> CrossFade = AudioSourceInSoundInf[soundinf];
        CrossFade.Remove(NewSoundFade);
        if (CrossFade.Count > 0)
        {
            while (AudioSourceInSoundInf[soundinf].Count > 0 || NewSoundFade.volume < soundinf.Volume)
            {
                float UsingTime = 0f;
                List<AudioSource> OtherSource = new List<AudioSource>(AudioSourceInSoundInf[soundinf]);
                OtherSource.Remove(NewSoundFade);
                foreach (AudioSource SoundFade in CrossFade)
                {
                    SoundFade.volume = Mathf.Max(0, SoundFade.volume - UsingTime / soundinf.Time);
                }
                CheckAndRecycleMuteAudioSurce(CrossFade, soundinf);
                NewSoundFade.volume = Mathf.Min(soundinf.Volume, UsingTime / soundinf.Time * soundinf.Volume);
                UsingTime += Time.deltaTime;
                yield return null;
            }
        }
        else { StartCoroutine(IE_FadeIn(NewSoundFade, soundinf)); }
    }
    private IEnumerator IE_CrossTransSound_M(SoundInf soundinf)
    {
        AudioSource NewSoundFade;
        NewSoundFade = PlayRamMixSound(soundinf);
        List<AudioSource> CrossFade = AudioSourceInSoundInf[soundinf];
        CrossFade.Remove(NewSoundFade);
        if (CrossFade.Count > 0)
        {
            while (AudioSourceInSoundInf[soundinf].Count > 0 || NewSoundFade.volume < soundinf.Volume)
            {
                float UsingTime = 0f;
                List<AudioSource> OtherSource = new List<AudioSource>(AudioSourceInSoundInf[soundinf]);
                OtherSource.Remove(NewSoundFade);
                foreach (AudioSource SoundFade in CrossFade)
                {
                    SoundFade.volume = Mathf.Max(0, SoundFade.volume - UsingTime / soundinf.Time);
                }
                CheckAndRecycleMuteAudioSurce(CrossFade, soundinf);
                NewSoundFade.volume = Mathf.Min(soundinf.Volume, UsingTime / soundinf.Time * soundinf.Volume);
                UsingTime += Time.deltaTime;
                yield return null;
            }
        }
        else { StartCoroutine(IE_FadeIn(NewSoundFade, soundinf)); }
    }
    private IEnumerator IE_CrossTrans_Random(SoundInf soundinf)
    {
        AudioSource NewSoundFade;
        NewSoundFade=PlayRamSound(soundinf);
        List<AudioSource> CrossFade = AudioSourceInSoundInf[soundinf];
        CrossFade.Remove(NewSoundFade);
        if (CrossFade.Count > 0)
        {
            while (CrossFade.Count > 0 || NewSoundFade.volume < soundinf.Volume)
            {
                float UsingTime = 0f;
                foreach (AudioSource SoundFade in CrossFade)
                {
                    SoundFade.volume = Mathf.Max(0, SoundFade.volume - UsingTime / soundinf.Time);
                }
                CheckAndRecycleMuteAudioSurce(CrossFade, soundinf);
                NewSoundFade.volume = Mathf.Min(soundinf.Volume, UsingTime / soundinf.Time * soundinf.Volume);
                UsingTime += Time.deltaTime;
                yield return null;
            }
        }
        else{StartCoroutine(IE_FadeIn(NewSoundFade,soundinf));}
    }
    private void CheckAndRecycleMuteAudioSurce(List<AudioSource> Sources, SoundInf soundinf)
    {
        List<AudioSource> toRecycle = new List<AudioSource>();
        foreach (AudioSource SoundFade in Sources)
        {
            if (SoundFade.volume <= 0)
            {
                StopCoroutine(OnFadeAudioSource[SoundFade]);
                toRecycle.Add(SoundFade);
            }
        }
        foreach (AudioSource SoundFade in toRecycle)
        {
            RecycleFromInfToQueue(SoundFade, soundinf);
        }
        
    }
    private AudioSource GetAudioSource(SoundInf soundinf)
    {
        AudioSource NewSoundFade;
        if (VirtulAudioSources.Count > 0)
        {
            NewSoundFade = GetAndBindAudioSource_FromQueueToSoundinf(soundinf);
        }
        else
        {
            NewSoundFade = NewEmptyAudioSource_ToQueue();
            NewSoundFade = GetAndBindAudioSource_FromQueueToSoundinf(soundinf);
        }
        return NewSoundFade;
    }
    private AudioSource GetAndBindAudioSource_FromQueueToSoundinf(SoundInf soundinf)
    {
        AudioSource NewSoundFade = VirtulAudioSources.Dequeue();
        NewSoundFade.enabled = true;
        AudioSourceInSoundInf[soundinf].Add(NewSoundFade);
        NewSoundFade = BindAudioSource(soundinf, NewSoundFade);
        return NewSoundFade;
    }
    private AudioSource NewEmptyAudioSource_ToQueue()
    {
        GameObject newgameobject = GameObject.Instantiate(DynamicSourceTemplate);
        newgameobject.name = "DynamicSoundSource" + (VirtulAudioSources.Count + 1);
        AudioSource audioSource = newgameobject.GetComponent<AudioSource>();
        audioSource.clip = null; 
        audioSource.playOnAwake = false; 
        audioSource.volume = 0.0f;
        audioSource.enabled =false;
        VirtulAudioSources.Enqueue(audioSource);
        return audioSource;
    }
    private AudioSource BindAudioSource(SoundInf soundinf, AudioSource NewSoundFade)
    {
        NewSoundFade.gameObject.transform.position = soundinf.Source.transform.position;
        NewSoundFade.gameObject.transform.parent = soundinf.Source.transform.parent;
        return NewSoundFade;
    }
    private AudioSource CleanAndBindAudioSource(SoundInf soundinf, AudioSource NewSoundFade)
    {
        NewSoundFade.transform.position = soundinf.Source.transform.position;
        NewSoundFade.transform.parent = soundinf.Source.transform.parent;
        NewSoundFade.volume = 0f;
        NewSoundFade.clip = null;
        NewSoundFade.Stop();
        return NewSoundFade;
    }
    private void WaitForRecycle()
    {

    }
    private IEnumerator IE_WaitRecycle(AudioSource WaitDeleteSource, float time,SoundInf soundinf)
    {
        yield return new WaitForSeconds(time);
        RecycleFromInfToQueue(WaitDeleteSource, soundinf);
    }
    private IEnumerator IE_StopFade(AudioSource FadeOutSource, float FadeTime,SoundInf soundinf)
    {
        float UsingTime = 0f;
        while (FadeOutSource.volume > 0)
        {
            FadeOutSource.volume = Mathf.Max(0, FadeOutSource.volume - UsingTime / FadeTime);
            UsingTime += Time.deltaTime;
            yield return null;
        }
        RecycleFromInfToQueue(FadeOutSource, soundinf);
    }
    private void RecycleFromInfToQueue(AudioSource RecycleSource, SoundInf soundinf)
    {
        OnFadeAudioSource.Remove(RecycleSource);
        AudioSourceInSoundInf[soundinf].Remove(RecycleSource);
        RecycleSource.gameObject.name = ("DynamicSoundSource" + (VirtulAudioSources.Count + 1));
        VirtulAudioSources.Enqueue(RecycleSource);
        RecycleSource.volume = 0f;
        RecycleSource.clip = null;
        RecycleSource.transform.parent = MyChildren;
        RecycleSource.transform.localPosition = Vector3.zero;
        RecycleSource.transform.rotation = Quaternion.identity;
        RecycleSource.enabled = false;
    }
    private IEnumerator IE_FadeIn(AudioSource SoundFade,SoundInf soundinf)
    {
        float UsingTime = 0f;
        while (SoundFade.volume < soundinf.Volume)
        {
            SoundFade.volume = Mathf.Min(soundinf.Volume, UsingTime / soundinf.Time * soundinf.Volume);
            UsingTime += Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator IE_FadeOut(SoundInf soundinf)
    {

        float UsingTime = 0f;
        while (AudioSourceInSoundInf[soundinf].Count > 0)
        {
            foreach (AudioSource SoundFade in AudioSourceInSoundInf[soundinf])
            {
                SoundFade.volume = Mathf.Max(0, SoundFade.volume - UsingTime / soundinf.Time);
            }
            CheckAndRecycleMuteAudioSurce(AudioSourceInSoundInf[soundinf],soundinf);
            yield return null;
        }
    }
    #endregion
    public void CrossTrans_Only1(SoundInf Inf)
    {
        StartCoroutine(IE_CrossTrans_Only1(Inf));
    }
    public void CrossTrans_1plusN(SoundInf Inf)
    {
        StartCoroutine(IE_CrossTransSound_M(Inf));
    }
    public void CrossTrans_RandomN(SoundInf Inf)
    {
        StartCoroutine(IE_CrossTrans_Random(Inf));
    }
    public void FadeInVolume(AudioSource source,SoundInf Inf)
    {
        StartCoroutine(IE_FadeIn(source,Inf));
    }
    public void FadeOutVolume(SoundInf Inf)
    {
        StartCoroutine(IE_FadeOut(Inf));
    }
    #endregion
}

