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
    #region ������
    [Header("������")]
    [SerializeField]
    private LampCoordReceiver _LampCoordReceiver;
    [SerializeField]
    private MRotateReciver _MRotateReciver;
    #endregion
    #region ���ֵ
    [Header("���ֵ")]
    [Tooltip("��λ�������ͼ�����꣬�ֱ���Ĭ����320X240")]
    public Vector2 Raw_LampPos;
    [Tooltip("��괫�����Ԫ����ת��Ϣ������MRotateReciver�޸�����Unity")]
    public Quaternion Raw_MRotation;
    [Tooltip("����긡��ʱ�����ͼ������\r\n��꿪ʼ����ʱ���������Ϊԭ��(0,0)\r\n����X��С������Y����")]
    public Vector2 Float_LampPos;
    [Tooltip("����긡��ʱ�������ת��Ϣ����꿪ʼ����ʱ����ת���Ϊ��Ԫ��(0,0,0,0)")]
    public Quaternion Float_MRotation;
    [Tooltip("���ߵĳ���")]
    public float LampLineLength;
    [Tooltip("��ǰ�����״̬")]
    public CurrMouseState mouseState;
    [Tooltip("������ʱ��")]
    public float LostTrackTime = 0;
    [Tooltip("����״̬")]
    public bool IsTracked;
    [Tooltip("�����ǹ���״̬\r\n����ֲ���ȴһֱ�ڹ���״̬��˵���������Լ�ת�����ˣ���Ҫ����")]
    public bool GyroWork;
    [Tooltip("�������ǲ��ǻ���\r\n������ˣ���Ҫ����")]
    public bool GyroBroken;
    [Tooltip("λ�����ݵĸ���Ƶ��")]
    public float PosFreq = 0.02f;//���ݵĸ���Ƶ��
    [Tooltip("��ת���ݵĸ���Ƶ��")]
    public float RotateFreq = 0.02f;//���ݵĸ���Ƶ��
    #endregion
    #region ��λ������
    [Header("��λ������")]
    [Tooltip("Խ��Խ�ѽ�������״̬")]
    [Range(0, 5)]
    public float Thr_Float = 1.8f;
    [Tooltip("��׷��ʱ�䣬ֵԽ�󣬶Զ�����Խ����")]
    public float Thr_ReSetTime = 1;
    [Tooltip("�����ֵ��ƶ���Χ��Խ��Խ���׳��磬ԽСԽ�Ѵ�������")]
    [Range(-20,20)]
    public float Thr_TrackPos = 0;
    [Tooltip("�����ֵ���ת��ֵ���ᵼ�¶�λ�����ڶ�����ʱ�������㵼�²�׼ȷ")]
    [Range(-20, 20)]
    public float Thr_TrackRotate = 0;
    #endregion
    #region ϵͳ
    [Header("ϵͳ����")]
    [Tooltip("�ֱ��ʣ���������ķֱ�����������һ�㲻Ҫ��")]
    public Vector3 ImgResolution = new(320, 240,0);
    [Tooltip("�����Hfov")]
    public float Hfov = 53.2f;
    [Tooltip("�����ǵ����ж�\r\nԽ�����ƮԽ���ݣ�ԽСϵͳ��������Ʒ��Ҫ��Խ��")]
    [Range(0, 5)]
    public float Thr_GyroMove = 1.7f;
    [Tooltip("�������ڹ�����ʱ��(��Ʈ�������Ҳ�ᱻʶ��ɹ���)")]
    public float GyroWorkTime = 0;
    [Tooltip("����������һֱ�Ҷ��������ǻᱻ�㻵\r\n������Ʈ��ǿҲ�ᵼ�³������ֵ��Ȼ�������Ǿ���Ҫ����")]
    [Range(5.0f,20f)]
    public float GyroMaxWorkTime = 10f;
    [Tooltip("��׷�ٷ�Χ�趨�ľ��嵽�ֱ��ʵİ汾\r\nXYΪ��С���������⣬ZWΪ����")]
    public Vector4 _TrackZone = new(30,270,20,220);
    [Tooltip("��ת��Ӧ��ʵ���ߵ�ֵ\r\nXΪ����ת��������׷�٣�YΪ����ת��������׷��")]
    public Vector2 _TrackRotate = new(20, 30);
    #endregion
    #region �㦤�õ�һЩ����
    private Vector2 Last_LampPos;//��һ֡�ĵ���λ��ֵ
    private Quaternion Last_MRotation;//��һ֡������תֵ
    private Vector2 PrevFoat_LampPos;//����������Floatʱ��λ����Ϣ
    private Quaternion PrevFoat_MRotation; //����Floatʱ����ת��Ϣ
    private Vector3 LastMousePos = Vector3.zero;//��һ֡���ԭ���������ĻPos
    public float LastPosUpdateTime = 0.00f;//���ݵĸ���Ƶ��
    public float LastRotateUpdateTime = 0.02f;//���ݵĸ���Ƶ��
    #endregion


    //Deltaֵ
    [Header("Deltaֵ")]
    /// <summary>
    /// ����ÿ֡�ƶ��ľ��룬����һ֡λ��ָ����һ֡λ��
    /// </summary>
    public Vector2 Delta_LampPos;
    /// <summary>
    /// ��������һ֡��ת����ת����
    /// </summary>
    public Quaternion Delta_MRotation;

    #region �����õ�һЩ��ֵ
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
    /// ���¦�ֵ��һ�Ѳ���
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
    /// ��ⶪ׷�ٵ��߼�
    /// </summary>
    private bool LostTrackHandler()
    {
        if(Raw_LampPos == Last_LampPos) return false;
        else return (TrackRotate() && TrackZone());
    }
    /// <summary>
    /// �����������������ᱨ��
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
