using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempHand : MonoBehaviour
{

    public FUKYMouse_MathBase FUKY_GAME;
    private Quaternion OriginRotate;
   
    #region 滤波器
    //位置信息更新
    private OneEuroFilter xFilter;
    private OneEuroFilter yFilter;
    private OneEuroFilter zFilter;
    //旋转信息更新
    private OneEuroFilter QxFilter;
    private OneEuroFilter QyFilter;
    private OneEuroFilter QzFilter;
    private OneEuroFilter QwFilter;
    #endregion

    [Header("OneEuro滤波器设置")]
    [Tooltip("低的minCutoff值会导致更多的高频噪声通过，而较高的值则会使输出更加平滑")]
    public float minCutoff = 0.5f;
    [Tooltip("增加beta会使滤波器在快速移动时更加响应，但也可能引入更多的高频噪声。\r\n减小beta则会使滤波器更加平滑，但可能导致在快速移动时响应滞后。")]
    public float beta = 0.5f;
    [Tooltip("较高的dCutoff值会使滤波器对速度变化更加敏感，而较低的值则会使输出在速度变化时更加平滑")]
    public float dCutoff = 0.5f;

    public float filteredX;
    public float filteredY;
    public float filteredZ;

    public float RotateSmooth = 0.33f;

    private void Start()
    {
        OriginRotate = this.transform.rotation;

        // 假设初始时间为0，初始位置为Vector3.zero
        float initialTime = 0.0f;
        Vector3 initialPosition = Vector3.zero;
        // 创建滤波器实例，
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

        // 对每个分量应用OneEuroFilter
        filteredX = xFilter.Compute(currentTime, FUKY_GAME.FukyHandPos.x);
        filteredY = yFilter.Compute(currentTime, FUKY_GAME.FukyHandPos.y);
        filteredZ = zFilter.Compute(currentTime, FUKY_GAME.FukyHandPos.z);

        Vector3 SmoothPos = new(filteredX, filteredY, filteredZ);

        // 组合滤波后的分量

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
