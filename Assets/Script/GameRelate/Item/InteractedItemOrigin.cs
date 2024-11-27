using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item_FSM;
using System;


[Serializable]
public class DefaultItemBoard : ItemAttrBoard 
{
    #region 音效相关
    [Header("物品音效")]
    public Rigidbody _rigidbody;
    public AudioSource _audiosource;
    public float V_CoolDown = 1f;
    public float V_Voulme = 1f;
    public bool V_Playable = true;
    public float V_CoolDownOffset = 1f;
    public float V_LastSoundPlay;
    #endregion
};

public class DefaultItemState : Item_FSM.InteractedItem
{
    private ItemFSM _MyFsm;
    public DefaultItemBoard _AttrBoard;
    public DefaultItemState(ItemFSM in_Fsm, ItemAttrBoard in_Board)
    {
        _MyFsm = in_Fsm;
        _AttrBoard = in_Board as DefaultItemBoard;
    }
    public void OnEnter()
    {
    }
    public void OnExit()
    {

    }
    public void OnFixUpdate()
    {
    }
    public void OnUpdate()
    {
        if (!_AttrBoard.V_Playable)
        {
            if (_AttrBoard.V_LastSoundPlay > _AttrBoard.V_CoolDownOffset)
            {
                _AttrBoard.V_Playable = true;
                _AttrBoard.V_CoolDownOffset = _AttrBoard.V_CoolDown + UnityEngine.Random.Range(-0.5f, 0.5f);
            }
            _AttrBoard.V_LastSoundPlay += Time.deltaTime;
            return;
        }

    }
    public void OnGrab()
    {
    }
    public void OnRelease()
    {
    }
}


public abstract class InteractedItemOrigin : MonoBehaviour
{
    public ItemFSM _MyFsm;
    [SerializeField]
    private DefaultItemBoard _MyAttrBoard;
    private void Start()
    {
        _MyFsm = new ItemFSM(_MyAttrBoard);
        _MyFsm.AddState(ItemState_Type.Default, new DefaultItemState(_MyFsm, _MyAttrBoard));
        _MyFsm.SwitchState(ItemState_Type.Default);
    }
    private void Update()
    {
        _MyFsm.OnUpdate();
    }
    private void FixedUpdate()
    {
        _MyFsm.OnFixUpdate();
    }
}
