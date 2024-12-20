using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MusicAndSound_Path :MonoBehaviour
{
    public static MusicAndSound_Path instance;
    public MusicAndSound_Path()
    {
        instance = this;
    }
    #region �����Ч
    //�����
    public string MeetDrop1 = "Game/Audio/MeetDrop1";
    public string MeetDrop2 = "Game/Audio/MeetDrop2";
    //��Ӵ�
    public string MeetGrab1 = "Game/Audio/MeetGrab1";
    public string MeetGrab2 = "Game/Audio/MeetGrab2";
    public string MeetGrab3 = "Game/Audio/MeetGrab3";
    public string MeetGrab4 = "Game/Audio/MeetGrab4";
    public string MeetGrab5 = "Game/Audio/MeetGrab5";
    public string MeetGrab6 = "Game/Audio/MeetGrab6";
    public string MeetGrab7 = "Game/Audio/MeetGrab7";
    #endregion
    #region ������Ч
    //ץ���
    public string PotGrab1 = "Game/Audio/PotGrab1";
    public string PotGrab2 = "Game/Audio/PotGrab2";
    public string PotGrab3 = "Game/Audio/PotGrab3";
    //����ײ
    public string PotKnock1 = "Game/Audio/Metalknock1";
    public string PotKnock2 = "Game/Audio/Metalknock2";
    public string PotKnock3 = "Game/Audio/Metalknock3";
    public string PotKnock4 = "Game/Audio/Metalknock4";
    public string PotKnock5 = "Game/Audio/Metalknock5";
    public string PotKnock6 = "Game/Audio/Metalknock6";
    //��ˤ��
    public string PotDrop1 = "Game/Audio/PotDrop1";
    public string PotDrop2 = "Game/Audio/PotDrop2";
    public string PotDrop3 = "Game/Audio/PotDrop3";
    #region ľ����Ч
    public string WoodPlateDrop = "Game/Audio/WoodPlateDrop";
    public string WoodPlateGrab = "Game/Audio/WoodPlateGrab";
    #endregion
    public string BGM = "Game/Audio/BGM/MIRWHITE_TIME_TO_COOK";
    #endregion

    #region ���������Ч
    //����Ч
    public string MeatCook1 = "Game/Audio/MeetCooking/MeatCook1";
    public string MeatCook_Bad = "Game/Audio/MeetCooking/MeatCook_Bad";
    public string MeatCook_S = "Game/Audio/MeetCooking/MeatCook_S";
    public string MeatCook_Syes = "Game/Audio/MeetCooking/MeatCook_Syes";
    //����������ɹ���
    public string RandomMix_GreenS_A = "Game/Audio/MeetCooking/RandomMix_GreenS_A";//�������������
    public string RandomMix_GreenS_B = "Game/Audio/MeetCooking/RandomMix_GreenS_B";
    public string RandomMix_GreenS_C = "Game/Audio/MeetCooking/RandomMix_GreenS_C";
    public string RandomMix_GreenS_D = "Game/Audio/MeetCooking/RandomMix_GreenS_D";
    public string RandomMix_GreenS_E = "Game/Audio/MeetCooking/RandomMix_GreenS_E";
    public string RandomMix_GreenS_F = "Game/Audio/MeetCooking/RandomMix_GreenS_F";
    //������������뻹�������
    public string RandomMix_RedS_A = "Game/Audio/MeetCooking/RandomMix_RedS_A";
    public string RandomMix_RedS_B = "Game/Audio/MeetCooking/RandomMix_RedS_B";
    public string RandomMix_RedS_C = "Game/Audio/MeetCooking/RandomMix_RedS_C";
    public string RandomMix_RedS_D = "Game/Audio/MeetCooking/RandomMix_RedS_D";
    public string RandomMix_RedS_E = "Game/Audio/MeetCooking/RandomMix_RedS_E";
    //��ͨ���������
    public string RandomMainMix_Normal_Core = "Game/Audio/MeetCooking/RandomMainMix_Normal_Core";//����-4.29��
    public string RandomMainMix_Normal_1 = "Game/Audio/MeetCooking/RandomMainMix_Normal_1";//����������
    public string RandomMainMix_Normal_2 = "Game/Audio/MeetCooking/RandomMainMix_Normal_2";//����Ħ������
    public string RandomMainMix_Normal_3 = "Game/Audio/MeetCooking/RandomMainMix_Normal_3";//��������������
    public string RandomMainMix_Normal_4 = "Game/Audio/MeetCooking/RandomMainMix_Normal_4";//��������������
    public string RandomMainMix_Normal_5 = "Game/Audio/MeetCooking/RandomMainMix_Normal_5";//��������������
    //�����������
    public string RandomMainMix_Green_Core = "Game/Audio/MeetCooking/RandomMainMix_Green_Core";//����-2��
    public string RandomMainMix_Green_1 = "Game/Audio/MeetCooking/RandomMainMix_Green_1";
    public string RandomMainMix_Green_2 = "Game/Audio/MeetCooking/RandomMainMix_Green_2";
    public string RandomMainMix_Green_3 = "Game/Audio/MeetCooking/RandomMainMix_Green_3";
    public string RandomMainMix_Green_4 = "Game/Audio/MeetCooking/RandomMainMix_Green_4";
    public string RandomMainMix_Green_5 = "Game/Audio/MeetCooking/RandomMainMix_Green_5";
    //�����������
    public string RandomMainMix_Red_Core = "Game/Audio/MeetCooking/RandomMainMix_Red_Core";//����-2��
    public string RandomMainMix_Red_1 = "Game/Audio/MeetCooking/RandomMainMix_Red_1";
    public string RandomMainMix_Red_2 = "Game/Audio/MeetCooking/RandomMainMix_Red_2";
    public string RandomMainMix_Red_3 = "Game/Audio/MeetCooking/RandomMainMix_Red_3";
    public string RandomMainMix_Red_4 = "Game/Audio/MeetCooking/RandomMainMix_Red_4";
    public string RandomMainMix_Red_5 = "Game/Audio/MeetCooking/RandomMainMix_Red_5";
    #endregion

    #region ʤ��/ʧ�ܽ�������
    public string Victory_Song = "Game/Audio/Victory_Song";//ʤ��
    public string Fail_Song = "Game/Audio/Fail_Song";//ʧ��
    #endregion

}
