using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static InteractedItemOrigin;
namespace Item_FSM
{
    public enum ItemState_Type
    {
        Default,
        State1,
        State2,
        State3,
        State4,
        State5,
        State6
    }
    public interface InteractedItem
    {
        public void OnEnter();
        public void OnExit();
        public void OnUpdate();
        public void OnGrab();
        public void OnRelease();
        public void OnFixUpdate();

    }
    [Serializable]
    public class ItemAttrBoard
    {

    }

    public class ItemFSM :MonoBehaviour
    {
        public InteractedItem CurrentState { get; private set; }
        public Dictionary<ItemState_Type, InteractedItem> States;
        public ItemAttrBoard AttrBoard;
        public ItemFSM(ItemAttrBoard _Board)
        {
            this.States = new Dictionary<ItemState_Type, InteractedItem>();
            this.AttrBoard = _Board;
        }
        public void AddState(ItemState_Type StateType, InteractedItem State)
        {
            if (States.ContainsKey(StateType)) { return; }
            States.Add(StateType, State);
        }
        public void SwitchState(ItemState_Type NewState_Type)
        {
            if (!States.ContainsKey(NewState_Type)) { return; }
            if (CurrentState != null)
            {
                CurrentState.OnExit();
            }
            CurrentState = States[NewState_Type];
            CurrentState.OnEnter();
        }
        public void OnUpdate()
        {
            CurrentState.OnUpdate();
        }
        public void OnFixUpdate()
        {
            CurrentState.OnFixUpdate();
        }
    }

}

//public FUKYMouse_MathBase FUKY_GAME;
//public ItemBase Object;
//private void Start()
//{
//    CurrentState = HandState_Type.Default;
//    EnterState(CurrentState);
//}
//void Update()
//{
//    //this.transform.position = FUKY_GAME.FukyHandPos;
//    //this.transform.rotation = Camera.main.transform.rotation * FUKY_GAME.FukyHandRotate;
//    // 根据当前状态执行相应的更新逻辑
//}

//private void TransitionToState(HandState newState)
//{
//    if (CurrentState != newState)
//    {
//        ExitState(CurrentState);
//        CurrentState = newState;
//        EnterState(CurrentState);
//    }
//}
// 切换状态的方法
