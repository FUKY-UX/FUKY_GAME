using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//这里是肉的成熟度状态，可以挂到一个空物体上

//这里得到外部输入数值，并在这里将数值发送到UI条上
public class Meat_State : MonoBehaviour
{
    //声明成熟值最大为100
    public float Meat_Max_Hp { get; set; } = 100;

    //当前的成熟度状态，私有字段
    private float Meat_Hp = 0;
    // 是否已经触发过成熟度完成事件
    private bool hasFinished = false;

    //外部代码必须通过公开的属性 Hp_Input 来读取或修改 hp
    public float Hp_Input
    {
        //转化为private的Meat_Hp值
        get => Meat_Hp;
        set
        {
            //set生命值最大最小值
            Meat_Hp = Mathf.Clamp(value, 0, Meat_Max_Hp);

            if (Meat_Hp >= 100 && !hasFinished)
            {
                hasFinished = true;  // 设置为已完成
                FinshMeat();
            }
            // 检查是否需要重置 FinshMeat 的触发状态
            else if (Meat_Hp == 0 && hasFinished)
            {
                hasFinished = false; // 重置为未完成
                StopMeatAudio();
            }
        }
    }

    // 引用 AudioSource 组件
    private AudioSource meatAudioSource;

    // 音频剪辑（可在编辑器中赋值）
    public AudioClip meatAudioClip;

    private void Awake()
    {
        // 获取或添加 AudioSource 组件
        meatAudioSource = GetComponent<AudioSource>();
        if (meatAudioSource == null)
        {
            meatAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // 设置 AudioSource 的音频剪辑
        if (meatAudioClip != null)
        {
            meatAudioSource.clip = meatAudioClip;
            meatAudioSource.loop = true; // 如果需要循环播放
        }
        else
        {
            Debug.LogWarning("没有分配 meatAudioClip，请在编辑器中分配一个音频剪辑。");
        }
    }

    //成熟度满了后触发的事件
    public void FinshMeat()
    {
        Debug.Log("你是，Cook Master");
        PlayMeatAudio();
    }

    // 播放成熟度满的音频
    private void PlayMeatAudio()
    {
        if (meatAudioSource != null && meatAudioClip != null)
        {
            meatAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource 或 meatAudioClip 未正确设置。");
        }
    }

    // 停止播放成熟度满的音频
    private void StopMeatAudio()
    {
        if (meatAudioSource != null && meatAudioSource.isPlaying)
        {
            meatAudioSource.Stop();
        }
    }
}
