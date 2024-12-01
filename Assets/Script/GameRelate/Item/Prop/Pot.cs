using UnityEngine;
using System;
using Item_FSM;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;

[Serializable]
public class PotAttrBoard : AttrBoard
{
    [Header("游戏机制")]
    public LayerMask CookFireOn;
    public float HeatRange=0.5f;

    public FireEffect[] Fires;
    [Header("铁锅物理")]
    public MeshCollider CookCollider;
    public Transform PotCenter;
    [Tooltip("调小这个值火焰会隐藏得更快")]
    [Range(0,1)]
    public float PotVsFire=0.7f;
    [Header("铁锅音效")]
    public string[] PotKnockSound = new string[]
        {
            MusicAndSound_Path.instance.PotKnock1,
            MusicAndSound_Path.instance.PotKnock2,
            MusicAndSound_Path.instance.PotKnock3,
            MusicAndSound_Path.instance.PotKnock4,
            MusicAndSound_Path.instance.PotKnock5,
            MusicAndSound_Path.instance.PotKnock6
        };
    public string[] PotGrabSound = new string[]
        {
            MusicAndSound_Path.instance.PotGrab1,
            MusicAndSound_Path.instance.PotGrab2,
            MusicAndSound_Path.instance.PotGrab3
        };
    public string[] PotDropSound = new string[]
        {
            MusicAndSound_Path.instance.PotDrop1,
            MusicAndSound_Path.instance.PotDrop2,
            MusicAndSound_Path.instance.PotDrop3
        };
    [Header("调试参数")]
    public bool ShowGizmo;

};

public class PotDefaultState : DefaultItemState
{
    protected ItemFSM _MyFsm;
    public DefaultItemAttrBoard _DefAttrBoard;
    public PotAttrBoard _PotAttrBoard;
    public PotDefaultState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, PotAttrBoard Extend_Board)
    {
        _MyFsm = in_Fsm;
        _DefAttrBoard = _defattrboard as DefaultItemAttrBoard;
        _PotAttrBoard = Extend_Board as PotAttrBoard;
    }
    public override void OnGrab()
    {
        _DefAttrBoard._collider.enabled=false;
        _PotAttrBoard.CookCollider.enabled = true;
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _PotAttrBoard.PotGrabSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnRelease()
    {
        _DefAttrBoard._collider.enabled = true;
        _PotAttrBoard.CookCollider.enabled = false;
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _PotAttrBoard.PotDropSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnColliderEnter()
    {
        Collider[] colliders = Physics.OverlapSphere(_DefAttrBoard._rigidbody.transform.position, _PotAttrBoard.HeatRange, _PotAttrBoard.CookFireOn);
        if (colliders.Length > 0) { _PotAttrBoard.Fires = colliders[0].GetComponentsInChildren<FireEffect>(); }
        if (_DefAttrBoard.V_Playable)
        {
            AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _PotAttrBoard.PotKnockSound, _DefAttrBoard.V_Voulme, 3);
            _DefAttrBoard.V_Playable = false;
            _DefAttrBoard.V_LastSoundPlay = 0;
        }
    }
    public override void Grabing()
    {
        Collider[] colliders = Physics.OverlapSphere(_DefAttrBoard._rigidbody.transform.position, _PotAttrBoard.HeatRange, _PotAttrBoard.CookFireOn);
        if (colliders.Length > 0){_PotAttrBoard.Fires = colliders[0].GetComponentsInChildren<FireEffect>();}
    }
    public override void OnUpdate()
    {
        if (_PotAttrBoard.Fires != null && _PotAttrBoard.Fires.Length>0)
        {
            Transform EffectParent = _PotAttrBoard.Fires[0].transform.parent;
            float Y =Mathf.Clamp((_DefAttrBoard._rigidbody.position - EffectParent.position).y,0,1);
            foreach (FireEffect fire in _PotAttrBoard.Fires)
            {
                fire.Opacity = Y * _PotAttrBoard.PotVsFire;
            }
            _PotAttrBoard.Fires = null;
        }
    }
}

public class Pot : InteractedItemOrigin
{
    public PotAttrBoard _potAttrBoard; 
    private void Start()
    {
        _MyFsm.AddState(ItemState_Type.Default,new PotDefaultState(_MyFsm, _DefaultAttrBoard, _potAttrBoard));
        _MyFsm.SwitchState(ItemState_Type.Default);
    }
    public void OnDrawGizmos()
    {
        if (_potAttrBoard.ShowGizmo)
        {
            Gizmos.DrawSphere(_DefaultAttrBoard._rigidbody.transform.position, _potAttrBoard.HeatRange);
        }
    }
}
