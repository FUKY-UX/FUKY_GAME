using UnityEngine;
using System;
using Unity.VisualScripting;
using static UnityEditor.Progress;
[Serializable]
public class HandAttributeBoard : AttributeBoard
{
    [Header("������")]
    public Rigidbody _HandRigidbody;
    public MeshRenderer _HandRender;
    public Collider _HandCollider;
    public Camera RefCamera;
    public Transform _HandPos;
    [Header("����ʱ�ִ�����������")]
    public Collider TouchCollider;
    public InteractedItemOrigin TouchItem;
    public ItemFSM TouchItemFSM;
    [Header("���Բ���")]
    public bool ShowGizmo;
    public float HandSize;
    public float DragStrength = 100f;
    [Range(0,100)]
    public float RotateSmooth = 10f;
    [Range(0, 2f)]
    public Vector3 _LastHandPos = Vector3.zero;
    public LayerMask HandCanDoLayerMask;
    [Header("����ץȡʱ")]
    public float _GrabTranslateTime = 0.5f;
    public float _TranslateUsingTime;
    public bool IsCaught = false;

    [Header("��һ�˳�ʱ�ֿ���")]
    public Transform DefaultHandPos;
    public float BackHomeStrength = 0.01f;
    public float CurrBackHomeStrength = 0f;
    public float ReachRange = 2f;
    //��������Ʋ���
    [Header("��ת����")]
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
    private Quaternion _prevItemRotation; // ��¼��ʼLookAt��ת

    private Vector3 _prevBallPosition;
    private Plane _rotationPlane;
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
            CurrRotate, _SBoard._TranslateUsingTime / _SBoard._GrabTranslateTime * ItemGrabFactor);// ��ת����Ĳ�ֱֵ��ת�ɴ�������
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
            // ��ʼ����תUI
            if (!InRotate)
            {
                InitializeRotationUI();
                return;
            }

            // ������ת����UI
            UpdateRotationUI();

            // ��ѹ��ֵ�㹻ʱ��ִ����ת����
            if (FUKYMouse.Instance.PressureValue > 0.8f)
            {
                if (!InRotateActive)
                {
                    InitializeRotationState();
                    return;
                }

                PerformRotation();
            }
            else
            {
                InRotateActive = false;
            }

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

    #region ��ת���Ʒ���

    /// <summary>
    /// ��ʼ����תUI
    /// </summary>
    private void InitializeRotationUI()
    {
        // ����תUI�ƶ���Ŀ������λ��
        _SBoard.Fuky_Ball.transform.position = _SBoard.TouchItem.transform.position;
        _SBoard.Fuky_Ball.SetActive(true);
        _SBoard.Fuky_Range.SetActive(true);

        // ����С���������ϵ����λ��
        Vector3 randomPoint = UnityEngine.Random.onUnitSphere * _SBoard.Range;
        _SBoard.Fuky_Ball.transform.localPosition = randomPoint;

        InRotate = true;
        InRotateActive = false;
    }

    /// <summary>
    /// ������תUIλ��
    /// </summary>
    private void UpdateRotationUI()
    {
        // ����UIλ��
        _SBoard.Fuky_Range.transform.position = _SBoard.TouchItem.transform.position;

        // �ƶ�С��
        Vector3 ballMovement = FUKYMouse.Instance.deltaTranslate * (1 - FUKYMouse.Instance.PressureValue);
        Vector3 newPosition = _SBoard.Fuky_Ball.transform.localPosition + ballMovement;

        // ����С�����������
        _SBoard.Fuky_Ball.transform.localPosition = Vector3.ClampMagnitude(newPosition, _SBoard.Range);

        // ���·�ΧUI��С
        _SBoard.Fuky_Range.transform.localScale = Vector3.one * _SBoard.Range;
    }

    /// <summary>
    /// ��ʼ����ת״̬
    /// </summary>
    private void InitializeRotationState()
    {
        // ��¼��ʼ��ת״̬
        _prevBallPosition = _SBoard.Fuky_Ball.transform.position;
        _prevItemRotation = _SBoard.TouchItem.transform.rotation;

        // �������ĵ�С�������
        Vector3 ballDirection = (_prevBallPosition - _SBoard.TouchItem.transform.position).normalized;

        // ������ת�ο�����ϵ
        _rotationPlane = new Plane(ballDirection, _SBoard.TouchItem.transform.position);

        InRotateActive = true;
    }

    /// <summary>
    /// ִ����ת����
    /// </summary>
    private void PerformRotation()
    {
        // ��ȡ��ǰС��λ��
        Vector3 currentBallPosition = _SBoard.Fuky_Ball.transform.position;

        // ������ת�ᣨ���ĵ�С���������
        Vector3 rotationAxis = (currentBallPosition - _SBoard.TouchItem.transform.position).normalized;

        // ������ת�Ƕȣ�����С���ƶ�����
        Vector3 moveDirection = (currentBallPosition - _prevBallPosition).normalized;

        // ������ת��
        float rotationAngle = Vector3.SignedAngle(
            _prevBallPosition - _SBoard.TouchItem.transform.position,
            currentBallPosition - _SBoard.TouchItem.transform.position,
            rotationAxis
        );

        // Ӧ����ת
        Quaternion rotationDelta = Quaternion.AngleAxis(rotationAngle, rotationAxis);
        _SBoard.TouchItem.transform.rotation = rotationDelta * _prevItemRotation;

        // ���²ο���
        _prevBallPosition = currentBallPosition;
        _prevItemRotation = _SBoard.TouchItem.transform.rotation;
    }

    /// <summary>
    /// ������תUI
    /// </summary>
    private void CleanupRotationUI()
    {
        _SBoard.Fuky_Ball.SetActive(false);
        _SBoard.Fuky_Range.SetActive(false);
        InRotate = false;
        InRotateActive = false;
    }

    #endregion


}
public class IdleHand :MonoBehaviour, HandState
{
    private HandFSM _ShandFsm;
    [SerializeField]
    public HandAttributeBoard _SBoard;

    // ������߼����ر���
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

        // ���������ǰ��������
        // ������������ķ���������
        Ray centerRay = _SBoard.RefCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hitInfo;
        Debug.DrawRay(centerRay.origin, centerRay.direction * _SBoard.ReachRange);
        if (Physics.Raycast(centerRay, out hitInfo, _SBoard.ReachRange, _SBoard.HandCanDoLayerMask))
        {
            // �ɹ�����Ŀ��㼶����ײ��
            Debug.Log($"��������: {hitInfo.collider.gameObject.name}", hitInfo.collider.gameObject);
            InteractedItemOrigin item = hitInfo.collider.GetComponentInParent<InteractedItemOrigin>();

            if (item != null)
            {
                if (Input.GetMouseButtonDown(0) && _SBoard.TouchItemFSM == null && item.CurrCanPickAble)
                {
                    _SBoard._HandPos.position = item.transform.position;
                    _SBoard._HandRigidbody.position = item.transform.position;
                    _SBoard.TouchItemFSM = item._MyFsm;
                    _SBoard.TouchItem = item;
                    Debug.Log("����ת��״̬");
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