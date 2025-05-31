using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampDebugUICtrol : MonoBehaviour
{

    public Vector2 OriD1;
    public Vector2 EstD2;
    public MouseState _mouseState;
    public FUKYMouse_MathBase _FUKYState;

    public Vector2 SC_OriginLampsPos;
    public Vector2 SC_EstRealLampPos;

    public RectTransform OriginPos;
    public RectTransform EstRealPos;
    public RectTransform refObj;

    void Update()
    {
        SC_OriginLampsPos = new(_mouseState.Raw_LampPos.x / _mouseState.ImgResolution.x * Camera.main.pixelWidth,
            _mouseState.Raw_LampPos.y/2 / _mouseState.ImgResolution.y * Camera.main.pixelWidth);

        SC_EstRealLampPos = new(_FUKYState.M_estLampMid.x / _mouseState.ImgResolution.x * Camera.main.pixelWidth,
            _FUKYState.M_estLampMid.y/2 / _mouseState.ImgResolution.y * Camera.main.pixelWidth);


        RectTransformUtility.ScreenPointToLocalPointInRectangle
            (refObj, SC_OriginLampsPos, Camera.main, out OriD1);
        OriginPos.anchoredPosition = OriD1;

        RectTransformUtility.ScreenPointToLocalPointInRectangle
    (refObj, SC_EstRealLampPos, Camera.main, out EstD2);
        EstRealPos.anchoredPosition = EstD2;

    }

    private void OnDisable()
    {
        OriginPos.gameObject.SetActive(false);
        EstRealPos.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        OriginPos.gameObject.SetActive(true);
        EstRealPos.gameObject.SetActive(true);

    }
}
