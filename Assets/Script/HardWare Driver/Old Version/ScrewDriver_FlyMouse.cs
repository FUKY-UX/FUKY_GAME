using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public class ScrewDriver_FlyMouse : MonoBehaviour
{
    public MouseState _mouseState;
    //public Transform LocatorObj;
    public Transform MouseObj;
    public Transform HandPosRef;
    //���ݻ�ȡ
    public float HV_rate = 2f;
    public float Dept_Rate = 1.5f;
    public float smoothingFactor = 0.1f;
    public LampCoordReceiver _LampCoordReceiver;
    public MRotateReciver _MRotateReciver;
    public Vector2 RealCamP= new(320,240);
    private float HFov = 53.2f;
    private float VFov = 39.9f;
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
    
    //��Լ������Ӱ��������ֲ�ƫ����
    public float ML_RotateOffset_Y = 38.9f;//�ڻ�ȡ����������ת�������
    public float ML_OffsetVec_Mul = 1.0f;
    //�Ʊ����߾�30.522����
    //��ǰ�ƶ�67.355���ף�2.206����
    //�������ƶ�17.190���ף�0.563����
    //�����ƶ�16.777���ף�0.549����
    //������Z������Ĵָָ���Լ���ʱ��38.9��(����������ת)
    public Vector3 ML_ScaleOffset => new Vector3(0.563f, -0.549f, 2.206f) * ML_OffsetVec_Mul;//����������ߵ���ʵ��ѧģ�ͣ��Ե���Ϊ��λ������ƫ����
    //������
    private Vector3 V_Line = new(1, 0, 0);
    private Vector3 V_Line_Offset;
    
    private Vector4 O_X_axis = new(1, 0, 0, 1);
    private Vector4 O_Y_axis = new(0, 1, 0, 1);
    private Vector4 O_Z_axis = new(0, 0, 1, 1);
    Vector4 L_X_axis;
    Vector4 L_Y_axis;
    Vector4 L_Z_axis;
    //���˵���תӰ����ʵ�ʵ��߳��Լ��Ƶ��е�����
    public float Tran_LampLength;
    public Vector2 tran_LampMid;
    private Vector2 tran_LampMid_P;//���ǵ���͸��
    //ģ�����
    public float Depth_Z;
    public float Interation_Z=160f;
    //���ؼ���õ������λ�����꣬�ṩ��Mul������������
    public Vector3 Raw_OffsetPos;
    public Vector3 tran_OffsetPos;//�ٴ�ת��������ͷƽ�ŵ�Ĭ����������

    private Quaternion LastRotation;
    public Vector3 LastOffsetPos = Vector3.zero;
    
    public int averageCount = 10; // ���г���  
    private Queue<Vector3> positionQueue = new Queue<Vector3>();


    void Start()
    {
        L_X_axis = LocatorRotation * O_X_axis;
        L_Y_axis = LocatorRotation * O_Y_axis;
        L_Z_axis = LocatorRotation * O_Z_axis;
        //��ʼ����ֵ�˲��Ķ���
        for (int i = 0; i < averageCount; i++)
        {
            positionQueue.Enqueue(Vector3.zero);
        }
    }

    private void Update()
    {
        _RawLampMid = _LampCoordReceiver.LampMidPoint;
        _RawLampLength = _LampCoordReceiver.LampLineLength;

        //ʵʱ���õ���ŷ����ת��Ϊ��Ԫ����ת�ɾ���
        // ��������ת������ˣ��õ�һ���µ������ת����  
        // ע��˷�˳��MouseRotatie4M  *  LocatorRotatie4M
        // ����ζ������Ӧ��LocatorRotatie4M��Ȼ��Ӧ��MouseRotatie4M  
        MLcombinedRotation = _MRotateReciver.RawMouseRotation * LocatorRotation * MouseEulerOffsetQ;
        //MouseObj.rotation = MLcombinedRotation;
        //LocatorObj.rotation = LocatorRotation;
        V_Line = MLcombinedRotation * V_Line;
        //Debug.DrawLine(this.transform.position, this.transform.position+V_Line, Color.gray);
        V_Line_Offset = _MRotateReciver.RawMouseRotation * ML_ScaleOffset;
        //Debug.DrawLine(this.transform.position, this.transform.position + V_Line_Offset, Color.white);
        ////�����ת��ĵ�������ת�������ռ�����ϵ�ͶӰ+ƫ��������ת�������ռ�����ϵ�ͶӰ
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
        ////������ͶӰ���ϵĵ������ĵ�ƫ��
        float Projection_MLOffsetX = Vector3.Dot(V_Line_Offset, L_X_axis)/ L_X_axis.magnitude;
        float Projection_MLOffsetY = Vector3.Dot(V_Line_Offset, L_Y_axis) / L_Y_axis.magnitude;
        float Projection_MLOffsetZ = Vector3.Dot(V_Line_Offset, L_Z_axis) / L_Z_axis.magnitude;
        tran_LampMid = new(_RawLampMid.x - Projection_MLOffsetX * Tran_LampLength, _RawLampMid.y + Projection_MLOffsetZ * Tran_LampLength);
        Vector3 DO = new(Projection_MLOffsetX * Tran_LampLength, 0, Projection_MLOffsetZ * Tran_LampLength);
        DO = LocatorRotation * DO;
        //Debug.DrawLine(this.transform.position, DO + this.transform.position, Color.green);
        float Line_Angle = (Tran_LampLength / RealCamP.x) * HFov;

        ////��ǰ�߶ζ�Ӧ�ĽǶ�,Ȼ����һ���������

        if(Line_Angle< 53.2f)
        {
            Depth_Z = Mathf.Tan((Line_Angle / 2) * (Mathf.PI / 180.0f)) * (Tran_LampLength / 2);
        }

        //��Ҫ������ȸı�XY�����λ��������ֹλ���ٶȹ��졾�����ٿ��ǰɡ�
        //tran_LampMid_P= new Vector2(tran_LampMid.x * (Interation_Z/ Depth_Z), tran_LampMid.y * (Interation_Z / Depth_Z));
        ////�������λ��
        Raw_OffsetPos = new Vector3((tran_LampMid.x / RealCamP.x - 0.5f)* HV_rate, -Depth_Z* Dept_Rate, -(tran_LampMid.y / RealCamP.y - 0.5f)* HV_rate);
        Raw_OffsetPos = LocatorRotation * Raw_OffsetPos;
        //Debug.DrawLine(this.transform.position, tran_OffsetPos + this.transform.position, Color.green);

        float Projection_OffsetPosX = Vector3.Dot(Raw_OffsetPos, O_X_axis) / O_X_axis.magnitude;
        float Projection_OffsetPosY = Vector3.Dot(Raw_OffsetPos, O_Y_axis) / O_Y_axis.magnitude;
        float Projection_OffsetPosZ = Vector3.Dot(Raw_OffsetPos, O_Z_axis) / O_Z_axis.magnitude;

        tran_OffsetPos = HandPosRef.rotation * new Vector3(Projection_OffsetPosX, Projection_OffsetPosY, Projection_OffsetPosZ);
        tran_OffsetPos = HandPosRef.position + tran_OffsetPos;


        // ���¶���  
        //positionQueue.Dequeue();
        //positionQueue.Enqueue(tran_OffsetPos);

        if (_mouseState.mouseState==CurrMouseState.Moving)
        {
            LastOffsetPos = tran_OffsetPos;
            LastRotation = _MRotateReciver.RawMouseRotation;
        }


        // �����Ȩƽ��λ��  

        //Vector3 smoothedPosition = Vector3.zero;
        //float totalWeight = 0f;
        //for (int i = 0; i < averageCount; i++)
        //{
        //    Vector3 pos = positionQueue.ToArray()[i];
        //    float weight = Mathf.Lerp(1.0f, 0.1f, (float)i / (averageCount - 1)); // ���Եݼ�Ȩ��  
        //    smoothedPosition += pos * weight;
        //    totalWeight += weight;
        //}
        //smoothedPosition /= totalWeight;

        // ʹ��Lerp����ƽ������  

        MouseObj.transform.position = Vector3.Lerp(LastOffsetPos, tran_OffsetPos, smoothingFactor);

        if((LastRotation.eulerAngles - _MRotateReciver.RawMouseRotation.eulerAngles).magnitude > 0.1f)
        {
            MouseObj.transform.rotation = HandPosRef.rotation * Quaternion.Lerp(LastRotation, _MRotateReciver.RawMouseRotation, smoothingFactor);
        }

        // ����lastֵΪ��ǰ֡��ֵ��Ϊ��һ֡��׼��  
        LastOffsetPos = MouseObj.transform.position;
        LastRotation = _MRotateReciver.RawMouseRotation;

    }
    // ����һ�������JSON��Ӧƥ���C#��    
}