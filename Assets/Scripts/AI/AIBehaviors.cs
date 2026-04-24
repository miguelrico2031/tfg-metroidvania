using System;
using UnityEngine;

public abstract class AAIBehaviorState : FSM.IState
{
    protected AIAgentComponent m_Agent;
    public AAIBehaviorState(AIAgentComponent agent) => m_Agent = agent;
    public virtual void Start(Type lastState = null) => Start();
    public virtual void Start() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void End() { }
}

public class AIBehaviorTreeState : AAIBehaviorState
{
    private readonly BT.BehaviorTree m_BehaviorTree;
    private BT.Output m_BehaviorTreeOutput;
    public AIBehaviorTreeState(AIAgentComponent agent, BT.BehaviorTree behaviorTree) : base(agent) { m_BehaviorTree = behaviorTree; }
    public override void Start()
    {
        m_BehaviorTree.Reset();
    }
    public override void Update()
    {
        if (m_BehaviorTreeOutput is BT.Output.Running)
        {
            m_Agent.BTLog("----------------------------FRAME START----------------------------");
            m_BehaviorTreeOutput = m_BehaviorTree.Run();
            if (m_BehaviorTreeOutput is not BT.Output.Running)
            {
                m_Agent.BTLog($"Behavior Tree finished: {m_BehaviorTreeOutput}");
            }
            m_Agent.BTLog("----------------------------FRAME END------------------------------");
        }
    }
}

public class AIDyingState : AAIBehaviorState
{
    public AIDyingState(AIAgentComponent agent) : base(agent) { }
    public override void Start()
    {
        m_Agent.Movement.Stop();
        m_Agent.Animator.StartDeathAnimation();
        m_Agent.DisableAllHitboxes();
    }
}

public abstract class AAIBehaviorTask : BT.ITask
{
    public bool Running { get; set; }
    protected AIAgentComponent m_Agent;
    public AAIBehaviorTask(AIAgentComponent agent) => m_Agent = agent;
    public virtual void Start() { }
    public abstract BT.Output Run();
    public virtual void End(BT.Output output) { }
    public virtual void OnInterrupted() { }
}


public class WaitForSecondsTask : AAIBehaviorTask
{
    private readonly float m_Time;
    private float m_Timer = 0f;
    public WaitForSecondsTask(AIAgentComponent agent, float time) : base(agent) { m_Time = time; }
    public override void Start()
    {
        m_Agent.BTLogAction($"Wait for {m_Time} seconds Started");
        m_Timer = m_Time;
    }
    public override BT.Output Run()
    {
        m_Timer -= Time.deltaTime;
        return m_Timer <= 0f ? BT.Output.Success : BT.Output.Running;
    }

    public override void End(BT.Output output)
    {
        m_Agent.BTLogAction($"Wait for {m_Time} seconds Ended");
    }
}

public class MoveForwardTask : AAIBehaviorTask
{
    public MoveForwardTask(AIAgentComponent agent) : base(agent) { }

    public override void Start()
    {
        m_Agent.BTLogAction("MoveForward Started");
        m_Agent.Animator.StartGroundedAnimation();
    }
    public override BT.Output Run()
    {
        m_Agent.BTLogAction("MoveForward Run");
        m_Agent.Movement.MoveForward();
        return BT.Output.Running;
    }
    public override void End(BT.Output output)
    {
        m_Agent.BTLogAction("MoveForward Ended");
    }
    public override void OnInterrupted()
    {
        m_Agent.BTLogAction("MoveForward Interrupted");
    }
}

public class AttackTargetTask : AAIBehaviorTask
{
    public AttackTargetTask(AIAgentComponent agent) : base(agent) { }

    public override void Start()
    {
        m_Agent.BTLogAction("Attack Started");
        m_Agent.Animator.StartAttackAnimation(true);
    }
    public override BT.Output Run()
    {
        m_Agent.BTLogAction("Attack Run");
        return m_Agent.Animator.AttackAnimationPhaseCompletedThisFrame is AttackAnimationPhase.Withdrawing
            ? BT.Output.Success
            : BT.Output.Running;
    }
}

public class IdleTask : AAIBehaviorTask
{
    public IdleTask(AIAgentComponent agent) : base(agent) { }

    public override void Start()
    {
        m_Agent.BTLogAction("Idle Started");
        m_Agent.Movement.Stop();
        m_Agent.Animator.StartGroundedAnimation();
    }
    public override BT.Output Run()
    {
        m_Agent.BTLogAction("Idle Run");
        return BT.Output.Running;
    }
}