using UnityEngine;
using System;

[Serializable]
public class WoodPlateAttrBoard : AttrBoard
{
};

public class WoodPlateDefState : DefaultItemState
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
        AudioManager.instance.Play(_DefAttrBoard.Sound.Sounds[DefaultItemSound.Grabing]);
    }
    public override void OnRelease()
    {
        AudioManager.instance.Play(_DefAttrBoard.Sound.Sounds[DefaultItemSound.Throwing]);
    }
    public override void OnRidigibodyEnter(Collision collision)
    {
        if (_DefAttrBoard.Sound.V_Playable)
        {
            AudioManager.instance.Play(_DefAttrBoard.Sound.Sounds[DefaultItemSound.Knock]);
            _DefAttrBoard.Sound.V_Playable = false;
            _DefAttrBoard.Sound.V_LastSoundPlay = 0;
        }
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
