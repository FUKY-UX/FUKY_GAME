using UnityEngine;
using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Runtime.InteropServices;
//using System.Numerics;
using Unity.Mathematics;


public class FUKYMouse : SingletonMono<FUKYMouse>
{
    ///////////////////////////////////////////////////////////////////////////
    // 配置参数 公开在前私有在后
    ///////////////////////////////////////////////////////////////////////////
    #region Configuration Parameters
    [Header(" 全局设置")]
    [Tooltip("全局缩放系数")]
    [Range(0.001F, 1F)]
    public float Scaler;
    [Tooltip("左右位移反转")]
    public bool InVerse;
    [Header("IMU安装轴问题")]
    [Tooltip("旋转的纠正量，用来处理IMU安装与设备朝向以及Unity坐标不对应的问题")]
    public Vector3 Rotation_Offset;
    public float Z_Offset = 10;

    [Header("各轴独立缩放")]
    [Tooltip("X轴单独的缩放")]
    [Range(0.001f, 10f)]
    public float X_Scale;
    [Tooltip("Y轴单独的缩放")]
    [Range(0.001f, 10f)]
    public float Y_Scale;
    [Tooltip("Z轴单独的缩放")]
    [Range(0.001f, 10f)]
    public float Z_Scale;

    [Header("加速度插帧设置")]
    [Tooltip("阈值，现实移动幅度超过该阈值才会计入位移，否则作为噪音")]
    [Range(0.001f, 2f)]
    public float AccelThershold;
    [Range(0.001f, 2f)]

    public float AccelScaler = 0.01f;
    private Vector3 LastAccel;
    private Vector3 CurrAccel;
    private bool SenseUpdate;



    [Header("滤波器参数")]
    [Tooltip("较高minCutoff值会导致更多的高频噪声通过，而较低的值则会使输出更加平滑")]
    public float minCutoff = 0.2f;
    [Tooltip("增加beta会使滤波器在快速移动时更加响应，但也可能引入更多的高频噪声。\r\n减小beta则会使滤波器更加平滑，但可能导致在快速移动时响应滞后。")]
    public float beta = 0.2f;
    [Tooltip("较高的dCutoff值会使滤波器对速度变化更加敏感，而较低的值则会使输出在速度变化时更加平滑")]
    public float dCutoff = 0.6f;
    [Tooltip("位置数据的更新频率")]
    public float PosFreq = 0.02f;//数据的更新频率
    [Tooltip("旋转数据的更新频率")]
    public float RotateFreq = 0.02f;//数据的更新频率
    public float LastPosUpdateTime = 0.00f;//上一次更新时间
    public float LastRotateUpdateTime = 0.02f;//上一次更新时间
    private OneEuroFilter<Vector3> PosFilter;
    #endregion

    ///////////////////////////////////////////////////////////////////////////
    // 运行时数据 公开在前私有在后
    ///////////////////////////////////////////////////////////////////////////
    #region Runtime Data
    [Header("按钮状态与压感")]
    public float PressureValue;
    public bool Left_pressed = false;
    public bool Right_pressed = false;
    public bool Left_Down = false;
    public bool Right_Down = false;
    public bool Middle_pressed = false;
    public bool isMouseFloating = false;
    public bool LastLeft_pressed = false;
    public bool LastRight_pressed = false;

    public Vector3 filteredTranslate { get; private set; }
    public Vector3 rawAcceleration { get; private set; }
    public Quaternion rawRotation { get; private set; }
    public Vector3 rawTranslate { get; private set; }
    public Vector3 FusionTranslate { get; private set; }

    public Vector3 deltaTranslate { get; private set; }
    public Quaternion deltaRotation { get; private set; }
    public Vector3 deltaEuler { get; private set; }
    public byte buttonState { get; private set; }

    private Vector3 lastRawTranslate;//上一帧的灯珠位置值
    private Vector3 lastFilteredTranslate;//上一帧的位置值
    private Quaternion lastRawRotation;//上一帧鼠标的旋转值
    #endregion
    ///////////////////////////////////////////////////////////////////////////
    // 共享内存设置和数据结构 公开在前私有在后
    ///////////////////////////////////////////////////////////////////////////
    #region Share Mem and data sturture
    // 共享内存配置（必须与Python代码完全一致）
    private const string IMU_MEM_NAME = "IMU_Memory";
    private const string BTN_MEM_NAME = "BTN_Memory";
    private const string PRESS_MEM_NAME = "PRESS_Memory";
    private const string LOCATOR_MEM_NAME = "FUKY_Locator_Memory";
    // 内存映射对象
    private MemoryMappedFile _IMU_MemFile;
    private MemoryMappedViewAccessor _IMU_Accessor;
    private MemoryMappedFile _BTN_MemFile;
    private MemoryMappedViewAccessor _BTN_Accessor;
    private MemoryMappedFile _PRESS_MemFile;
    private MemoryMappedViewAccessor _PRESS_Accessor;
    private MemoryMappedFile _locatorMemFile;
    private MemoryMappedViewAccessor _locatorAccessor;
    // 数据结构定义（与Python打包方式一致）
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct IMUData
    {
        public float accelX;
        public float accelY;
        public float accelZ;
        public float quatW;
        public float quatX;
        public float quatY;
        public float quatZ;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LocatorData
    {
        public float CoordX;
        public float CoordY;
        public float CoordZ;
    }

#endregion


    void Start()
    {
        try
        {
            // IMU 打开已存在的共享内存-IMU的数据     访问器
            _IMU_MemFile = MemoryMappedFile.OpenExisting(IMU_MEM_NAME);
            _IMU_Accessor = _IMU_MemFile.CreateViewAccessor();

            // 定位器 打开已存在的共享内存-定位器的数据 访问器
            _locatorMemFile = MemoryMappedFile.OpenExisting(LOCATOR_MEM_NAME);
            _locatorAccessor = _locatorMemFile.CreateViewAccessor();

            // 打开已存在的共享内存-鼠标的数据  访问器
            _BTN_MemFile = MemoryMappedFile.OpenExisting(BTN_MEM_NAME);
            _BTN_Accessor = _BTN_MemFile.CreateViewAccessor();

            // 打开已存在的共享内存-鼠标的数据   访问器
            _PRESS_MemFile = MemoryMappedFile.OpenExisting(PRESS_MEM_NAME);
            _PRESS_Accessor = _PRESS_MemFile.CreateViewAccessor();
            Debug.Log("成功连接共享内存");
        }
        catch (Exception e)
        {
            Debug.LogError($"没有连接鼠标: {e.Message}");
        }
        PosFilter = new OneEuroFilter<Vector3>(50);
    }

    void Update()
    {
        if (_IMU_Accessor == null) return;
        try
        {
            IMUData data;
            LocatorData data2;
            // 读取数据结构 加速度和四元数坐标系的转换
            _IMU_Accessor.Read(0, out data);
            rawAcceleration = new Vector3(
                data.accelY,
                data.accelZ,
                data.accelX
            );
            rawRotation = quaternion.Euler(Rotation_Offset) * new Quaternion(
                data.quatY,
                data.quatX,
                data.quatZ,
                data.quatW
            );
            //Debug.Log("加速度数据:" + rawAcceleration + "四元数数据:" + rawRotation);
            // 读取数据结构 定位器数据
            _locatorAccessor.Read(0, out data2);
            if (InVerse)
            {
                rawTranslate = new Vector3
                (
                    -data2.CoordX * X_Scale,
                    data2.CoordY * Y_Scale,
                    data2.CoordZ * Z_Scale - Z_Offset
                ) * Scaler;
            }
            else
            {
                rawTranslate = new Vector3
                (
                    data2.CoordX * X_Scale,
                    -data2.CoordY * Y_Scale,
                    -data2.CoordZ * Z_Scale - Z_Offset
                ) * Scaler;
            }
            //Debug.Log("定位器坐标数据:" + rawTranslate);

            // 读取按钮数据 
            byte buttonState = _BTN_Accessor.ReadByte(0);
            // 解析按钮位状态（使用位掩码）
            Left_pressed = (buttonState & 0x01) != 0;    // 第0位：左键
            Right_pressed = (buttonState & 0x02) != 0;   // 第1位：右键
            Middle_pressed = (buttonState & 0x04) != 0;  // 第2位：中键                                           解析第四位（bit3）的浮动状态
            isMouseFloating = (buttonState & 0x08) != 0; // 第3位：浮动状态
            //Debug.Log($"按钮值: {buttonState}");

            if (Left_pressed && Left_pressed != LastLeft_pressed)  Left_Down = true;  else Left_Down = false;
            if (Right_pressed && Right_pressed != LastRight_pressed) Right_Down = true; else Right_Down = false;
            LastLeft_pressed = Left_pressed;
            LastRight_pressed = Right_pressed;

            byte low = _PRESS_Accessor.ReadByte(0);
            byte high = _PRESS_Accessor.ReadByte(1);
            PressureValue =math.max(0, ((ushort)((high << 8) | low) / 65535.0f))*2;
            //Debug.Log(PressureValue);

            var AccelVector = rawAcceleration - LastAccel;
            if (AccelVector.magnitude > AccelThershold)
            {
                CurrAccel += AccelVector;
                if (!SenseUpdate)
                {
                    FusionTranslate += CurrAccel * AccelScaler;
                    SenseUpdate = false;
                    Debug.Log("插帧"+ CurrAccel);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"读取失败: {e.Message}");
        }

        if (rawRotation != lastRawRotation)
        {
            RotateFreq = 1 / LastRotateUpdateTime;
            LastRotateUpdateTime = 0f;
        }
        if (rawTranslate != lastRawTranslate)
        {
            PosFreq = 1 / LastPosUpdateTime;
            LastPosUpdateTime = 0f;
            SenseUpdate = true;
        }

        lastRawTranslate = rawTranslate;

        deltaRotation = rawRotation * Quaternion.Inverse(lastRawRotation);
        deltaEuler = rawRotation.eulerAngles - lastRawRotation.eulerAngles;
        lastRawRotation = rawRotation;

        PosFilter.UpdateParams(PosFreq, minCutoff, beta, dCutoff);

        // OE_Rotation = RotationFilter.Filter(Raw_Rotation);
        filteredTranslate = PosFilter.Filter(rawTranslate);

        deltaTranslate = filteredTranslate - lastFilteredTranslate;
        if (deltaTranslate.magnitude > 0.5f)
        {
            deltaTranslate = Vector3.zero;
        }
        lastFilteredTranslate = filteredTranslate;


        LastPosUpdateTime += Time.deltaTime;
        LastRotateUpdateTime += Time.deltaTime;


    }

}