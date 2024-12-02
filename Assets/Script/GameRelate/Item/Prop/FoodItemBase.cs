using UnityEngine;
using System;
using System.Collections.Generic;

public interface I_EdibleFood
{
    public void Food_Cooking(Pot CookMePot);
    public void Food_Leaving(Pot CookMePot);

}

public struct DefaultFoodPartState
{
    public DefaultFoodPartState(float InitCookValue, float InitOverCookValue)
    {
        CookValue = InitCookValue;
        SuperMoment = InitOverCookValue;
    }
    public float CookValue{get; set; }
    public float SuperMoment{ get; set; }
}


[Serializable]
public class MeetAttrBoard : AttrBoard
{
    #region 音效相关
    [Header("普通音效")]
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
    #region 烹饪机制
    [Header("烹饪机制")]
    public Pot _CookMePot;
    public Collider _CookedPart;
    public bool IsLeavingPot;
    public float Food_TotalCook = 0f;
    [Tooltip("变温速度")]
    public float Food_TCR = 0.2f;
    [SerializeField]
    public Dictionary<Collider, DefaultFoodPartState> Food_PartState;
    [Tooltip("肉的各部分，用来模拟了食材各部分的温度情况")]
    public Collider[] Food_Part;
    #endregion
    #region 烹饪音效
    [Header("烹饪音效变化")]
    public AudioSource CookStateSound;
    public AudioSource SuperStateSound;

    public string[] CookingSound= 
    {
            MusicAndSound_Path.instance.MeatCook1,
            MusicAndSound_Path.instance.MeatCook_S,
            MusicAndSound_Path.instance.MeatCook_Syes,
            MusicAndSound_Path.instance.MeatCook_Bad
    };    // 音效数组
    #endregion
    #region 烹饪视觉
    [Header("烹饪视觉")]
    public Material Meat_Mat;
    #endregion
    #region 烹饪物理
    [Header("烹饪物理")]
    public float Meat_Stickiness = 0.04f;
    public float Meat_JumpStren = 0.02f;
    public float Meat_RotateStren = 0.1f;
    public float FloatingTime = 0f;
    [Tooltip("判定食物离开厨具的时间")]
    public float LeavingMoment = 0.5f;
    #endregion

    [HideInInspector]
    public Quaternion Meat_Rotate = Quaternion.identity;
};
public class FoodDefaultState : DefaultItemState
{
    protected ItemFSM _MyFsm;
    public DefaultItemAttrBoard _DefAttrBoard;
    public MeetAttrBoard _FoodAttr;
    
    public FoodDefaultState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, MeetAttrBoard Extend_Board)
    {
        _MyFsm = in_Fsm;
        _DefAttrBoard = _defattrboard as DefaultItemAttrBoard;
        _FoodAttr = Extend_Board as MeetAttrBoard;
    }

    public override void OnGrab()
    {
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _FoodAttr.MeatGrabSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnRelease()
    {
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _FoodAttr.MeatDropSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnRidigibodyEnter(Collision collision)
    {
        if (_FoodAttr.Food_TotalCook > 0f) { _DefAttrBoard._rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; }
        else { _DefAttrBoard._rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete; }
        //音效
        if (_DefAttrBoard.V_Playable)
        {
            AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _FoodAttr.MeatGrabSound, _DefAttrBoard.V_Voulme, 2);
            _DefAttrBoard.V_Playable = false;
            _DefAttrBoard.V_LastSoundPlay = 0;
        }
    }
    public override void OnFixUpdate()
    {
        if (_FoodAttr.FloatingTime < _FoodAttr.LeavingMoment)
        {
            _FoodAttr.FloatingTime += Time.deltaTime;
            if(_FoodAttr.FloatingTime > _FoodAttr.LeavingMoment)
            {
                _DefAttrBoard._rigidbody.angularVelocity = _FoodAttr.Meat_Rotate.eulerAngles * _FoodAttr.Meat_RotateStren / _FoodAttr.Food_Part.Length;
                _FoodAttr.IsLeavingPot = true;
                return;
            }
        }
    }
    public override void OnUpdate()
    {
        if (_FoodAttr.IsLeavingPot)
        {
            float Temperature_Delta = Time.deltaTime * _FoodAttr.Food_TCR;
            if (_FoodAttr.Food_TotalCook > 0 && _FoodAttr.Food_TotalCook < 1) { _FoodAttr.Food_TotalCook -= Temperature_Delta; } else { _FoodAttr.Meat_Mat.color = Color.white; return; }
            _FoodAttr.Meat_Mat.color = new(1, 1 - _FoodAttr.Food_TotalCook, 1 - _FoodAttr.Food_TotalCook, 1);
            AudioManager.instance.FadeOut(_FoodAttr.CookStateSound, 2f, _FoodAttr.CookStateSound.volume);
        }
    }
}

public class FoodCookingState : DefaultItemState
{
    protected ItemFSM _MyFsm;
    public DefaultItemAttrBoard _DefAttrBoard;
    public MeetAttrBoard _FoodAttr;

    public FoodCookingState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, MeetAttrBoard Extend_Board)
    {
        _MyFsm = in_Fsm;
        _DefAttrBoard = _defattrboard as DefaultItemAttrBoard;
        _FoodAttr = Extend_Board as MeetAttrBoard;
    }
    public override void OnEnter()
    {
        #region 参数和烹饪物理
        _FoodAttr.IsLeavingPot = false;
        _FoodAttr.FloatingTime = 0f;
        _FoodAttr.Meat_Rotate = _DefAttrBoard._rigidbody.rotation;
        Vector3 ForceDir = _FoodAttr._CookMePot._potAttrBoard.PotCenter.position - _DefAttrBoard._rigidbody.transform.position;
        _DefAttrBoard._rigidbody.AddForce(ForceDir * _FoodAttr.Meat_Stickiness / _FoodAttr.Food_Part.Length);
        _DefAttrBoard._rigidbody.AddForce(Vector3.up * _FoodAttr._CookMePot._DefaultAttrBoard.RubFactor.y * _FoodAttr.Meat_JumpStren / _FoodAttr.Food_Part.Length);
        #endregion

        #region 食物烹饪数据更新
        if (_FoodAttr._CookMePot._potAttrBoard.Heating) 
        {
            DefaultFoodPartState _FoodPartState = _FoodAttr.Food_PartState[_FoodAttr._CookedPart];
            float CookValue = Time.deltaTime * _FoodAttr.Food_TCR / _FoodAttr.Food_Part.Length;
            _FoodPartState.CookValue += CookValue;
            _FoodPartState.SuperMoment += CookValue;
            _FoodAttr.Food_TotalCook += CookValue;
            _FoodAttr.Food_PartState[_FoodAttr._CookedPart] = _FoodPartState;
        }
        #endregion
        if (_FoodAttr.Food_TotalCook >= 0)
        {
            AudioManager.instance.FadeIn(_FoodAttr.CookStateSound, 1f, _FoodAttr.CookStateSound.volume, 2f);
        }
        else
        {
            AudioManager.instance.FadeOut(_FoodAttr.CookStateSound, 2f, _FoodAttr.CookStateSound.volume);
        }

        _MyFsm.SwitchState(ItemState_Type.Default);
    }
}


public class FoodItemBase : InteractedItemOrigin
{
    public MeetAttrBoard _MeatAttr;

    private void Start()
    {
        _MyFsm.AddState(ItemState_Type.Default, new FoodDefaultState(_MyFsm, _DefaultAttrBoard, _MeatAttr));
        _MyFsm.AddState(ItemState_Type.State1, new FoodCookingState(_MyFsm, _DefaultAttrBoard, _MeatAttr));
        _MyFsm.SwitchState(ItemState_Type.Default);
        _MeatAttr.Food_Part = GetComponentsInChildren<Collider>();
        _MeatAttr.Food_PartState = new Dictionary<Collider, DefaultFoodPartState>();
        foreach (Collider FoodPart in _MeatAttr.Food_Part)
        {
            DefaultFoodPartState FoodPartState = new DefaultFoodPartState(0f,0f);
            _MeatAttr.Food_PartState.Add(FoodPart, FoodPartState);
        }
        _MeatAttr.CookStateSound.loop = true;
        _MeatAttr.SuperStateSound.loop = true;
        AudioManager.instance.PlaySound(_MeatAttr.CookStateSound, MusicAndSound_Path.instance.MeatCook1, 0);
        AudioManager.instance.PlaySound(_MeatAttr.SuperStateSound, MusicAndSound_Path.instance.MeatCook_S, 0);
    }

}

