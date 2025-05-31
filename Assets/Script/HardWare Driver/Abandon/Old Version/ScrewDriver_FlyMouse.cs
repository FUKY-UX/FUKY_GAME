using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public class ScrewDriver_FlyMouse : MonoBehaviour
{
    public MouseState _mouseState;
    //public Transform LocatorObj;
    public Transform MouseObj;
    public Transform HandPosRef;
    //数据获取
    public float HV_rate = 2f;
    public float Dept_Rate = 1.5f;
    public float smoothingFactor = 0.1f;
    public LampCoordReceiver _LampCoordReceiver;
    public MRotateReciver _MRotateReciver;
    public Vector2 RealCamP= new(320,240);
    private float HFov = 53.2f;
    private Quaternion LocatorRotation = new Quaternion(-0.8675f, -0.085f, 0.1175f, 0.4675f);//RawMouseRotation = new Quaternion(y, z, x, w);
    public Vector3 MouseEulerOffset = Vector3.zero;
    public Quaternion MouseEulerOffsetQ => Quaternion.Euler(MouseEulerOffset);
    public Quaternion MouseRotationOffset => Quaternion.Euler(MouseEulerOffset);
    private Quaternion MLcombinedRotation;

    private Matrix4x4 LocatorRotatie4M;
    private Vector3 RawMouseRotate = Vector3.zero;
    private Matrix4x4 MouseRotatie4M;
    private Vector2 _RawLampMid = Vector2.zero;
    private float _RawLampLength = 0.0f;
    
    //会对计算产生影响的两个局部偏移量
    public float ML_RotateOffset_Y = 38.9f;//在获取到陀螺仪旋转量后添加
    public float ML_OffsetVec_Mul = 1.0f;
    //灯本身线距30.522厘米
    //往前移动67.355厘米（2.206倍）
    //往侧面移动17.190厘米（0.563倍）
    //往下移动16.777厘米（0.549倍）
    //纵向俯视Z轴右手拇指指向自己逆时针38.9°(乘上鼠标的旋转)
    public Vector3 ML_ScaleOffset => new Vector3(0.563f, -0.549f, 2.206f) * ML_OffsetVec_Mul;//陀螺仪与灯线的现实数学模型，以灯线为单位衡量的偏移量
    //虚拟线
    private Vector3 V_Line = new(1, 0, 0);
    private Vector3 V_Line_Offset;
    
    private Vector4 O_X_axis = new(1, 0, 0, 1);
    private Vector4 O_Y_axis = new(0, 1, 0, 1);
    private Vector4 O_Z_axis = new(0, 0, 1, 1);
    Vector4 L_X_axis;
    Vector4 L_Y_axis;
    Vector4 L_Z_axis;
    //过滤掉旋转影响后的实际灯线长以及灯的中点坐标
    public float Tran_LampLength;
    public Vector2 tran_LampMid;
    private Vector2 tran_LampMid_P;//考虑到了透视
    //模拟深度
    public float Depth_Z;
    public float Interation_Z=160f;
    //返回计算得到的相对位移坐标，提供个Mul来调整灵敏度
    public Vector3 Raw_OffsetPos;
    public Vector3 tran_OffsetPos;//再次转换成摄像头平放的默认世界坐标

    private Quaternion LastRotation;
    public Vector3 LastOffsetPos = Vector3.zero;
    
    public int averageCount = 10; // 队列长度  
    private Queue<Vector3> positionQueue = new Queue<Vector3>();


    void Start()
    {
        L_X_axis = LocatorRotation * O_X_axis;
        L_Y_axis = LocatorRotation * O_Y_axis;
        L_Z_axis = LocatorRotation * O_Z_axis;
        //初始化均值滤波的队列
        for (int i = 0; i < averageCount; i++)
        {
            positionQueue.Enqueue(Vector3.zero);
        }
    }

    private void Update()
    {
        _RawLampMid = _LampCoordReceiver.LampMidPoint;
        _RawLampLength = _LampCoordReceiver.LampLineLength;

        //实时将得到的欧拉旋转变为四元数再转成矩阵
        // 将两个旋转矩阵相乘，得到一个新的组合旋转矩阵  
        // 注意乘法顺序：MouseRotatie4M  *  LocatorRotatie4M
        // 这意味着首先应用LocatorRotatie4M，然后应用MouseRotatie4M  
        MLcombinedRotation = _MRotateReciver.RawMouseRotation * LocatorRotation * MouseEulerOffsetQ;
        //MouseObj.rotation = MLcombinedRotation;
        //LocatorObj.rotation = LocatorRotation;
        V_Line = MLcombinedRotation * V_Line;
        //Debug.DrawLine(this.transform.position, this.transform.position+V_Line, Color.gray);
        V_Line_Offset = _MRotateReciver.RawMouseRotation * ML_ScaleOffset;
        //Debug.DrawLine(this.transform.position, this.transform.position + V_Line_Offset, Color.white);
        ////获得旋转后的灯线在旋转后的相机空间的轴上的投影+偏移量在旋转后的相机空间的轴上的投影
        float Projection_X = Vector3.Dot(V_Line, L_X_axis) / L_X_axis.magnitude;
        float Projection_Y = Vector3.Dot(V_Line, L_Y_axis) / L_X_axis.magnitude;
        float Projection_Z = Vector3.Dot(V_Line, L_Z_axis) / L_X_axis.magnitude;
        Vector3 DX = new(Projection_X, 0, 0);
        Vector3 DY = new(0, Projection_Y, 0);
        Vector3 DZ = new(0, 0, Projection_Z);
        DX = LocatorRotation * DX;
        DY = LocatorRotation * DY;
        DZ = LocatorRotation * DZ;
        //Debug.DrawLine(this.transform.position, DX + this.transform.position, Color.red);
        //Debug.DrawLine(this.transform.position, DY + this.transform.position, Color.green);
        //Debug.DrawLine(this.transform.position, DZ + this.transform.position, Color.blue);

        float B_Ratio = Mathf.Sqrt((Projection_X * Projection_X) + (Projection_Z * Projection_Z));
        Vector3 DB = new(Projection_X, 0, Projection_Z);
        DB = LocatorRotation * DB;
        Debug.DrawLine(this.transform.position, DB + this.transform.position, new(1f,1f,0f,0.5f));
        Tran_LampLength = _RawLampLength / B_Ratio;
        ////矫正在投影面上的灯线重心的偏移
        float Projection_MLOffsetX = Vector3.Dot(V_Line_Offset, L_X_axis)/ L_X_axis.magnitude;
        float Projection_MLOffsetY = Vector3.Dot(V_Line_Offset, L_Y_axis) / L_Y_axis.magnitude;
        float Projection_MLOffsetZ = Vector3.Dot(V_Line_Offset, L_Z_axis) / L_Z_axis.magnitude;
        tran_LampMid = new(_RawLampMid.x - Projection_MLOffsetX * Tran_LampLength, _RawLampMid.y + Projection_MLOffsetZ * Tran_LampLength);
        Vector3 DO = new(Projection_MLOffsetX * Tran_LampLength, 0, Projection_MLOffsetZ * Tran_LampLength);
        DO = LocatorRotation * DO;
        //Debug.DrawLine(this.transform.position, DO + this.transform.position, Color.green);
        float Line_Angle = (Tran_LampLength / RealCamP.x) * HFov;

        ////求当前线段对应的角度,然后求一个估计深度

        if(Line_Angle< 53.2f)
        {
            Depth_Z = Mathf.Tan((Line_Angle / 2) * (Mathf.PI / 180.0f)) * (Tran_LampLength / 2);
        }

        //需要随着深度改变XY坐标的位移量，防止位移速度过快【后面再考虑吧】
        //tran_LampMid_P= new Vector2(tran_LampMid.x * (Interation_Z/ Depth_Z), tran_LampMid.y * (Interation_Z / Depth_Z));
        ////计算相对位移
        Raw_OffsetPos = new Vector3((tran_LampMid.x / RealCamP.x - 0.5f)* HV_rate, -Depth_Z* Dept_Rate, -(tran_LampMid.y / RealCamP.y - 0.5f)* HV_rate);
        Raw_OffsetPos = LocatorRotation * Raw_OffsetPos;
        //Debug.DrawLine(this.transform.position, tran_OffsetPos + this.transform.position, Color.green);

        float Projection_OffsetPosX = Vector3.Dot(Raw_OffsetPos, O_X_axis) / O_X_axis.magnitude;
        float Projection_OffsetPosY = Vector3.Dot(Raw_OffsetPos, O_Y_axis) / O_Y_axis.magnitude;
        float Projection_OffsetPosZ = Vector3.Dot(Raw_OffsetPos, O_Z_axis) / O_Z_axis.magnitude;

        tran_OffsetPos = HandPosRef.rotation * new Vector3(Projection_OffsetPosX, Projection_OffsetPosY, Projection_OffsetPosZ);
        tran_OffsetPos = HandPosRef.position + tran_OffsetPos;


        // 更新队列  
        //positionQueue.Dequeue();
        //positionQueue.Enqueue(tran_OffsetPos);

        if (_mouseState.mouseState==CurrMouseState.Moving)
        {
            LastOffsetPos = tran_OffsetPos;
            LastRotation = _MRotateReciver.RawMouseRotation;
        }


        // 计算加权平均位置  

        //Vector3 smoothedPosition = Vector3.zero;
        //float totalWeight = 0f;
        //for (int i = 0; i < averageCount; i++)
        //{
        //    Vector3 pos = positionQueue.ToArray()[i];
        //    float weight = Mathf.Lerp(1.0f, 0.1f, (float)i / (averageCount - 1)); // 线性递减权重  
        //    smoothedPosition += pos * weight;
        //    totalWeight += weight;
        //}
        //smoothedPosition /= totalWeight;

        // 使用Lerp进行平滑过渡  

        MouseObj.transform.position = Vector3.Lerp(LastOffsetPos, tran_OffsetPos, smoothingFactor);

        if((LastRotation.eulerAngles - _MRotateReciver.RawMouseRotation.eulerAngles).magnitude > 0.1f)
        {
            MouseObj.transform.rotation = HandPosRef.rotation * Quaternion.Lerp(LastRotation, _MRotateReciver.RawMouseRotation, smoothingFactor);
        }

        // 更新last值为当前帧的值，为下一帧做准备  
        LastOffsetPos = MouseObj.transform.position;
        LastRotation = _MRotateReciver.RawMouseRotation;

    }
    // 定义一个与你的JSON响应匹配的C#类    
}