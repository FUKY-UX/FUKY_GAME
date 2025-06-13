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
    [Serializable]
    public class HandFSM
    {
        public HandState_Type CurrentStateType;

        [HideInInspector]
        public HandState CurrentState { get; private set; }

        [HideInInspector]
        public Dictionary<HandState_Type, HandState> States;

        [HideInInspector]
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
            CurrentStateType = NewState_Type;
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
//    // ���ݵ�ǰ״ִ̬����Ӧ�ĸ����߼�
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
// �л�״̬�ķ���
