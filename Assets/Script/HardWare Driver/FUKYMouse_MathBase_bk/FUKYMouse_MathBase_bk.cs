using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;

public class FUKYMouse_MathBase_bk : MonoBehaviour
{
    #region 下级组件
    [Header("依赖项")]
    public MouseState _mouseState;
    #endregion

    #region 可调参数
    public Vector3 MouseRotateAdj = Vector3.zero;
    [Header("调试用对象")]
    public Transform sim;
    #endregion
    #region L1已知量
    private Quaternion LocatorRotation = new Quaternion(-0.8675f, -0.085f, 0.1175f, 0.4675f);//多次测量后的近似定位器旋转
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
    #endregion
    #region Debug 的临时值
    [Header("Debug值")]
    public  float M_RealLampLength;
    public Vector2 M_estLampMid;
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
    }

    // Update is called once per frame
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
        M_RealLampLength = M_RawLampLength / B_Ratio;
        ////矫正在投影面上的灯线重心的偏移
        M_LampMidOffset = _mouseState.Float_MRotation * M_OrgLampMOffset;
        Debug.DrawLine(this.transform.position, this.transform.position + M_LampMidOffset, Color.black);
        float Projection_MLOffsetX = Vector3.Dot(M_LampMidOffset, LocatorX) / LocatorX.magnitude;
        float Projection_MLOffsetY = Vector3.Dot(M_LampMidOffset, LocatorY) / LocatorY.magnitude;
        float Projection_MLOffsetZ = Vector3.Dot(M_LampMidOffset, LocatorZ) / LocatorZ.magnitude;
        
        Vector3 DO = new(Projection_MLOffsetX * M_RealLampLength, 0, Projection_MLOffsetZ * M_RealLampLength);
        DO = LocatorRotation * DO;
        Debug.DrawLine(sim.position, sim.position + DO, Color.white);

        M_estLampMid = new(_mouseState.Raw_LampPos.x - Projection_MLOffsetX * M_RealLampLength, 
            _mouseState.Raw_LampPos.y - Projection_MLOffsetZ * M_RealLampLength);

        DebugDraw();
    }





    public void DebugDraw()
    {
        DebugToolDrawCoordLine(LocatorRotation, sim);//调试
        DebugToolDrawCoordLine(M_LampRotation, this.transform);//调试
    }
    private void DebugToolDrawCoordLine(Quaternion DebugQ,Transform ShowPos)
    {
        Vector3 X = DebugQ * Vector3.right;
        Vector3 Y = DebugQ * Vector3.up;
        Vector3 Z = DebugQ * Vector3.forward;
        Debug.DrawLine(ShowPos.position, ShowPos.position + X, Color.red);
        Debug.DrawLine(ShowPos.position, ShowPos.position + Y, Color.green);
        Debug.DrawLine(ShowPos.position, ShowPos.position + Z, Color.blue);
    }
}
