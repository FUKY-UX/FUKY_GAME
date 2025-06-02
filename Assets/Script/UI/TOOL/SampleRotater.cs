using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleRotater : MonoBehaviour
{
    public RectTransform RotateItem;
    public RectTransform CenterRotateItem;
    public float RotateSpeed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RotateItem.rotation *= Quaternion.Euler(0,0, RotateSpeed * Time.deltaTime);
        CenterRotateItem.rotation *= Quaternion.Euler(0, 0, -RotateSpeed*2 * Time.deltaTime);
        CenterRotateItem.anchoredPosition = RotateItem.anchoredPosition;
    }
}
