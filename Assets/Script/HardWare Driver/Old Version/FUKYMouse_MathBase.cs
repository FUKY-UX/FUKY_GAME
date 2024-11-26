using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using System;

/// <summary>
/// 硬件其实又改了，这里的计算是为挂在屏幕上的定位器设计的，不过现在还能跑就先不改了<para></para>
/// 而且做到后面才发现图像同时包含了俩东西，要准确的位置就只能插值啥的去估计<para></para>
/// 而且因为这种定位器不像VR那样子可以在头部挂个摄像头,导致这些位置只能以很别扭的方式估计出来
/// </summary>

public class FUKYMouse_MathBase : MonoBehaviour
{
    #region 下级组件
    [Header("依赖项")]
    [Tooltip("鼠标输入系统的所有状态汇总")]
    public MouseState _mouseState;
    [Tooltip("第一人称相机\r\n目前只考虑第一人称")]
    public Camera RefCamera;
    #endregion
    #region 调参的地方
    [Header("浮奇鼠—FUKYMOUSE")]
    [Tooltip("浮奇手的位置")]
    public Vector3 FukyHandPos;
    [Tooltip("浮奇手的旋转")]
    public Quaternion FukyHandRotate;
    [Tooltip("浮奇手的往前伸的灵敏度")]
    [Range(0f, 2f)]
    public float FukySens = 0.4f;
    [Tooltip("浮奇手的深度偏移")]
    [Range(0f, 2f)]
    public float UIDist = 0.6f;
    [Tooltip("浮奇手的起始旋转")]
    public Vector3 MouseRotateAdj = Vector3.zero;
    [Tooltip("浮奇手的旋转灵敏度")]
    public float RotateSens = 0.33f;
    #endregion
    #region 滤波器
    //位置信息更新
    private OneEuroFilter<Vector3> PosFilter;
    //旋转信息更新
    private OneEuroFilter<Quaternion> RotationFilter;
    [Header("OneEuro滤波器设置")]
    [Tooltip("较高minCutoff值会导致更多的高频噪声通过，而较低的值则会使输出更加平滑")]
    public float minCutoff = 0.2f;
    [Tooltip("增加beta会使滤波器在快速移动时更加响应，但也可能引入更多的高频噪声。\r\n减小beta则会使滤波器更加平滑，但可能导致在快速移动时响应滞后。")]
    public float beta = 0.2f;
    [Tooltip("较高的dCutoff值会使滤波器对速度变化更加敏感，而较低的值则会使输出在速度变化时更加平滑")]
    public float dCutoff = 0.6f;
    #endregion
    #region 数学模型控制
    [Header("数学模型控制")]
    [Tooltip("通过偏移的方式推测手心在图像上的坐标\r\n越大越激进，越小越接近灯的坐标")]
    [Range(0.0f, 2.0f)]
    public float est_Offset = 0.5f;
    [Tooltip("允许的估计偏移量\r\n越大对激进的估计越宽容\r\n如果陀螺仪失常，该值过大会导致手乱飞")]
    [Range(0.0f, 1.0f)]
    public float est_Range = 0.3f;
    [Tooltip("如果估计值过于离谱，该值就为False\r\n避免陀螺仪失常造成物体乱飞")]
    public bool IsUnStable = true;
    #endregion
    #region L1已知量
    //private Quaternion LocatorRotation = new Quaternion(-0.8675f, -0.085f, 0.1175f, 0.4675f);//旧近似定位器旋转
    private Quaternion LocatorRotation = Quaternion.Euler(-90f, -20f, 0);//新近似定位器旋转
    private Vector3 M_OrgLampMOffset = new Vector3(0.563f, -0.549f, 2.206f);//陀螺仪到灯线距离(数模)，以灯线长为单位衡量的中心到灯线偏移量
    private Quaternion LampOriginRotation;//灯本来就是倾斜的
    private float M_RawLampLength;
    private Vector3 VLine = new(1, 0, 0);//省人脑cpu虚拟灯线
    private Vector3 OriginX = new(1, 0, 0);//省人脑cpu虚拟世界轴
    private Vector3 OriginY = new(0, 1, 0);
    private Vector3 OriginZ = new(0, 0, 1);
    #endregion
    #region L2处理量
    private Vector3 LocatorX;//省人脑cpu模拟世界坐标的xyz轴
    private Vector3 LocatorY;
    private Vector3 LocatorZ;
    private Quaternion M_LampRotation;//先转动定位器旋转量，再转动鼠标
    private Vector3 ML_VLine = new(1, 0, 0);//省人脑cpu虚拟灯线,旋转后
    private Vector3 M_LampMidOffset = new(1, 0, 0);//省人脑cpu虚拟灯线,旋转后
    /// <summary>
    /// XY是左右和上下移动的比率，Z值为前后移动的比率
    /// </summary>
    #region 非固定定位器考虑因素(暂时不开发)
    [Obsolete] private Vector3 PrjImgResolution;//分辨率也需要投影才能得到映射值
    [Obsolete] private float PrjLampDispRatioX;
    [Obsolete] private float PrjLampDispRatioY;
    [Obsolete] private float PrjLampDispRatioZ;
    #endregion
    #endregion
    #region Debug 的临时值
    [Header("输出值")]
    [SerializeField]
    [Tooltip("估计出的免去透视旋转影响的红外灯线长\r\n(陀螺仪失灵的话该值会炸)")]
    public float M_estLampLength;
    [SerializeField]
    [Tooltip("根据灯线长度估计出的掌心位置\r\n(陀螺仪失灵的话该值会炸)")]
    public Vector2 M_estLampMid;
    [SerializeField]
    [Tooltip("根据灯线长度估算的角度，用来求距离相机的深度\r\n(陀螺仪失灵的话该值会炸)")]
    public float M_estLineAngle;
    [SerializeField]
    [Tooltip("估算的掌心距离相机的深度，越大越远，越小越近")]
    public float M_estDept;
    [SerializeField]
    [Tooltip("掌心的估计坐标")]
    public Vector3 M_estPos;
    #endregion

    #region 一些隐式转换
    public Quaternion MouseRotateAdjQ => Quaternion.Euler(MouseRotateAdj);
    #endregion

    void Start()
    {
        LocatorX = LocatorRotation * OriginX;
        LocatorY = LocatorRotation * OriginY;
        LocatorZ = LocatorRotation * OriginZ;
        LampOriginRotation = Quaternion.Euler(0, -38.9f, 0);
        
        RotationFilter = new OneEuroFilter<Quaternion>(50);
        PosFilter = new OneEuroFilter<Vector3>(50);
        //UpdatePrjRatio();
    }

    void Update()
    {
        M_RawLampLength = _mouseState.LampLineLength;
        M_LampRotation = _mouseState.Float_MRotation * LampOriginRotation * MouseRotateAdjQ;
        ML_VLine = M_LampRotation * VLine;
        //估计不受透视影响的LampLength有多长
        float Projection_X = Vector3.Dot(ML_VLine, LocatorX) / LocatorX.magnitude;//可以简化，但是人脑需要这样
        float Projection_Y = Vector3.Dot(ML_VLine, LocatorY) / LocatorY.magnitude;
        float Projection_Z = Vector3.Dot(ML_VLine, LocatorZ) / LocatorZ.magnitude;
        float B_Ratio = Mathf.Sqrt((Projection_X * Projection_X) + (Projection_Z * Projection_Z));
        M_estLampLength = M_RawLampLength / B_Ratio;
        //用估计近似的方式矫正在投影面上的灯线重心的偏移
        M_LampMidOffset = _mouseState.Float_MRotation * M_OrgLampMOffset;
        float Projection_MLOffsetX = Vector3.Dot(M_LampMidOffset, LocatorX) / LocatorX.magnitude * est_Offset;
        float Projection_MLOffsetY = Vector3.Dot(M_LampMidOffset, LocatorY) / LocatorY.magnitude * est_Offset;
        float Projection_MLOffsetZ = Vector3.Dot(M_LampMidOffset, LocatorZ) / LocatorZ.magnitude * est_Offset;
        #region Debug01
        //Vector3 DO = new(Projection_MLOffsetX * M_RealLampLength, 0, Projection_MLOffsetZ * M_RealLampLength);
        //DO = LocatorRotation * DO;
        //Debug.DrawLine(sim.position, sim.position + DO, Color.white);
        //Debug.DrawLine(this.transform.position, this.transform.position + M_LampMidOffset, Color.black);
        #endregion
        M_estLampMid = new(_mouseState.Raw_LampPos.x - Projection_MLOffsetX * M_estLampLength,
        _mouseState.Raw_LampPos.y - Projection_MLOffsetZ * M_estLampLength);//检查下LampPos偏移量有没有爆炸
        IsUnStable = CheckEstValue();
        if (IsUnStable)
        {
            M_estLampMid = _mouseState.Raw_LampPos;
            M_estLampLength = M_RawLampLength;
        }
        M_estLineAngle = (M_estLampLength / _mouseState.ImgResolution.x) * _mouseState.Hfov;//估算角度然后估计深度
        if (M_estLineAngle < 53.2f)//固定定位器的近似
        {
            M_estDept = Mathf.Tan((M_estLineAngle / 2) * (Mathf.PI / 180.0f)) * (M_estLampLength / 2);
        }
        float SC_PrjEstPosX = (M_estLampMid.x / _mouseState.ImgResolution.x) * Camera.main.pixelWidth;
        float SC_PrjEstPosY = (M_estLampMid.y / _mouseState.ImgResolution.y) * Camera.main.pixelHeight;
        //PrjImgResolution = new Vector3(SC_PrjEstPosX, SC_PrjEstPosY, 0f);

        FukyHandPos = RefCamera.ScreenToWorldPoint(new(SC_PrjEstPosX, SC_PrjEstPosY, Mathf.Max(0, M_estDept * FukySens + RefCamera.nearClipPlane)));
        
        RotationFilter.UpdateParams(_mouseState.RotateFreq, minCutoff, beta, dCutoff);
        PosFilter.UpdateParams(_mouseState.PosFreq, minCutoff, beta, dCutoff);


        FukyHandPos = PosFilter.Filter(FukyHandPos);
        FukyHandRotate = AdjustQuaternion(RotationFilter.Filter(_mouseState.Float_MRotation));

        //DebugDraw();
    }

    /// <summary>
    /// 如果真，就是估计量爆炸了，假的话就是在接受范围
    /// </summary>
    /// <returns></returns>
    private bool CheckEstValue()
    {
        return (M_estLampMid - _mouseState.Raw_LampPos).magnitude > _mouseState.ImgResolution.magnitude * est_Range;
    }
    private void DebugDraw()
    {
        //DebugToolDrawCoordLine(LocatorRotation, sim);//调试
        DebugToolDrawCoordLine(M_LampRotation, this.transform);//调试
    }
    private void DebugToolDrawCoordLine(Quaternion DebugQ, Transform ShowPos)
    {
        Vector3 X = DebugQ * Vector3.right;
        Vector3 Y = DebugQ * Vector3.up;
        Vector3 Z = DebugQ * Vector3.forward;
        Debug.DrawLine(ShowPos.position, ShowPos.position + X, Color.red);
        Debug.DrawLine(ShowPos.position, ShowPos.position + Y, Color.green);
        Debug.DrawLine(ShowPos.position, ShowPos.position + Z, Color.blue);
    }
    /// <summary>
    /// 如果定位器摆放位置奇怪，需要用插值的方式来估算位置
    /// <para></para>(上下左右前后各采集一组数据求均，然后在运行时实时求插值)
    /// <para></para>这时会需要复杂的线性映射，而且需要两个陀螺仪而非一个
    /// <para></para>所以该方式暂时弃用
    /// </summary>
    [Obsolete]
    private void UpdatePrjRatio()
    {
        //处理实际图像坐标(xy)与虚拟成像面(xz)之间的映射关系
        //先把坐标转成xz(模拟时xz对应图片的xy)，然后旋转图像与摄影角度重合，计算此时的点积关系
        Vector3 SimImgResolutionX = LocatorX * _mouseState.ImgResolution.x;
        Vector3 SimImgResolutionY = LocatorZ * _mouseState.ImgResolution.y; //origin是Unity坐标，y向上，SimImgResolution为z向上
        Vector3 SimImgResolutionZ = LocatorY * 1; //origin是Unity坐标，Z向前，SimImgResolution为Y向前，因为图像没深度，这里用1来求关系
        float PrjResolutionX = Vector3.Dot(SimImgResolutionX, OriginX) / OriginX.magnitude;
        float PrjResolutionY = Vector3.Dot(SimImgResolutionY, OriginY) / OriginY.magnitude;
        float PrjResolutionZ = Vector3.Dot(SimImgResolutionZ, OriginZ) / OriginY.magnitude;

        PrjImgResolution = new Vector3(PrjResolutionX, PrjResolutionY, PrjResolutionZ);
        PrjLampDispRatioX = PrjImgResolution.x / _mouseState.ImgResolution.x;
        PrjLampDispRatioY = PrjImgResolution.y / _mouseState.ImgResolution.y;
        PrjLampDispRatioZ = Mathf.Abs(1f / PrjImgResolution.z);
    }

    private Quaternion AdjustQuaternion(Quaternion Input)
    {
        return Quaternion.Euler(Input.eulerAngles * (90 * RotateSens / 15));
    }

}