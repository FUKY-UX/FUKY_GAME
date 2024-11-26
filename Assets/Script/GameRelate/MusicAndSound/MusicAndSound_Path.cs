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
    #region ÈâµÄÒôÐ§
    //ÈâµôÂä
    public string MeetDrop1 = "Game/Audio/MeetDrop1";
    public string MeetDrop2 = "Game/Audio/MeetDrop2";
    //Èâ½Ó´¥
    public string MeetGrab1 = "Game/Audio/MeetGrab1";
    public string MeetGrab2 = "Game/Audio/MeetGrab2";
    public string MeetGrab3 = "Game/Audio/MeetGrab3";
    public string MeetGrab4 = "Game/Audio/MeetGrab4";
    public string MeetGrab5 = "Game/Audio/MeetGrab5";
    public string MeetGrab6 = "Game/Audio/MeetGrab6";
    public string MeetGrab7 = "Game/Audio/MeetGrab7";
    #endregion
    #region ¹øµÄÒôÐ§
    //×¥Æð¹ø
    public string PotGrab1 = "Game/Audio/PotGrab1";
    public string PotGrab2 = "Game/Audio/PotGrab2";
    public string PotGrab3 = "Game/Audio/PotGrab3";
    //¹øÅö×²
    public string PotKnock1 = "Game/Audio/Metalknock1";
    public string PotKnock2 = "Game/Audio/Metalknock2";
    public string PotKnock3 = "Game/Audio/Metalknock3";
    public string PotKnock4 = "Game/Audio/Metalknock4";
    public string PotKnock5 = "Game/Audio/Metalknock5";
    public string PotKnock6 = "Game/Audio/Metalknock6";
    //¹øË¤Âä
    public string PotDrop1 = "Game/Audio/PotDrop1";
    public string PotDrop2 = "Game/Audio/PotDrop2";
    public string PotDrop3 = "Game/Audio/PotDrop3";
    #region Ä¾µúÒôÐ§
    public string WoodPlateDrop = "Game/Audio/WoodPlateDrop";
    public string WoodPlateGrab = "Game/Audio/WoodPlateGrab";
    #endregion

    #endregion


}
