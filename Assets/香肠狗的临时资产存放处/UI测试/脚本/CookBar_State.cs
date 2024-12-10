using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookBar_State : MonoBehaviour
{
    //��λ���Ƶ�UIͼƬ
    public Image Ѫ��ͼ��;
    //��ȡ�������ݵĿ���Ʒ
    public GameObject �������ݵ���Ʒ;
    //��ֵ�ٶ�
    public float ƽ������ = 3f;
    //Ŀ�������
    private float targetFillAmount;

    // ���������ڿ��Ƴ����������GameObject
    public GameObject �ⲿ���ƽӿ�; // �������Inspector�н���Ҫ��GameObject��ק������

    // ����HP��������Ľű�
    private Object_Controller Object_ControllerScript;

    void Start()
    {
        // ��ʼ��Ŀ�������
        if (�������ݵ���Ʒ != null)
        {
            Meat_State Meat_State = �������ݵ���Ʒ.GetComponent<Meat_State>();
            if (Meat_State != null && Meat_State.Meat_Max_Hp != 0)
            {
                targetFillAmount = Meat_State.Hp_Input / Meat_State.Meat_Max_Hp;
                Ѫ��ͼ��.fillAmount = targetFillAmount;
            }
        }
        //��ȡ�ⲿ�ӿڵĽű�����
        if (�ⲿ���ƽӿ� != null)
        {
            Object_ControllerScript = �ⲿ���ƽӿ�.GetComponent<Object_Controller>();
            if (Object_ControllerScript == null)
            {
                Debug.LogWarning("�ⲿ���ƽӿ���δ�ҵ�ObjectController�ű���");
            }
        }
        else
        {
            Debug.LogWarning("�ⲿ���ƽӿ�δ�����䡣");
        }
    }
    void Update()
    {
        // ������ⲿ�ӿڿ��ƽű�����������ͨ���ⲿ�ӿ��Զ�����HP
        if (Object_ControllerScript != null && Object_ControllerScript.�����Զ�����)
        {
            // ����HPֵ
            if (�������ݵ���Ʒ != null)
            {
                Meat_State meatState = �������ݵ���Ʒ.GetComponent<Meat_State>();
                if (meatState != null)
                {
                    meatState.Hp_Input += Object_ControllerScript.������������� * Time.deltaTime;
                    // ȷ��Hp_Input���������ֵ
                    meatState.Hp_Input = Mathf.Clamp(meatState.Hp_Input, 0, meatState.Meat_Max_Hp);
                    ˢ��״̬��();
                }
            }
        }
        // ƽ�����ɵ�Ŀ�������
        if (Ѫ��ͼ��.fillAmount != targetFillAmount)
        {
            Ѫ��ͼ��.fillAmount = Mathf.Lerp(Ѫ��ͼ��.fillAmount, targetFillAmount, ƽ������ * Time.deltaTime);

            // Ϊ��ȷ������ֵ��ȷ�������ڽӽ�Ŀ��ʱֱ�Ӹ�ֵ
            if (Mathf.Abs(Ѫ��ͼ��.fillAmount - targetFillAmount) < 0.01f)
            {
                Ѫ��ͼ��.fillAmount = targetFillAmount;
            }
        }
    }
    /// ˢ��״̬��������Ŀ�������
    public void ˢ��״̬��()
    {
        if (�������ݵ���Ʒ != null)
        {
            Meat_State Meat_State = �������ݵ���Ʒ.GetComponent<Meat_State>();

            if (Meat_State != null && Meat_State.Meat_Max_Hp != 0)
            {
                float x = Meat_State.Hp_Input;
                float y = Meat_State.Meat_Max_Hp;

                // ����Ŀ���������ȷ��y��Ϊ��
                targetFillAmount = Mathf.Clamp(x / y, 0f, 1f);
            }
        }
    }
    //���ӳ������ֵ��ˢ���Ը���״̬
    public void ����10�����()
    {
        if (�������ݵ���Ʒ != null)
        {
            Meat_State Meat_State = �������ݵ���Ʒ.GetComponent<Meat_State>();
            if (Meat_State != null)
            {
                Meat_State.Hp_Input += 10;
                // ȷ��Hp_Input���������ֵ
                Meat_State.Hp_Input = Mathf.Clamp(Meat_State.Hp_Input, 0, Meat_State.Meat_Max_Hp);
                ˢ��״̬��();
            }
        }
    }
    public void ���ó����()
    {
        if (�������ݵ���Ʒ != null)
        {
            Meat_State Meat_State = �������ݵ���Ʒ.GetComponent<Meat_State>();
            if (Meat_State != null)
            {
                Meat_State.Hp_Input = 0;
                ˢ��״̬��();
            }
        }
    }
}
