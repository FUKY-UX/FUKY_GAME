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
        // 根据当前状态执行相应的更新逻辑

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

    // 进入状态时调用
    private void EnterState(HandState newState)
    {
        Debug.Log("Entering state: " + newState);
        // 可以在这里执行进入状态时需要的一些初始化操作
    }

    // 退出状态时调用
    private void ExitState(HandState oldState)
    {
        Debug.Log("Exiting state: " + oldState);
        // 可以在这里执行退出状态时需要的一些清理操作
    }

    // 切换状态的方法
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
