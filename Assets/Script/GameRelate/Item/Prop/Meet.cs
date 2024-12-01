using UnityEngine;
using System;
using Item_FSM;
using UnityEngine.Windows;
using Unity.VisualScripting;
using System.Collections.Generic;

[Serializable]
public class MeetAttrBoard : AttrBoard
{
    #region 音效相关
    [Header("肉肉音效")]
    public string[] MeetGrabSound = new string[]
        {
            MusicAndSound_Path.instance.MeetGrab1,
            MusicAndSound_Path.instance.MeetGrab2,
            MusicAndSound_Path.instance.MeetGrab3,
            MusicAndSound_Path.instance.MeetGrab4,
            MusicAndSound_Path.instance.MeetGrab5,
            MusicAndSound_Path.instance.MeetGrab6,
            MusicAndSound_Path.instance.MeetGrab7
        };
    public string[] MeetDropSound = new string[]
        {
        MusicAndSound_Path.instance.MeetDrop1,
        MusicAndSound_Path.instance.MeetDrop2,
        };
    #endregion
    #region 烹饪机制相关
    public Pot Pot_On;
    public bool IsLeaveing;
    public Quaternion Meat_Rotate = Quaternion.identity;
    public float Meat_Stickiness = 1f;
    public float Meat_JumpStren = 1f;
    public float Meat_RotateStren = 0.1f;

    #endregion
};

public class MeetDefaultState : DefaultItemState
{
    protected ItemFSM _MyFsm;
    public DefaultItemAttrBoard _DefAttrBoard;
    public MeetAttrBoard _MeetAttrBoard;
    public MeetDefaultState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, MeetAttrBoard Extend_Board)
    {
        _MyFsm = in_Fsm;
        _DefAttrBoard = _defattrboard as DefaultItemAttrBoard;
        _MeetAttrBoard = Extend_Board as MeetAttrBoard;
    }
    public override void OnGrab()
    {
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _MeetAttrBoard.MeetGrabSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnRelease()
    {
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _MeetAttrBoard.MeetDropSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnColliderEnter()
    {
        _MeetAttrBoard.Pot_On = _DefAttrBoard.HitMe.collider.GetComponentInParent<Pot>();
        if (_MeetAttrBoard.Pot_On)
        {
            _DefAttrBoard._rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        else
        {
            _DefAttrBoard._rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }
        if (_DefAttrBoard.V_Playable)
        {
            AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _MeetAttrBoard.MeetGrabSound, _DefAttrBoard.V_Voulme, 2);
            _DefAttrBoard.V_Playable = false;
            _DefAttrBoard.V_LastSoundPlay = 0;
        }
    }
    public override void OnColliderStay()
    {
        _MeetAttrBoard.Pot_On = _DefAttrBoard.HitMe.collider.GetComponentInParent<Pot>();
    }
    public override void OnColliderExit()
    {
        _MeetAttrBoard.Pot_On = _DefAttrBoard.HitMe.collider.GetComponentInParent<Pot>();
    }
    public override void OnFixUpdate()
    {
        if (_MeetAttrBoard.Pot_On)
        {
            _MeetAttrBoard.IsLeaveing = true;
            _MeetAttrBoard.Meat_Rotate = _DefAttrBoard._rigidbody.rotation;
            _DefAttrBoard._rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Vector3 ForceDir = _MeetAttrBoard.Pot_On._potAttrBoard.PotCenter.position - _DefAttrBoard._rigidbody.transform.position;
            _DefAttrBoard._rigidbody.AddForce(ForceDir * _MeetAttrBoard.Meat_Stickiness);
            _DefAttrBoard._rigidbody.AddForce(Vector3.up * _MeetAttrBoard.Pot_On._DefaultAttrBoard.RubFactor.y * _MeetAttrBoard.Meat_JumpStren);
            _MeetAttrBoard.Pot_On = null;
        }
        else 
        {
            if (_MeetAttrBoard.IsLeaveing)
            {
                _DefAttrBoard._rigidbody.angularVelocity = _MeetAttrBoard.Meat_Rotate.eulerAngles * _MeetAttrBoard.Meat_RotateStren;
                _MeetAttrBoard.IsLeaveing = false;   
            }
        }
    }
}

public class Meet : InteractedItemOrigin
{
    public MeetAttrBoard _MeetAttrBoard;
    private void Start()
    {
        _MyFsm.AddState(ItemState_Type.Default, new MeetDefaultState(_MyFsm, _DefaultAttrBoard, _MeetAttrBoard));
        _MyFsm.SwitchState(ItemState_Type.Default);
    }
}

