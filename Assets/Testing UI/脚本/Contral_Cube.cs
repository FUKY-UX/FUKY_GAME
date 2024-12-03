using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 控制方块对象的脚本

public class Contral_Cube : MonoBehaviour
{
    //声明生命条的最大最小值
    public float HPMax { get; set; } = 100;
    

    //当前的生命值
    private float hp = 100;

    public float HP
    {
        get => hp;
        set
        {
            //声明生命条的值最小为0，最大为HPMax
            hp = Mathf.Clamp(value, 0, HPMax); 
            
            if(hp<=0)
            {
                //如果hp小于等于0，触发以下效果
                Die();
            }
        }
    }

    //触发效果的逻辑
    public void Die()
    {
        Debug.Log("哈哈哈傻逼");
    }
}
