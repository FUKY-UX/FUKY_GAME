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

    [Tooltip("铁锅的受热面积")]
    public float HeatingRange = 1f; 
    
    [Header("铁锅物理")]
    public GameObject CookCollider;
    public Transform PotCenter;
    [Header("调试参数")]
    public bool ShowGizmo;
};


public class PotDefaultState : GrabPhyItem_DefaultState
{
    protected ItemFSM _MyFsm;
    public PotAttrBoard _PotAttrBoard;
    public PotDefaultState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, PotAttrBoard Extend_Board): base(in_Fsm,_defattrboard,Extend_Board)
    {
        _MyFsm = in_Fsm;
        _PotAttrBoard = Extend_Board as PotAttrBoard;
    }

    public override void OnTriggerStay(Collider collider)
    {
        base.OnTriggerStay(collider);
        FireEffect fire;
        fire = collider.gameObject.GetComponentInParent<FireEffect>();
        if (fire)
        {
            _MyFsm.SwitchState(ItemState_Type.State1);
        }
    }
    public override void OnEnter()
    {
        base.OnEnter();
        _PotAttrBoard.Heating = false;
    }

    public override void OnGrab()
    {
        base.OnGrab();
        _PotAttrBoard.CookCollider.SetActive(true);
        _DefAttrBoard.Phy._collider.enabled = false;
    }
    public override void OnRelease()
    {
        base.OnRelease();
        _PotAttrBoard.CookCollider.SetActive(false);
        _DefAttrBoard.Phy._collider.enabled = true;

    }

}
public class PotHeatingState : GrabPhyItem_DefaultState
{
    public PotAttrBoard _PotAttrBoard;
    public PotHeatingState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, PotAttrBoard Extend_Board): base(in_Fsm,_defattrboard,Extend_Board)
    {
        _ItemFsm = in_Fsm;
        _DefAttrBoard = _defattrboard as DefaultItemAttrBoard;
        _PotAttrBoard = Extend_Board as PotAttrBoard;
    }
    public override void OnEnter()
    {
        base.OnEnter();
        _PotAttrBoard.Heating = true;
    }
    public override void OnTriggerEnter(Collider collider)
    {
        Debug.Log("表哥，我进来力");
        //通过重写的方式尝试去除掉碰撞的音效，但不知道行不行
        FoodBase Food = collider.GetComponentInParent<FoodBase>();
        if (Food != null)
        {
            //更新食物与锅的接触面
            Food.Cal_CookedMeatFace(_PotAttrBoard.Me);
            CookingMech.Instance.ImBeCooking(_PotAttrBoard.Me, Food);
        }
    }
    public override void OnFixUpdate()
    {

        Collider[] colliders = Physics.OverlapSphere(_PotAttrBoard.PotCenter.position,
        _PotAttrBoard.HeatingRange, _PotAttrBoard.CookFireOn);

        if (colliders.Length == 0)
        {
            _ItemFsm.SwitchState(ItemState_Type.Default);
        }
    }
    public override void OnGrab()
    {
        base.OnGrab();
        _PotAttrBoard.CookCollider.SetActive(true);
        _DefAttrBoard.Phy._collider.enabled = false;
    }
    public override void OnRelease()
    {
        base.OnRelease();
        _PotAttrBoard.CookCollider.SetActive(false);
        _DefAttrBoard.Phy._collider.enabled = true;
    }

}
public class Pot : GrabInteractedItemOrigin
{
    public PotAttrBoard _potAttrBoard;
    void Awake()
    {
        base.InitItemStateAndPhy();
        Item_FSM.AddState(ItemState_Type.Default, new PotDefaultState(Item_FSM, DefaultAttr, _potAttrBoard));
        Item_FSM.AddState(ItemState_Type.State1, new PotHeatingState(Item_FSM, DefaultAttr, _potAttrBoard));
        Item_FSM.SwitchState(ItemState_Type.Default);
    }
    private void Start()
    {
        _potAttrBoard.Me = this;
        base.registerAduioList();
    }
    public void OnDrawGizmos()
    {
        if (_potAttrBoard.ShowGizmo) Gizmos.DrawSphere(DefaultAttr.Phy._rigidbody.transform.position, _potAttrBoard.HeatingRange);
    }

}