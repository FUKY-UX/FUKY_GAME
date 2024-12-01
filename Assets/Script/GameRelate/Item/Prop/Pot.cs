using UnityEngine;
using System;

[Serializable]
public class PotAttrBoard : AttrBoard
{
    [Header("��Ϸ����")]
    [HideInInspector]
    public Pot SelfRef;
    public LayerMask CookFireOn;
    public bool Heating;
    public FireEffect[] Fires;
    
    [Header("��������")]
    public MeshCollider CookCollider;
    public Transform PotCenter;
    [Tooltip("��С���ֵ��������صø���")]
    [Range(0,1)]
    public float PotVsFire=0.5f;
    [Header("������Ч")]
    public string[] PotKnockSound = new string[]
        {
            MusicAndSound_Path.instance.PotKnock1,
            MusicAndSound_Path.instance.PotKnock2,
            MusicAndSound_Path.instance.PotKnock3,
            MusicAndSound_Path.instance.PotKnock4,
            MusicAndSound_Path.instance.PotKnock5,
            MusicAndSound_Path.instance.PotKnock6
        };
    public string[] PotGrabSound = new string[]
        {
            MusicAndSound_Path.instance.PotGrab1,
            MusicAndSound_Path.instance.PotGrab2,
            MusicAndSound_Path.instance.PotGrab3
        };
    public string[] PotDropSound = new string[]
        {
            MusicAndSound_Path.instance.PotDrop1,
            MusicAndSound_Path.instance.PotDrop2,
            MusicAndSound_Path.instance.PotDrop3
        };
    [Header("���Բ���")]
    public bool ShowGizmo;
    [HideInInspector]
    public bool LastHeatInf, CurrHeatInf;

};
public class PotDefaultState : DefaultItemState
{
    protected ItemFSM _MyFsm;
    public DefaultItemAttrBoard _DefAttrBoard;
    public PotAttrBoard _PotAttrBoard;
    public PotDefaultState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, PotAttrBoard Extend_Board)
    {
        _MyFsm = in_Fsm;
        _DefAttrBoard = _defattrboard as DefaultItemAttrBoard;
        _PotAttrBoard = Extend_Board as PotAttrBoard;
    }
    public override void OnTriggerStay(Collider collider)
    {
        _PotAttrBoard.Fires = collider.GetComponentsInChildren<FireEffect>();
        if (_PotAttrBoard.Fires != null) { _PotAttrBoard.Heating = true; _MyFsm.SwitchState(ItemState_Type.State1); }
        Debug.Log(collider.GetComponentInChildren<FireEffect>());
    }
    public override void OnGrab()
    {
        _DefAttrBoard._collider.isTrigger=true;
        _PotAttrBoard.CookCollider.enabled = true;
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _PotAttrBoard.PotGrabSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnRelease()
    {
        _DefAttrBoard._collider.isTrigger = false;
        _PotAttrBoard.CookCollider.enabled = false;
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _PotAttrBoard.PotDropSound, _DefAttrBoard.V_Voulme, 2);
    }
}
public class PotHeatingState : DefaultItemState
{
    protected ItemFSM _MyFsm;
    public DefaultItemAttrBoard _DefAttrBoard;
    public PotAttrBoard _PotAttrBoard;
    public PotHeatingState(ItemFSM in_Fsm, DefaultItemAttrBoard _defattrboard, PotAttrBoard Extend_Board)
    {
        _MyFsm = in_Fsm;
        _DefAttrBoard = _defattrboard as DefaultItemAttrBoard;
        _PotAttrBoard = Extend_Board as PotAttrBoard;
    }
    public override void OnTriggerStay(Collider collider)
    {
        _PotAttrBoard.Fires = collider.GetComponentsInChildren<FireEffect>();
        if (_PotAttrBoard.Fires == null) { _PotAttrBoard.Heating = false; _MyFsm.SwitchState(ItemState_Type.Default);}
        //���»����͸����
        if (_PotAttrBoard.Fires.Length > 0)
        {
            Transform EffectParent = _PotAttrBoard.Fires[0].transform.parent;
            float Y = Mathf.Clamp((_DefAttrBoard._rigidbody.position - EffectParent.position).y, 0, 1);
            Y *= _PotAttrBoard.PotVsFire;
            foreach (FireEffect fire in _PotAttrBoard.Fires)
            {
                fire.Opacity = Y;
            }
        }

    }
    public override void OnGrab()
    {
        _DefAttrBoard._collider.isTrigger = true;
        _PotAttrBoard.CookCollider.enabled = true;
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _PotAttrBoard.PotGrabSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnRelease()
    {
        _DefAttrBoard._collider.isTrigger = false;
        _PotAttrBoard.CookCollider.enabled = false;
        AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _PotAttrBoard.PotDropSound, _DefAttrBoard.V_Voulme, 2);
    }
    public override void OnRidigibodyEnter(Collision collision)
    {
        //��Ч
        if (_DefAttrBoard.V_Playable)
        {
            AudioManager.instance.PlayRamSound(_DefAttrBoard._audiosource, _PotAttrBoard.PotKnockSound, _DefAttrBoard.V_Voulme/2, 3);
            _DefAttrBoard.V_Playable = false;
            _DefAttrBoard.V_LastSoundPlay = 0;
        }
    }
    public override void OnRidigibodyStay(Collision collision)
    {
        //ʳ���߼�
        FoodItemBase Food =collision.collider.GetComponentInParent<FoodItemBase>();
        if (Food != null) { Food.Food_Cooking(_PotAttrBoard.SelfRef); }//ִ���ڹ���ʳ��������Ϊ
    }
    public override void OnRidigibodyExit(Collision collision)
    {
        //ʳ���߼�
        FoodItemBase Food = collision.collider.GetComponentInParent<FoodItemBase>();
        if (Food != null) { Food.Food_Cooking(_PotAttrBoard.SelfRef); }//ִ���ڹ���ʳ��������Ϊ

    }

}

public class Pot : InteractedItemOrigin
{
    public PotAttrBoard _potAttrBoard; 
    private void Start()
    {
        _MyFsm.AddState(ItemState_Type.Default,new PotDefaultState(_MyFsm, _DefaultAttrBoard, _potAttrBoard));
        _MyFsm.AddState(ItemState_Type.State1, new PotHeatingState(_MyFsm, _DefaultAttrBoard, _potAttrBoard));
        _MyFsm.SwitchState(ItemState_Type.Default);
        _potAttrBoard.SelfRef = this;
    }
    public void OnDrawGizmos()
    {
    }

}
