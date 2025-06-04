using UnityEngine;
using System;

[Serializable]
public class DefaultAttrBoard : AttrBoard
{
};

public class Default_ItemState : ItemState
{
    protected ItemFSM _MyFsm;
    public DefaultItemAttrBoard _DefAttrBoard;
    public Default_ItemState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard)
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

public class DefaultItem : InteractedItemOrigin
{
    public DefaultAttrBoard _WoodPlateAttrBoard;
    private void Start()
    {
        _MyFsm.AddState(ItemState_Type.Default, new Default_ItemState(_MyFsm, Default));
        _MyFsm.SwitchState(ItemState_Type.Default);
    }
}
