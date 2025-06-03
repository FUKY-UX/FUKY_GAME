using UnityEngine;
using System;
using Unity.VisualScripting;
[Serializable]
public class HandAttributeBoard : AttributeBoard
{
    [Header("依赖项")]
    public Rigidbody _HandRigidbody;
    public MeshRenderer _HandRender;
    public Collider _HandCollider;
    public Camera RefCamera;
    public Transform _HandPos;
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
    public GameObject Fuky_Range;
    public float Range = 2f;
    public float Range_Offset = 1.3f;
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
        if (FUKYMouse.Instance.Left_pressed)
        {
            _SBoard._HandPos.localPosition = _SBoard._HandPos.localPosition + FUKYMouse.Instance.deltaTranslate * FUKYMouse.Instance.PressureValue;
        }

        Vector3 ForceDir = _SBoard._HandPos.position - _SBoard._LastHandPos;
        Quaternion CurrRotate = Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f) * FUKYMouse.Instance.rawRotation;

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
        if (Input.GetMouseButtonDown(0) || FUKYMouse.Instance.Left_Down && _SBoard.TouchItem != null && _SBoard.TouchItem.CurrCanPickAble)
        {
            _SBoard.TouchItemFSM = _SBoard.TouchItem._MyFsm;
            _ShandFsm.SwitchState(HandState_Type.Grab);
        }
        if (!FUKYMouse.Instance.isMouseFloating)
        {
            _ShandFsm.SwitchState(HandState_Type.Idle);
        }
    }
}
public class GrabHand :MonoBehaviour, HandState 
{
    private HandFSM _ShandFsm;
    private HandAttributeBoard _SBoard;
    private int ClickCounter;
    private float LastClickTime;
    private bool Idle;
    private bool InRotate;
    private bool InRotateActive;
    private Quaternion Init_Rota_Range_LookAt; // 记录初始LookAt旋转
    private Quaternion Init_Delta_Rotation; // 记录初始旋转差
    public GrabHand(HandFSM in_handFsm, HandAttributeBoard in_Board)
    {
        _ShandFsm = in_handFsm;
        _SBoard = in_Board as HandAttributeBoard;
    }
    public void OnEnter()
    {
        Idle = false;
        _SBoard._HandCollider.isTrigger = true;
        _SBoard.TouchItem.Default.Phy._rigidbody.velocity = Vector3.zero;
        _SBoard.TouchItem.Default.Phy._rigidbody.freezeRotation = true;
        _SBoard.TouchItem.Default.Phy._rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _SBoard._HandRender.enabled = false;
        InRotate = false;

    }
    public void OnExit()
    {
        if(Idle== true) { return; }
        _SBoard.TouchItem.Default.Phy._rigidbody.freezeRotation = false;
        _SBoard._HandRender.enabled = true;
        _SBoard.IsCaught = false;
        _SBoard.TouchItem.Default.Phy._rigidbody.isKinematic = false;
        _SBoard.TouchItem.Default.Phy._rigidbody.velocity = _SBoard._HandRigidbody.velocity;
        _SBoard.TouchItemFSM.OnRelease();
        _SBoard.TouchItemFSM = null;
        _SBoard.TouchItem = null;
    }
    public void OnFixUpdate()
    {
        float ItemGrabFactor = _SBoard.TouchItem.Default.Phy.GrabTimeFactor;
        if (FUKYMouse.Instance.Left_pressed)
        {
            _SBoard._HandPos.localPosition = _SBoard._HandPos.localPosition + FUKYMouse.Instance.deltaTranslate * FUKYMouse.Instance.PressureValue;
        }
        Vector3 TouchingItemRigiPos = _SBoard.TouchItem.Default.Phy._rigidbody.transform.position;
        Vector3 HandOriRigiPos = _SBoard._HandRigidbody.transform.position;
        Quaternion ItemQuat = _SBoard.TouchItem.Default.Phy._rigidbody.transform.rotation;
        
        Vector3 ForceDir = _SBoard._HandPos.position - _SBoard._LastHandPos;
        Quaternion CurrRotate  = _SBoard.TouchItem.CustomRotateOffset * Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f) * FUKYMouse.Instance.rawRotation;

        _SBoard._HandRigidbody.AddForce(ForceDir * _SBoard.DragStrength * Time.deltaTime);
        _SBoard._HandRigidbody.transform.rotation = Quaternion.Lerp(_SBoard._HandRigidbody.transform.rotation, CurrRotate, _SBoard.RotateSmooth * Time.deltaTime);

        if (!_SBoard.IsCaught)
        {
            _SBoard.TouchItem.gameObject.transform.rotation = Quaternion.Lerp(_SBoard.TouchItem.Default.Phy._rigidbody.transform.rotation,
            CurrRotate, _SBoard._TranslateUsingTime / _SBoard._GrabTranslateTime * ItemGrabFactor);// 旋转对象的插值直接转成触摸对象
            _SBoard.TouchItem.Default.Phy._rigidbody.transform.position = Vector3.Lerp(TouchingItemRigiPos, HandOriRigiPos, _SBoard._TranslateUsingTime/_SBoard._GrabTranslateTime * ItemGrabFactor);
            
            _SBoard._TranslateUsingTime += Time.deltaTime;
            if (_SBoard._TranslateUsingTime > _SBoard._GrabTranslateTime) 
            {
                _SBoard.TouchItem.Default.Phy._rigidbody.isKinematic = true;
                _SBoard.IsCaught = true; 
                _SBoard._TranslateUsingTime = 0;
                _SBoard.TouchItemFSM.OnGrab();
            }
            _SBoard._LastHandPos = _SBoard._HandRigidbody.transform.position;
            return;
        }
        if (FUKYMouse.Instance.Right_pressed)
        {
            //在第一次进入旋转状态的时候，把两个当UI的3D物体移动到操控对象上
            if (!InRotate)
            {
                _SBoard.Fuky_Ball.transform.position = _SBoard.TouchItem.gameObject.transform.position;
                _SBoard.Fuky_Ball.SetActive(true);
                _SBoard.Fuky_Range.SetActive(true);
                InRotate = true;
                return;
            }

            //更新UI的位置
            _SBoard.Fuky_Range.transform.position = _SBoard.TouchItem.gameObject.transform.position;
            //移动小球,越压越慢
            _SBoard.Fuky_Ball.transform.localPosition = _SBoard.Fuky_Ball.transform.localPosition + FUKYMouse.Instance.deltaTranslate * (1-FUKYMouse.Instance.PressureValue);
            Vector3 offset = _SBoard.Fuky_Ball.transform.position - _SBoard.TouchItem.gameObject.transform.position;
            if (offset.magnitude > _SBoard.Range)
            {
                _SBoard.Fuky_Ball.transform.localPosition = _SBoard.TouchItem.gameObject.transform.position + offset.normalized * _SBoard.Range;
            }// 超出半径时，强制到球体表面
            _SBoard.Fuky_Range.transform.localScale = Vector3.one * offset.magnitude * _SBoard.Range_Offset;        //更新UI
            //如果在按左键过程中力度强一定程度，就进入控制模式
            if (FUKYMouse.Instance.PressureValue > 0.8f)
            {
                _SBoard.Fuky_Range.transform.LookAt(_SBoard.Fuky_Ball.transform);
                if (!InRotateActive)//如果初次进入循环
                {

                    // D                                                  A                           B                              A*D=B
                    //1相对量                                        LookAt旋转               初始物体旋转        <===得到  LookAt旋转 乘相对量
                    //2相对量                                        移动前的range旋转        移动后的range旋转   <===得到  移动前的range旋转 乘相对量
                    Init_Delta_Rotation = Quaternion.Inverse(_SBoard.Fuky_Range.transform.rotation) * _SBoard._HandPos.rotation;
                    Init_Rota_Range_LookAt = _SBoard.Fuky_Range.transform.rotation;
                    //data.holdPos.rotation = Fuky_Range.transform.rotation * Init_Delta_Rotation;
                    InRotateActive = true;
                    return;
                }
                //quaternion CurrPointerRotation_Delta = Quaternion.Inverse(Init_Rota_Range_LookAt) * Fuky_Range.transform.rotation;
                Quaternion CurrHoldRotation_Delta = Init_Delta_Rotation * (Quaternion.Inverse(Init_Rota_Range_LookAt) * _SBoard.Fuky_Range.transform.rotation);

                _SBoard.TouchItem.CustomRotateOffset = _SBoard.Fuky_Range.transform.rotation * CurrHoldRotation_Delta
                    * Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f) * FUKYMouse.Instance.rawRotation;

                _SBoard.TouchItem.transform.rotation = _SBoard.TouchItem.CustomRotateOffset * Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f)
                    * FUKYMouse.Instance.rawRotation;
                return;
            }
            InRotateActive = false;
            return;
        }
        if (InRotate)
        {
            InRotate = false;
            _SBoard.Fuky_Ball.SetActive(false);
            _SBoard.Fuky_Range.SetActive(false);
        }

        //_SBoard.TouchItem.Default.Phy.RubFactor = ForceDir;
        _SBoard.TouchItem.gameObject.transform.rotation = _SBoard._HandRigidbody.transform.rotation;
        _SBoard.TouchItem.Default.Phy._rigidbody.transform.position = _SBoard._HandRigidbody.transform.position;
        _SBoard._LastHandPos = _SBoard._HandRigidbody.transform.position;
        if (!FUKYMouse.Instance.isMouseFloating)
        {
            Idle = true;
            _ShandFsm.SwitchState(HandState_Type.Idle);
        }
    }
    public void OnUpdate()
    {
        _SBoard.TouchItemFSM.Grabing();

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
    }
}
public class IdleHand :MonoBehaviour, HandState
{
    private HandFSM _ShandFsm;
    [SerializeField]
    public HandAttributeBoard _SBoard;

    // 添加射线检测相关变量
    private RaycastHit _rayHit;

    public IdleHand(HandFSM in_handFsm, HandAttributeBoard in_Board)
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
        Vector3 ForceDir = (_SBoard.DefaultHandPos.position - _SBoard._HandRigidbody.transform.position).normalized;
        Quaternion CurrRotate = Quaternion.Euler(0f, _SBoard.RefCamera.transform.rotation.eulerAngles.y, 0f) * FUKYMouse.Instance.rawRotation;
        _SBoard.CurrBackHomeStrength += _SBoard.BackHomeStrength;
        _SBoard._HandRigidbody.AddForce(ForceDir * Math.Min(_SBoard.DragStrength, _SBoard.CurrBackHomeStrength * Time.deltaTime));
        _SBoard._HandRigidbody.transform.rotation = Quaternion.Lerp(_SBoard._HandRigidbody.transform.rotation, CurrRotate, _SBoard.RotateSmooth * Time.deltaTime);
        _SBoard._HandPos.position = _SBoard._HandRigidbody.transform.position;
        if (_SBoard.TouchItemFSM != null)
        {
            _SBoard.TouchItem.Default.Phy._rigidbody.transform.rotation = _SBoard._HandRigidbody.transform.rotation;
            _SBoard.TouchItem.Default.Phy._rigidbody.transform.position = _SBoard._HandRigidbody.transform.position;
        }

        if ((_SBoard.DefaultHandPos.position - _SBoard._HandRigidbody.transform.position).magnitude < 0.1f)
        {
            _SBoard.CurrBackHomeStrength = 0f;

        }
        _SBoard._LastHandPos = _SBoard._HandRigidbody.transform.position;

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
                    Debug.Log("尝试转换状态");
                    _ShandFsm.SwitchState(HandState_Type.Grab);
                }
            }
            
        }
    }
    public void OnUpdate()
    {
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