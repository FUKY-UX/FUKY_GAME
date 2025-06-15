using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleHand :MonoBehaviour, HandState
{
    private HandFSM _ShandFsm;
    public HandAttributeBoard _SBoard;
    public bool IsThrowing;
    private float _targetFov;
    private float _currentFov;

    public IdleHand(HandFSM in_handFsm, HandAttributeBoard in_Board)
    {
        _ShandFsm = in_handFsm;
        _SBoard = in_Board as HandAttributeBoard;
    }
    public void OnEnter()
    {
        _SBoard.CurrBackHomeStrength = 0f;
        _currentFov = _SBoard._originalFov;
        _targetFov = _SBoard._originalFov;
    }
    public void OnExit()
    {
        if (IsThrowing)
        {
            if(_SBoard.TouchItem != null)
            {
                _SBoard._HandRigidbody.position = _SBoard.TouchItemRigidbody.position;
                _SBoard.TouchItem.DefaultAttr.Phy._rigidbody.freezeRotation = false;
                _SBoard._HandRender.enabled = true;
                _SBoard.IsCaught = false;
                _SBoard.TouchItem.DefaultAttr.Phy._rigidbody.isKinematic = false;
                _SBoard.TouchItem.DefaultAttr.Phy._rigidbody.useGravity = true;
                _SBoard.TouchItem.ReSet_GrabRigidbody();
                _SBoard.TouchItem.DefaultAttr.Phy._rigidbody.velocity = _SBoard._HandRigidbody.velocity;
                _SBoard.TouchItemFSM.OnRelease();
                _SBoard.TouchItemFSM = null;
                _SBoard.TouchItemRigidbody = null;
                _SBoard.TouchItem = null;
                IsThrowing = false;
            }
            else
            {
                IsThrowing = false;
            }
        }
#if FUKYMOUSE
         _SBoard._HandPos.position = _SBoard.DefaultHandPos.position;
#endif
    }
    public void OnFixUpdate()
    {
        Quaternion CurrRotate;


#if FUKYMOUSE
        if (_SBoard.TouchItemFSM != null && (_SBoard.TouchItemRigidbody.position - _SBoard.DefaultHandPos.position).magnitude > 0.1f)
        {
            Vector3 ForceDir = _SBoard.DefaultHandPos.position - _SBoard.TouchItemRigidbody.transform.position;
            _SBoard.CurrBackHomeStrength += _SBoard.BackHomeStrength;
            _SBoard.TouchItemRigidbody.AddForce(ForceDir * Math.Min(_SBoard.DragStrength, _SBoard.CurrBackHomeStrength) * Time.deltaTime);
            CurrRotate = _SBoard.TouchItem.ItemAdjRotation * Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f);
            Quaternion DeltaRotation = Quaternion.Lerp(CurrRotate, _SBoard.TouchItemRigidbody.rotation, _SBoard.RotateSmooth * Time.deltaTime);
            _SBoard.TouchItemRigidbody.MoveRotation(DeltaRotation);

            if ((_SBoard.DefaultHandPos.position - _SBoard.TouchItemRigidbody.transform.position).magnitude < 0.1f)
            {
                _SBoard.CurrBackHomeStrength = 0f;
            }
        }
        else
        {
            Vector3 ForceDir = _SBoard.DefaultHandPos.position - _SBoard._HandRigidbody.transform.position;
            _SBoard._HandRigidbody.AddForce(ForceDir * _SBoard.DragStrength * Time.deltaTime);
            CurrRotate = Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f);
            _SBoard._HandRigidbody.MoveRotation(CurrRotate);
        }
#else

        if (_SBoard.TouchItemRigidbody != null && (_SBoard.TouchItemRigidbody.position - _SBoard.DefaultHandPos.position).magnitude > 0.1f)
        {
            Vector3 ForceDir = _SBoard.DefaultHandPos.position - _SBoard.TouchItemRigidbody.transform.position;
            _SBoard.CurrBackHomeStrength += _SBoard.BackHomeStrength;
            _SBoard.TouchItemRigidbody.AddForce(ForceDir * Math.Min(_SBoard.DragStrength, _SBoard.CurrBackHomeStrength) * Time.deltaTime);
            CurrRotate = _SBoard.TouchItem.ItemAdjRotation * Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f);
            Quaternion DeltaRotation = Quaternion.Lerp(CurrRotate, _SBoard.TouchItemRigidbody.rotation, _SBoard.RotateSmooth * Time.deltaTime);
            _SBoard.TouchItemRigidbody.MoveRotation(DeltaRotation);
            
            if ((_SBoard.DefaultHandPos.position - _SBoard.TouchItemRigidbody.transform.position).magnitude < 0.1f)
            {
                _SBoard.CurrBackHomeStrength = 0f;
            }
        }
        else
        {
            Vector3 ForceDir = _SBoard.DefaultHandPos.position - _SBoard._HandRigidbody.transform.position;
            _SBoard._HandRigidbody.AddForce(ForceDir * _SBoard.DragStrength * Time.deltaTime);
            CurrRotate = Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f);
            _SBoard._HandRigidbody.MoveRotation(CurrRotate);
        }

#endif

        // 从摄像机向前发射射线
        // 创建从相机中心发出的射线
        Ray centerRay = _SBoard.RefCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hitInfo;
        Debug.DrawRay(centerRay.origin, centerRay.direction * _SBoard.ReachRange);
        if (Physics.Raycast(centerRay, out hitInfo, _SBoard.ReachRange, _SBoard.HandCanDoLayerMask))
        {
            // 成功命中目标层级的碰撞器
            //Debug.Log($"命中物体: {hitInfo.collider.gameObject.name}", hitInfo.collider.gameObject);
            GrabInteractedItemOrigin item = hitInfo.collider.GetComponentInParent<GrabInteractedItemOrigin>();

            if (item != null)
            {
                if (Input.GetMouseButtonDown(0) && _SBoard.TouchItem == null && item.CurrCanPickAble)
                {
                    _SBoard._HandPos.position = item.transform.position;
                    _SBoard._HandRigidbody.position = item.transform.position;
                    _SBoard.TouchItemFSM = item.Item_FSM;
                    _SBoard.TouchItem = item;
                    _SBoard.TouchItemRigidbody = _SBoard.TouchItem.DefaultAttr.Phy._rigidbody;
                    Debug.Log("尝试转换状态");
                    _ShandFsm.SwitchState(HandState_Type.Grab);
                }
            }
            
        }
    }
    public void OnUpdate()
    {
#if FUKYMOUSE
        if (FUKYMouse.Instance.isMouseFloating)
        {
            if (_SBoard.TouchItemFSM != null)
            {
                _ShandFsm.SwitchState(HandState_Type.Grab);
            }
            else
            {
                _ShandFsm.SwitchState(HandState_Type.Default);
            }
        }
        if ((Input.GetMouseButtonDown(0) || FUKYMouse.Instance.Left_Down) && _SBoard.TouchItemFSM != null)
        {
            IsThrowing = true;
            _ShandFsm.SwitchState(HandState_Type.Default);
        }
#else
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_SBoard.TouchItem != null)
            {
                _ShandFsm.SwitchState(HandState_Type.Grab);
            }
            else
            {
                _ShandFsm.SwitchState(HandState_Type.Default);
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            IsThrowing = true;
            _ShandFsm.SwitchState(HandState_Type.Default);
        }
#endif

#if FUKYMOUSE
        _targetFov = _SBoard._originalFov * _SBoard._zoomFactor;
        _currentFov = Mathf.Lerp(_currentFov, _targetFov, _SBoard._fovChangeSpeed * Time.deltaTime);
        _SBoard.RefCamera.fieldOfView = _currentFov;
#else
        // 将摄像机的Fov渐变到原来的特定倍数
        _targetFov = _SBoard._originalFov * _SBoard._zoomFactor;
        _currentFov = Mathf.Lerp(_currentFov, _targetFov, _SBoard._fovChangeSpeed * Time.deltaTime);
        _SBoard.RefCamera.fieldOfView = _currentFov;
#endif


    }

}
