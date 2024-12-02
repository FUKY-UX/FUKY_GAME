using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;
    public AudioSource CamAudioSource;
    private Dictionary<string, AudioClip> dictAudio;

    public float AudioSoundStrength = 1f;
    private void Awake()
    {
        instance = this;
        dictAudio = new Dictionary<string, AudioClip>();
    }
    private void Start()
    {
        PlayBGM(MusicAndSound_Path.instance.BGM);

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

    public void PlayBGM(string name,float Volume = 1.0f)
    {
        CamAudioSource.Stop();
        CamAudioSource.clip = GetAudio(name);
        CamAudioSource.Play();
    }

    public void PlaySound(AudioSource _audioSource,string path,float volume = 1.0f)
    {
        _audioSource.PlayOneShot(LoadAudio(path), volume * AudioSoundStrength);
    }
    public void CrossSound(AudioSource _CurrAudioSource, AudioSource _DestAudioSource, float volume = 1.0f,float UsingTime = 1f)
    {
        if (_CurrAudioSource.isPlaying)
        {
            FadeOut(_CurrAudioSource, UsingTime, _CurrAudioSource.volume);
            FadeIn(_DestAudioSource, UsingTime, _DestAudioSource.volume, volume * AudioSoundStrength);
        }
    }

    public void PlayRamMixSound(AudioSource _audioSource, string MainPath, string[] SecPath, float volume = 1.0f)
    {
        _audioSource.PlayOneShot(LoadAudio(MainPath), volume);
        for (int i = 0; i < SecPath.Length; i++)
        {
            float SecVolume = Random.Range(0, volume * 0.5f * AudioSoundStrength);
            _audioSource.PlayOneShot(LoadAudio(SecPath[i]), SecVolume);
        }

    }

    public void PlayRamSound(AudioSource _audioSource ,string[] SecPath, float volume = 1.0f,int Audios = 0)
    {
        int PlayAudios = Mathf.Clamp(Audios, 0, SecPath.Length);
        float lastVolume = 0f;
        List<string> audioList = new List<string>(SecPath);
        for (int i = 0; i < PlayAudios; i++)
        {
            int index = Random.Range(0,audioList.Count);
            float SecVolume = Random.Range(0, 
                Mathf.Max(0,volume - lastVolume) * AudioSoundStrength);
            lastVolume = SecVolume;
            _audioSource.PlayOneShot(LoadAudio(SecPath[index]), SecVolume);
            audioList.RemoveAt(index);
        }
    }



    public void FadeIn(AudioSource _audioSource, float fadeInDuration,float Currvolume, float Destvolume)
    {
        StartCoroutine(FadeVolume(_audioSource, Currvolume, Destvolume, fadeInDuration));
    }

    public void FadeOut(AudioSource _audioSource, float fadeOutDuration, float Currvolume)
    {
        StartCoroutine(FadeVolume(_audioSource, Currvolume, 0f, fadeOutDuration));
    }

    private IEnumerator FadeVolume(AudioSource source, float startVolume, float targetVolume, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            source.volume = Mathf.Lerp(startVolume, targetVolume, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        source.volume = targetVolume;
    }
}

