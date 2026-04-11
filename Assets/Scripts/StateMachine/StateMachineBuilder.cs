using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class StateMachineBuilder
{

    private IState m_InitialState;
    private Dictionary<Type, StateMachine.StateNode> m_States = new();
    private List<StateMachine.TransitionNode> m_TransitionsFromAnyState = new();

    public StateMachineBuilder AddState(IState state, bool isInitialState = false)
    {
        Assert.IsNotNull(state);
        Assert.IsFalse(m_States.ContainsKey(state.GetType()),
            "State already added to StateMachineBuilder. Only one instance per state type allowed.");
        m_States[state.GetType()] = new() { State = state };

        if (isInitialState)
        {
            Assert.IsNull(m_InitialState, "Initial State already set.");
            m_InitialState = state;
        }
        return this;
    }
    public StateMachineBuilder AddTransition<TFromState, TTargetState>(Func<bool> transition)
    {
        Assert.IsTrue(m_States.ContainsKey(typeof(TFromState)), "From State not registered.");
        Assert.IsTrue(m_States.ContainsKey(typeof(TTargetState)), "Target State not registered.");
        m_States[typeof(TFromState)].Transitions.Add(new()
        {
            TargetState = m_States[typeof(TTargetState)].State,
            Transition = state => transition.Invoke()
        });
        return this;
    }

    public StateMachineBuilder AddTransitionFromAnyState<TTargetState>(Func<Type, bool> transition)
    {
        Assert.IsTrue(m_States.ContainsKey(typeof(TTargetState)), $"Target State {nameof(TTargetState)} not registered.");
        m_TransitionsFromAnyState.Add(new()
        {
            TargetState = m_States[typeof(TTargetState)].State,
            Transition = transition
        });
        return this;
    }

    public StateMachineBuilder AddTransitionFromAnyState<TTargetState>(Func<bool> transition)
    {
        Assert.IsTrue(m_States.ContainsKey(typeof(TTargetState)), $"Target State {nameof(TTargetState)} not registered.");
        m_TransitionsFromAnyState.Add(new()
        {
            TargetState = m_States[typeof(TTargetState)].State,
            Transition = state => transition.Invoke()
        });
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