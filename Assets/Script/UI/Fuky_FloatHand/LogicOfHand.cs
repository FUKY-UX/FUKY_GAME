using UnityEngine;
using Hand_FSM;
using System;

[Serializable]
public class HandAttributeBoard : AttributeBoard
{
    public GameObject _Hand;
    public Rigidbody _HandRigidbody;
    public MeshRenderer _HandRender;
    public Collider _HandCollider;
    public Collider Touchthing;
    public InteractedItemBase TouchThingItemBase;
    public InteractedItem interactedItem;
    public FUKYMouse_MathBase FukyGameBase;
    public Camera RefCamera;
    public float HandSize;
    public float GrabingSmooth = 0.1f;
    public LayerMask HandCanDoLayerMask;

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
        _SBoard.Touchthing = null;
        _SBoard.TouchThingItemBase = null;
    }

    public void OnExit()
    {

    }
    public void OnFixUpdate()
    {
    }
    public void OnUpdate()
    {
        _SBoard._Hand.transform.position = _SBoard.FukyGameBase.FukyHandPos;
        _SBoard._Hand.transform.rotation = Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f)
            * _SBoard.FukyGameBase.FukyHandRotate;
        if (Physics.OverlapSphere(_SBoard._Hand.transform.position, _SBoard.HandSize, _SBoard.HandCanDoLayerMask).Length > 0)
        {
            _SBoard.Touchthing = Physics.OverlapSphere(_SBoard._Hand.transform.position, _SBoard.HandSize, _SBoard.HandCanDoLayerMask)[0];
        }
        else{_SBoard._HandCollider.enabled = true;}
        if (_SBoard.Touchthing != null) { _SBoard.TouchThingItemBase = _SBoard.Touchthing.GetComponentInParent<InteractedItemBase>(); } 
        if (Input.GetMouseButtonDown(0) && _SBoard.TouchThingItemBase != null)
        {
            _SBoard.interactedItem = _SBoard.TouchThingItemBase as InteractedItem;
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
        _SBoard.interactedItem.OnGrab();
        _SBoard._HandRender.enabled = false;
        _SBoard.TouchThingItemBase._rigidbody.useGravity = false;
        _SBoard.TouchThingItemBase._rigidbody.isKinematic = true;
    }

    public void OnExit()
    {
        _SBoard.interactedItem.OnRelease();
        _SBoard._HandRender.enabled = true;
        _SBoard.TouchThingItemBase._rigidbody.useGravity = true;
        _SBoard._HandCollider.enabled = false;
        _SBoard.TouchThingItemBase._rigidbody.isKinematic = false;
    }

    public void OnFixUpdate()
    {
        _SBoard.TouchThingItemBase._rigidbody.velocity = Vector3.zero;
        
        _SBoard.TouchThingItemBase.transform.position = Vector3.Lerp(_SBoard.TouchThingItemBase.transform.position, _SBoard.FukyGameBase.FukyHandPos, _SBoard.GrabingSmooth * Time.deltaTime);

        _SBoard.TouchThingItemBase.transform.rotation = Quaternion.Lerp(_SBoard.TouchThingItemBase.transform.rotation, _SBoard._Hand.transform.rotation, _SBoard.GrabingSmooth * Time.deltaTime);

    }

    public void OnUpdate()
    {
        _SBoard._Hand.transform.position = _SBoard.FukyGameBase.FukyHandPos;
        _SBoard._Hand.transform.rotation = Quaternion.Euler(0f,_SBoard.RefCamera.transform.rotation.eulerAngles.y,0f)
            * _SBoard.FukyGameBase.FukyHandRotate;

        if (Input.GetMouseButtonUp(0))
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
        Gizmos.DrawSphere(_board._Hand.transform.position, _board.HandSize);
    }
}