using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum HandState_Type
{
    Default,
    Grab
}

public class HandState
{

}



public class HandFSM : MonoBehaviour
{
    public HandState CurrentState { get; private set; }
    public FUKYMouse_MathBase FUKY_GAME;
    public ItemBase Object;
    private void Start()
    {
        CurrentState = HandState.Default;
        EnterState(CurrentState);
    }

    void Update()
    {
        this.transform.position = FUKY_GAME.FukyHandPos;
        this.transform.rotation = Camera.main.transform.rotation * FUKY_GAME.FukyHandRotate;
        // ���ݵ�ǰ״ִ̬����Ӧ�ĸ����߼�

        switch (CurrentState)
        {
            case HandState.Default:
                UpdateIdleState();
                break;

            case HandState.Grab:
                UpdateWalkingState();
                break;

             default: break;

        }
    }

    // ����״̬ʱ����
    private void EnterState(HandState newState)
    {
        Debug.Log("Entering state: " + newState);
        // ����������ִ�н���״̬ʱ��Ҫ��һЩ��ʼ������
    }

    // �˳�״̬ʱ����
    private void ExitState(HandState oldState)
    {
        Debug.Log("Exiting state: " + oldState);
        // ����������ִ���˳�״̬ʱ��Ҫ��һЩ�������
    }

    // �л�״̬�ķ���
    private void TransitionToState(HandState newState)
    {
        if (CurrentState != newState)
        {
            ExitState(CurrentState);
            CurrentState = newState;
            EnterState(CurrentState);
        }
    }


}
