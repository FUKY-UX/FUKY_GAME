using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;
using System.Linq;
using Unity.Collections;
using System.Collections;


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
public class ItemDefaultSound : AttrBoard
{
    [Tooltip("声音发出来的地方")]
    public AudioSource MyAduioSource;
    [Tooltip("和其他物品碰撞的声音")]
    public AudioClip[] _OnHitSound;
    [Tooltip("抓住物品时发出的声音")]
    public AudioClip[] _OnGrabSound;
    [Tooltip("扔掉物品时发出的声音")]
    public AudioClip[] _OnReleaseSound;

    [HideInInspector] public string[] _OnHitSound_Str;
    [HideInInspector] public string[] _OnGrabSound_Str;
    [HideInInspector] public string[] _OnReleaseSound_Str;

}

[Serializable]
public class ItemSound : AttrBoard
{
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
    [Header("基础音效")]
    public ItemDefaultSound Sound;
    
}
public abstract class ItemState : InteractedItemState
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
/// <summary>
/// 可抓取物体的基础状态类，一般作为默认状态使用，但是挺方便的也可以进一步修改添加逻辑作为其他状态使用
/// </summary>
public class GrabPhyItem_DefaultState : ItemState
{
    [Header("当前物品状态")]
    [ReadOnly]public ItemFSM _ItemFsm;
    public DefaultItemAttrBoard _DefAttrBoard;

    /// <summary>
    /// 可抓握物理物品的默认状态
    /// </summary>
    /// <param name="in_Fsm">不需要二次赋值，传递状态机而已</param>
    /// <param name="_defattrboard">如果需要更新默认状态的逻辑，可以创建一个继承类传进来</param>
    /// <param name="NeedExtrAttr">如果有任何拓展逻辑，建议与默认属性分开</param>
    public GrabPhyItem_DefaultState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, AttrBoard NeedExtrAttr = null)
    {
        _ItemFsm = in_Fsm;
        _DefAttrBoard = _defattrboard;
    }
    /// <summary>
    /// 在物品被抓取时默认的行为
    /// </summary>
    public override void OnGrab()
    {
        AudioManager2025.Instance.PlayRandomSound(_DefAttrBoard.Sound.MyAduioSource, _DefAttrBoard.Sound._OnGrabSound_Str);
    }
    /// <summary>
    /// 在物品被扔掉时时默认的行为
    /// </summary>
    public override void OnRelease()
    {
        AudioManager2025.Instance.PlayRandomSound(_DefAttrBoard.Sound.MyAduioSource, _DefAttrBoard.Sound._OnReleaseSound_Str);
    }
    /// <summary>
    /// 在物品与其他东西碰撞时的默认行为
    /// </summary>
    public override void OnRidigibodyEnter(Collision collision)
    {
        AudioManager2025.Instance.PlayRandomSound(_DefAttrBoard.Sound.MyAduioSource, _DefAttrBoard.Sound._OnHitSound_Str);
    }
}


public class GrabInteractedItemOrigin : MonoBehaviour
{
    [Header("当前物体状态")]
    public ItemFSM Item_FSM;
    [Header("物体的基础属性")]
    public DefaultItemAttrBoard DefaultAttr;
    public GrabPhyItem_DefaultState DefaultState;
    public bool CurrCanPickAble = true;
    public float PickUpCoolDown = 2f;
    public quaternion ItemAdjRotation = quaternion.identity;
    
    private float LastDropTime_Delay = 0;
    private float ItemMass;
    private float ItemDrag;
    private float ItemAngleDrag;

    /// <summary>
    /// 请在Awake中调用该方法，在调用前可以调整参数
    /// 可交互物体基类的Init，会初始化状态机和记录该物体的原始物理设置，
    /// 默认带一个默认属性版和默认状态，默认状态可以用类似base.DefaultState = new...
    /// 的方式去覆写
    /// </summary>
    protected void InitItemStateAndPhy()
    {
        Item_FSM = new ItemFSM(DefaultAttr);
        Item_FSM.AddState(ItemState_Type.Default, new GrabPhyItem_DefaultState(Item_FSM, DefaultAttr));
        Item_FSM.SwitchState(ItemState_Type.Default);

        // 确保 Rigidbody 存在并配置
        if (DefaultAttr.Phy._rigidbody == null)
        {
            DefaultAttr.Phy._rigidbody = GetComponent<Rigidbody>();
            if (DefaultAttr.Phy._rigidbody == null)
            {
                DefaultAttr.Phy._rigidbody = gameObject.AddComponent<Rigidbody>();
            }
        }

        // 确保 Collider 存在（假设需要 BoxCollider，按需修改）
        if (DefaultAttr.Phy._collider == null)
        {
            DefaultAttr.Phy._collider = GetComponent<Collider>();
            if (DefaultAttr.Phy._collider == null)
            {
                // 根据需求选择 Collider 类型（如 BoxCollider、SphereCollider）
                DefaultAttr.Phy._collider = gameObject.AddComponent<BoxCollider>();
            }
        }
        ItemMass = DefaultAttr.Phy._rigidbody.mass;
        ItemDrag = DefaultAttr.Phy._rigidbody.drag;
        ItemAngleDrag = DefaultAttr.Phy._rigidbody.angularDrag;
    }

    /// <summary>
    /// 请在Start时调用该方法，确保audiomanger2025已初始化结束
    /// 可交互物体基类注册音效的方法
    /// 自动把列表里的audioclip转成string的形式，并检查有没有声源
    /// 没有的话会从audiomanger2025那毛一个配置好的默认状态的声源
    /// </summary>
    public void registerAduioList()
    {
        if (DefaultAttr.Sound.MyAduioSource == null)
        {
            DefaultAttr.Sound.MyAduioSource = gameObject.AddComponent<AudioSource>();
            CopyAudioSourceProperties(AudioManager2025.Instance.UniversalAudioSoure, DefaultAttr.Sound.MyAduioSource);
        }
        // 建立音效字符串列表
        DefaultAttr.Sound._OnGrabSound_Str = ConvertClipsToNames(DefaultAttr.Sound._OnGrabSound);
        DefaultAttr.Sound._OnReleaseSound_Str = ConvertClipsToNames(DefaultAttr.Sound._OnReleaseSound);
        DefaultAttr.Sound._OnHitSound_Str = ConvertClipsToNames(DefaultAttr.Sound._OnHitSound);
    }
    private string[] ConvertClipsToNames(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return new[] { "Error" };

        return clips.Select(clip => clip?.name ?? "null").ToArray();
    }
    public void Update()
    {
        Item_FSM.OnUpdate();
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
        Item_FSM.OnFixUpdate();
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (!CurrCanPickAble) { return; }
        Item_FSM.OnRidigibodyEnter(collision);
    }
    public void OnCollisionStay(Collision collision)
    {
        if (!CurrCanPickAble) { return; }
        Item_FSM.OnRidigibodyStay(collision);
    }
    public void OnCollisionExit(Collision collision)
    {
        if (!CurrCanPickAble) { return; }
        Item_FSM.OnRidigibodyExit(collision);
    }
    public void OnTriggerEnter(Collider other)
    {
        if (!CurrCanPickAble) { return; }
        Item_FSM.OnTriggerEnter(other);
    }
    public void OnTriggerExit(Collider other)
    {
        if (!CurrCanPickAble) { return; }
        Item_FSM.OnTriggerExit(other);
    }
    public void OnTriggerStay(Collider other)
    {
        if (!CurrCanPickAble) { return; }
        Item_FSM.OnTriggerStay(other);
    }
    /// <summary>
    /// 可交互物体基类的冷却型丢弃方法，大白话:用这种方式丢掉物体会让物体进入一个在Inspector中设置的冷却时间
    /// </summary>
    public void DropItem_DelayPick()
    {
        LastDropTime_Delay = Time.time;
        CurrCanPickAble =false;
    }

    public void Change_GrabRigidbody(Rigidbody Hand)
    {
        DefaultAttr.Phy._rigidbody.mass = Hand.mass;
        DefaultAttr.Phy._rigidbody.drag = Hand.drag;
        DefaultAttr.Phy._rigidbody.angularDrag = Hand.angularDrag;
    }

    public void ReSet_GrabRigidbody()
    {
        DefaultAttr.Phy._rigidbody.mass = ItemMass;
        DefaultAttr.Phy._rigidbody.drag = ItemDrag;
        DefaultAttr.Phy._rigidbody.angularDrag = ItemAngleDrag;
    }

    public void CopyAudioSourceProperties(AudioSource source, AudioSource target)
    {
        target.volume = source.volume;
        target.pitch = source.pitch;
        target.spatialBlend = source.spatialBlend;
        target.loop = source.loop;
        target.playOnAwake = source.playOnAwake;
        target.outputAudioMixerGroup = source.outputAudioMixerGroup;
        target.minDistance = source.minDistance;
        target.maxDistance = source.maxDistance;
        target.rolloffMode = source.rolloffMode;
    }
}
