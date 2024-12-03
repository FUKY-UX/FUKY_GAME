using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BossContral : MonoBehaviour
{
    //获取血条图片文件
    public Image Bloodbar;
    //public Contral_Cube Contral_CubeScript;
    public void RefreshBloodBar()
    {
        //获取血量最大值和目前值
        GameObject TheQObject = GameObject.FindWithTag("CUBOSS");
        Contral_Cube Contral_Cube = TheQObject.GetComponent<Contral_Cube>();
        float hp = Contral_Cube.HP;
        float HPMax = Contral_Cube.HPMax;

        //改变血条的填充条
        Bloodbar.fillAmount = hp / HPMax;
    }

    //加血
    //后续可用public void ADDHP(float value)来获取特定的输入值
    public void ADDHP()
    {
        //改变怪物的血量
        GameObject.FindWithTag("CUBOSS").GetComponent<Contral_Cube>().HP += 10;

        //刷新UI
        RefreshBloodBar();
    }

    //减血
    public void ReduceHp()
    {
        GameObject.FindWithTag("CUBOSS").GetComponent<Contral_Cube>().HP -= 10;

        RefreshBloodBar();
    }
}