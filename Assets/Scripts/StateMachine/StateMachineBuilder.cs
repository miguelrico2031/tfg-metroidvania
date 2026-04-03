using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class StateMachineBuilder
{

    private IState m_InitialState;
    private Dictionary<Type, StateNode> m_States = new();
    private HashSet<ITransition> m_TransitionsFromAnyState = new();
    
    public StateMachineBuilder AddState(IState state, bool isInitialState = false)
    {
        Assert.IsNotNull(state);
        Assert.IsFalse(m_States.ContainsKey(state.GetType()),
            "State already added to StateMachineBuilder. Only one instance per state type allowed.");
        m_States[state.GetType()] = new StateNode(state);

        if(isInitialState)
        {
            Assert.IsNull(m_InitialState, "Initial State already set.");
            m_InitialState = state;
        }
        return this;
    }
    public StateMachineBuilder AddTransition<TFromState, TTargetState>(ICondition condition)
    {
        Assert.IsTrue(m_States.ContainsKey(typeof(TFromState)), "From State not registered.");
        Assert.IsTrue(m_States.ContainsKey(typeof(TTargetState)), "Target State not registered.");
        m_States[typeof(TFromState)].Transitions.Add(new Transition(m_States[typeof(TTargetState)].State, condition));
        return this;
    }

    public StateMachineBuilder AddTransitionFromAnyState<TTargetState>(ICondition condition)
    {
        Assert.IsTrue(m_States.ContainsKey(typeof(TTargetState)), "Target State not registered.");
        m_TransitionsFromAnyState.Add(new Transition(m_States[typeof(TTargetState)].State, condition));
        return this;
    }


    public StateMachine Build()
    {
        var stateMachine = new StateMachine(m_InitialState, m_States, m_TransitionsFromAnyState);
        m_InitialState = null;
        m_States = null;
        m_TransitionsFromAnyState = null;
        return stateMachine;
    }

}