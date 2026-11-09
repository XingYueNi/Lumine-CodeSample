using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StateType
{
    IDLE,
    MOVE,
    DEATH,
    PARRY,
    GUARD,
    HIT
}
public  class  FSM
{
    public StateBase currentState;
    private Dictionary<StateType, StateBase> allSaveState;
    public FSM()
    {
        allSaveState = new Dictionary<StateType, StateBase>();
    }
    public void OnTick()
    {
        currentState?.OnUpdate();
    }
    public void AddState(StateType stateType,StateBase state)
    {
        if (allSaveState.ContainsKey(stateType)) return;
        allSaveState.Add(stateType, state);
    }
    public void SetState(StateType stateType)
    {
        if (currentState == allSaveState[stateType]) return;
        currentState?.OnExit();
        currentState = allSaveState[stateType];
        currentState?.OnEnter();
    }
}
