using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(AnimatorComponent), typeof(ObstacleCheckComponent), typeof(EdgeCheckComponent))]
[RequireComponent(typeof(AITargetterComponent), typeof(AIMovementComponent))]
public class AIAgentComponent : MonoBehaviour
{
    public AIMovementComponent Movement { get; private set; }
    public ObstacleCheckComponent ObstacleCheck { get; private set; }
    public EdgeCheckComponent EdgeCheck { get; private set; }
    public AITargetterComponent Targetter { get; private set; }
    public AnimatorComponent Animator { get; private set; }

    [SerializeField] private BehaviorTree m_SelectedBehaviorTree;
    [SerializeField] private bool m_LogBehavior;

    private BT.BehaviorTree m_BehaviorTree;
    private BT.Output m_BehaviorTreeOutput = BT.Output.Running;


    private void Awake()
    {
        Movement = GetComponent<AIMovementComponent>();
        ObstacleCheck = GetComponent<ObstacleCheckComponent>();
        EdgeCheck = GetComponent<EdgeCheckComponent>();
        Targetter = GetComponent<AITargetterComponent>();
        Animator = GetComponent<AnimatorComponent>();

        m_BehaviorTree = SetUpSelectedBehaviorTree();
    }

    private void Update()
    {
        if (m_BehaviorTreeOutput is BT.Output.Running)
        {
            Log("----------------------------FRAME START----------------------------");
            m_BehaviorTreeOutput = m_BehaviorTree.Run();
            if (m_BehaviorTreeOutput is not BT.Output.Running)
            {
                Log($"Behavior Tree finished: {m_BehaviorTreeOutput}");
            }
            Log("----------------------------FRAME END------------------------------");
        }
    }

    private BT.BehaviorTree SetUpSelectedBehaviorTree()
    {
        BT.INode selectedBT = m_SelectedBehaviorTree switch
        {
            BehaviorTree.Patrol => m_PatrolBT,
            BehaviorTree.ChaseTarget => m_ChaseTargetBT,
            BehaviorTree.TryAttackTarget => m_TryAttackTargetBT,
            BehaviorTree.CrabClawBehavior => m_CrabClawBT,
            _ => null
        };
        return new BT.BehaviorTree(selectedBT);
    }

    private void Log(string message)
    {
        if (m_LogBehavior)
        {
            Debug.Log($"[BT] [{gameObject.name}] {message}");
        }
    }
    public void LogAction(string message) => Log($"[ACTION] {message}");
    public void LogCondition(string message) => Log($"[CONDITION] {message}");

    #region CONDITIONS
    private bool IsFacingObstacle()
    {
        LogCondition($"IsFacingObstacle: {ObstacleCheck.IsObstructedForward}");
        return ObstacleCheck.IsObstructedForward;
    }

    private bool IsFacingEdge()
    {
        LogCondition($"IsFacingEdge: {EdgeCheck.HasEdgeForward}");
        return EdgeCheck.HasEdgeForward;
    }

    private bool HasAliveTarget()
    {
        bool hasAliveTarget = Targetter.ActiveTarget is { IsAlive: true };
        LogCondition($"HasAliveTarget: {hasAliveTarget}");
        return hasAliveTarget;
    }

    private bool IsFacingTarget()
    {
        Assert.IsNotNull(Targetter.ActiveTarget, "Wrong BT desgn, IsFacingTarget should not be called if there is no target.");
        float directionToTarget = Targetter.ActiveTarget.transform.position.x - transform.position.x;
        bool isFacingTarget = Mathf.Abs(directionToTarget) < 0.01f ||
            Mathf.CeilToInt(Mathf.Sign(directionToTarget)) == Mathf.CeilToInt(Mathf.Sign(transform.right.x));
        LogCondition($"IsFacingTarget: {isFacingTarget}");
        return isFacingTarget;
    }

    private bool IsTargetMelee()
    {
        Assert.IsNotNull(Targetter.ActiveTarget, "Wrong BT desgn, IsTargetMelee should not be called if there is no target.");
        bool isTargetMelee = ObstacleCheck.ObstacleForward != null &&
            ObstacleCheck.ObstacleForward.TryGetComponent<AttackTargetComponent>(out var target) &&
            target == Targetter.ActiveTarget;
        LogCondition($"IsTargetMelee: {isTargetMelee}");
        return isTargetMelee;
    }
    #endregion

    #region ACTIONS
    private void Turn()
    {
        LogAction("Turn");
        Movement.Turn();
    }

    private void Stop()
    {
        LogAction("Stop");
        Movement.Stop();
    }
    #endregion

    #region TREES
    enum BehaviorTree
    {
        Patrol,
        ChaseTarget,
        TryAttackTarget,
        CrabClawBehavior,
    }

    private BT.INode m_PatrolBT => new BT.ReactiveSequencer(
        new BT.Succeeder(
            new BT.Sequencer(
                new BT.Selector(
                    new BT.Condition(IsFacingObstacle),
                    new BT.Condition(IsFacingEdge)
                ),
                new BT.InstantAction(Turn)
            )
        ),
        new BT.TaskAction(new MoveForwardTask(this))
    );

    private BT.INode m_ChaseTargetBT => new BT.ReactiveSequencer(
        new BT.Selector(
            new BT.Condition(IsFacingTarget),
            new BT.InstantAction(Turn)
        ),
        new BT.TaskAction(new MoveForwardTask(this))
    );

    private BT.INode m_TryAttackTargetBT => new BT.Sequencer(
        new BT.Condition(HasAliveTarget),
        new BT.Condition(IsTargetMelee),
        new BT.InstantAction(Stop),
        new BT.TaskAction(new WaitForSecondsTask(this, .25f)),
        new BT.TaskAction(new AttackTargetTask(this)),
        new BT.TaskAction(new WaitForSecondsTask(this, .25f))
    );

    private BT.INode m_CrabClawBT => new BT.Loop(
        new BT.ReactiveSelector(
            new BT.ReactiveSelector(
                m_TryAttackTargetBT,
                new BT.ReactiveSequencer(
                    new BT.Condition(HasAliveTarget),
                    m_ChaseTargetBT
                )
            ),
            m_PatrolBT
        ),
        BT.Loop.Type.UntilFailure
    );
    #endregion
}

public abstract class AAIBehaviorTask : BT.ITask
{
    public bool Running { get ; set; }
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
        m_Agent.LogAction($"Wait for {m_Time} seconds Started");
        m_Timer = m_Time;
    }
    public override BT.Output Run()
    {
        m_Timer -= Time.deltaTime;
        return m_Timer <= 0f ? BT.Output.Success : BT.Output.Running;
    }

    public override void End(BT.Output output)
    {
        m_Agent.LogAction($"Wait for {m_Time} seconds Ended");
    }
}

public class MoveForwardTask : AAIBehaviorTask
{
    public MoveForwardTask(AIAgentComponent agent) : base(agent) { }

    public override void Start()
    {
        m_Agent.LogAction("MoveForward Started");
        m_Agent.Animator.StartGroundedAnimation();
    }
    public override BT.Output Run()
    {
        m_Agent.LogAction("MoveForward Run");
        m_Agent.Movement.MoveForward();
        return BT.Output.Running;
    }

    public override void End(BT.Output output)
    {
        m_Agent.LogAction("MoveForward Ended");
    }
    public override void OnInterrupted()
    {
        m_Agent.LogAction("MoveForward Interrupted");
    }
}

public class AttackTargetTask : AAIBehaviorTask
{
    public AttackTargetTask(AIAgentComponent agent) : base(agent) { }

    public override void Start()
    {
        m_Agent.LogAction("Attack Started");
        m_Agent.Animator.StartAttackAnimation(true);
    }
    public override BT.Output Run()
    {
        m_Agent.LogAction("Attack Run");
        return m_Agent.Animator.AttackAnimationPhaseCompletedThisFrame is AttackAnimationPhase.Withdrawing
            ? BT.Output.Success
            : BT.Output.Running;
    }

    public override void End(BT.Output output)
    {
        m_Agent.LogAction("Attack Ended");
    }
    public override void OnInterrupted()
    {
        m_Agent.LogAction("Attack Interrupted");
    }
}