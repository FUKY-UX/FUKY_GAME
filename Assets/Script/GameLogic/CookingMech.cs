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
    public delegate void CookingMomentFinish(CookingMoment moment,Pot pot,MeatBase meat);
    public delegate void CookingStateChangeHandler(CookingMoment moment,MeatBase meat);
    public event CookingMomentFinish OnCookingMomentFinish;
    public event CookingStateChangeHandler OnCookingStateChange;
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
        // 1. 参数安全检查
        if (pot == null || pot._potAttrBoard == null || food == null)
        {
            Debug.LogWarning("无效的锅或食物参数");
            return;
        }

        // 2. 获取或创建该锅对应的食物列表
        if (!Instance.MeatCooking_FacePair.TryGetValue(pot._potAttrBoard.Me, out var foodList))
        {
            // 如果锅不存在于字典中，创建新条目
            foodList = new List<MeatBase>();
            Instance.MeatCooking_FacePair[pot._potAttrBoard.Me] = foodList;
        }

        // 3. 检查是否已存在该食物
        if (!foodList.Contains(food))
        {
            foodList.Add(food);
            Debug.Log($"已注册食物 {food.name} 到锅 {pot.name}");
        }
        else
        {
            Debug.Log($"食物 {food.name} 已在锅 {pot.name} 的烹饪列表中");
        }
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
                    food.AttachToPot(pot);
                }
            }
        }
    }

    // 更新单个食物的烹饪进度
    private void UpdateFoodCooking(Pot pot, MeatBase food)
    {
        // 检查当前烹饪部分是不是空的
        if (food.CookAttr.Cook._CurrCookedPart == null) { Debug.Log("当前烹饪部分是空的"); return; }
        // 检查是不是翻面了
        if (food.CookAttr.Cook._CurrCookedPart != food.CookAttr.Cook._LastCookedPart)
        {
            var LastPartState = food.CookAttr.Cook._LastCookedPart;
            food.CookAttr.Cook._LastCookedPart ??= food.CookAttr.Cook._CurrCookedPart;
            Debug.Log($"上一面'{food.CookAttr.Cook._LastCookedPart}'翻转了，结算情况:{food.CookAttr.Cook._LastCookedPart.CurrPartCookMoment}");
            OnCookingMomentFinish?.Invoke(food.CookAttr.Cook._LastCookedPart.CurrPartCookMoment,pot,food);
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
            CreatNewMoment(LastPartState);
            food.CookAttr.Cook._LastCookedPart = food.CookAttr.Cook._CurrCookedPart;
        }
        FoodPartInf_Def partState = food.CookAttr.Cook._CurrCookedPart;
        //计算总花样度

        food.CookAttr.Cook.Food_TotalCook += food.CookAttr.Cook.UpFace.CookValue + food.CookAttr.Cook.DownFace.CookValue;

        food.CookAttr.Cook.Food_TotalCook = food.CookAttr.Cook.Food_TotalCook * food.CookAttr.Phy.Food_TCR / 2;
        // 计算烹饪增量
        float cookIncrement = Time.deltaTime * defaultCookRate * food.CookAttr.Phy.Food_TCR;
        // 更新时刻值
        partState.MomentInWhere += cookIncrement;
        // 根据烹饪值更新状态
        UpdateCookingState(partState, partState.SuperMoment_V2,food);
    }

    // 更新烹饪状态
    private void UpdateCookingState(FoodPartInf_Def partState, Vector2 superMomentRange,MeatBase meat)
    {
        switch (partState.CurrPartCookMoment)
        {
            case CookingMoment.UnCook:
                if (partState.MomentInWhere > 0)
                {
                    partState.CurrPartCookMoment = CookingMoment.Normal;
                    OnCookingStateChange?.Invoke(CookingMoment.Normal,meat);
                }
                partState.CookValue += defaultCookRate * Time.deltaTime;

                break;
            case CookingMoment.Normal:
                if (partState.MomentInWhere > superMomentRange.x)
                {
                    partState.CurrPartCookMoment = CookingMoment.Super;
                    OnCookingStateChange?.Invoke(CookingMoment.Super,meat);

                }
                partState.CookValue += defaultCookRate * Time.deltaTime;
                break;
            case CookingMoment.Super:
                if (partState.MomentInWhere > superMomentRange.y)
                {
                    partState.CurrPartCookMoment = CookingMoment.Lost;
                    OnCookingStateChange?.Invoke(CookingMoment.Lost,meat);
                }
                partState.CookValue += defaultCookRate * Time.deltaTime;
                break;
            case CookingMoment.Lost:
                if (partState.MomentInWhere > 1.0f)
                {
                    partState.CurrPartCookMoment = CookingMoment.Bad;
                    OnCookingStateChange?.Invoke(CookingMoment.Bad,meat);
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
        FoodPart.CurrPartCookMoment = CookingMoment.UnCook;
        FoodPart.MomentInWhere = 0;
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
    
}
