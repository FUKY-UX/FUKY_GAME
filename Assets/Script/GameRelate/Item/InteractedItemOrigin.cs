using UnityEngine;
using Item_FSM;
using System;

[Serializable]
public class DefaultItemAttrBoard : AttrBoard
{
    #region 物品的引擎属性
    [Header("物品引擎属性")]
    public Rigidbody _rigidbody;
    public Collider _collider;
    public Collision HitMe;
    public float GrabTimeFactor = 1f;
    public Vector3 RubFactor;
    public float RubStrength = 1f;
    #endregion
    #region 音效播放基础
    [Header("物品音效")]
    public AudioSource _audiosource;
    public float V_CoolDown = 1f;
    public float V_Voulme = 1f;
    public bool V_Playable = true;
    public float V_CoolDownOffset = 1f;
    public float V_LastSoundPlay;
    #endregion
}

public abstract class DefaultItemState : Item_FSM.InteractedItem
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
    public virtual void OnRelease()
    {
    }
    public virtual void OnColliderEnter()
    {
    }
    public virtual void OnColliderStay()
    {
    }
    public virtual void OnColliderExit()
    {
    }
    public virtual void Grabing()
    {
    }
}

public class InteractedItemOrigin : MonoBehaviour
{
    public ItemFSM _MyFsm;
    public DefaultItemAttrBoard _DefaultAttrBoard;
    private void Awake()
    {
        _MyFsm = new ItemFSM(_DefaultAttrBoard);
    }
    public void Update()
    {
        if (!_DefaultAttrBoard.V_Playable)
        {
            if (_DefaultAttrBoard.V_LastSoundPlay > _DefaultAttrBoard.V_CoolDownOffset)
            {
                _DefaultAttrBoard.V_Playable = true;
                _DefaultAttrBoard.V_CoolDownOffset = _DefaultAttrBoard.V_CoolDown + UnityEngine.Random.Range(-0.5f, 0.5f);
            }
            _DefaultAttrBoard.V_LastSoundPlay += Time.deltaTime;
            return;
        }
        if (_MyFsm.CurrentState != null)
        {
            _MyFsm.OnUpdate();
        }
    }
    public void FixedUpdate()
    {
        if (_MyFsm.CurrentState != null)
        {

            _MyFsm.OnFixUpdate();
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        _DefaultAttrBoard.HitMe = collision;
        if (_MyFsm.CurrentState != null)
        {
            _MyFsm.OnColliderEnter();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        _DefaultAttrBoard.HitMe = collision;
        _MyFsm.OnColliderStay();
    }

    public void OnCollisionExit(Collision collision)
    {
        _DefaultAttrBoard.HitMe = collision;
        _MyFsm.OnColliderExit();
    }

}
