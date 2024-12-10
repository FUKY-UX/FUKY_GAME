using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��������ĳ����״̬�����Թҵ�һ����������

//����õ��ⲿ������ֵ���������ｫ��ֵ���͵�UI����
public class Meat_State : MonoBehaviour
{
    //��������ֵ���Ϊ100
    public float Meat_Max_Hp { get; set; } = 100;

    //��ǰ�ĳ����״̬��˽���ֶ�
    private float Meat_Hp = 0;
    // �Ƿ��Ѿ����������������¼�
    private bool hasFinished = false;

    //�ⲿ�������ͨ������������ Hp_Input ����ȡ���޸� hp
    public float Hp_Input
    {
        //ת��Ϊprivate��Meat_Hpֵ
        get => Meat_Hp;
        set
        {
            //set����ֵ�����Сֵ
            Meat_Hp = Mathf.Clamp(value, 0, Meat_Max_Hp);

            if (Meat_Hp >= 100 && !hasFinished)
            {
                hasFinished = true;  // ����Ϊ�����
                FinshMeat();
            }
            // ����Ƿ���Ҫ���� FinshMeat �Ĵ���״̬
            else if (Meat_Hp == 0 && hasFinished)
            {
                hasFinished = false; // ����Ϊδ���
                StopMeatAudio();
                cookingGoodController.HidePanelExternally();// ���ó����ʱ����UI��������������ã�
            }
        }
    }

    // ���� AudioSource ���
    private AudioSource meatAudioSource;

    // ��Ƶ���������ڱ༭���и�ֵ��
    public AudioClip meatAudioClip;

    // ��������Cooking_Good�ű������ã��Ա��ڳ�������ʱ��ʾUI
    public Cooking_Good cookingGoodController;

    private void Awake()
    {
        // ��ȡ����� AudioSource ���
        meatAudioSource = GetComponent<AudioSource>();
        if (meatAudioSource == null)
        {
            meatAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // ���� AudioSource ����Ƶ����
        if (meatAudioClip != null)
        {
            meatAudioSource.clip = meatAudioClip;
            meatAudioSource.loop = true; // �����Ҫѭ������
        }
        else
        {
            Debug.LogWarning("û�з��� meatAudioClip�����ڱ༭���з���һ����Ƶ������");
        }
    }

    //��������˺󴥷����¼�
    public void FinshMeat()
    {
        Debug.Log("���ǣ�Cook Master");
        PlayMeatAudio();
        // ������ȴﵽ100ʱ������UI���
        if (cookingGoodController != null)
        {
            cookingGoodController.ShowPanelExternally();
        }
        else
        {
            Debug.LogWarning("cookingGoodController δ�����䣬����Inspector��ΪMeat_Stateָ��Cooking_Good���á�");
        }
    }

    // ���ų����������Ƶ
    private void PlayMeatAudio()
    {
        if (meatAudioSource != null && meatAudioClip != null)
        {
            meatAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource �� meatAudioClip δ��ȷ���á�");
        }
    }

    // ֹͣ���ų����������Ƶ
    private void StopMeatAudio()
    {
        if (meatAudioSource != null && meatAudioSource.isPlaying)
        {
            meatAudioSource.Stop();
        }
    }
}
