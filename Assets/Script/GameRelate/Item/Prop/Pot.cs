using UnityEngine;
using System;

[Serializable]
public class PotAttrBoard : AttrBoard
{
    [Header("游戏机制")]
    [HideInInspector]
    public Pot Me;
    public bool Heating;
    public LayerMask CookFireOn;
    public FireEffect[] Fires;
    [Tooltip("铁锅的受热面积")]
    public float HeatingRange = 1f; 
    
    [Header("铁锅物理")]
    public MeshCollider CookCollider;
    public Transform PotCenter;
    [Tooltip("调小这个值火焰会隐藏得更快")]
    [Range(0,1)]
    public float PotVsFire=0.5f;
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
    [HideInInspector]
    public bool LastHeatInf, CurrHeatInf;

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
    public override void OnTriggerStay(Collider collider)
    {
        _PotAttrBoard.Fires = collider.GetComponentsInChildren<FireEffect>();
        if (_PotAttrBoard.Fires.Length > 0) { _PotAttrBoard.Heating = true; _MyFsm.SwitchState(ItemState_Type.State1); }
    }
    public override void OnGrab()
    {
        _DefAttrBoard._collider.isTrigger=true;
        _PotAttrBoard.CookCollider.enabled = true;
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _PotAttrBoard.PotGrabSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnRelease()
    {
        _DefAttrBoard._collider.isTrigger = false;
        _PotAttrBoard.CookCollider.enabled = false;
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _PotAttrBoard.PotDropSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnRidigibodyEnter(Collision collision)
    {
        //音效
        if (_DefAttrBoard.V_Playable)
        {
            AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _PotAttrBoard.PotKnockSound, _DefAttrBoard.V_Voulme / 2, 3);
            _DefAttrBoard.V_Playable = false;
            _DefAttrBoard.V_LastSoundPlay = 0;
        }
    }
    public override void OnRidigibodyStay(Collision collision)
    {
        //食物逻辑
        FoodItemBase Food = collision.collider.GetComponentInParent<FoodItemBase>();
        if (Food != null) 
        {
            Food._MeatAttr._CookMePot = _PotAttrBoard.Me;
            Food._MeatAttr._CookedPart = collision.collider;
            Food._MyFsm.SwitchState(ItemState_Type.State1); 
        }//执行在锅上食物的烹饪行为
    }

}
public class PotHeatingState : DefaultItemState
{
    protected ItemFSM _MyFsm;
    public DefaultItemAttrBoard _DefAttrBoard;
    public PotAttrBoard _PotAttrBoard;
    public PotHeatingState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, PotAttrBoard Extend_Board)
    {
        _MyFsm = in_Fsm;
        _DefAttrBoard = _defattrboard as DefaultItemAttrBoard;
        _PotAttrBoard = Extend_Board as PotAttrBoard;
    }

    public override void OnUpdate()
    {
        Transform EffectParent = _PotAttrBoard.Fires[0].transform.parent;
        float Y = Mathf.Clamp((_DefAttrBoard._rigidbody.position - EffectParent.position).y, 0, 1);
        Y *= _PotAttrBoard.PotVsFire;
        foreach (FireEffect fire in _PotAttrBoard.Fires)
        {
            fire.Opacity = Y;
        }

        Collider[] colliders = Physics.OverlapSphere(_DefAttrBoard._rigidbody.position, _PotAttrBoard.HeatingRange, _PotAttrBoard.CookFireOn);
        if(colliders.Length ==0)
        {
            _PotAttrBoard.Fires = null;
            _PotAttrBoard.Heating = false;
            _MyFsm.SwitchState(ItemState_Type.Default);
        }
    }
    public override void OnGrab()
    {
        _DefAttrBoard._collider.isTrigger = true;
        _PotAttrBoard.CookCollider.enabled = true;
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _PotAttrBoard.PotGrabSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnRelease()
    {
        _DefAttrBoard._collider.isTrigger = false;
        _PotAttrBoard.CookCollider.enabled = false;
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _PotAttrBoard.PotDropSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnRidigibodyEnter(Collision collision)
    {
        //音效
        if (_DefAttrBoard.V_Playable)
        {
            AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _PotAttrBoard.PotKnockSound, _DefAttrBoard.V_Voulme/2, 3);
            _DefAttrBoard.V_Playable = false;
            _DefAttrBoard.V_LastSoundPlay = 0;
        }
    }
    public override void OnRidigibodyStay(Collision collision)
    {
        //食物逻辑
        FoodItemBase Food = collision.collider.GetComponentInParent<FoodItemBase>();
        if (Food != null)
        {
            Food._MeatAttr._CookMePot = _PotAttrBoard.Me;
            Food._MeatAttr._CookedPart = collision.collider;
            Food._MyFsm.SwitchState(ItemState_Type.State1);
        }//执行在锅上食物的烹饪行为
    }
    public override void OnTriggerStay(Collider collider)
    {
        //食物逻辑
        FoodItemBase Food = collider.GetComponentInParent<FoodItemBase>();
        if (Food != null)
        {
            Food._MeatAttr._CookMePot = _PotAttrBoard.Me;
            Food._MeatAttr._CookedPart = collider;
            Food._MyFsm.SwitchState(ItemState_Type.State1);
        }//执行在锅上食物的烹饪行为

    }


}

public class Pot : InteractedItemOrigin
{
    public PotAttrBoard _potAttrBoard; 
    private void Start()
    {
        _potAttrBoard.Me = this;
        _MyFsm.AddState(ItemState_Type.Default,new PotDefaultState(_MyFsm, _DefaultAttrBoard, _potAttrBoard));
        _MyFsm.AddState(ItemState_Type.State1, new PotHeatingState(_MyFsm, _DefaultAttrBoard, _potAttrBoard));
        _MyFsm.SwitchState(ItemState_Type.Default);

    }
    public void OnDrawGizmos()
    {
        if (_potAttrBoard.ShowGizmo) Gizmos.DrawSphere(_DefaultAttrBoard._rigidbody.transform.position, _potAttrBoard.HeatingRange);
    }

}
