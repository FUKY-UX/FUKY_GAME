using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meet : InteractedItemBase
{
    private string[] MeetGrabSound;
    private string[] MeetDropSound;
    public bool AfterGrab =false;

    private void Start()
    {
        MeetGrabSound = new string[]
        {
            MusicAndSound_Path.instance.MeetGrab1,
            MusicAndSound_Path.instance.MeetGrab2,
            MusicAndSound_Path.instance.MeetGrab3,
            MusicAndSound_Path.instance.MeetGrab4,
            MusicAndSound_Path.instance.MeetGrab5,
            MusicAndSound_Path.instance.MeetGrab6,
            MusicAndSound_Path.instance.MeetGrab7


        };
        MeetDropSound = new string[]
        {
        MusicAndSound_Path.instance.MeetDrop1,
        MusicAndSound_Path.instance.MeetDrop2,
        };
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (V_Playable)
        {
            AudioManager.instance.PlayRamSound(_audiosource, MeetGrabSound, V_Voulme, 3);
            V_Playable = false;
            V_LastSoundPlay = 0;
        }
    }

    public override void OnGrab()
    {
        AudioManager.instance.PlayRamSound(_audiosource, MeetDropSound, V_Voulme, 1);
    }

    public override void OnRelease()
    {
        AudioManager.instance.PlayRamSound(_audiosource, MeetDropSound, V_Voulme, 2);
    }

}
