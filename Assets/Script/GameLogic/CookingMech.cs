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
    public delegate void CookingStateChangeHandler(MeatBase food, Transform part, CookingMoment Moment);
    public event CookingStateChangeHandler OnCookingStateFinish;
    public event CookingStateChangeHandler CookingStateUpdate;
    // ��Ծ����⿹���ʳ��
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
    // ע����⿹�ϵ
    public void ImBeCooking(Pot pot, MeatBase food)
    {
        if (Instance.MeatCooking_FacePair[pot._potAttrBoard.Me].Contains(food))
        {
            return;
        }
        Instance.MeatCooking_FacePair[pot._potAttrBoard.Me].Add(food);
    }

    // ȡ����⿹�ϵ
    public void ImNotBeCooking(Pot pot, MeatBase food, Collider FoodPart)
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
                foreach (MeatBase food in pair.Value.ToList())
                {
                    UpdateFoodCooking(pot, food);
                }
            }
        }
    }

    // ���µ���ʳ�����⿽���
    private void UpdateFoodCooking(Pot pot, MeatBase food)
    {
        if (food.CookAttr.Cook._CurrCookedPart == null) { Debug.Log("��ǰ��⿲����ǿյ�"); return; }
        // ��ȡ��ǰ��⿲�λ
        FoodPartInf_Def partState;
        food.CookAttr.Food_PartState.TryGetValue(food.CookAttr.Cook._CurrCookedPart, out partState);

        if (food.CookAttr.Cook._CurrCookedPart != food.CookAttr.Cook._LastCookedPart && food.CookAttr.Cook._LastCookedPart != null)
        {

            FoodPartInf_Def LastPartState;
            food.CookAttr.Food_PartState.TryGetValue(food.CookAttr.Cook._LastCookedPart, out LastPartState);
            Debug.Log($"��һ��'{food.CookAttr.Cook._LastCookedPart}'��ת�ˣ��������:{LastPartState.CurrPartCookMoment}");
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
            //�����ܻ�����
            foreach (var item in food.CookAttr.Food_PartState.Values)
            {
                food.CookAttr.Cook.Food_TotalCook += item.CookValue;
            }
            food.CookAttr.Cook.Food_TotalCook = food.CookAttr.Cook.Food_TotalCook * food.CookAttr.Phy.Food_TCR / food.CookAttr.Cook.Food_Part.Length;
            // �����������
            float cookIncrement = Time.deltaTime * defaultCookRate * food.CookAttr.Phy.Food_TCR;

            // ����ʱ��ֵ
            partState.MomentInWhere += cookIncrement;

            // �������ֵ����״̬
            UpdateCookingState(partState, partState.SuperMoment_V2);

            CookingStateUpdate?.Invoke(food, food.CookAttr.Cook._CurrCookedPart, partState.CurrPartCookMoment);

        }
    }

    // �������״̬
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
    
        // ������⿽���Ч��
    private void HandleCookingMomentFinish(MeatBase food, Transform part, CookingMoment FinishMoment)
    {
        Debug.Log($"���Բ�����Ч{FinishMoment}");

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

    // �������ʱ����Ч
    private void HandleCookingSound(MeatBase food, Transform part, CookingMoment Moment)
    {
        Debug.Log("�յ���Ч����");
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
