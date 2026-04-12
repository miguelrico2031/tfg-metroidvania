using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public interface IState
{
    public void Start(Type lastState);
    public void Update();
    public void FixedUpdate();
    public void End();
}

[Serializable]
public class StateMachine
{
    public Type CurrentState => m_CurrentState?.State.GetType();
    public event Action OnStateChanged;

    private StateNode m_CurrentState;
    private readonly IState m_InitialState;
    private readonly IReadOnlyDictionary<Type, StateNode> m_States;
    private readonly IReadOnlyCollection<TransitionNode> m_TransitionsFromAnyState;

    public StateMachine(
        IState initialState,
        IReadOnlyDictionary<Type, StateNode> states,
        IReadOnlyCollection<TransitionNode> transitionsFromAnyState)
    {
        Assert.IsNotNull(initialState);
        Assert.IsNotNull(states);
        Assert.IsNotNull(transitionsFromAnyState);
        m_InitialState = initialState;
        m_States = states;
        m_TransitionsFromAnyState = transitionsFromAnyState;
    }

    public void Start()
    {
        ChangeCurrentState(m_InitialState);
    }

    public void Update()
    {
        TransitionNode transition = EvaluateTransitions();
        if (transition is not null)
        {
            ChangeCurrentState(transition.TargetState);
        }
        m_CurrentState.State?.Update();
    }

    public void FixedUpdate()
    {
        m_CurrentState.State?.FixedUpdate();
    }

    private void ChangeCurrentState(IState state)
    {
        Assert.IsNotNull(state);
        m_CurrentState?.State.End();
        StateNode newStateNode = m_States[state.GetType()];
        newStateNode.State.Start(CurrentState);
        m_CurrentState = newStateNode;
        OnStateChanged?.Invoke();
    }

    private TransitionNode EvaluateTransitions()
    {
        foreach (var transition in m_TransitionsFromAnyState)
        {
            if (transition.Transition.Invoke(CurrentState))
                return transition;
        }
        foreach (var transition in m_CurrentState.Transitions)
        {
            if (transition.Transition.Invoke(CurrentState))
                return transition;
        }
        return null;
    }


    public class StateNode
    {
        public IState State;
        public readonly List<TransitionNode> Transitions = new();
    }

    public class TransitionNode
    {
        public IState TargetState;
        public Func<Type, bool> Transition;
    }
}