using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���Ʒ������Ľű�

public class Contral_Cube : MonoBehaviour
{
    //�����������������Сֵ
    public float HPMax { get; set; } = 100;
    

    //��ǰ������ֵ
    private float hp = 100;

    public float HP
    {
        get => hp;
        set
        {
            //������������ֵ��СΪ0�����ΪHPMax
            hp = Mathf.Clamp(value, 0, HPMax); 
            
            if(hp<=0)
            {
                //���hpС�ڵ���0����������Ч��
                Die();
            }
        }
    }

    //����Ч�����߼�
    public void Die()
    {
        Debug.Log("������ɵ��");
    }
}
