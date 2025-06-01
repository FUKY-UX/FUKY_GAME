using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public enum CookingMoment
{
    UnCook,
    Normal,
    Super,
    Lost,
    Bad
}
[Serializable]
public class FoodPartInf_Def:AttrBoard
{
    public FoodPartInf_Def(float InitCookValue, float InitOverCookValue)
    {
        CookValue = InitCookValue;
        SuperMoment = InitOverCookValue;
    }

    public CookingMoment CurrPartCookMoment;
    public float CookValue = 0f;
    public float SuperMoment = 0f;
    public float BadCookValue = 0f;

}
[Serializable]
public class FoodPhysics : AttrBoard
{
    [Tooltip("食材对锅的吸力")]
    public float Meat_Stickiness = 0.04f;
    [Tooltip("锅对食物施加的动力")]
    public float Meat_JumpStren = 0.02f;
    [Tooltip("食物离开锅时，会根据离开时的角度\r\n获得相应旋转速度强度")]
    public float Meat_RotateStren = 0.1f;
    [Tooltip("食物的加热速度")]
    public float Food_TCR = 0.1f;
    [Tooltip("食物离开厨具多久算不接触")]
    public float LeavingMoment = 0.35f;
    [HideInInspector]
    public float FloatingTime = 0f;
}
[Serializable]
public class FoodCooking : AttrBoard
{
    public Pot _CookMePot;
    public CookingMoment _CookingMoment;
    public Collider _CookedPart;
    public bool IsLeavingPot;
    public float Food_TotalCook = 0f;
    [Tooltip("肉的各部分，用来模拟食材各部分的烹饪情况")]
    public Collider[] Food_Part;
    public FoodPartInf_Def CurrCookingPart;
    [Tooltip("烹饪时机难度，越大机会越稍纵即逝")]
    [Range(0, 1f)]
    public float SuperMomentSpeed = 0.1f;
    [Tooltip("惊喜时刻的反应时间，越大反应时间越足")]
    [Range(0, 0.5f)]
    public float SuperMomentTime = 0.1f;
    [Tooltip("惊喜时刻")]
    public float SuperMoment;
    [HideInInspector]
    public Vector2 SuperMoment_V2;
    [Tooltip("惊喜时刻的时间段")]
    public Vector2 SuperMomentRange = new(0.2f, 0.8f);
}
[Serializable]
public class FoodSounds : AttrBoard
{
    [SerializeField]
    [Tooltip("Key【物品状态】—Value【状态音效】")]
    public SerializableDictionary<CookingMoment,SoundInf> Sounds;
}

[Serializable]
public class FoodAttr : AttrBoard
{
    [HideInInspector]
    public FoodPartInf_Def LastFoodPartState;
    public Dictionary<Collider, FoodPartInf_Def> Food_PartState;
    
    [Header("烹饪机制")]
    public FoodCooking Cook;
    [Header("烹饪物理")]
    public FoodPhysics Phy;
    [Header("烹饪音效")]
    public FoodSounds Sound;

    [HideInInspector]
    public Quaternion Meat_Rotate = Quaternion.identity;
};
public class FoodDefaultState : DefaultItemState
{
    protected ItemFSM _MyFsm;
    public DefaultItemAttrBoard _DefAttr;
    public FoodAttr _FoodAttr;
    
    public FoodDefaultState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, FoodAttr Extend_Board)
    {
        _MyFsm = in_Fsm;
        _DefAttr = _defattrboard as DefaultItemAttrBoard;
        _FoodAttr = Extend_Board as FoodAttr;
    }

    public override void OnGrab()
    {
    }
    public override void OnRelease()
    {
    }
    public override void OnRidigibodyEnter(Collision collision)
    {
        //音效
        if (_DefAttr.Sound.V_Playable)
        {
            _DefAttr.Sound.V_Playable = false;
            _DefAttr.Sound.V_LastSoundPlay = 0;
        }
    }
    public override void OnFixUpdate()
    {
        //判断是否离开锅的逻辑
        if (_FoodAttr.Phy.FloatingTime < _FoodAttr.Phy.LeavingMoment && !_FoodAttr.Cook.IsLeavingPot)
        {
            _FoodAttr.Phy.FloatingTime += Time.deltaTime;
            if(_FoodAttr.Phy.FloatingTime > _FoodAttr.Phy.LeavingMoment)
            {   
                _DefAttr.Phy._rigidbody.angularVelocity = _FoodAttr.Meat_Rotate.eulerAngles * _FoodAttr.Phy.Meat_RotateStren / _FoodAttr.Cook.Food_Part.Length;
                _FoodAttr.Cook.IsLeavingPot = true;
                _FoodAttr.Cook._CookingMoment = CookingMoment.UnCook;
                return;
            }
        }
    }

}
public class FoodCookingState : DefaultItemState
{
    protected ItemFSM _MyFsm;
    public DefaultItemAttrBoard _DefAttr;
    public FoodAttr _FoodAttr;

    public FoodCookingState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, FoodAttr Extend_Board)
    {
        _MyFsm = in_Fsm;
        _DefAttr = _defattrboard as DefaultItemAttrBoard;
        _FoodAttr = Extend_Board as FoodAttr;
    }
    public override void OnEnter()
    {
        if(_FoodAttr.Cook.SuperMoment == 0) 
        { 
            _FoodAttr.Cook.SuperMoment = UnityEngine.Random.Range(_FoodAttr.Cook.SuperMomentRange.x, _FoodAttr.Cook.SuperMomentRange.y);
            _FoodAttr.Cook.SuperMoment_V2 = new(Mathf.Max(_FoodAttr.Cook.SuperMomentRange.x, _FoodAttr.Cook.SuperMoment - _FoodAttr.Cook.SuperMomentTime), Mathf.Min(_FoodAttr.Cook.SuperMomentRange.y, _FoodAttr.Cook.SuperMoment + _FoodAttr.Cook.SuperMomentTime));
        }
        #region 参数和烹饪物理
        _FoodAttr.Cook.IsLeavingPot = false;
        _FoodAttr.Phy.FloatingTime = 0f;
        _FoodAttr.Meat_Rotate = _DefAttr.Phy._rigidbody.rotation;
        Vector3 ForceDir = _FoodAttr.Cook._CookMePot._potAttrBoard.PotCenter.position - _DefAttr.Phy._rigidbody.transform.position;
        _DefAttr.Phy._rigidbody.AddForce(ForceDir * _FoodAttr.Phy.Meat_Stickiness / _FoodAttr.Cook.Food_Part.Length);
        _DefAttr.Phy._rigidbody.AddForce(Vector3.up * _FoodAttr.Cook._CookMePot.Default.Phy.RubFactor.y * _FoodAttr.Phy.Meat_JumpStren / _FoodAttr.Cook.Food_Part.Length);
        FoodPartInf_Def _CurrFoodPartState;
        _FoodAttr.Food_PartState.TryGetValue(_FoodAttr.Cook._CookedPart, out _CurrFoodPartState);
        #endregion
        if (_CurrFoodPartState != null)
        {
            _FoodAttr.Cook.CurrCookingPart = _CurrFoodPartState;
            #region 食物烹饪数据更新
            if (_FoodAttr.Cook._CookMePot._potAttrBoard.Heating) 
            {
                float CookValue = Time.deltaTime * _FoodAttr.Phy.Food_TCR / _FoodAttr.Cook.Food_Part.Length;
                _CurrFoodPartState.SuperMoment += CookValue;
                switch (_CurrFoodPartState.CurrPartCookMoment)
                {
                    case CookingMoment.UnCook:
                        if (_CurrFoodPartState.SuperMoment > 0)
                        {
                            _FoodAttr.Cook._CookingMoment = _CurrFoodPartState.CurrPartCookMoment;
                        }
                        else
                        {
                        }
                        break;
                    case CookingMoment.Normal:
                        if (_CurrFoodPartState.SuperMoment > _FoodAttr.Cook.SuperMoment_V2.x)
                        {

                            //if(_DefAttr._audiosource.clip. != )
                            _CurrFoodPartState.CurrPartCookMoment = CookingMoment.Normal;
                            _CurrFoodPartState.CurrPartCookMoment = CookingMoment.Super;
                        }
                        break;
                    case CookingMoment.Super:
                        if (_CurrFoodPartState.SuperMoment > _FoodAttr.Cook.SuperMoment_V2.y)
                        {
                            _CurrFoodPartState.CurrPartCookMoment = CookingMoment.Lost;
                        }
                        break;
                    case CookingMoment.Lost:

                        break;
                    case CookingMoment.Bad:
                        if (_CurrFoodPartState.SuperMoment > _FoodAttr.Cook.SuperMoment_V2.x) { }

                        break;
                    default:
                        break;
                }

                if (_FoodAttr.LastFoodPartState != _CurrFoodPartState)
                {
                }
                _FoodAttr.LastFoodPartState = _CurrFoodPartState;

            }
        }
        #endregion

        _MyFsm.SwitchState(ItemState_Type.Default);
    }
}

public class FoodItemBase : InteractedItemOrigin
{
    [Header("烹饪基础属性")]
    public FoodAttr CookAttr;
    private void Start()
    {
        _MyFsm.AddState(ItemState_Type.Default, new FoodDefaultState(_MyFsm, Default, CookAttr));
        _MyFsm.AddState(ItemState_Type.State1, new FoodCookingState(_MyFsm, Default, CookAttr));
        _MyFsm.SwitchState(ItemState_Type.Default);
        //_MeatAttr.Food_Part = GetComponentsInChildren<Collider>();
        CookAttr.Food_PartState = new Dictionary<Collider, FoodPartInf_Def>();
        foreach (Collider FoodPart in CookAttr.Cook.Food_Part)
        {
            //(AudioClip)Resources.Load(path)
            FoodPartInf_Def FoodPartState = new FoodPartInf_Def(0f,0f);
            CookAttr.Food_PartState.Add(FoodPart, FoodPartState);
            foreach (var item in CookAttr.Cook.Food_Part)
            {

            }
        }
    }

}

