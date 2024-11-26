using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface InteractedItem
{
    public void OnGrab();
    public void OnRelease();
}

public class InteractedItemBase : MonoBehaviour,InteractedItem
{
    public Rigidbody _rigidbody;
    public AudioSource _audiosource;
    public float V_CoolDown = 1f;
    public float V_Voulme = 1f;

    protected bool V_Playable;
    protected float V_CoolDownOffset = 1f;
    protected float V_LastSoundPlay;
    private void Start()
    {
        V_Playable = true;
    }
    private void Update()
    {
        if (!V_Playable)
        {
            if (V_LastSoundPlay > V_CoolDownOffset)
            {
                V_Playable = true;
                V_CoolDownOffset = V_CoolDown + Random.Range(-0.5f, 0.5f);
            }
            V_LastSoundPlay += Time.deltaTime;
            return;
        }

    }

    public virtual void OnGrab()
    {
        
    }

    public virtual void OnRelease()
    {

    }
}
