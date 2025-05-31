using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
    public enum HandState_Type
    {
        Default,
        Grab,
        Idle
    }

    public interface HandState
    {
        void OnEnter();
        void OnExit();
        void OnUpdate();
        void OnFixUpdate();

    }
    [Serializable]
    public class AttributeBoard
    {

    }

    public class HandFSM
    {
        public HandState CurrentState { get; private set; }
        public Dictionary<HandState_Type, HandState> States;
        public AttributeBoard Board;
        public HandFSM(AttributeBoard _Board) 
        { 
            this.States = new Dictionary<HandState_Type, HandState>();
            this.Board = _Board;
        }
        public void AddState(HandState_Type StateType,HandState State)
        {
            if (States.ContainsKey(StateType)) { return; }
            States.Add(StateType, State);
        }
        public void SwitchState(HandState_Type NewState_Type)
        {
            if (!States.ContainsKey(NewState_Type)){return;}
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
