
//#define FUKYMOUSE
//==================================
//如果要用默认操控方式就注销掉上面这个编译宏
//==================================



using UnityEngine;
using System;
using Unity.VisualScripting;
[Serializable]
public class HandAttributeBoard : AttributeBoard
{

    [Header("依赖项")]
    public Rigidbody _HandRigidbody;//只能使用AddForce的方式控制手的位置，千万被设置位置
    public MeshRenderer _HandRender;
    public Collider _HandCollider;
    public Camera RefCamera;
    public Transform _HandPos;
    public RelativeLookAt RotationManager;
    [Header("运行时手触摸到的物体")]
    public Collider TouchCollider;
    public InteractedItemOrigin TouchItem;
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

    [Header("第一人称时手控制")]
    public Transform DefaultHandPos;
    public float BackHomeStrength = 0.01f;
    public float CurrBackHomeStrength = 0f;
    public float ReachRange = 2f;
    //摄像机控制参数
    [Header("旋转控制")]
    public GameObject Fuky_Ball;
    public float Range = 2f;
    public float Range_Offset = 1.3f;
    public Quaternion CurrAdjRotation;

}
public class DefaultHand : HandState
{
    private bool Idle;
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
        Idle = false;
    }
    public void OnExit()
    {

    }
    public void OnFixUpdate()
    {
        Quaternion CurrRotate;
        Vector3 ForceDir;
#if FUKYMOUSE
        if (FUKYMouse.Instance.Left_pressed)//有鼠标的话
        {
            _SBoard._HandPos.localPosition = _SBoard._HandPos.localPosition + FUKYMouse.Instance.deltaTranslate * FUKYMouse.Instance.PressureValue;
        }
        ForceDir = _SBoard._HandPos.position - _SBoard._LastHandPos;
        CurrRotate = Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f) * FUKYMouse.Instance.rawRotation;


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
        if (_SBoard.TouchCollider != null) { _SBoard.TouchItem = _SBoard.TouchCollider.GetComponentInParent<InteractedItemOrigin>(); }

#if FUKYMOUSE
        if (Input.GetMouseButtonDown(0) || FUKYMouse.Instance.Left_Down && _SBoard.TouchItem != null && _SBoard.TouchItem.CurrCanPickAble)
        {
            _SBoard.TouchItemFSM = _SBoard.TouchItem._MyFsm;
            _SBoard.TouchItemRigidbody = _SBoard.TouchItem.Default.Phy._rigidbody;
            _SBoard.TouchItemFSM = _SBoard.TouchItem._MyFsm;
            _ShandFsm.SwitchState(HandState_Type.Grab);
        }
        if (!FUKYMouse.Instance.isMouseFloating)
        {
            _ShandFsm.SwitchState(HandState_Type.Idle);
        }
#else
        //没有鼠标
        if (Input.GetMouseButtonDown(0) && _SBoard.TouchItem != null && _SBoard.TouchItem.CurrCanPickAble)
        {
            _SBoard.TouchItemFSM = _SBoard.TouchItem._MyFsm;
            _SBoard.TouchItemRigidbody = _SBoard.TouchItem.Default.Phy._rigidbody;
            _ShandFsm.SwitchState(HandState_Type.Grab);
        }
        if (Input.GetKeyDown(KeyCode.E))//用于模仿鼠标浮起时的检测
        {
            Idle = true;
            _ShandFsm.SwitchState(HandState_Type.Idle);
        }
#endif
    }
}
public class GrabHand :MonoBehaviour, HandState 
{
    private HandFSM _ShandFsm;
    private HandAttributeBoard _SBoard;
    private int ClickCounter;
    private float LastClickTime;
    private bool Idle;
#if FUKYMOUSE //FUKY 旋转控制方式
    private bool InRotate;
    private bool InRotateActive;
    private bool DataUpdate;
#endif

    public GrabHand(HandFSM in_handFsm, HandAttributeBoard in_Board)
    {
        _ShandFsm = in_handFsm;
        _SBoard = in_Board as HandAttributeBoard;
    }
    public void OnEnter()
    {
        Idle = false;
        _SBoard._HandRender.enabled = false;//关掉手的显示
        _SBoard._HandCollider.isTrigger = true;

        //处理抓住物体的逻辑
        _SBoard.TouchItem.Default.Phy._rigidbody.velocity = Vector3.zero;
        _SBoard.TouchItem.Default.Phy._rigidbody.useGravity = false;
        _SBoard.TouchItem.Default.Phy._rigidbody.freezeRotation = true;
        _SBoard.TouchItem.Change_GrabRigidbody(_SBoard._HandRigidbody);
        _SBoard.TouchItem.Default.Phy._rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _SBoard.CurrBackHomeStrength = 0f;

#if FUKYMOUSE
        InRotate = false;
#endif

    }
    public void OnExit()
    {
        if(Idle== true) { return; }
        _SBoard._HandRigidbody.position = _SBoard.TouchItemRigidbody.position;
        _SBoard.TouchItem.Default.Phy._rigidbody.freezeRotation = false;
        _SBoard._HandRender.enabled = true;
        _SBoard.IsCaught = false;
        _SBoard.TouchItem.Default.Phy._rigidbody.isKinematic = false;
        _SBoard.TouchItem.Default.Phy._rigidbody.useGravity = true;
        _SBoard.TouchItem.ReSet_GrabRigidbody();
        _SBoard.TouchItem.Default.Phy._rigidbody.velocity = _SBoard._HandRigidbody.velocity;
        _SBoard.TouchItemFSM.OnRelease();
        _SBoard.TouchItemFSM = null;
        _SBoard.TouchItemRigidbody = null;
        _SBoard.TouchItem = null;
    }
    public void OnFixUpdate()
    {
        Vector3 ForceDir;
        Quaternion CurrRotate;
        float ItemGrabFactor = _SBoard.TouchItem.Default.Phy.GrabTimeFactor;
#if FUKYMOUSE
        if (FUKYMouse.Instance.Left_pressed && FUKYMouse.Instance.enabled == true)
        {
            _SBoard._HandPos.localPosition = _SBoard._HandPos.localPosition + FUKYMouse.Instance.deltaTranslate * FUKYMouse.Instance.PressureValue;
        }
        CurrRotate = _SBoard.TouchItem.ItemAdjRotation * Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f) * FUKYMouse.Instance.rawRotation;
        ForceDir = _SBoard._HandPos.position - _SBoard._LastHandPos;
#else
        ForceDir = _SBoard._HandPos.position - _SBoard._LastHandPos;
        CurrRotate =  _SBoard.TouchItem.ItemAdjRotation * Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f);
#endif

        if (!_SBoard.IsCaught)
        {
            Quaternion ITP_Rotation = Quaternion.Lerp(_SBoard.TouchItemRigidbody.rotation, CurrRotate, _SBoard._TranslateUsingTime / _SBoard._GrabTranslateTime * ItemGrabFactor);
            _SBoard.TouchItemRigidbody.MoveRotation(ITP_Rotation);
            _SBoard.TouchItemRigidbody.AddForce(ForceDir * Mathf.Lerp(0, _SBoard.DragStrength, _SBoard._TranslateUsingTime / _SBoard._GrabTranslateTime * ItemGrabFactor) * Time.deltaTime);
            
            _SBoard._LastHandPos = _SBoard.TouchItemRigidbody.transform.position;
            _SBoard._TranslateUsingTime += Time.deltaTime;
            if ((_SBoard._HandPos.position - _SBoard.TouchItemRigidbody.transform.position).magnitude < 0.1f && Quaternion.Angle(CurrRotate, _SBoard.TouchItemRigidbody.gameObject.transform.rotation) < 0.1f)
            {
                _SBoard.CurrBackHomeStrength = 0f;
                _SBoard.TouchItemFSM.OnGrab();
                _SBoard._TranslateUsingTime = 0;
                _SBoard.IsCaught = true;
            }
            return;
        }

        _SBoard.TouchItemRigidbody.AddForce(ForceDir * _SBoard.DragStrength * Time.deltaTime);
        Quaternion DeltaRotation = Quaternion.Lerp(CurrRotate, _SBoard.TouchItemRigidbody.rotation, _SBoard.RotateSmooth * Time.deltaTime);
        _SBoard.TouchItemRigidbody.MoveRotation(DeltaRotation);

#if FUKYMOUSE //如果开浮奇的话就提供旋转控制方式
        if (FUKYMouse.Instance.Right_pressed && FUKYMouse.Instance== true)
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
            _SBoard.Fuky_Ball.transform.localPosition = _SBoard.Fuky_Ball.transform.localPosition + FUKYMouse.Instance.deltaTranslate * (1-FUKYMouse.Instance.PressureValue);
            Vector3 offset = _SBoard.Fuky_Ball.transform.position - _SBoard.TouchItem.gameObject.transform.position;
            if (offset.magnitude > _SBoard.Range){_SBoard.Fuky_Ball.transform.localPosition = 
                    _SBoard.TouchItem.gameObject.transform.position + offset.normalized * _SBoard.Range;}// 超出半径时，强制到球体表面
            //如果在按左键过程中力度强一定程度，就进入控制模式
            if (FUKYMouse.Instance.PressureValue > 0.7f)
            {
                if (!InRotateActive)//如果初次进入循环
                {
                    _SBoard.RotationManager.SelectMode = false;
                    _SBoard.RotationManager.StartRotation();
                    InRotateActive = true;
                    DataUpdate = true;
                    return;
                }
                _SBoard.CurrAdjRotation = _SBoard.RotationManager.ApplyRelativeRotation();
            }
            else if(DataUpdate)
            {
                _SBoard.TouchItem.ItemAdjRotation = _SBoard.CurrAdjRotation * _SBoard.TouchItem.ItemAdjRotation;
                DataUpdate = false;
            }
            _SBoard.TouchItem.transform.rotation = _SBoard.TouchItem.ItemAdjRotation * Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f) * FUKYMouse.Instance.rawRotation;

            return;
        }
        if (InRotate)
        {
            InRotate = false;
            _SBoard.Fuky_Ball.SetActive(false);

        }
#endif
        _SBoard._LastHandPos = _SBoard.TouchItemRigidbody.transform.position;



    }
    public void OnUpdate()
    {
        _SBoard.TouchItemFSM.Grabing();
#if FUKYMOUSE
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
        if (Input.GetMouseButtonDown(0))
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
        if (Input.GetKeyDown(KeyCode.E))//用于模仿鼠标浮起时的检测
        {
            Idle = true;
            _ShandFsm.SwitchState(HandState_Type.Idle);
        }
#endif
    }
}
public class IdleHand :MonoBehaviour, HandState
{
    private HandFSM _ShandFsm;
    [SerializeField]
    public HandAttributeBoard _SBoard;
    public bool IsThrowing;
    public IdleHand(HandFSM in_handFsm, HandAttributeBoard in_Board)
    {
        _ShandFsm = in_handFsm;
        _SBoard = in_Board as HandAttributeBoard;
    }
    public void OnEnter()
    {
        _SBoard.CurrBackHomeStrength = 0f;
    }
    public void OnExit()
    {
        if (IsThrowing)
        {
            if(_SBoard.TouchItemFSM != null)
            {
                _SBoard._HandRigidbody.position = _SBoard.TouchItemRigidbody.position;
                _SBoard.TouchItem.Default.Phy._rigidbody.freezeRotation = false;
                _SBoard._HandRender.enabled = true;
                _SBoard.IsCaught = false;
                _SBoard.TouchItem.Default.Phy._rigidbody.isKinematic = false;
                _SBoard.TouchItem.Default.Phy._rigidbody.useGravity = true;
                _SBoard.TouchItem.ReSet_GrabRigidbody();
                _SBoard.TouchItem.Default.Phy._rigidbody.velocity = _SBoard._HandRigidbody.velocity;
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
        if (_SBoard.TouchItemFSM != null)
        {
            CurrRotate = _SBoard.TouchItem.ItemAdjRotation * Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f) * FUKYMouse.Instance.rawRotation;
        }
        else
        {
            CurrRotate = Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f) * FUKYMouse.Instance.rawRotation;
        }
#else

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

#endif

        // 从摄像机向前发射射线
        // 创建从相机中心发出的射线
        Ray centerRay = _SBoard.RefCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hitInfo;
        Debug.DrawRay(centerRay.origin, centerRay.direction * _SBoard.ReachRange);
        if (Physics.Raycast(centerRay, out hitInfo, _SBoard.ReachRange, _SBoard.HandCanDoLayerMask))
        {
            // 成功命中目标层级的碰撞器
            Debug.Log($"命中物体: {hitInfo.collider.gameObject.name}", hitInfo.collider.gameObject);
            InteractedItemOrigin item = hitInfo.collider.GetComponentInParent<InteractedItemOrigin>();

            if (item != null)
            {
                if (Input.GetMouseButtonDown(0) && _SBoard.TouchItemFSM == null && item.CurrCanPickAble)
                {
                    _SBoard._HandPos.position = item.transform.position;
                    _SBoard._HandRigidbody.position = item.transform.position;
                    _SBoard.TouchItemFSM = item._MyFsm;
                    _SBoard.TouchItem = item;
                    _SBoard.TouchItemRigidbody = _SBoard.TouchItem.Default.Phy._rigidbody;
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
            _SBoard.TouchItem.Default.Phy._rigidbody.freezeRotation = false;
            _SBoard._HandRender.enabled = true;
            _SBoard.IsCaught = false;
            _SBoard.TouchItem.Default.Phy._rigidbody.isKinematic = false;
            _SBoard.TouchItem.Default.Phy._rigidbody.velocity = _SBoard._HandRigidbody.velocity;
            _SBoard.TouchItemFSM.OnRelease();
            _SBoard.TouchItemFSM = null;
            _SBoard.TouchItem = null;
            _ShandFsm.SwitchState(HandState_Type.Default);
        }
#else
        if (Input.GetKeyDown(KeyCode.E))
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
        if (Input.GetMouseButtonDown(0))
        {
            IsThrowing = true;
            _ShandFsm.SwitchState(HandState_Type.Default);
        }
#endif
    }

}

public class LogicOfHand : MonoBehaviour
{
    private HandFSM _handfsm;
    public HandAttributeBoard _board;
    private void Start()
    {
        _handfsm = new HandFSM(_board);
        //_board._HandPos.position = _board.DefaultHandPos.position; 
        _handfsm.AddState(HandState_Type.Default,new DefaultHand(_handfsm,_board));
        _handfsm.AddState(HandState_Type.Grab, new GrabHand(_handfsm, _board));
        _handfsm.AddState(HandState_Type.Idle, new IdleHand(_handfsm, _board));
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