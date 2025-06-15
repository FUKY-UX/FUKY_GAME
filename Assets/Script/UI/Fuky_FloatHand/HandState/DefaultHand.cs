using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultHand : HandState
{
    private bool Idle;
    private HandFSM _ShandFsm;
    [SerializeField]
    public HandAttributeBoard _SBoard;
    Vector3 camRight;
    Vector3 camUp;
    Vector3 camForward;
    public DefaultHand(HandFSM in_handFsm, HandAttributeBoard in_Board)
    {
        _ShandFsm = in_handFsm;
        _SBoard = in_Board as HandAttributeBoard;
    }
    public void OnEnter()
    {
        Idle = false;
        _SBoard.TouchItemFSM = null;
    }
    public void OnExit()
    {

    }
    public void OnFixUpdate()
    {
        Quaternion CurrRotate;
        Vector3 ForceDir;
#if FUKYMOUSE
        camRight = _SBoard.RefCamera.transform.right.normalized;
        camUp = _SBoard.RefCamera.transform.up.normalized;
        camForward = _SBoard.RefCamera.transform.forward.normalized;
        if (FUKYMouse.Instance.Left_pressed)//有鼠标的话,沿着视平面的位移
        {
            _SBoard._HandPos.localPosition = _SBoard._HandPos.localPosition + FUKYMouse.Instance.deltaTranslate * FUKYMouse.Instance.PressureValue;
        }
        ForceDir = _SBoard._HandPos.position - _SBoard._LastHandPos;
        CurrRotate = Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f);
#else
            ForceDir = _SBoard._HandPos.position - _SBoard._LastHandPos;
            CurrRotate = Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f);
#endif
        //施加力
        _SBoard._HandRigidbody.AddForce(ForceDir * _SBoard.DragStrength * Time.deltaTime);
        _SBoard._HandRigidbody.transform.rotation = Quaternion.Lerp(_SBoard._HandRigidbody.transform.rotation, CurrRotate, _SBoard.RotateSmooth * Time.deltaTime);
        _SBoard._LastHandPos = _SBoard._HandRigidbody.transform.position;
    }
    public void OnUpdate()
    {
        //用手上的球碰撞获取对象并检查有没有InteractedItemOrigin类，然后如果有且点击了左键就进入GrabHand
        if (Physics.OverlapSphere(_SBoard._HandRigidbody.transform.position, _SBoard.HandSize, _SBoard.HandCanDoLayerMask).Length > 0)
        {
            _SBoard.TouchCollider = Physics.OverlapSphere(_SBoard._HandRigidbody.transform.position, _SBoard.HandSize, _SBoard.HandCanDoLayerMask)[0];
        }
        else{_SBoard._HandCollider.isTrigger = false; _SBoard.TouchItem = null; _SBoard.TouchCollider = null;}
        if (_SBoard.TouchCollider != null) { _SBoard.TouchItem = _SBoard.TouchCollider.GetComponentInParent<GrabInteractedItemOrigin>(); }

#if FUKYMOUSE
        if (Input.GetMouseButtonDown(0) || FUKYMouse.Instance.Left_Down && _SBoard.TouchItem != null && _SBoard.TouchItem.CurrCanPickAble)
        {
            _SBoard.TouchItemFSM = _SBoard.TouchItem.Item_FSM;
            _SBoard.TouchItemRigidbody = _SBoard.TouchItem.DefaultAttr.Phy._rigidbody;
            _ShandFsm.SwitchState(HandState_Type.Grab);
        }
        if (!FUKYMouse.Instance.isMouseFloating)
        {
            Idle = true;
            _ShandFsm.SwitchState(HandState_Type.Idle);
        }
#else
        //没有鼠标
        if (Input.GetMouseButtonDown(0) && _SBoard.TouchItem != null && _SBoard.TouchItem.CurrCanPickAble)
        {
            _SBoard.TouchItemFSM = _SBoard.TouchItem.Item_FSM;
            _SBoard.TouchItemRigidbody = _SBoard.TouchItem.DefaultAttr.Phy._rigidbody;
            _ShandFsm.SwitchState(HandState_Type.Grab);
        }
        if (Input.GetKeyDown(KeyCode.E))//用于模仿鼠标浮起时的检测
        {
            Idle = true;
            _SBoard.TouchCollider = null;
            _SBoard.TouchItemFSM = null;
            _SBoard.TouchItem = null;
            _ShandFsm.SwitchState(HandState_Type.Idle);
        }
#endif
        // 将摄像机的Fov渐变到原来
        _SBoard.RefCamera.fieldOfView = Mathf.Lerp(_SBoard.RefCamera.fieldOfView, _SBoard._originalFov, _SBoard._fovChangeSpeed * Time.deltaTime);

    }
}

