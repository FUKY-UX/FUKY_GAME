using UnityEngine;
using System;

[Serializable]
public class WoodPlateAttrBoard : AttrBoard
{
};

public class WoodPlateDefState : ItemState
{
    protected ItemFSM _MyFsm;
    public DefaultItemAttrBoard _DefAttrBoard;
    public WoodPlateAttrBoard _WoodPlateAttrBoard;
    public WoodPlateDefState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, WoodPlateAttrBoard Extend_Board)
    {
        _MyFsm = in_Fsm;
        _DefAttrBoard = _defattrboard as DefaultItemAttrBoard;
        _WoodPlateAttrBoard = Extend_Board as WoodPlateAttrBoard;
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

public class WoodPlate : InteractedItemOrigin
{
    public WoodPlateAttrBoard _WoodPlateAttrBoard;
    private void Start()
    {
        _MyFsm.AddState(ItemState_Type.Default, new WoodPlateDefState(_MyFsm, Default, _WoodPlateAttrBoard));
        _MyFsm.SwitchState(ItemState_Type.Default);
    }
}
