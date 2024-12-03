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
    #region 肉的音效
    //肉掉落
    public string MeetDrop1 = "Game/Audio/MeetDrop1";
    public string MeetDrop2 = "Game/Audio/MeetDrop2";
    //肉接触
    public string MeetGrab1 = "Game/Audio/MeetGrab1";
    public string MeetGrab2 = "Game/Audio/MeetGrab2";
    public string MeetGrab3 = "Game/Audio/MeetGrab3";
    public string MeetGrab4 = "Game/Audio/MeetGrab4";
    public string MeetGrab5 = "Game/Audio/MeetGrab5";
    public string MeetGrab6 = "Game/Audio/MeetGrab6";
    public string MeetGrab7 = "Game/Audio/MeetGrab7";
    #endregion
    #region 锅的音效
    //抓起锅
    public string PotGrab1 = "Game/Audio/PotGrab1";
    public string PotGrab2 = "Game/Audio/PotGrab2";
    public string PotGrab3 = "Game/Audio/PotGrab3";
    //锅碰撞
    public string PotKnock1 = "Game/Audio/Metalknock1";
    public string PotKnock2 = "Game/Audio/Metalknock2";
    public string PotKnock3 = "Game/Audio/Metalknock3";
    public string PotKnock4 = "Game/Audio/Metalknock4";
    public string PotKnock5 = "Game/Audio/Metalknock5";
    public string PotKnock6 = "Game/Audio/Metalknock6";
    //锅摔落
    public string PotDrop1 = "Game/Audio/PotDrop1";
    public string PotDrop2 = "Game/Audio/PotDrop2";
    public string PotDrop3 = "Game/Audio/PotDrop3";
    #region 木碟音效
    public string WoodPlateDrop = "Game/Audio/WoodPlateDrop";
    public string WoodPlateGrab = "Game/Audio/WoodPlateGrab";
    #endregion
    public string BGM = "Game/Audio/BGM/MIRWHITE_TIME_TO_COOK";
    #endregion

    #region 肉类烹饪音效
    //旧音效
    public string MeatCook1 = "Game/Audio/MeetCooking/MeatCook1";
    public string MeatCook_Bad = "Game/Audio/MeetCooking/MeatCook_Bad";
    public string MeatCook_S = "Game/Audio/MeetCooking/MeatCook_S";
    public string MeatCook_Syes = "Game/Audio/MeetCooking/MeatCook_Syes";
    //肉绿区翻面成功↓
    public string RandomMix_GreenS_A = "Game/Audio/MeetCooking/RandomMix_GreenS_A";//最正常的是这个
    public string RandomMix_GreenS_B = "Game/Audio/MeetCooking/RandomMix_GreenS_B";
    public string RandomMix_GreenS_C = "Game/Audio/MeetCooking/RandomMix_GreenS_C";
    public string RandomMix_GreenS_D = "Game/Audio/MeetCooking/RandomMix_GreenS_D";
    public string RandomMix_GreenS_E = "Game/Audio/MeetCooking/RandomMix_GreenS_E";
    public string RandomMix_GreenS_F = "Game/Audio/MeetCooking/RandomMix_GreenS_F";
    //肉红区超过两秒还不翻面↓
    public string RandomMix_RedS_A = "Game/Audio/MeetCooking/RandomMix_RedS_A";
    public string RandomMix_RedS_B = "Game/Audio/MeetCooking/RandomMix_RedS_B";
    public string RandomMix_RedS_C = "Game/Audio/MeetCooking/RandomMix_RedS_C";
    public string RandomMix_RedS_D = "Game/Audio/MeetCooking/RandomMix_RedS_D";
    public string RandomMix_RedS_E = "Game/Audio/MeetCooking/RandomMix_RedS_E";
    //普通区主音混合
    public string RandomMainMix_Normal_Core = "Game/Audio/MeetCooking/RandomMainMix_Normal_Core";//主音-4.29秒
    public string RandomMainMix_Normal_1 = "Game/Audio/MeetCooking/RandomMainMix_Normal_1";//加入气泡音
    public string RandomMainMix_Normal_2 = "Game/Audio/MeetCooking/RandomMainMix_Normal_2";//加入摩擦颗粒
    public string RandomMainMix_Normal_3 = "Game/Audio/MeetCooking/RandomMainMix_Normal_3";//混合其他烹饪声音
    public string RandomMainMix_Normal_4 = "Game/Audio/MeetCooking/RandomMainMix_Normal_4";//混合其他烹饪声音
    public string RandomMainMix_Normal_5 = "Game/Audio/MeetCooking/RandomMainMix_Normal_5";//混合其他烹饪声音
    //绿区主音混合
    public string RandomMainMix_Green_Core = "Game/Audio/MeetCooking/RandomMainMix_Green_Core";//主音-2秒
    public string RandomMainMix_Green_1 = "Game/Audio/MeetCooking/RandomMainMix_Green_1";
    public string RandomMainMix_Green_2 = "Game/Audio/MeetCooking/RandomMainMix_Green_2";
    public string RandomMainMix_Green_3 = "Game/Audio/MeetCooking/RandomMainMix_Green_3";
    public string RandomMainMix_Green_4 = "Game/Audio/MeetCooking/RandomMainMix_Green_4";
    public string RandomMainMix_Green_5 = "Game/Audio/MeetCooking/RandomMainMix_Green_5";
    //红区主音混合
    public string RandomMainMix_Red_Core = "Game/Audio/MeetCooking/RandomMainMix_Red_Core";//主音-2秒
    public string RandomMainMix_Red_1 = "Game/Audio/MeetCooking/RandomMainMix_Red_1";
    public string RandomMainMix_Red_2 = "Game/Audio/MeetCooking/RandomMainMix_Red_2";
    public string RandomMainMix_Red_3 = "Game/Audio/MeetCooking/RandomMainMix_Red_3";
    public string RandomMainMix_Red_4 = "Game/Audio/MeetCooking/RandomMainMix_Red_4";
    public string RandomMainMix_Red_5 = "Game/Audio/MeetCooking/RandomMainMix_Red_5";
    #endregion

    #region 胜利/失败结算音乐
    public string Victory_Song = "Game/Audio/Victory_Song";//胜利
    public string Fail_Song = "Game/Audio/Fail_Song";//失败
    #endregion

}
