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
    [Tooltip("�����������ĵط�")]
    public AudioSource MyAduioSource;
    [Tooltip("��������Ʒ��ײ������")]
    public AudioClip[] _OnHitSound;
    [Tooltip("ץס��Ʒʱ����������")]
    public AudioClip[] _OnGrabSound;
    [Tooltip("�ӵ���Ʒʱ����������")]
    public AudioClip[] _OnReleaseSound;

    [HideInInspector] public string[] _OnHitSound_Str;
    [HideInInspector] public string[] _OnGrabSound_Str;
    [HideInInspector] public string[] _OnReleaseSound_Str;

}

[Serializable]
public class ItemSound : AttrBoard
{
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
    [Header("������Ч")]
    public ItemDefaultSound Sound;
    
}
public abstract class ItemState : InteractedItem
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
/// ��ץȡ����Ļ���״̬�࣬һ����ΪĬ��״̬ʹ�ã�����ͦ�����Ҳ���Խ�һ���޸�����߼���Ϊ����״̬ʹ��
/// </summary>
public class GrabPhyItem_DefaultState : ItemState
{
    [Header("��ǰ��Ʒ״̬")]
    [ReadOnly]public ItemFSM _ItemFsm;
    public DefaultItemAttrBoard _DefAttrBoard;

    /// <summary>
    /// ��ץ��������Ʒ��Ĭ��״̬
    /// </summary>
    /// <param name="in_Fsm">����Ҫ���θ�ֵ������״̬������</param>
    /// <param name="_defattrboard">�����Ҫ����Ĭ��״̬���߼������Դ���һ���̳��ഫ����</param>
    /// <param name="NeedExtrAttr">������κ���չ�߼���������Ĭ�����Էֿ�</param>
    public GrabPhyItem_DefaultState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, AttrBoard NeedExtrAttr = null)
    {
        _ItemFsm = in_Fsm;
        _DefAttrBoard = _defattrboard;
    }
    /// <summary>
    /// ����Ʒ��ץȡʱĬ�ϵ���Ϊ
    /// </summary>
    public override void OnGrab()
    {
        AudioManager2025.Instance.PlayRandomSound(_DefAttrBoard.Sound.MyAduioSource, _DefAttrBoard.Sound._OnGrabSound_Str);
    }
    /// <summary>
    /// ����Ʒ���ӵ�ʱʱĬ�ϵ���Ϊ
    /// </summary>
    public override void OnRelease()
    {
        AudioManager2025.Instance.PlayRandomSound(_DefAttrBoard.Sound.MyAduioSource, _DefAttrBoard.Sound._OnReleaseSound_Str);
    }
    /// <summary>
    /// ����Ʒ������������ײʱ��Ĭ����Ϊ
    /// </summary>
    public override void OnRidigibodyEnter(Collision collision)
    {
        AudioManager2025.Instance.PlayRandomSound(_DefAttrBoard.Sound.MyAduioSource, _DefAttrBoard.Sound._OnHitSound_Str);
    }
}


public class GrabInteractedItemOrigin : MonoBehaviour
{
    [Header("��ǰ����״̬")]
    public ItemFSM Item_FSM;
    [Header("����Ļ�������")]
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
    /// ����Awake�е��ø÷������ڵ���ǰ���Ե�������
    /// �ɽ�����������Init�����ʼ��״̬���ͼ�¼�������ԭʼ�������ã�
    /// Ĭ�ϴ�һ��Ĭ�����԰��Ĭ��״̬��Ĭ��״̬����������base.DefaultState = new...
    /// �ķ�ʽȥ��д
    /// </summary>
    protected void InitItemStateAndPhy()
    {
        Item_FSM = new ItemFSM(DefaultAttr);
        Item_FSM.AddState(ItemState_Type.Default, new GrabPhyItem_DefaultState(Item_FSM, DefaultAttr));
        Item_FSM.SwitchState(ItemState_Type.Default);

        // ȷ�� Rigidbody ���ڲ�����
        if (DefaultAttr.Phy._rigidbody == null)
        {
            DefaultAttr.Phy._rigidbody = GetComponent<Rigidbody>();
            if (DefaultAttr.Phy._rigidbody == null)
            {
                DefaultAttr.Phy._rigidbody = gameObject.AddComponent<Rigidbody>();
            }
        }

        // ȷ�� Collider ���ڣ�������Ҫ BoxCollider�������޸ģ�
        if (DefaultAttr.Phy._collider == null)
        {
            DefaultAttr.Phy._collider = GetComponent<Collider>();
            if (DefaultAttr.Phy._collider == null)
            {
                // ��������ѡ�� Collider ���ͣ��� BoxCollider��SphereCollider��
                DefaultAttr.Phy._collider = gameObject.AddComponent<BoxCollider>();
            }
        }
        ItemMass = DefaultAttr.Phy._rigidbody.mass;
        ItemDrag = DefaultAttr.Phy._rigidbody.drag;
        ItemAngleDrag = DefaultAttr.Phy._rigidbody.angularDrag;
    }

    /// <summary>
    /// ���ڿ�ʼʱ���ø÷�����ȷ��audiomanger2025�ѳ�ʼ������
    /// �ɽ����������ע����Ч�ķ���
    /// �Զ����б����audioclipת��string����ʽ���������û����Դ
    /// û�еĻ����audiomanger2025��ëһ�����úõ�Ĭ��״̬����Դ
    /// </summary>
    public void registerAduioList()
    {
        if (DefaultAttr.Sound.MyAduioSource == null)
        {
            DefaultAttr.Sound.MyAduioSource = gameObject.AddComponent<AudioSource>();
            CopyAudioSourceProperties(AudioManager2025.Instance.UniversalAudioSoure, DefaultAttr.Sound.MyAduioSource);
        }
        // ������Ч�ַ����б�
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
    /// �ɽ�������������ȴ�Ͷ�����������׻�:�����ַ�ʽ������������������һ����Inspector�����õ���ȴʱ��
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
