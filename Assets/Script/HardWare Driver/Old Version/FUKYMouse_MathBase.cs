using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using System;

/// <summary>
/// Ӳ����ʵ�ָ��ˣ�����ļ�����Ϊ������Ļ�ϵĶ�λ����Ƶģ��������ڻ����ܾ��Ȳ�����<para></para>
/// ������������ŷ���ͼ��ͬʱ��������������Ҫ׼ȷ��λ�þ�ֻ�ܲ�ֵɶ��ȥ����<para></para>
/// ������Ϊ���ֶ�λ������VR�����ӿ�����ͷ���Ҹ�����ͷ,������Щλ��ֻ���Ժܱ�Ť�ķ�ʽ���Ƴ���
/// </summary>

public class FUKYMouse_MathBase : MonoBehaviour
{
    #region �¼����
    [Header("������")]
    [Tooltip("�����鿴���ƽ�����Ч��")]
    public LampDebugUICtrol _lampDebugUICtrol;
    [Tooltip("�������ϵͳ������״̬����")]
    public MouseState _mouseState;
    [Tooltip("��һ�˳����\r\nĿǰֻ���ǵ�һ�˳�")]
    public Camera RefCamera;
    #endregion
    #region ���εĵط�
    [Header("������FUKYMOUSE")]
    [Tooltip("�����ֵ�λ��")]
    public Vector3 FukyHandPos;
    [Tooltip("�����ֵ���ת")]
    public Quaternion FukyHandRotate;
    [Tooltip("�����ֵ���ǰ���������")]
    [Range(0f, 2f)]
    public float FukySens = 0.4f;
    [Tooltip("�����ֵ����ƫ��")]
    [Range(0f, 2f)]
    public float UIDist = 0.6f;
    [Tooltip("�����ֵ���ʼ��ת")]
    public Vector3 MouseRotateAdj = Vector3.zero;
    [Tooltip("�����ֵ���ת������")]
    public float RotateSens = 0.33f;

    #endregion
    #region ��ѧģ�Ϳ���
    [Header("��ѧģ�Ϳ���")]
    [Tooltip("ͨ��ƫ�Ƶķ�ʽ�Ʋ�������ͼ���ϵ�����\r\nԽ��Խ������ԽСԽ�ӽ��Ƶ�����")]
    [Range(0.0f, 2.0f)]
    public float est_Offset = 0.5f;
    [Tooltip("����Ĺ���ƫ����\r\nԽ��Լ����Ĺ���Խ����\r\n���������ʧ������ֵ����ᵼ�����ҷ�")]
    [Range(0.0f, 1.0f)]
    public float est_Range = 0.3f;
    [Tooltip("�������ֵ�������ף���ֵ��ΪFalse\r\n����������ʧ����������ҷ�")]
    public bool IsUnStable = true;
    #endregion
    #region �ɵ�����
    [Header("�����ö���")]
    public Transform sim;
    #endregion
    #region L1��֪��
    //private Quaternion LocatorRotation = new Quaternion(-0.8675f, -0.085f, 0.1175f, 0.4675f);//�ɽ��ƶ�λ����ת
    private Quaternion LocatorRotation = Quaternion.Euler(-90f, -20f, 0);//�½��ƶ�λ����ת
    private Vector3 M_OrgLampMOffset = new Vector3(0.563f, -0.549f, 2.206f);//�����ǵ����߾���(��ģ)���Ե��߳�Ϊ��λ���������ĵ�����ƫ����
    private Quaternion LampOriginRotation;//�Ʊ���������б��
    private float M_RawLampLength;
    private Vector3 VLine = new(1, 0, 0);//ʡ����cpu�������
    private Vector3 OriginX = new(1, 0, 0);//ʡ����cpu����������
    private Vector3 OriginY = new(0, 1, 0);
    private Vector3 OriginZ = new(0, 0, 1);
    #endregion
    #region L2������
    private Vector3 LocatorX;//ʡ����cpuģ�����������xyz��
    private Vector3 LocatorY;
    private Vector3 LocatorZ;
    private Quaternion M_LampRotation;//��ת����λ����ת������ת�����
    private Vector3 ML_VLine = new(1, 0, 0);//ʡ����cpu�������,��ת��
    private Vector3 M_LampMidOffset = new(1, 0, 0);//ʡ����cpu�������,��ת��
    /// <summary>
    /// XY�����Һ������ƶ��ı��ʣ�ZֵΪǰ���ƶ��ı���
    /// </summary>
    #region �ǹ̶���λ����������(��ʱ������)
    [Obsolete] public Vector3 PrjImgResolution;//�ֱ���Ҳ��ҪͶӰ���ܵõ�ӳ��ֵ
    [Obsolete] public float PrjLampDispRatioX;
    [Obsolete] public float PrjLampDispRatioY;
    [Obsolete] public float PrjLampDispRatioZ;
    #endregion
    #endregion
    #region Debug ����ʱֵ
    [Header("���ֵ")]
    [Tooltip("���Ƴ�����ȥ͸����תӰ��ĺ�����߳�\r\n(������ʧ��Ļ���ֵ��ը)")]
    public float M_estLampLength;
    [Tooltip("���ݵ��߳��ȹ��Ƴ�������λ��\r\n(������ʧ��Ļ���ֵ��ը)")]
    public Vector2 M_estLampMid;
    [Tooltip("���ݵ��߳��ȹ���ĽǶȣ������������������\r\n(������ʧ��Ļ���ֵ��ը)")]
    public float M_estLineAngle;
    [Tooltip("��������ľ����������ȣ�Խ��ԽԶ��ԽСԽ��")]
    public float M_estDept;
    [Tooltip("���ĵĹ�������")]
    public Vector3 M_estPos;
    #endregion
    #region �˲���
    //λ����Ϣ����
    private OneEuroFilter<Vector3> PosFilter;
    //��ת��Ϣ����
    private OneEuroFilter<Quaternion> RotationFilter;
    [Header("OneEuro�˲�������")]
    [Tooltip("�ϸ�minCutoffֵ�ᵼ�¸���ĸ�Ƶ����ͨ�������ϵ͵�ֵ���ʹ�������ƽ��")]
    public float minCutoff = 0.2f;
    [Tooltip("����beta��ʹ�˲����ڿ����ƶ�ʱ������Ӧ����Ҳ�����������ĸ�Ƶ������\r\n��Сbeta���ʹ�˲�������ƽ���������ܵ����ڿ����ƶ�ʱ��Ӧ�ͺ�")]
    public float beta = 0.2f;
    [Tooltip("�ϸߵ�dCutoffֵ��ʹ�˲������ٶȱ仯�������У����ϵ͵�ֵ���ʹ������ٶȱ仯ʱ����ƽ��")]
    public float dCutoff = 0.6f;
    #endregion


    #region һЩ��ʽת��
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
        //���Ʋ���͸��Ӱ���LampLength�ж೤
        float Projection_X = Vector3.Dot(ML_VLine, LocatorX) / LocatorX.magnitude;//���Լ򻯣�����������Ҫ����
        float Projection_Y = Vector3.Dot(ML_VLine, LocatorY) / LocatorY.magnitude;
        float Projection_Z = Vector3.Dot(ML_VLine, LocatorZ) / LocatorZ.magnitude;
        float B_Ratio = Mathf.Sqrt((Projection_X * Projection_X) + (Projection_Z * Projection_Z));
        M_estLampLength = M_RawLampLength / B_Ratio;
        //�ù��ƽ��Ƶķ�ʽ������ͶӰ���ϵĵ������ĵ�ƫ��
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
        _mouseState.Raw_LampPos.y - Projection_MLOffsetZ * M_estLampLength);//�����LampPosƫ������û�б�ը
        IsUnStable = CheckEstValue();
        if (IsUnStable)
        {
            M_estLampMid = _mouseState.Raw_LampPos;
            M_estLampLength = M_RawLampLength;
        }
        M_estLineAngle = (M_estLampLength / _mouseState.ImgResolution.x) * _mouseState.Hfov;//����Ƕ�Ȼ��������
        if (M_estLineAngle < 53.2f)//�̶���λ���Ľ���
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
    /// ����棬���ǹ�������ը�ˣ��ٵĻ������ڽ��ܷ�Χ
    /// </summary>
    /// <returns></returns>
    private bool CheckEstValue()
    {
        return (M_estLampMid - _mouseState.Raw_LampPos).magnitude > _mouseState.ImgResolution.magnitude * est_Range;
    }
    private void DebugDraw()
    {
        DebugToolDrawCoordLine(LocatorRotation, sim);//����
        DebugToolDrawCoordLine(M_LampRotation, this.transform);//����
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
    /// �����λ���ڷ�λ����֣���Ҫ�ò�ֵ�ķ�ʽ������λ��
    /// <para></para>(��������ǰ����ɼ�һ�����������Ȼ��������ʱʵʱ���ֵ)
    /// <para></para>��ʱ����Ҫ���ӵ�����ӳ�䣬������Ҫ���������Ƕ���һ��
    /// <para></para>���Ը÷�ʽ��ʱ����
    /// </summary>
    [Obsolete]
    private void UpdatePrjRatio()
    {
        //����ʵ��ͼ������(xy)�����������(xz)֮���ӳ���ϵ
        //�Ȱ�����ת��xz(ģ��ʱxz��ӦͼƬ��xy)��Ȼ����תͼ������Ӱ�Ƕ��غϣ������ʱ�ĵ����ϵ
        Vector3 SimImgResolutionX = LocatorX * _mouseState.ImgResolution.x;
        Vector3 SimImgResolutionY = LocatorZ * _mouseState.ImgResolution.y; //origin��Unity���꣬y���ϣ�SimImgResolutionΪz����
        Vector3 SimImgResolutionZ = LocatorY * 1; //origin��Unity���꣬Z��ǰ��SimImgResolutionΪY��ǰ����Ϊͼ��û��ȣ�������1�����ϵ
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