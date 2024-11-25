using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Jobs;

public class FUKYMouse_MathBase_bk : MonoBehaviour
{
    #region �¼����
    [Header("������")]
    public MouseState _mouseState;
    #endregion

    #region �ɵ�����
    public Vector3 MouseRotateAdj = Vector3.zero;
    [Header("�����ö���")]
    public Transform sim;
    #endregion
    #region L1��֪��
    private Quaternion LocatorRotation = new Quaternion(-0.8675f, -0.085f, 0.1175f, 0.4675f);//��β�����Ľ��ƶ�λ����ת
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
    #endregion
    #region Debug ����ʱֵ
    [Header("Debugֵ")]
    public  float M_RealLampLength;
    public Vector2 M_estLampMid;
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
    }

    // Update is called once per frame
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
        M_RealLampLength = M_RawLampLength / B_Ratio;
        ////������ͶӰ���ϵĵ������ĵ�ƫ��
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
        DebugToolDrawCoordLine(LocatorRotation, sim);//����
        DebugToolDrawCoordLine(M_LampRotation, this.transform);//����
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
