using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookBar_State : MonoBehaviour
{
    //定位控制的UI图片
    public Image 血条图标;
    //获取输入数据的空物品
    public GameObject 储存数据的物品;
    //插值速度
    public float 平滑过渡 = 3f;
    //目标填充量
    private float targetFillAmount;

    // 新增：用于控制成熟度增长的GameObject
    public GameObject 外部控制接口; // 你可以在Inspector中将需要的GameObject拖拽到这里

    // 引用HP控制物体的脚本
    private Object_Controller Object_ControllerScript;

    void Start()
    {
        // 初始化目标填充量
        if (储存数据的物品 != null)
        {
            Meat_State Meat_State = 储存数据的物品.GetComponent<Meat_State>();
            if (Meat_State != null && Meat_State.Meat_Max_Hp != 0)
            {
                targetFillAmount = Meat_State.Hp_Input / Meat_State.Meat_Max_Hp;
                血条图标.fillAmount = targetFillAmount;
            }
        }
        //获取外部接口的脚本引用
        if (外部控制接口 != null)
        {
            Object_ControllerScript = 外部控制接口.GetComponent<Object_Controller>();
            if (Object_ControllerScript == null)
            {
                Debug.LogWarning("外部控制接口上未找到ObjectController脚本。");
            }
        }
        else
        {
            Debug.LogWarning("外部控制接口未被分配。");
        }
    }
    void Update()
    {
        // 如果有外部接口控制脚本，并且允许通过外部接口自动增加HP
        if (Object_ControllerScript != null && Object_ControllerScript.允许自动增加)
        {
            // 增加HP值
            if (储存数据的物品 != null)
            {
                Meat_State meatState = 储存数据的物品.GetComponent<Meat_State>();
                if (meatState != null)
                {
                    meatState.Hp_Input += Object_ControllerScript.成熟度增长速率 * Time.deltaTime;
                    // 确保Hp_Input不超过最大值
                    meatState.Hp_Input = Mathf.Clamp(meatState.Hp_Input, 0, meatState.Meat_Max_Hp);
                    刷新状态条();
                }
            }
        }
        // 平滑过渡到目标填充量
        if (血条图标.fillAmount != targetFillAmount)
        {
            血条图标.fillAmount = Mathf.Lerp(血条图标.fillAmount, targetFillAmount, 平滑过渡 * Time.deltaTime);

            // 为了确保最终值精确，可以在接近目标时直接赋值
            if (Mathf.Abs(血条图标.fillAmount - targetFillAmount) < 0.01f)
            {
                血条图标.fillAmount = targetFillAmount;
            }
        }
    }
    /// 刷新状态条，设置目标填充量
    public void 刷新状态条()
    {
        if (储存数据的物品 != null)
        {
            Meat_State Meat_State = 储存数据的物品.GetComponent<Meat_State>();

            if (Meat_State != null && Meat_State.Meat_Max_Hp != 0)
            {
                float x = Meat_State.Hp_Input;
                float y = Meat_State.Meat_Max_Hp;

                // 设置目标填充量，确保y不为零
                targetFillAmount = Mathf.Clamp(x / y, 0f, 1f);
            }
        }
    }
    //增加成熟度数值并刷新以更新状态
    public void 增加10成熟度()
    {
        if (储存数据的物品 != null)
        {
            Meat_State Meat_State = 储存数据的物品.GetComponent<Meat_State>();
            if (Meat_State != null)
            {
                Meat_State.Hp_Input += 10;
                // 确保Hp_Input不超过最大值
                Meat_State.Hp_Input = Mathf.Clamp(Meat_State.Hp_Input, 0, Meat_State.Meat_Max_Hp);
                刷新状态条();
            }
        }
    }
    public void 重置成熟度()
    {
        if (储存数据的物品 != null)
        {
            Meat_State Meat_State = 储存数据的物品.GetComponent<Meat_State>();
            if (Meat_State != null)
            {
                Meat_State.Hp_Input = 0;
                刷新状态条();
            }
        }
    }
}
