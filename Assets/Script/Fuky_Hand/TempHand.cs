using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempHand : MonoBehaviour
{

    public FUKYMouse_MathBase FUKY_GAME;
    private Quaternion OriginRotate;
   
    #region �˲���
    //λ����Ϣ����
    private OneEuroFilter xFilter;
    private OneEuroFilter yFilter;
    private OneEuroFilter zFilter;
    //��ת��Ϣ����
    private OneEuroFilter QxFilter;
    private OneEuroFilter QyFilter;
    private OneEuroFilter QzFilter;
    private OneEuroFilter QwFilter;
    #endregion

    [Header("OneEuro�˲�������")]
    [Tooltip("�͵�minCutoffֵ�ᵼ�¸���ĸ�Ƶ����ͨ�������ϸߵ�ֵ���ʹ�������ƽ��")]
    public float minCutoff = 0.5f;
    [Tooltip("����beta��ʹ�˲����ڿ����ƶ�ʱ������Ӧ����Ҳ�����������ĸ�Ƶ������\r\n��Сbeta���ʹ�˲�������ƽ���������ܵ����ڿ����ƶ�ʱ��Ӧ�ͺ�")]
    public float beta = 0.5f;
    [Tooltip("�ϸߵ�dCutoffֵ��ʹ�˲������ٶȱ仯�������У����ϵ͵�ֵ���ʹ������ٶȱ仯ʱ����ƽ��")]
    public float dCutoff = 0.5f;

    public float filteredX;
    public float filteredY;
    public float filteredZ;

    public float RotateSmooth = 0.33f;

    private void Start()
    {
        OriginRotate = this.transform.rotation;

        // �����ʼʱ��Ϊ0����ʼλ��ΪVector3.zero
        float initialTime = 0.0f;
        Vector3 initialPosition = Vector3.zero;
        // �����˲���ʵ����
        InitFilter(initialTime, initialPosition);
    }

    private void InitFilter(float initialTime, Vector3 initialPosition)
    {
        xFilter = new OneEuroFilter(initialTime, initialPosition.x);
        yFilter = new OneEuroFilter(initialTime, initialPosition.y);
        zFilter = new OneEuroFilter(initialTime, initialPosition.z);

        QxFilter = new OneEuroFilter(initialTime, initialPosition.x);
        QyFilter = new OneEuroFilter(initialTime, initialPosition.y);
        QzFilter = new OneEuroFilter(initialTime, initialPosition.z);
        QwFilter = new OneEuroFilter(initialTime, initialPosition.x);
    }

    void Update()
    {
        float currentTime = Time.deltaTime;

        xFilter.UpdateSetting(minCutoff, beta, dCutoff);
        yFilter.UpdateSetting(minCutoff, beta, dCutoff);
        zFilter.UpdateSetting(minCutoff, beta, dCutoff);

        // ��ÿ������Ӧ��OneEuroFilter
        filteredX = xFilter.Compute(currentTime, FUKY_GAME.FukyHandPos.x);
        filteredY = yFilter.Compute(currentTime, FUKY_GAME.FukyHandPos.y);
        filteredZ = zFilter.Compute(currentTime, FUKY_GAME.FukyHandPos.z);

        Vector3 SmoothPos = new(filteredX, filteredY, filteredZ);

        // ����˲���ķ���

        Vector3 filteredPosition = new Vector3(filteredX, filteredY, filteredZ);



        this.transform.position = SmoothPos;
        this.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation 
            * AdjustQuaternion(FUKY_GAME._mouseState.Float_MRotation),this.transform.rotation, 0.1f);
   
    }

    private Quaternion AdjustQuaternion(Quaternion Input)
    {
        return Quaternion.Euler(Input.eulerAngles * (90 * RotateSmooth / 15));
    }
}
