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
public class FoodPartInf_Def : AttrBoard
{
    public FoodPartInf_Def(float InitCookValue, float InitOverCookValue)
    {
        CookValue = InitCookValue;
        SuperMoment = InitOverCookValue;
    }

    public CookingMoment CurrPartCookMoment;
    public float CookValue = 0f;
    public float BadCookValue = 0f;
    public float MomentInWhere = 0f;


    [Tooltip("惊喜时刻")]
    public float SuperMoment;
    [HideInInspector]
    public Vector2 SuperMoment_V2;

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
    [Tooltip("会影响总的花样度的积累速度")]
    public float Food_TCR = 0.1f;
    [Tooltip("食物离开厨具多久算不接触")]
    public float LeavingMoment = 0.35f;
    [SerializeField] // 添加SerializeField特性，使其在Inspector中可见
    public float FloatingTime = 0f;
}
[Serializable]
public class FoodCooking : AttrBoard
{
    [Header("烹饪情况")]

    public Transform _LastCookedPart;
    public bool IsLeavingPot;
    public float Food_TotalCook = 0f;

    [Header("上一次烹饪情况")]
    public FoodPartInf_Def UpFace;
    public FoodPartInf_Def DownFace;
    public FoodPartInf_Def _CurrCookedPart;
    private FoodPartInf_Def LastFoodPart;

}
[Serializable]
public class FoodSounds : AttrBoard
{
    [SerializeField]
    [Tooltip("Key【烹饪状态】—Value【状态长音效】")]
    public SerializableDictionary<CookingMoment, AudioClip> Sounds;
    
    [SerializeField]
    [Tooltip("Key【翻面奖励】—Value【翻面时的音效】")]
    public SerializableDictionary<CookingMoment,AudioClip> MomentSounds;
}


[Serializable]
public class FoodAttr : AttrBoard
{
    [Header("烹饪机制")]
    public FoodCooking Cook;
    [Header("烹饪物理")]
    public FoodPhysics Phy;
    [Header("烹饪音效")]
    public FoodSounds Sound;

    [HideInInspector]
    public Quaternion Meat_Rotate = Quaternion.identity;
};

public class FoodCookingState : ItemState
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
    public override void OnRidigibodyEnter(Collision collision)
    {
        base.OnRidigibodyEnter(collision);

    }

}

public class MeatBase : GrabInteractedItemOrigin
{
    [Header("烹饪基础属性")]
    public FoodAttr CookAttr;
    public bool ShowGizmo;

    // 烹饪状态变化事件
    public delegate void FoodStateChangeHandler(FoodPartInf_Def ChangeToFoodPart);
    public event FoodStateChangeHandler CurrCookedFoodPart_Change;
    public void Awake()
    {
        base.InitItemStateAndPhy();
        base.Item_FSM.AddState(ItemState_Type.State1, new FoodCookingState(base.Item_FSM, base.DefaultAttr, CookAttr));
    }

    private void Start()
    {
        base.registerAduioList();
        // 初始化食物部位状态
        InitializeFoodParts();
        // 订阅烹饪时刻的状态事件
        CookingMech.Instance.OnCookingStateFinish += HandleCookingMomentFinish;
        // 订阅烹饪状态的状态事件
        //CookingMech.Instance.CookingStateUpdate += HandleCookingSound;
    }

    // 初始化食物部位
    private void InitializeFoodParts()
    {
        CookAttr.Cook.UpFace = new FoodPartInf_Def(0f, 0f);
        CookAttr.Cook.DownFace = new FoodPartInf_Def(0f, 0f);
        CookingMech.Instance.CreatNewMoment(CookAttr.Cook.UpFace);
        CookingMech.Instance.CreatNewMoment(CookAttr.Cook.DownFace);
    }



    /// <summary>
    /// 肉不会自己煮自己，调用该方法肉会告诉调用厨具对象自己哪一面是朝向加热锅底的
    /// </summary>
    /// <param name="pot"></param>
    /// <returns></returns>
    public FoodPartInf_Def Cal_CookedMeatFace(Pot pot)
    {
        float Dot = Vector3.Dot(pot._potAttrBoard.PotCenter.up, this.transform.up);
        FoodPartInf_Def CookedPart = Dot > 0 ? CookAttr.Cook.UpFace : CookAttr.Cook.DownFace;
        CookAttr.Cook._CurrCookedPart = CookedPart;
        Debug.Log($"【1】Dot:{Dot}");
        return CookedPart;
    }

    // 结束烹饪
    public void EndCooking()
    {
        CookAttr.Cook.IsLeavingPot = true;

        // 如果已经离开锅足够长时间，切换回默认状态
        if (CookAttr.Phy.FloatingTime >= CookAttr.Phy.LeavingMoment)
        {
            if (Item_FSM.CurrItemState != ItemState_Type.Default)
            {
                Item_FSM.SwitchState(ItemState_Type.Default);
            }
        }
    }

    // 处理烹饪结算效果
    private void HandleCookingMomentFinish(MeatBase food, Transform part, CookingMoment FinishMoment)
    {
        Debug.Log($"尝试播放音效{FinishMoment}");

        if (food == this && part == CookAttr.Cook._LastCookedPart)
        {
            switch (FinishMoment)
            {
                case CookingMoment.Normal:
                    AudioManager2025.Instance.PlaySound(CookAttr.Sound.MomentSounds[CookingMoment.Normal].name); break;
                case CookingMoment.Super:
                    AudioManager2025.Instance.PlaySound(CookAttr.Sound.MomentSounds[CookingMoment.Super].name); break;
                case CookingMoment.Lost:
                    AudioManager2025.Instance.PlaySound(CookAttr.Sound.MomentSounds[CookingMoment.Lost].name); break;
                case CookingMoment.Bad:
                    AudioManager2025.Instance.PlaySound(CookAttr.Sound.MomentSounds[CookingMoment.Bad].name); break;
                default: break;
            }
        }
    }

    // 处理烹饪时的音效
    private void HandleCookingSound(MeatBase food, Transform part, CookingMoment Moment)
    {
        Debug.Log("收到音效触发");
        if (food == this && part == CookAttr.Cook._CurrCookedPart)
        {
            switch (Moment)
            {
                case CookingMoment.Normal:
                    AudioManager2025.Instance.StopLongSound();
                    AudioManager2025.Instance.PlaySound(CookAttr.Sound.Sounds[CookingMoment.Normal].name); break;
                case CookingMoment.Super:
                    AudioManager2025.Instance.StopLongSound();
                    AudioManager2025.Instance.PlaySound(CookAttr.Sound.Sounds[CookingMoment.Super].name); break;
                case CookingMoment.Lost:
                    AudioManager2025.Instance.StopLongSound();
                    AudioManager2025.Instance.PlaySound(CookAttr.Sound.Sounds[CookingMoment.Lost].name); break;
                case CookingMoment.Bad:
                    AudioManager2025.Instance.StopLongSound();
                    AudioManager2025.Instance.PlaySound(CookAttr.Sound.Sounds[CookingMoment.Bad].name); break;

                default: break;
            }
        }

    }

    public void OnDrawGizmos()
    {
        if (ShowGizmo)
        {
            if (CookAttr.Food_PartState.Keys.Count > 0)
            {
                foreach (var item in CookAttr.Food_PartState.Keys)
                {
                    Gizmos.DrawSphere(item.position, cook.HeatingRange);
                }
            }
        }
    }

    private void AttachToPot(Pot _CookMePot)
    {
        Vector3 ForceDir = _CookMePot._potAttrBoard.PotCenter.position - DefaultAttr.Phy._rigidbody.transform.position;
        DefaultAttr.Phy._rigidbody.AddForce(ForceDir * CookAttr.Phy.Meat_Stickiness / 2);
        DefaultAttr.Phy._rigidbody.AddForce(Vector3.up * _CookMePot.DefaultAttr.Phy.RubFactor.y * CookAttr.Phy.Meat_JumpStren / 2);
    }
}


        // //在肉离开锅的时候给点物理量，让肉自己转偏一点
        // if (_FoodAttr.Cook.IsLeavingPot)
        // {
        //     _FoodAttr.Phy.FloatingTime += Time.deltaTime;

        //     if (_FoodAttr.Phy.FloatingTime > _FoodAttr.Phy.LeavingMoment)
        //     {
        //         // 应用旋转效果
        //         if (_DefAttrBoard.Phy._rigidbody != null)
        //         {
        //             _DefAttrBoard.Phy._rigidbody.angularVelocity =
        //                 _FoodAttr.Meat_Rotate.eulerAngles *
        //                 _FoodAttr.Phy.Meat_RotateStren / 2;
        //         }
        //     }
        // }
