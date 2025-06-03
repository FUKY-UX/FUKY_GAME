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
    [Header("调试参数")]
    public bool ShowGizmo;
    [HideInInspector]
    public bool LastHeatInf, CurrHeatInf;

};
public class PotDefaultState : ItemState
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
        _DefAttrBoard.Phy._collider.isTrigger=true;
        _PotAttrBoard.CookCollider.enabled = true;
    }
    public override void OnRelease()
    {
        _DefAttrBoard.Phy._collider.isTrigger = false;
        _PotAttrBoard.CookCollider.enabled = false;
    }
    public override void OnRidigibodyEnter(Collision collision)
    {
    }
    public override void OnRidigibodyStay(Collision collision)
    {
        //食物逻辑
        FoodItemBase Food = collision.collider.GetComponentInParent<FoodItemBase>();
        if (Food != null) 
        {
            Food.CookAttr.Cook._CookMePot = _PotAttrBoard.Me;
            Food.CookAttr.Cook._CookedPart = collision.collider;
            Food._MyFsm.SwitchState(ItemState_Type.State1); 
        }//执行在锅上食物的烹饪行为
    }

}
public class PotHeatingState : ItemState
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
        float Y = Mathf.Clamp((_DefAttrBoard.Phy._rigidbody.position - EffectParent.position).y, 0, 1);
        Y *= _PotAttrBoard.PotVsFire;
        foreach (FireEffect fire in _PotAttrBoard.Fires)
        {
            fire.Opacity = Y;
        }

        Collider[] colliders = Physics.OverlapSphere(_DefAttrBoard.Phy._rigidbody.position, _PotAttrBoard.HeatingRange, _PotAttrBoard.CookFireOn);
        if(colliders.Length ==0)
        {
            _PotAttrBoard.Fires = null;
            _PotAttrBoard.Heating = false;
            _MyFsm.SwitchState(ItemState_Type.Default);
        }
    }
    public override void OnGrab()
    {
        _DefAttrBoard.Phy._collider.isTrigger = true;
        _PotAttrBoard.CookCollider.enabled = true;
    }
    public override void OnRelease()
    {
        _DefAttrBoard.Phy._collider.isTrigger = false;
        _PotAttrBoard.CookCollider.enabled = false;
    }
    public override void OnRidigibodyEnter(Collision collision)
    {
    }
    public override void OnRidigibodyStay(Collision collision)
    {
        //食物逻辑
        FoodItemBase Food = collision.collider.GetComponentInParent<FoodItemBase>();
        if (Food != null)
        {
            Food.CookAttr.Cook._CookMePot = _PotAttrBoard.Me;
            Food.CookAttr.Cook._CookedPart = collision.collider;
            Food._MyFsm.SwitchState(ItemState_Type.State1);
        }//执行在锅上食物的烹饪行为
    }
    public override void OnTriggerStay(Collider collider)
    {
        //食物逻辑
        FoodItemBase Food = collider.GetComponentInParent<FoodItemBase>();
        if (Food != null)
        {
            Food.CookAttr.Cook._CookMePot = _PotAttrBoard.Me;
            Food.CookAttr.Cook._CookedPart = collider;
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
        _MyFsm.AddState(ItemState_Type.Default,new PotDefaultState(_MyFsm, Default, _potAttrBoard));
        _MyFsm.AddState(ItemState_Type.State1, new PotHeatingState(_MyFsm, Default, _potAttrBoard));
        _MyFsm.SwitchState(ItemState_Type.Default);
    }
    public void OnDrawGizmos()
    {
        if (_potAttrBoard.ShowGizmo) Gizmos.DrawSphere(Default.Phy._rigidbody.transform.position, _potAttrBoard.HeatingRange);
    }

}
