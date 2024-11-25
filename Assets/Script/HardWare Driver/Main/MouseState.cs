using Unity.Mathematics;
using UnityEngine;

public enum CurrMouseState
{
    Moving,
    Stop,
    Floating
}
public class MouseState : MonoBehaviour
{
    #region 依赖项
    [Header("依赖项")]
    [SerializeField]
    private LampCoordReceiver _LampCoordReceiver;
    [SerializeField]
    private MRotateReciver _MRotateReciver;
    #endregion
    #region 输出值
    [Header("输出值")]
    [Tooltip("定位器传入的图像坐标，分辨率默认是320X240")]
    public Vector2 Raw_LampPos;
    [Tooltip("鼠标传入的四元数旋转信息，经过MRotateReciver修改适配Unity")]
    public Quaternion Raw_MRotation;
    [Tooltip("当鼠标浮空时的相对图像坐标\r\n鼠标开始浮起时的坐标理解为原点(0,0)\r\n往左X减小，往上Y增大")]
    public Vector2 Float_LampPos;
    [Tooltip("当鼠标浮空时的相对旋转信息，鼠标开始浮起时的旋转理解为四元数(0,0,0,0)")]
    public Quaternion Float_MRotation;
    [Tooltip("灯线的长度")]
    public float LampLineLength;
    [Tooltip("当前的鼠标状态")]
    public CurrMouseState mouseState;
    [Tooltip("丢跟踪时间")]
    public float LostTrackTime = 0;
    [Tooltip("跟踪状态")]
    public bool IsTracked;
    [Tooltip("陀螺仪工作状态\r\n如果手不动却一直在工作状态就说明陀螺仪自己转起来了，需要重启")]
    public bool GyroWork;
    [Tooltip("陀螺仪是不是坏了\r\n如果坏了，需要重启")]
    public bool GyroBroken;
    [Tooltip("位置数据的更新频率")]
    public float PosFreq = 0.02f;//数据的更新频率
    [Tooltip("旋转数据的更新频率")]
    public float RotateFreq = 0.02f;//数据的更新频率
    #endregion
    #region 定位器设置
    [Header("定位器设置")]
    [Tooltip("越大越难进入悬浮状态")]
    [Range(0, 5)]
    public float Thr_Float = 1.8f;
    [Tooltip("丢追踪时间，值越大，对丢跟踪越宽容")]
    public float Thr_ReSetTime = 1;
    [Tooltip("限制手的移动范围，越大越容易出界，越小越难触发出界")]
    [Range(-20,20)]
    public float Thr_TrackPos = 0;
    [Tooltip("限制手的旋转，值变大会导致定位计算在丢跟踪时继续计算导致不准确")]
    [Range(-20, 20)]
    public float Thr_TrackRotate = 0;
    #endregion
    #region 系统
    [Header("系统配置")]
    [Tooltip("分辨率，根据摄像的分辨率来决定，一般不要调")]
    public Vector3 ImgResolution = new(320, 240,0);
    [Tooltip("摄像的Hfov")]
    public float Hfov = 53.2f;
    [Tooltip("陀螺仪的敏感度\r\n越大对零飘越宽容，越小系统对陀螺仪品质要求越高")]
    [Range(0, 5)]
    public float Thr_GyroMove = 1.7f;
    [Tooltip("陀螺仪在工作的时间(零飘如果过大也会被识别成工作)")]
    public float GyroWorkTime = 0;
    [Tooltip("如果你举着手一直乱动，陀螺仪会被搞坏\r\n持续零飘过强也会导致超过这个值，然后陀螺仪就需要重启")]
    [Range(5.0f,20f)]
    public float GyroMaxWorkTime = 10f;
    [Tooltip("丢追踪范围设定的具体到分辨率的版本\r\nXY为最小、最大横向检测，ZW为纵向")]
    public Vector4 _TrackZone = new(30,270,20,220);
    [Tooltip("旋转对应现实灯线的值\r\nX为向左转多少仍能追踪，Y为向右转多少仍能追踪")]
    public Vector2 _TrackRotate = new(20, 30);
    #endregion
    #region 算Δ用的一些玩意
    private Vector2 Last_LampPos;//上一帧的灯珠位置值
    private Quaternion Last_MRotation;//上一帧鼠标的旋转值
    private Vector2 PrevFoat_LampPos;//灯珠在鼠标刚Float时的位置信息
    private Quaternion PrevFoat_MRotation; //鼠标刚Float时的旋转信息
    private Vector3 LastMousePos = Vector3.zero;//上一帧鼠标原生输入的屏幕Pos
    public float LastPosUpdateTime = 0.00f;//数据的更新频率
    public float LastRotateUpdateTime = 0.00f;//数据的更新频率
    #endregion


    //Delta值
    [Header("Delta值")]
    /// <summary>
    /// 灯珠每帧移动的距离，从上一帧位置指向下一帧位置
    /// </summary>
    public Vector2 Delta_LampPos;
    /// <summary>
    /// 鼠标相对上一帧旋转的旋转增量
    /// </summary>
    public Quaternion Delta_MRotation;

    #region 调整用的一些杂值
    private Vector2 Origin_LampPos = Vector2.zero;
    #endregion

    private void Start()
    {
        LastMousePos = Input.mousePosition;
    }

    void Update()
    {
        Raw_LampPos = _LampCoordReceiver.LampMidPoint * Vector2.right + ImgResolution * Vector2.up - _LampCoordReceiver.LampMidPoint * Vector2.up;
        if (Raw_LampPos.y == ImgResolution.y) { Raw_LampPos.y = 0; }
        Raw_MRotation = _MRotateReciver.RawMouseRotation;

        if (Input.mousePosition != LastMousePos)
        {
            mouseState = CurrMouseState.Moving;

        }
        else if (mouseState != CurrMouseState.Floating)
        {
            mouseState = CurrMouseState.Stop;
        }

        if (mouseState == CurrMouseState.Stop && Quaternion.Angle( Last_MRotation,Raw_MRotation)> Thr_Float)
        {
            mouseState = CurrMouseState.Floating;
            PrevFoat_LampPos = Raw_LampPos;
            PrevFoat_MRotation = Raw_MRotation;
        }

        if (mouseState == CurrMouseState.Floating)
        {
            Float_LampPos = Raw_LampPos - PrevFoat_LampPos;
            Float_MRotation = Raw_MRotation * Quaternion.Inverse(PrevFoat_MRotation);
            LampLineLength = _LampCoordReceiver.LampLineLength;
        }
        else
        {
            Float_LampPos = Vector2.zero;
            Float_MRotation = Quaternion.identity;
        }
        GyroErrHandler();
        UpdateDelta();
    }
    /// <summary>
    /// 更新Δ值的一堆操作
    /// </summary>
    private void UpdateDelta()
    {
        if (LostTrackHandler())
        {
            LostTrackTime = 0f;
        }
        else
        {
            LostTrackTime += Time.deltaTime;
        }
        if (LostTrackTime >= Thr_ReSetTime) { IsTracked = false; }
        else { IsTracked = true; }

        if (Raw_MRotation != Last_MRotation)
        {
            RotateFreq = 1 / LastRotateUpdateTime;
            RotateFreq = Mathf.Clamp(RotateFreq, 0.01F, 1);
            LastRotateUpdateTime = 0f;
        }
        if (Raw_LampPos != Last_LampPos)
        {
            PosFreq = 1 / LastPosUpdateTime;
            RotateFreq = Mathf.Clamp(PosFreq, 0.01F, 1);
            LastPosUpdateTime = 0f;
        }

        LastMousePos = Input.mousePosition;
        Delta_LampPos = Raw_LampPos - Last_LampPos;
        Delta_MRotation = Raw_MRotation * Quaternion.Inverse(Last_MRotation);
        Last_LampPos = Raw_LampPos;
        Last_MRotation = Raw_MRotation;
        LastPosUpdateTime += Time.deltaTime;
        LastRotateUpdateTime += Time.deltaTime;
    }    
    /// <summary>
    /// 检测丢追踪的逻辑
    /// </summary>
    private bool LostTrackHandler()
    {
        if(Raw_LampPos == Last_LampPos) return false;
        else return (TrackRotate() && TrackZone());
    }
    /// <summary>
    /// 如果陀螺仪自旋过快会报错
    /// </summary>
    /// <returns></returns>
    private void GyroErrHandler()
    {
        GyroWork = Quaternion.Angle(Last_MRotation, Raw_MRotation) > Thr_GyroMove;
        if (GyroWork) 
        {
            GyroWorkTime += Time.deltaTime;
        }
        else
        {
            GyroWorkTime = 0f;
        }
        if(GyroWorkTime > GyroMaxWorkTime){GyroBroken = true;}
        else{GyroBroken = false;}
    }
    private bool TrackRotate()
    {
        float YRotate;
        if (Float_MRotation.eulerAngles.y > 180)
        {
            YRotate = 360 - Float_MRotation.eulerAngles.y;
            if (YRotate > _TrackRotate.y + Thr_TrackRotate) { return false; }
            return true;
        }
        else
        {
            YRotate = Float_MRotation.eulerAngles.y;
            if (YRotate > _TrackRotate.x + Thr_TrackRotate) { return false; }
            return true;
        }
    }
    private bool TrackZone()
    {
        return !(Raw_LampPos.x < _TrackZone.x + Thr_TrackPos || Raw_LampPos.x > _TrackZone.y - Thr_TrackPos
            || Raw_LampPos.y < _TrackZone.z + Thr_TrackPos || Raw_LampPos.y > _TrackZone.w - Thr_TrackPos);
    }
}
