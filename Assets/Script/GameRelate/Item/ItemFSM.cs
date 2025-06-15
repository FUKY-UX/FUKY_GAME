using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
public interface InteractedItemState
{
    public void OnEnter();
    public void OnExit();
    public void OnUpdate();
    public void OnGrab();
    public void OnRelease();
    public void OnFixUpdate();
    public void Grabing();
    public void OnRidigibodyEnter(Collision collision);
    public void OnRidigibodyStay(Collision collision);
    public void OnRidigibodyExit(Collision collision);
    public void OnTriggerEnter(Collider collider)
    {
    }
    public void OnTriggerExit(Collider collider)
    {
    }
    public void OnTriggerStay(Collider collider)
    {
    }

}
[Serializable]
public abstract class AttrBoard
{

}
[Serializable]
public class ItemFSM
{
    public ItemState_Type CurrItemState;
    [HideInInspector]
    public InteractedItemState CurrentState { get; private set; }
    [HideInInspector] 
    public Dictionary<ItemState_Type, InteractedItemState> States;
    [HideInInspector] 
    public DefaultItemAttrBoard AttrBoard;
    public ItemFSM(DefaultItemAttrBoard _Board)
    {
        this.States = new Dictionary<ItemState_Type, InteractedItemState>();
        this.AttrBoard = _Board;
    }
    public void AddState(ItemState_Type StateType, InteractedItemState State)
    {
        if (States.ContainsKey(StateType)) 
        {
            States[StateType] = State;
            return; 
        }
        States.Add(StateType, State);
    }
    public void SwitchState(ItemState_Type NewState_Type)
    {
        if (!States.ContainsKey(NewState_Type)) { Debug.LogError("��Ʒû������״̬"); return; }
        if (CurrentState != null)
        {
            CurrentState.OnExit();
                
        }
        CurrentState = States[NewState_Type];
        CurrItemState = NewState_Type;
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
    public void OnRidigibodyStay(Collision collision)
    {
        CurrentState.OnRidigibodyStay(collision);
    }
    public void OnRidigibodyEnter(Collision collision)
    {
        CurrentState.OnRidigibodyEnter(collision);
    }
    public void OnRidigibodyExit(Collision collision)
    {
        CurrentState.OnRidigibodyExit(collision);

    }
    public void OnTriggerEnter(Collider collider)
    {
        CurrentState.OnTriggerEnter(collider);
    }
    public void OnTriggerExit(Collider collider)
    {
        CurrentState.OnTriggerExit(collider);
    }
    public void OnTriggerStay(Collider collider)
    {
        CurrentState.OnTriggerStay(collider);

    }
}

