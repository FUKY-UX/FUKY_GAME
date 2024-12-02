using UnityEngine;
using System;
[Serializable]
public class HandAttributeBoard : AttributeBoard
{
    [Header("依赖项")]
    public Rigidbody _HandRigidbody;
    public MeshRenderer _HandRender;
    public Collider _HandCollider;
    public FUKYMouse_MathBase FukyGameBase;
    public Camera RefCamera;
    [Header("运行时手触摸到的物体")]
    public Collider TouchCollider;
    public InteractedItemOrigin TouchItem;
    public ItemFSM TouchItemFSM;
    [Header("调试参数")]
    public bool ShowGizmo;
    public float HandSize;
    public float DragStrength = 100f;
    [Range(0,100)]
    public float RotateSmooth = 10f;
    [Range(0, 2f)]
    public Vector3 _LastHandPos = Vector3.zero;
    public LayerMask HandCanDoLayerMask;
    [Header("物体抓取用时")]
    public float _GrabTranslateTime = 0.5f;
    public float _TranslateUsingTime;
    public bool IsCaught = false;

}
public class DefaultHand : HandState
{
    private HandFSM _ShandFsm;
    [SerializeField]
    public HandAttributeBoard _SBoard;
    public DefaultHand(HandFSM in_handFsm, HandAttributeBoard in_Board)
    {
        _ShandFsm = in_handFsm;
        _SBoard = in_Board as HandAttributeBoard;
    }
    public void OnEnter()
    {
    }
    public void OnExit()
    {

    }
    public void OnFixUpdate()
    {
        Vector3 ForceDir = _SBoard.FukyGameBase.FukyHandPos - _SBoard._LastHandPos;
        Quaternion CurrRotate = Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f) * _SBoard.FukyGameBase.FukyHandRotate;

        _SBoard._HandRigidbody.AddForce(ForceDir * _SBoard.DragStrength * Time.deltaTime);
        _SBoard._HandRigidbody.transform.rotation = Quaternion.Lerp(_SBoard._HandRigidbody.transform.rotation, CurrRotate, _SBoard.RotateSmooth * Time.deltaTime);

        _SBoard._LastHandPos = _SBoard._HandRigidbody.transform.position;
    }
    public void OnUpdate()
    {

        if (Physics.OverlapSphere(_SBoard._HandRigidbody.transform.position, _SBoard.HandSize, _SBoard.HandCanDoLayerMask).Length > 0)
        {
            _SBoard.TouchCollider = Physics.OverlapSphere(_SBoard._HandRigidbody.transform.position, _SBoard.HandSize, _SBoard.HandCanDoLayerMask)[0];
        }
        else{_SBoard._HandCollider.isTrigger = false; _SBoard.TouchItem = null; _SBoard.TouchCollider = null;}
        if (_SBoard.TouchCollider != null) { _SBoard.TouchItem = _SBoard.TouchCollider.GetComponentInParent<InteractedItemOrigin>(); } 
        if (Input.GetMouseButtonDown(0) && _SBoard.TouchItem != null)
        {
            _SBoard.TouchItemFSM = _SBoard.TouchItem._MyFsm;
            _ShandFsm.SwitchState(HandState_Type.Grab);
        }
    }
}
public class GrabHand : HandState
{
    private HandFSM _ShandFsm;
    private HandAttributeBoard _SBoard;

    public GrabHand(HandFSM in_handFsm, HandAttributeBoard in_Board)
    {
        _ShandFsm = in_handFsm;
        _SBoard = in_Board as HandAttributeBoard;
    }
    public void OnEnter()
    {
        _SBoard._HandCollider.isTrigger = true;
        _SBoard.TouchItem._DefaultAttrBoard._rigidbody.velocity = Vector3.zero;
        _SBoard.TouchItem._DefaultAttrBoard._rigidbody.freezeRotation = true;
        _SBoard.TouchItem._DefaultAttrBoard._rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _SBoard._HandRender.enabled = false;

    }
    public void OnExit()
    {
        _SBoard.TouchItem._DefaultAttrBoard._rigidbody.freezeRotation = false;
        _SBoard._HandRender.enabled = true;
        _SBoard.IsCaught = false;
        _SBoard.TouchItem._DefaultAttrBoard._rigidbody.isKinematic = false;
        _SBoard.TouchItem._DefaultAttrBoard._rigidbody.velocity = _SBoard._HandRigidbody.velocity;
        _SBoard.TouchItemFSM.OnRelease();
    }
    public void OnFixUpdate()
    {
        float ItemGrabFactor = _SBoard.TouchItem._DefaultAttrBoard.GrabTimeFactor;

        Vector3 TouchingItemRigiPos = _SBoard.TouchItem._DefaultAttrBoard._rigidbody.transform.position;
        Vector3 HandOriRigiPos = _SBoard._HandRigidbody.transform.position;
        Quaternion ItemQuat = _SBoard.TouchItem._DefaultAttrBoard._rigidbody.transform.rotation;

        Vector3 ForceDir = _SBoard.FukyGameBase.FukyHandPos - _SBoard._LastHandPos;
        Quaternion CurrRotate = Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f) * _SBoard.FukyGameBase.FukyHandRotate;

        _SBoard._HandRigidbody.AddForce(ForceDir * _SBoard.DragStrength * Time.deltaTime);
        _SBoard._HandRigidbody.transform.rotation = Quaternion.Lerp(_SBoard._HandRigidbody.transform.rotation, CurrRotate, _SBoard.RotateSmooth * Time.deltaTime);

        if (!_SBoard.IsCaught)
        {
            _SBoard.TouchItem._DefaultAttrBoard._rigidbody.transform.rotation = Quaternion.Lerp(_SBoard.TouchItem._DefaultAttrBoard._rigidbody.transform.rotation,
                CurrRotate, _SBoard._TranslateUsingTime / _SBoard._GrabTranslateTime * ItemGrabFactor);// 旋转对象的插值直接转成触摸对象
            _SBoard.TouchItem._DefaultAttrBoard._rigidbody.transform.position = Vector3.Lerp(TouchingItemRigiPos, HandOriRigiPos, _SBoard._TranslateUsingTime/_SBoard._GrabTranslateTime * ItemGrabFactor);
            
            _SBoard._TranslateUsingTime += Time.deltaTime;
            if (_SBoard._TranslateUsingTime > _SBoard._GrabTranslateTime) 
            {
                _SBoard.TouchItem._DefaultAttrBoard._rigidbody.isKinematic = true;
                _SBoard.IsCaught = true; 
                _SBoard._TranslateUsingTime = 0;
                _SBoard.TouchItemFSM.OnGrab();
            }
            _SBoard._LastHandPos = _SBoard._HandRigidbody.transform.position;
            return;
        }
        
        _SBoard.TouchItem._DefaultAttrBoard.RubFactor = ForceDir;
        _SBoard.TouchItem._DefaultAttrBoard._rigidbody.transform.rotation = _SBoard._HandRigidbody.transform.rotation;
        _SBoard.TouchItem._DefaultAttrBoard._rigidbody.transform.position = _SBoard._HandRigidbody.transform.position;
        _SBoard._LastHandPos = _SBoard._HandRigidbody.transform.position;
    }
    public void OnUpdate()
    {
        _SBoard.TouchItemFSM.Grabing();
        if (Input.GetMouseButtonDown(0))
        {
            _ShandFsm.SwitchState(HandState_Type.Default);
        }
    }
}

public class LogicOfHand : MonoBehaviour
{
    private HandFSM _handfsm;
    public HandAttributeBoard _board;
    private void Start()
    {
        _handfsm = new HandFSM(_board);
        _handfsm.AddState(HandState_Type.Default,new DefaultHand(_handfsm,_board));
        _handfsm.AddState(HandState_Type.Grab, new GrabHand(_handfsm, _board));
        _handfsm.SwitchState(HandState_Type.Default);
    }

    private void Update()
    {
        _handfsm.OnUpdate();
    }

    private void FixedUpdate()
    {
        _handfsm.OnFixUpdate();
    }

    private void OnDrawGizmos()
    {
        if(_board.ShowGizmo) Gizmos.DrawSphere(_board._HandRigidbody.transform.position, _board.HandSize);
    }
}