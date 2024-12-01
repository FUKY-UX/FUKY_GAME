using UnityEngine;
using System;

public interface I_EdibleFood
{
    public void Food_Cooking(Pot CookMePot);
    public void Food_Leaving(Pot CookMePot);

}

[Serializable]
public class MeetAttrBoard : AttrBoard
{
    #region 音效相关
    [Header("肉肉音效")]
    public string[] MeatGrabSound = new string[]
        {
            MusicAndSound_Path.instance.MeetGrab1,
            MusicAndSound_Path.instance.MeetGrab2,
            MusicAndSound_Path.instance.MeetGrab3,
            MusicAndSound_Path.instance.MeetGrab4,
            MusicAndSound_Path.instance.MeetGrab5,
            MusicAndSound_Path.instance.MeetGrab6,
            MusicAndSound_Path.instance.MeetGrab7
        };
    public string[] MeatDropSound = new string[]
        {
        MusicAndSound_Path.instance.MeetDrop1,
        MusicAndSound_Path.instance.MeetDrop2,
        };
    #endregion
    #region 烹饪机制相关
    [Header("烹饪机制")]
    public Pot _CookMePot;
    public bool IsCooking = false;
    public Material Meat_Mat;
    public float Meat_Temperature = 0f;
    [Tooltip("变温速度")]
    public float Meat_TCR = 0.2f;
    [Header("烹饪物理")]
    public float Meat_Stickiness = 1f;
    public float Meat_JumpStren = 1f;
    public float Meat_RotateStren = 0.1f;
    [HideInInspector]
    public bool LastPotInf, CurrPotInf;
    [HideInInspector]
    public Quaternion Meat_Rotate = Quaternion.identity;

    #endregion
};
public class FoodDefaultState : DefaultItemState
{
    protected ItemFSM _MyFsm;
    public DefaultItemAttrBoard _DefAttrBoard;
    public MeetAttrBoard _MeatAttr;
    
    public FoodDefaultState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, MeetAttrBoard Extend_Board)
    {
        _MyFsm = in_Fsm;
        _DefAttrBoard = _defattrboard as DefaultItemAttrBoard;
        _MeatAttr = Extend_Board as MeetAttrBoard;
    }
    public override void OnGrab()
    {
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _MeatAttr.MeatGrabSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnRelease()
    {
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _MeatAttr.MeatDropSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnRidigibodyEnter(Collision collision)
    {
        if (_MeatAttr.Meat_Temperature > 0f) { _DefAttrBoard._rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; }
        else { _DefAttrBoard._rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete; }
        //音效
        if (_DefAttrBoard.V_Playable)
        {
            AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _MeatAttr.MeatGrabSound, _DefAttrBoard.V_Voulme, 2);
            _DefAttrBoard.V_Playable = false;
            _DefAttrBoard.V_LastSoundPlay = 0;
        }
    }
    public override void OnFixUpdate()
    {
        float Temperature_Delta = Time.deltaTime * _MeatAttr.Meat_TCR / 2;
        if (_MeatAttr.Meat_Temperature > 0) { _MeatAttr.Meat_Temperature -= Temperature_Delta; } else { _MeatAttr.Meat_Mat.color = Color.white; return; }
        _MeatAttr.Meat_Mat.color = new(1, 1- _MeatAttr.Meat_Temperature, 1- _MeatAttr.Meat_Temperature, 1);
    }
}


public class FoodItemBase : InteractedItemOrigin
{
    public MeetAttrBoard _MeatAttr;

    public void Food_Cooking(Pot CookMePot)
    {
        _MeatAttr._CookMePot = CookMePot;
        _MeatAttr.Meat_Rotate = _DefaultAttrBoard._rigidbody.rotation;
        Vector3 ForceDir = _MeatAttr._CookMePot._potAttrBoard.PotCenter.position - _DefaultAttrBoard._rigidbody.transform.position;
        _DefaultAttrBoard._rigidbody.AddForce(ForceDir * _MeatAttr.Meat_Stickiness);
        _DefaultAttrBoard._rigidbody.AddForce(Vector3.up * _MeatAttr._CookMePot._DefaultAttrBoard.RubFactor.y * _MeatAttr.Meat_JumpStren);
        if (CookMePot._potAttrBoard.Heating && _MeatAttr.Meat_Temperature<1)
        {
            _MeatAttr.IsCooking = true;
            _MeatAttr.Meat_Temperature += Time.deltaTime * _MeatAttr.Meat_TCR;

        }
    }
    public void Food_Leaving(Pot CookMePot)
    {
        _MeatAttr._CookMePot = null;
        _MeatAttr.IsCooking = false;
        _DefaultAttrBoard._rigidbody.angularVelocity = _MeatAttr.Meat_Rotate.eulerAngles * _MeatAttr.Meat_RotateStren;
        _MeatAttr.Meat_Rotate = Quaternion.identity;
    }

    private void Start()
    {
        _MyFsm.AddState(ItemState_Type.Default, new FoodDefaultState(_MyFsm, _DefaultAttrBoard, _MeatAttr));
        _MyFsm.SwitchState(ItemState_Type.Default);
    }

}

