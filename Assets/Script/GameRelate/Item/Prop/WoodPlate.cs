using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodPlate : InteractedItemBase
{
    private string[] WoodPlateSound;

    private void Start()
    {
        WoodPlateSound = new string[]
        {
        MusicAndSound_Path.instance.WoodPlateDrop,
        MusicAndSound_Path.instance.WoodPlateGrab
        };
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (V_Playable)
        {
            AudioManager.instance.PlayRamSound(_audiosource, WoodPlateSound, V_Voulme, 2);
            V_Playable = false;
            V_LastSoundPlay = 0;
        }
    }

    public override void OnGrab()
    {
        AudioManager.instance.PlayRamSound(_audiosource, WoodPlateSound, V_Voulme, 2);
    }

    public override void OnRelease()
    {
        AudioManager.instance.PlayRamSound(_audiosource, WoodPlateSound, V_Voulme, 2);
    }

}
