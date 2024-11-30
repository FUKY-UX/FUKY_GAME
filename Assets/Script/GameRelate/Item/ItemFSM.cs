using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
        public void OnColliderEnter();
        public void Grabing();
        public void OnColliderStay();
        public void OnColliderExit();
    }
    [Serializable]
    public class AttrBoard
    {

    }
    public class ItemFSM
    {
        public InteractedItem CurrentState { get; private set; }
        public Dictionary<ItemState_Type, InteractedItem> States;
        public DefaultItemAttrBoard AttrBoard;
        public ItemFSM(DefaultItemAttrBoard _Board)
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
        public void OnColliderEnter()
        {
            CurrentState.OnColliderEnter();
        }
        public void OnGrab()
        {
            CurrentState.OnGrab();
        }
        public void Grabing()
        {
            CurrentState.Grabing();
        }
        public void OnRelease()
        {
            CurrentState.OnRelease();
        }
        public void OnColliderStay()
        {
            CurrentState.OnColliderStay();
        }
        public void OnColliderExit()
        {
        }
    }

}
