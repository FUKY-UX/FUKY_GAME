using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManger : MonoBehaviour
{

    public DialogData[] Talking;
    private bool IsDialogEnd = false;

    public void DialogEnd()
    {
        IsDialogEnd = true;
    }
    public void FirstStep()
    {
        StartCoroutine(WelComeAndUseMouseTask());
    }

    private IEnumerator WelComeAndUseMouseTask()
    {
        DialogController.Instance.StartDialog(Talking[0]);
        while (!IsDialogEnd)
        {
            yield return null;
        }
        Debug.Log("Ω≤ÕÍ¡À");
        GuideSystem.Instance.CompleteCurrentStep();
        yield break;

    }



}
