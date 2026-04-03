using System;
using System.Collections.Generic;
using UnityEngine.Assertions;


public interface IState
{
    public void Start();
    public void Update();
    public void FixedUpdate();
    public void End();
}

public abstract class AState : IState 
{
    public virtual void Start() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void End() { }
}

public interface ICondition
{
    public bool Check();
}

public class Condition : ICondition
{
    private Func<bool> m_Condition;
    public Condition(Func<bool> condition)
    {
        m_Condition = condition;
    }

    public bool Check()
    {
        return m_Condition.Invoke();
    }
}

public interface ITransition
{
    public IState TargetState {  get; }
    public ICondition Condition { get; }
}

public class Transition : ITransition
{
    public IState TargetState { get; }
    public ICondition Condition { get; }
    public Transition(IState targetState,  ICondition condition)
    {
        TargetState = targetState;
        Condition = condition;
    }
}
public class StateNode
{
    public IState State { get; }
    public HashSet<ITransition> Transitions { get; }

    public StateNode(IState state)
    {
        State = state;
        Transitions = new();
    }

    public void AddTransition(IState state, ICondition condition)
    {
        Transitions.Add(new Transition(state, condition));
    }
}

[Serializable]
public class StateMachine
{
    public IState CurrentState => m_CurrentState.State;
    private StateNode m_CurrentState;
    private readonly IState m_InitialState;
    private readonly IReadOnlyDictionary<Type, StateNode> m_States;
    private readonly IReadOnlyCollection<ITransition> m_TransitionsFromAnyState;

    public StateMachine(
        IState initialState,
        IReadOnlyDictionary<Type, StateNode> states,
        IReadOnlyCollection<ITransition> transitionsFromAnyState)
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
        var transition = CheckTransitions();
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
        var previousState = m_CurrentState?.State;
        if (state == previousState)
            return;

        var newStateNode = m_States[state.GetType()];

        previousState?.End();
        newStateNode.State?.Start();
        m_CurrentState = newStateNode;
    }

    private ITransition CheckTransitions()
    {
        foreach(var transition in m_TransitionsFromAnyState)
        {
            if(transition.Condition.Check())
                return transition;
        }
        foreach (var transition in m_CurrentState.Transitions)
        {
            if (transition.Condition.Check())
                return transition;
        }
        return null;
    }
}
