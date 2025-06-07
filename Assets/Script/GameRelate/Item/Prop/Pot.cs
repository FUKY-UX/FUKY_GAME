using UnityEngine;
using System;

[Serializable]
public class PotAttrBoard : AttrBoard
{
    [Header("��Ϸ����")]
    [HideInInspector]
    public Pot Me;
    public bool Heating;
    public LayerMask CookFireOn;
    public FireEffect[] Fires;
    [Tooltip("�������������")]
    public float HeatingRange = 1f; 
    
    [Header("��������")]
    public MeshCollider CookCollider;
    public Transform PotCenter;
    [Tooltip("��С���ֵ��������صø���")]
    [Range(0,1)]
    public float PotVsFire=0.5f;
    [Header("���Բ���")]
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
        base.OnTriggerEnter(collider);
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
        _DefAttrBoard.Phy._collider.enabled = false;
        _PotAttrBoard.CookCollider.enabled = true;
    }
    public override void OnRelease()
    {
        _DefAttrBoard.Phy._collider.enabled = true;
        _PotAttrBoard.CookCollider.enabled = false;
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
    public override void OnEnter()
    {
        base.OnEnter();
        _PotAttrBoard.Heating = true;
    }

    public override void OnRidigibodyEnter(Collision collision)
    {
        //ͨ����д�ķ�ʽ����ȥ������ײ����Ч������֪���в���
        MeatBase Food = collision.collider.GetComponentInParent<MeatBase>();
        if (Food != null)
        {
            //����ʳ������ĽӴ���
            Food.Cal_CookedMeatFace(_PotAttrBoard.Me);
            CookingMech.Instance.ImBeCooking(_PotAttrBoard.Me, Food);
        }

    }

    public override void OnFixUpdate()
    {
        FireEffect fire;
        fire = collider.gameObject.GetComponentInParent<FireEffect>();
        if (fire)
        {
            _MyFsm.SwitchState(ItemState_Type.State1);
        }

    }

    public override void OnGrab()
    {
        base.OnGrab();
        _DefAttrBoard.Phy._collider.enabled = false;
        _PotAttrBoard.CookCollider.enabled = true;
    }
    public override void OnRelease()
    {
        
        _DefAttrBoard.Phy._collider.enabled = true;
        _PotAttrBoard.CookCollider.enabled = false;
    }

    bool IsPointInTrigger(Collider trigger, Vector3 point)
    {
        // ����һ����С���������
        Collider[] colliders = Physics.OverlapSphere(point, 0.001f);
        
        foreach (Collider col in colliders)
        {
            if (col == trigger)
                return true;
        }
        return false;
    }
}

public class Pot : GrabInteractedItemOrigin
{
    public PotAttrBoard _potAttrBoard; 
    private void Start()
    {
        _potAttrBoard.Me = this;
        Item_FSM.AddState(ItemState_Type.Default,new PotDefaultState(Item_FSM, DefaultAttr, _potAttrBoard));
        Item_FSM.AddState(ItemState_Type.State1, new PotHeatingState(Item_FSM, DefaultAttr, _potAttrBoard));
        Item_FSM.SwitchState(ItemState_Type.Default);
    }
    public void OnDrawGizmos()
    {
        if (_potAttrBoard.ShowGizmo) Gizmos.DrawSphere(DefaultAttr.Phy._rigidbody.transform.position, _potAttrBoard.HeatingRange);
    }

}
