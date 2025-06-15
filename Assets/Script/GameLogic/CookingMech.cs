using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CookingMech : MonoBehaviour
{
    // ����ģʽ
    public static CookingMech Instance { get; private set; }

    // ��⿲�������
    [Header("���ϵͳ����")]
    [Tooltip("������⿵Ļ����Ȼ����ٶ�")]
    public float defaultCookRate = 0.1f;
    [Tooltip("����ʱ�̣����ڼ���Ŀ�ʼ��������ʲôʱ����ڳ���")]
    public Vector2 defaultSuperMomentRange = new Vector2(0.2f, 0.8f);
    [Tooltip("ʱ�䴰�ڴ�Ŷ�ã�������˵0.1����0.1S��Ӱ���ֵ����ʳ�ĵ�TCRֵ��defaultcookrate")]
    public float superMomentDuration = 0.1f;
    [Tooltip("�����ʳ�ĵ����״̬���ұ��Ǹ�״̬�·���ɹ���Ļ����ȼӳɱ���,ע:UNCOOK����Ч�ģ������ܲ�ճ�����㷭��")]
    public SerializableDictionary<CookingMoment, float> CookMoment_PLUS = new SerializableDictionary<CookingMoment, float>();
    // �¼�ϵͳ
    public delegate void CookingMomentFinish(CookingMoment moment,Pot pot,FoodBase meat);
    public delegate void CookingStateChangeHandler(CookingMoment moment,FoodBase meat);
    public event CookingMomentFinish OnCookingMomentFinish;
    public event CookingStateChangeHandler OnCookingStateChange;
    // ��Ծ����⿹���ʳ��
    public SerializableDictionary<Pot, List<FoodBase>> MeatCooking_FacePair
                                = new SerializableDictionary<Pot, List<FoodBase>>();

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
    // ע����⿹�ϵ
    public void ImBeCooking(Pot pot, FoodBase food)
    {
        // 1. ������ȫ���
        if (pot == null || pot._potAttrBoard == null || food == null)
        {
            Debug.LogWarning("��Ч�Ĺ���ʳ�����");
            return;
        }

        // 2. ��ȡ�򴴽��ù���Ӧ��ʳ���б�
        if (!Instance.MeatCooking_FacePair.TryGetValue(pot._potAttrBoard.Me, out var foodList))
        {
            // ��������������ֵ��У���������Ŀ
            foodList = new List<FoodBase>();
            Instance.MeatCooking_FacePair[pot._potAttrBoard.Me] = foodList;
        }

        // 3. ����Ƿ��Ѵ��ڸ�ʳ��
        if (!foodList.Contains(food))
        {
            foodList.Add(food);
            Debug.Log($"��ע��ʳ�� {food.name} ���� {pot.name}");
        }
        else
        {
            Debug.Log($"ʳ�� {food.name} ���ڹ� {pot.name} ������б���");
        }
    }

    // ȡ����⿹�ϵ
    public void ImNotBeCooking(Pot pot, FoodBase food, Collider FoodPart)
    {
        if (MeatCooking_FacePair.ContainsKey(pot))
        {
            MeatCooking_FacePair[pot].Remove(food);
            food.EndCooking();
        }
    }

    // ����������⿽���
    private void Update()
    {
        foreach (var pair in MeatCooking_FacePair)
        {
            Pot pot = pair.Key;
            if (pot._potAttrBoard.Heating)
            {
                foreach (FoodBase food in pair.Value.ToList())
                {
                    UpdateFoodCooking(pot, food);
                    food.AttachToPot(pot);
                }
            }
        }
    }

    // ���µ���ʳ�����⿽���
    private void UpdateFoodCooking(Pot pot, FoodBase food)
    {
        // ��鵱ǰ��⿲����ǲ��ǿյ�
        if (food.CookAttr.Cook._CurrCookedPart == null) { Debug.Log("��ǰ��⿲����ǿյ�"); return; }
        // ����ǲ��Ƿ�����
        if (food.CookAttr.Cook._CurrCookedPart != food.CookAttr.Cook._LastCookedPart)
        {
            var LastPartState = food.CookAttr.Cook._LastCookedPart;
            food.CookAttr.Cook._LastCookedPart ??= food.CookAttr.Cook._CurrCookedPart;
            Debug.Log($"��һ��'{food.CookAttr.Cook._LastCookedPart}'��ת�ˣ��������:{food.CookAttr.Cook._LastCookedPart.CurrPartCookMoment}");
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
        //�����ܻ�����

        food.CookAttr.Cook.Food_TotalCook += food.CookAttr.Cook.UpFace.CookValue + food.CookAttr.Cook.DownFace.CookValue;

        food.CookAttr.Cook.Food_TotalCook = food.CookAttr.Cook.Food_TotalCook * food.CookAttr.Phy.Food_TCR / 2;
        // �����������
        float cookIncrement = Time.deltaTime * defaultCookRate * food.CookAttr.Phy.Food_TCR;
        // ����ʱ��ֵ
        partState.MomentInWhere += cookIncrement;
        // �������ֵ����״̬
        UpdateCookingState(partState, partState.SuperMoment_V2,food);
    }

    // �������״̬
    private void UpdateCookingState(FoodPartInf_Def partState, Vector2 superMomentRange,FoodBase meat)
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

    // ����"��ϲʱ��"��Χ
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
        // ��ʼ��"��ϲʱ��"
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