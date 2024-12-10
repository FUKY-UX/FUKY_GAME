using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cooking_Good : MonoBehaviour
{
    public AnimationCurve showCurve;
    public AnimationCurve hideCurve;
    public float animationSpeed;
    public GameObject panel;

    IEnumerator ShowPanel(GameObject gameObject)
    {
        float timer = 0;
        while (timer <= 1) 
        {
            gameObject.transform.localScale = Vector3.one * showCurve.Evaluate(timer);
            timer += Time.deltaTime * animationSpeed;
            yield return null;
        }
    }

    IEnumerator HidePanel(GameObject gameObject)
    {
        float timer = 0;
        while (timer <= 1)
        {
            gameObject.transform.localScale = Vector3.one * hideCurve.Evaluate(timer);
            timer += Time.deltaTime * animationSpeed;
            yield return null;
        }
        // 循环结束，强制将缩放设为0
        gameObject.transform.localScale = Vector3.zero;
    }

    // 新增：对外公开的显示面板方法
    public void ShowPanelExternally()
    {
        StartCoroutine(ShowPanel(panel));
    }
    // 新增：对外公开的隐藏面板方法
    public void HidePanelExternally()
    {
        StartCoroutine(HidePanel(panel));
    }
}
