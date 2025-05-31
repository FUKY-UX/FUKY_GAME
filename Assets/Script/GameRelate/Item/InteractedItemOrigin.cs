using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;


public enum DefaultItemSound
{
    [Tooltip("抓住物品时发出的声音")]
    Grabing,
    [Tooltip("扔掉物品时发出的声音")]
    Throwing,
    [Tooltip("和其他物品碰撞的声音")]
    Knock
}
[Serializable]
public class ItemPhysics:AttrBoard
{
    public Rigidbody _rigidbody;
    public Collider _collider;
    public float GrabTimeFactor = 1f;
    public Vector3 RubFactor;
    public float RubStrength = 1f;
}
[Serializable]
public class ItemSound : AttrBoard
{
    [Tooltip("Key【物品状态】—Value【状态音效】")]
    public SerializableDictionary<DefaultItemSound, SoundInf> Sounds;
    [Tooltip("默认的声源，每个物体上至少有一个")]
    public Transform AudioSource;
    [Tooltip("该物体音效的音量")]
    public float Volume = 1f;
    [Tooltip("音效播放的冷却，主要影响碰撞音效")]
    public float NoiseCd = 1f;
    [Tooltip("音效播放的冷却随机量，主要影响碰撞音效")]
    public float NoiseCdOffset = 1f;
    [HideInInspector]
    public float V_LastSoundPlay;
    [HideInInspector]
    public bool V_Playable = true;

}

[Serializable]
public class DefaultItemAttrBoard : AttrBoard
{
    [Header("基础物理属性")]
    public ItemPhysics Phy;
    [Header("基础音效属性")]
    public ItemSound Sound;
}
public abstract class DefaultItemState : InteractedItem
{
    public virtual void OnEnter()
    {
    }
    public virtual void OnExit()
    {

    }
    public virtual void OnFixUpdate()
    {
    }
    public virtual void OnUpdate()
    {

    }
    public virtual void OnGrab()
    {
    }
    public virtual void OnRelease() { }
    public virtual void Grabing() { }
    public virtual void OnRidigibodyEnter(Collision collision) { }
    public virtual void OnRidigibodyStay(Collision collision) { }
    public virtual void OnRidigibodyExit(Collision collision) { }
    public virtual void OnTriggerEnter(Collider collider) { }
    public virtual void OnTriggerExit(Collider collider) { }
    public virtual void OnTriggerStay(Collider collider) { }

}
public class InteractedItemOrigin : MonoBehaviour
{
    public ItemFSM _MyFsm;
    [Header("物体的基础属性")]
    public DefaultItemAttrBoard Default;
    public bool CurrCanPickAble = true;
    public float PickUpCoolDown = 2f;
    private float LastDropTime_Delay = 0;
    private quaternion CustomRotateOffset = quaternion.identity;
    private void Awake()
    {
        if (AudioManager.instance.AudioSourceInSoundInf == null)
        {
            AudioManager.instance.AudioSourceInSoundInf = new SerializableDictionary<SoundInf, List<AudioSource>>();
        }
        _MyFsm = new ItemFSM(Default);
        if (Default.Sound.Sounds == null)
        {
            Default.Sound.Sounds = new SerializableDictionary<DefaultItemSound, SoundInf>();
        }
        if (Default.Sound.AudioSource == null) { Debug.LogError("不要忘了给" + this.gameObject.name + "的音效指定声音源头"); }
        Array AllSoundType = DefaultItemSound.GetValues(typeof(DefaultItemSound));
        if (Default.Sound.Sounds.Count == 0)
        {
            foreach (DefaultItemSound soundType in AllSoundType)
            {
                if (!Default.Sound.Sounds.ContainsKey(soundType))
                {
                    Default.Sound.Sounds[soundType] = new SoundInf(this.transform);
                    Default.Sound.Sounds[soundType].Sounds.Add((AudioClip)Resources.Load("Game/Audio/System/Error"));
                    Debug.LogWarning(this.gameObject.name + "物品的默认音效列表是空的，临时配了个错误提示音");
                }
            }
        }
        if (Default.Sound.Sounds.Count > 0)
        {
            foreach (DefaultItemSound soundType in AllSoundType)
            {
                SoundInf soundInf = Default.Sound.Sounds[soundType];
                Default.Sound.Sounds[soundType] = soundInf;

                if (soundInf.Source == null) { soundInf.Source = this.transform; }
                if (soundInf.Sounds.Count == 0)
                {
                    soundInf.Sounds = new List<AudioClip>();
                    soundInf.Sounds.Add((AudioClip)Resources.Load("Game/Audio/System/Error"));
                    Debug.LogWarning(this.gameObject.name + "在" + soundType + "状态下的音效是空的");
                }
            }

        }

        foreach (SoundInf soundInf in Default.Sound.Sounds.Values)
        {
            AudioManager.instance.AudioSourceInSoundInf.Add(soundInf,new List<AudioSource>());
        }
    }

    public void Update()
    {
        _MyFsm.OnUpdate();
        if (!Default.Sound.V_Playable)
        {
            if (Default.Sound.V_LastSoundPlay > Default.Sound.NoiseCdOffset)
            {
                Default.Sound.V_Playable = true;
                Default.Sound.NoiseCdOffset = Default.Sound.NoiseCd + UnityEngine.Random.Range(-0.5f, 0.5f);
            }
            Default.Sound.V_LastSoundPlay += Time.deltaTime;
            return;
        }
        if (!CurrCanPickAble)
        {
            if(Time.time - LastDropTime_Delay > PickUpCoolDown)
            {
                CurrCanPickAble =true;
            }
        }
    }
    public void FixedUpdate()
    {
        if (!CurrCanPickAble) { return;}
        _MyFsm.OnFixUpdate();
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (!CurrCanPickAble) { return; }
        _MyFsm.OnRidigibodyEnter(collision);
    }
    private void OnCollisionStay(Collision collision)
    {
        if (!CurrCanPickAble) { return; }
        _MyFsm.OnRidigibodyStay(collision);
    }
    public void OnCollisionExit(Collision collision)
    {
        if (!CurrCanPickAble) { return; }
        _MyFsm.OnRidigibodyExit(collision);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!CurrCanPickAble) { return; }
        _MyFsm.OnTriggerEnter(other);
    }
    private void OnTriggerExit(Collider other)
    {
        if (!CurrCanPickAble) { return; }
        _MyFsm.OnTriggerExit(other);
    }
    private void OnTriggerStay(Collider other)
    {
        if (!CurrCanPickAble) { return; }
        _MyFsm.OnTriggerStay(other);
    }

    public void DropItem_DelayPick()
    {
        LastDropTime_Delay = Time.time;
        CurrCanPickAble =false;
    }
}
