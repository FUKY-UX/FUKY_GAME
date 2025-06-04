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

public class DefaultItemState : ItemState
{
    protected ItemFSM _MyFsm;
    public DefaultItemAttrBoard _DefAttrBoard;

    public DefaultItemState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard)
    {
        _MyFsm = in_Fsm;
        _DefAttrBoard = _defattrboard as DefaultItemAttrBoard;
    }
    public override void OnGrab()
    {
    }
    public override void OnRelease()
    {
    }
    public override void OnRidigibodyEnter(Collision collision)
    {
    }
}


public class InteractedItemOrigin : MonoBehaviour
{
    public ItemFSM _MyFsm;
    [Header("物体的基础属性")]
    public DefaultItemAttrBoard Default;
    public bool CurrCanPickAble = true;
    public float PickUpCoolDown = 2f;
    public quaternion ItemAdjRotation = quaternion.identity;
    private float LastDropTime_Delay = 0;
    private float ItemMass;
    private float ItemDrag;
    private float ItemAngleDrag;

    private void Awake()
    {
        _MyFsm = new ItemFSM(Default);
        _MyFsm.AddState(ItemState_Type.Default, new DefaultItemState(_MyFsm, Default));
        _MyFsm.SwitchState(ItemState_Type.Default);
        ItemMass = Default.Phy._rigidbody.mass;
        ItemDrag = Default.Phy._rigidbody.drag;
        ItemAngleDrag = Default.Phy._rigidbody.angularDrag;

    }

    public void Update()
    {
        _MyFsm.OnUpdate();
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

    public void Change_GrabRigidbody(Rigidbody Hand)
    {
        Default.Phy._rigidbody.mass = Hand.mass;
        Default.Phy._rigidbody.drag = Hand.drag;
        Default.Phy._rigidbody.angularDrag = Hand.angularDrag;
    }

    public void ReSet_GrabRigidbody()
    {
        Default.Phy._rigidbody.mass = ItemMass;
        Default.Phy._rigidbody.drag = ItemDrag;
        Default.Phy._rigidbody.angularDrag = ItemAngleDrag;
    }
}
