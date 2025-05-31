using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;


public enum DefaultItemSound
{
    [Tooltip("ץס��Ʒʱ����������")]
    Grabing,
    [Tooltip("�ӵ���Ʒʱ����������")]
    Throwing,
    [Tooltip("��������Ʒ��ײ������")]
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
    [Tooltip("Key����Ʒ״̬����Value��״̬��Ч��")]
    public SerializableDictionary<DefaultItemSound, SoundInf> Sounds;
    [Tooltip("Ĭ�ϵ���Դ��ÿ��������������һ��")]
    public Transform AudioSource;
    [Tooltip("��������Ч������")]
    public float Volume = 1f;
    [Tooltip("��Ч���ŵ���ȴ����ҪӰ����ײ��Ч")]
    public float NoiseCd = 1f;
    [Tooltip("��Ч���ŵ���ȴ���������ҪӰ����ײ��Ч")]
    public float NoiseCdOffset = 1f;
    [HideInInspector]
    public float V_LastSoundPlay;
    [HideInInspector]
    public bool V_Playable = true;

}

[Serializable]
public class DefaultItemAttrBoard : AttrBoard
{
    [Header("������������")]
    public ItemPhysics Phy;
    [Header("������Ч����")]
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
    [Header("����Ļ�������")]
    public DefaultItemAttrBoard Default;
    public bool CurrCanPickAble = true;
    public float PickUpCoolDown = 2f;
    public quaternion CustomRotateOffset = quaternion.identity;
    private float LastDropTime_Delay = 0;
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
        if (Default.Sound.AudioSource == null) { Debug.LogError("��Ҫ���˸�" + this.gameObject.name + "����Чָ������Դͷ"); }
        Array AllSoundType = DefaultItemSound.GetValues(typeof(DefaultItemSound));
        if (Default.Sound.Sounds.Count == 0)
        {
            foreach (DefaultItemSound soundType in AllSoundType)
            {
                if (!Default.Sound.Sounds.ContainsKey(soundType))
                {
                    Default.Sound.Sounds[soundType] = new SoundInf(this.transform);
                    Default.Sound.Sounds[soundType].Sounds.Add((AudioClip)Resources.Load("Game/Audio/System/Error"));
                    Debug.LogWarning(this.gameObject.name + "��Ʒ��Ĭ����Ч�б��ǿյģ���ʱ���˸�������ʾ��");
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
                    Debug.LogWarning(this.gameObject.name + "��" + soundType + "״̬�µ���Ч�ǿյ�");
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
