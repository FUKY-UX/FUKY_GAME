using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabHand :MonoBehaviour, HandState 
{
    private HandFSM _ShandFsm;
    private HandAttributeBoard _SBoard;
    private int ClickCounter;
    private float LastClickTime;
    private bool Idle;
#if FUKYMOUSE
    private bool InRotate;
    private bool InRotateActive;
#else
    private bool InRotate;
    private bool InRotateActive;
    private bool ActiveRotateMode = false;
#endif

    public GrabHand(HandFSM in_handFsm, HandAttributeBoard in_Board)
    {
        _ShandFsm = in_handFsm;
        _SBoard = in_Board as HandAttributeBoard;
    }
    public void OnEnter()
    {
        _SBoard.TouchItemFSM.OnGrab();
        Idle = false;
        _SBoard._HandRender.enabled = false;//关掉手的显示
        _SBoard._HandCollider.isTrigger = true;

        //处理抓住物体的逻辑
        _SBoard.TouchItem.DefaultAttr.Phy._rigidbody.velocity = Vector3.zero;
        _SBoard.TouchItem.DefaultAttr.Phy._rigidbody.useGravity = false;
        _SBoard.TouchItem.DefaultAttr.Phy._rigidbody.freezeRotation = true;
        _SBoard.TouchItem.Change_GrabRigidbody(_SBoard._HandRigidbody);
        _SBoard.TouchItem.DefaultAttr.Phy._rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _SBoard.CurrBackHomeStrength = 0f;
#if FUKYMOUSE
        InRotate = false;
#endif

    }
    public void OnExit()
    {
        if(Idle== true) { return; }
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
    }
    public void OnFixUpdate()
    {
        Vector3 ForceDir;
        Quaternion CurrRotate;
        float ItemGrabFactor = _SBoard.TouchItem.DefaultAttr.Phy.GrabTimeFactor;
#if FUKYMOUSE
        if (FUKYMouse.Instance.Left_pressed && FUKYMouse.Instance.enabled == true)
        {
            _SBoard._HandPos.localPosition = _SBoard._HandPos.localPosition + FUKYMouse.Instance.deltaTranslate * FUKYMouse.Instance.PressureValue;
        }
        ForceDir = _SBoard._HandPos.position - _SBoard._LastHandPos;
        CurrRotate = Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f);
#else
        ForceDir = _SBoard._HandPos.position - _SBoard._LastHandPos;
        CurrRotate =  _SBoard.TouchItem.ItemAdjRotation * Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f);
#endif

        if (!_SBoard.IsCaught)
        {
            float lerpFactor = _SBoard._GrabTranslateTime > Mathf.Epsilon? 
                (_SBoard._TranslateUsingTime / _SBoard._GrabTranslateTime) * ItemGrabFactor: 1f; // 如果时间为0，直接使用目标旋转
            Quaternion ITP_Rotation = Quaternion.Lerp(_SBoard.TouchItemRigidbody.rotation, CurrRotate, lerpFactor);

            _SBoard.TouchItemRigidbody.MoveRotation(ITP_Rotation);
            _SBoard.TouchItemRigidbody.AddForce(ForceDir * Mathf.Lerp(0, _SBoard.DragStrength, _SBoard._TranslateUsingTime / _SBoard._GrabTranslateTime * ItemGrabFactor) * Time.deltaTime);
            
            _SBoard._LastHandPos = _SBoard.TouchItemRigidbody.transform.position;
            _SBoard._TranslateUsingTime += Time.deltaTime;
            if ((_SBoard._HandPos.position - _SBoard.TouchItemRigidbody.transform.position).magnitude < 0.1f && Quaternion.Angle(CurrRotate, _SBoard.TouchItemRigidbody.gameObject.transform.rotation) < 0.1f)
            {
                _SBoard.CurrBackHomeStrength = 0f;
                _SBoard._TranslateUsingTime = 0;
                _SBoard.IsCaught = true;
            }
            return;
        }

#if FUKYMOUSE //如果开浮奇的话就提供旋转控制方式
        if (FUKYMouse.Instance.Right_pressed)
        {
            if (!InRotate)
            {
                _SBoard.RotationManager.Earth = _SBoard.TouchItem.transform;
                _SBoard.RotationManager.Finger.position = _SBoard.TouchItem.transform.position;
                _SBoard.RotationManager.SelectMode = true;
                InRotate = true;
                return;
            }
            _SBoard.RotationManager.UpdateGlowLine();
            //移动小球,越压越慢
            //需要在scene中手动移动fukyball来演示
            Vector3 offset = _SBoard.TouchItem.gameObject.transform.position - _SBoard.RotationManager.Finger.position;
            if (offset.magnitude > _SBoard.Range)
            {
                _SBoard.RotationManager.Finger.position =
                            _SBoard.TouchItem.gameObject.transform.position + offset.normalized * _SBoard.Range;
            }// 超出半径时，强制到球体表面
            //如果在按左键过程中力度强一定程度，就进入控制模式
            if (FUKYMouse.Instance.PressureValue > 0.4f)
            {
                if (!InRotateActive)//如果初次进入循环
                {
                    _SBoard.RotationManager.SelectMode = false;
                    _SBoard.RotationManager.StartRotation();
                    InRotateActive = true;
                    return;
                }
                //这里是按压力度大时的旋转状态预览
                _SBoard.CurrAdjRotation = _SBoard.RotationManager.ApplyRelativeRotation();
                _SBoard.TouchItem.transform.rotation = _SBoard.CurrAdjRotation * _SBoard.TouchItem.ItemAdjRotation * Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f);
                return;
            }
            //如果按压力度减小，就退出并应用旋转变换
            _SBoard.TouchItem.ItemAdjRotation = _SBoard.CurrAdjRotation * _SBoard.TouchItem.ItemAdjRotation;
            return;
        }
        if (InRotate)//如果没有按右键了，就重置下状态
        {
            InRotateActive = false;
            InRotate = false;
            _SBoard.PlayerBaseMovement.IsDisAbleMovementAndLook = false;
        }

#else
        if (Input.GetKeyDown(KeyCode.R))
        {
            _SBoard.ObserveRotateMode =!_SBoard.ObserveRotateMode;
            ActiveRotateMode = false;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            ActiveRotateMode = !ActiveRotateMode;
        }
        if (_SBoard.ObserveRotateMode)
        {
            _SBoard.PlayerBaseMovement.IsDisAbleMovementAndLook = true;
            if (!InRotate)
            {
                _SBoard.RotationManager.Earth = _SBoard.TouchItemRigidbody.transform;
                _SBoard.RotationManager.Finger.position = _SBoard.TouchItemRigidbody.position;
                _SBoard.RotationManager.SelectMode = true;
                _SBoard.TouchCollider.isTrigger = true;
                InRotate = true;
                return;
            }
            //移动小球,越压越慢
            //需要在scene中手动移动fukyball来演示
            Vector3 offset = _SBoard.RotationManager.Finger.position - _SBoard.TouchItemRigidbody.position;
            if (offset.magnitude > _SBoard.Range){_SBoard.RotationManager.Finger.position = 
                            _SBoard.TouchItem.gameObject.transform.position + offset.normalized * _SBoard.Range;}// 超出半径时，强制到球体表面
            //如果在按左键过程中力度强一定程度，就进入控制模式
            if (ActiveRotateMode)
            {
                if (!InRotateActive)//如果初次进入循环
                {
                    _SBoard.RotationManager.SelectMode = false;
                    _SBoard.RotationManager.StartRotation();
                    InRotateActive = true;
                    return;
                }
                //这里是按压力度大时的旋转状态预览
                _SBoard.CurrAdjRotation = _SBoard.RotationManager.ApplyRelativeRotation();
                _SBoard.TouchItem.transform.rotation = _SBoard.CurrAdjRotation * _SBoard.TouchItem.ItemAdjRotation * Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f);
                return;
            }
            //如果按压力度减小，就退出并应用旋转变换
            _SBoard.TouchItem.ItemAdjRotation = _SBoard.CurrAdjRotation * _SBoard.TouchItem.ItemAdjRotation;
            return;
        }
        if (InRotate)//如果没有按右键了，就重置下状态
        {
            _SBoard.RotationManager.SelectMode = false;
            _SBoard.RotationManager.inRotationMode = false;
            _SBoard.RotationManager.Earth = null;
            ActiveRotateMode = false;
            InRotateActive = false;
            InRotate = false;
            _SBoard.TouchCollider.isTrigger = false;
            _SBoard.PlayerBaseMovement.IsDisAbleMovementAndLook = false;
        }
#endif

        _SBoard.TouchItemRigidbody.AddForce(ForceDir * _SBoard.DragStrength * Time.deltaTime);
        Quaternion DeltaRotation = Quaternion.Lerp(CurrRotate, _SBoard.TouchItemRigidbody.rotation, _SBoard.RotateSmooth * Time.deltaTime);
        _SBoard.TouchItemRigidbody.MoveRotation(DeltaRotation);

        _SBoard._LastHandPos = _SBoard.TouchItemRigidbody.transform.position;

    }
    public void OnUpdate()
    {
        _SBoard.TouchItemFSM.Grabing();
#if FUKYMOUSE //转换状态
        if (Input.GetMouseButtonDown(0) || FUKYMouse.Instance.Left_Down)
        {
            LastClickTime = Time.time;
            ClickCounter++;
            if (ClickCounter >= 2)
            {
                _SBoard.TouchItem.DropItem_DelayPick();

                _ShandFsm.SwitchState(HandState_Type.Default);
            }

        }
        if(Time.time - LastClickTime > 0.7f)
        {
            ClickCounter =0;
        }
        if (!FUKYMouse.Instance.isMouseFloating)
        {
            Idle = true;
            _ShandFsm.SwitchState(HandState_Type.Idle);
        }
#else
        if (Input.GetMouseButtonDown(0) && !_SBoard.ObserveRotateMode)
        {
            LastClickTime = Time.time;
            ClickCounter++;
            if (ClickCounter >= 2)
            {
                _SBoard.TouchItem.DropItem_DelayPick();
                _ShandFsm.SwitchState(HandState_Type.Default);
            }
        }
        if (Time.time - LastClickTime > 0.7f)
        {
            ClickCounter = 0;
        }
        if (Input.GetKeyDown(KeyCode.E) && !_SBoard.ObserveRotateMode)//用于模仿鼠标浮起时的检测
        {
            Idle = true;
            _ShandFsm.SwitchState(HandState_Type.Idle);
        }

#endif
        // 将摄像机的Fov渐变到原来
        _SBoard.RefCamera.fieldOfView = Mathf.Lerp(_SBoard.RefCamera.fieldOfView, _SBoard._originalFov, _SBoard._fovChangeSpeed * Time.deltaTime);


    }
}