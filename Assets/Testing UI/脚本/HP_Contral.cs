using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BossContral : MonoBehaviour
{
    //��ȡѪ��ͼƬ�ļ�
    public Image Bloodbar;
    //public Contral_Cube Contral_CubeScript;
    public void RefreshBloodBar()
    {
        //��ȡѪ�����ֵ��Ŀǰֵ
        GameObject TheQObject = GameObject.FindWithTag("CUBOSS");
        Contral_Cube Contral_Cube = TheQObject.GetComponent<Contral_Cube>();
        float hp = Contral_Cube.HP;
        float HPMax = Contral_Cube.HPMax;

        //�ı�Ѫ���������
        Bloodbar.fillAmount = hp / HPMax;
    }

    //��Ѫ
    //��������public void ADDHP(float value)����ȡ�ض�������ֵ
    public void ADDHP()
    {
        //�ı�����Ѫ��
        GameObject.FindWithTag("CUBOSS").GetComponent<Contral_Cube>().HP += 10;

        //ˢ��UI
        RefreshBloodBar();
    }

    //��Ѫ
    public void ReduceHp()
    {
        GameObject.FindWithTag("CUBOSS").GetComponent<Contral_Cube>().HP -= 10;

        RefreshBloodBar();
    }
}