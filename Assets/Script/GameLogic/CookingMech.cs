using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CookingMech : MonoBehaviour
{
    // 单例模式
    public static CookingMech Instance { get; private set; }

    // 烹饪参数配置
    [Header("烹饪系统参数")]
    [Tooltip("正常烹饪的花样度积累速度")]
    public float defaultCookRate = 0.1f;
    [Tooltip("超级时刻，会在煎肉的开始到结束的什么时间段内出现")]
    public Vector2 defaultSuperMomentRange = new Vector2(0.2f, 0.8f);
    [Tooltip("时间窗口大概多久，并不是说0.1就是0.1S，影响该值得有食材的TCR值和defaultcookrate")]
    public float superMomentDuration = 0.1f;
    [Tooltip("左边是食材的烹饪状态，右边是该状态下翻面成功后的花样度加成倍率,注:UNCOOK是无效的，不可能不粘锅就算翻面")]
    public SerializableDictionary<CookingMoment, float> CookMoment_PLUS = new SerializableDictionary<CookingMoment, float>();
    // 事件系统
    public delegate void CookingStateChangeHandler(MeatBase food, Transform part, CookingMoment Moment);
    public event CookingStateChangeHandler OnCookingStateFinish;
    public event CookingStateChangeHandler CookingStateUpdate;
    // 活跃的烹饪锅和食物
    public SerializableDictionary<Pot, List<MeatBase>> MeatCooking_FacePair
                                = new SerializableDictionary<Pot, List<MeatBase>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // 注册烹饪关系
    public void ImBeCooking(Pot pot, MeatBase food)
    {
        if (Instance.MeatCooking_FacePair[pot._potAttrBoard.Me].Contains(food))
        {
            return;
        }
        Instance.MeatCooking_FacePair[pot._potAttrBoard.Me].Add(food);
    }

    // 取消烹饪关系
    public void ImNotBeCooking(Pot pot, MeatBase food, Collider FoodPart)
    {
        if (MeatCooking_FacePair.ContainsKey(pot))
        {
            MeatCooking_FacePair[pot].Remove(food);
            food.EndCooking();
        }
    }

    // 更新所有烹饪进度
    private void Update()
    {
        foreach (var pair in MeatCooking_FacePair)
        {
            Pot pot = pair.Key;
            if (pot._potAttrBoard.Heating)
            {
                foreach (MeatBase food in pair.Value.ToList())
                {
                    UpdateFoodCooking(pot, food);
                }
            }
        }
    }

    // 更新单个食物的烹饪进度
    private void UpdateFoodCooking(Pot pot, MeatBase food)
    {
        if (food.CookAttr.Cook._CurrCookedPart == null) { Debug.Log("当前烹饪部分是空的"); return; }
        // 获取当前烹饪部位
        FoodPartInf_Def partState;
        food.CookAttr.Food_PartState.TryGetValue(food.CookAttr.Cook._CurrCookedPart, out partState);

        if (food.CookAttr.Cook._CurrCookedPart != food.CookAttr.Cook._LastCookedPart && food.CookAttr.Cook._LastCookedPart != null)
        {

            FoodPartInf_Def LastPartState;
            food.CookAttr.Food_PartState.TryGetValue(food.CookAttr.Cook._LastCookedPart, out LastPartState);
            Debug.Log($"上一面'{food.CookAttr.Cook._LastCookedPart}'翻转了，结算情况:{LastPartState.CurrPartCookMoment}");
            OnCookingStateFinish?.Invoke
                (
                    food,
                    food.CookAttr.Cook._LastCookedPart,

                    food.CookAttr.Food_PartState
                    [food.CookAttr.Cook._CurrCookedPart].
                    CurrPartCookMoment
                );
            switch (LastPartState.CurrPartCookMoment)
            {
                case CookingMoment.Normal:
                    food.CookAttr.Cook.Food_TotalCook +=
                LastPartState.SuperMoment * defaultCookRate * CookMoment_PLUS[CookingMoment.Normal]; break;

                case CookingMoment.Super:
                    food.CookAttr.Cook.Food_TotalCook +=
                LastPartState.SuperMoment * defaultCookRate * CookMoment_PLUS[CookingMoment.Super]; break;

                case CookingMoment.Lost:
                    food.CookAttr.Cook.Food_TotalCook +=
                LastPartState.SuperMoment * defaultCookRate * CookMoment_PLUS[CookingMoment.Lost]; break;

                case CookingMoment.Bad:
                    food.CookAttr.Cook.Food_TotalCook +=
                LastPartState.SuperMoment * defaultCookRate * CookMoment_PLUS[CookingMoment.Bad]; break;

                default: break;
            }
            LastPartState.CurrPartCookMoment = CookingMoment.UnCook;
            LastPartState.MomentInWhere = 0;
            CreatNewMoment(LastPartState);

            food.CookAttr.Cook._LastCookedPart = food.CookAttr.Cook._CurrCookedPart;
        }

        if (partState != null)
        {
            //计算总花样度
            foreach (var item in food.CookAttr.Food_PartState.Values)
            {
                food.CookAttr.Cook.Food_TotalCook += item.CookValue;
            }
            food.CookAttr.Cook.Food_TotalCook = food.CookAttr.Cook.Food_TotalCook * food.CookAttr.Phy.Food_TCR / food.CookAttr.Cook.Food_Part.Length;
            // 计算烹饪增量
            float cookIncrement = Time.deltaTime * defaultCookRate * food.CookAttr.Phy.Food_TCR;

            // 更新时刻值
            partState.MomentInWhere += cookIncrement;

            // 根据烹饪值更新状态
            UpdateCookingState(partState, partState.SuperMoment_V2);

            CookingStateUpdate?.Invoke(food, food.CookAttr.Cook._CurrCookedPart, partState.CurrPartCookMoment);

        }
    }

    // 更新烹饪状态
    private void UpdateCookingState(FoodPartInf_Def partState, Vector2 superMomentRange)
    {
        switch (partState.CurrPartCookMoment)
        {
            case CookingMoment.UnCook:
                if (partState.MomentInWhere > 0)
                {
                    partState.CurrPartCookMoment = CookingMoment.Normal;
                }
                partState.CookValue += defaultCookRate * Time.deltaTime;

                break;
            case CookingMoment.Normal:
                if (partState.MomentInWhere > superMomentRange.x)
                {
                    partState.CurrPartCookMoment = CookingMoment.Super;
                }
                partState.CookValue += defaultCookRate * Time.deltaTime;
                break;
            case CookingMoment.Super:
                if (partState.MomentInWhere > superMomentRange.y)
                {
                    partState.CurrPartCookMoment = CookingMoment.Lost;
                }
                partState.CookValue += defaultCookRate * Time.deltaTime;
                break;
            case CookingMoment.Lost:
                if (partState.MomentInWhere > 1.0f)
                {
                    partState.CurrPartCookMoment = CookingMoment.Bad;
                }
                partState.BadCookValue += defaultCookRate * Time.deltaTime;
                break;
        }
    }

    // 计算"惊喜时刻"范围
    public Vector2 CalculateSuperMomentRange(float baseMoment, float timeWindow)
    {
        return new Vector2(
            Mathf.Max(defaultSuperMomentRange.x, baseMoment - timeWindow),
            Mathf.Min(defaultSuperMomentRange.y, baseMoment + timeWindow)
        );
    }

    public void CreatNewMoment(FoodPartInf_Def FoodPart)
    {
        // 初始化"惊喜时刻"
        FoodPart.SuperMoment = UnityEngine.Random.Range(
            defaultSuperMomentRange.x,
            defaultSuperMomentRange.y
        );

        FoodPart.SuperMoment_V2 = CookingMech.Instance.CalculateSuperMomentRange(
            FoodPart.SuperMoment,
            superMomentDuration
        );
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

}
