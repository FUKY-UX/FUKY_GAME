
//#define FUKYMOUSE
//==================================
//如果要用默认操控方式就注销掉上面这个编译宏
//==================================



using UnityEngine;
using System;
using Unity.VisualScripting;
using Unity.Collections;
[Serializable]
public class HandAttributeBoard : AttributeBoard
{

    [Header("依赖项")]
    public Rigidbody _HandRigidbody;//只能使用AddForce的方式控制手的位置，千万被设置位置
    public MeshRenderer _HandRender;
    public Collider _HandCollider;
    public Camera RefCamera;
    public Transform _HandPos;
    public FUKY_RotateMtehod RotationManager;
    [Header("运行时手触摸到的物体")]
    public Collider TouchCollider;
    public GrabInteractedItemOrigin TouchItem;
    public ItemFSM TouchItemFSM;
    public Rigidbody TouchItemRigidbody;//使用AddForce的方式控制物品的移动
    [Header("调试参数")]
    public bool ShowGizmo;
    public float HandSize;
    public float DragStrength = 100f;
    [Range(0,100)]
    public float RotateSmooth = 10f;
    public Vector3 _LastHandPos = Vector3.zero;
    public LayerMask HandCanDoLayerMask;
    [Header("物体抓取时")]
    public float _GrabTranslateTime = 0.5f;
    public float _TranslateUsingTime;
    public bool IsCaught = false;

    [Header("第一人称模式")]
    public Transform DefaultHandPos;
    public float BackHomeStrength = 0.01f;
    public float CurrBackHomeStrength = 0f;
    public float ReachRange = 2f;
    public float _originalFov;
    public float _zoomFactor;
    public float _fovChangeSpeed = 3f; // FOV变化速度
    //摄像机控制参数
    [Header("旋转控制")]
    public bool ObserveRotateMode = false;
    public FirstPersonController PlayerBaseMovement;
    public float Range = 2f;
    public float Range_Offset = 1.3f;
    public Quaternion CurrAdjRotation;

}
public class LogicOfHand : MonoBehaviour
{
    [Header("当前手的状态")]
    public HandFSM _HandFSM;
    [Header("手的相关属性")]
    public HandAttributeBoard _HandAttr;
    private void Start()
    {
        _HandAttr._originalFov = _HandAttr.RefCamera.fieldOfView;
        _HandFSM = new HandFSM(_HandAttr);
        //_board._HandPos.position = _board.DefaultHandPos.position; 
        _HandFSM.AddState(HandState_Type.Default,new DefaultHand(_HandFSM,_HandAttr));
        _HandFSM.AddState(HandState_Type.Grab, new GrabHand(_HandFSM, _HandAttr));
        _HandFSM.AddState(HandState_Type.Idle, new IdleHand(_HandFSM, _HandAttr));
        _HandFSM.SwitchState(HandState_Type.Default);

    }

    private void Update()
    {

        _HandFSM.OnUpdate();
    }

    private void FixedUpdate()
    {
        _HandFSM.OnFixUpdate();
    }

    private void OnDrawGizmos()
    {
        if(_HandAttr.ShowGizmo) Gizmos.DrawSphere(_HandAttr._HandRigidbody.transform.position, _HandAttr.HandSize);
    }
    

}
