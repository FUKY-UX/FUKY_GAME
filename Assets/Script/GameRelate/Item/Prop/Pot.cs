using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : InteractedItemBase
{
    private string[] PotKnockSound;
    private string[] PotGrabSound;
    private string[] PotDropSound;
    public bool AfterGrab =false;

    private void Start()
    {
        PotKnockSound = new string[]
        {
            MusicAndSound_Path.instance.PotKnock1,
            MusicAndSound_Path.instance.PotKnock2,
            MusicAndSound_Path.instance.PotKnock3,
            MusicAndSound_Path.instance.PotKnock4,
            MusicAndSound_Path.instance.PotKnock5,
            MusicAndSound_Path.instance.PotKnock6
        };
        PotGrabSound = new string[]
        {
            MusicAndSound_Path.instance.PotGrab1,
            MusicAndSound_Path.instance.PotGrab2,
            MusicAndSound_Path.instance.PotGrab3
        };
        PotDropSound = new string[]
{
        MusicAndSound_Path.instance.PotDrop1,
        MusicAndSound_Path.instance.PotDrop2,
        MusicAndSound_Path.instance.PotDrop3
};

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (V_Playable)
        {
            AudioManager.instance.PlayRamSound(_audiosource, PotKnockSound, V_Voulme, 3);
            V_Playable = false;
            V_LastSoundPlay = 0;
        }
    }

    public override void OnGrab()
    {
        AudioManager.instance.PlayRamSound(_audiosource, PotGrabSound, V_Voulme, 2);
    }

    public override void OnRelease()
    {
        AudioManager.instance.PlayRamSound(_audiosource, PotDropSound, V_Voulme, 2);
    }

}
