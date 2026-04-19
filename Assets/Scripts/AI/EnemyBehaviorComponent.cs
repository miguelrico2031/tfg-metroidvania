using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(AIMovementComponent), typeof(ObstacleCheckComponent), typeof(EdgeCheckComponent))]
[RequireComponent(typeof(AITargetterComponent), typeof(AnimatorComponent))]
public class EnemyBehaviorComponent : MonoBehaviour
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

public abstract class AEnemyTask : BT.ITask
{
    public bool Running { get ; set; }
    protected EnemyBehaviorComponent m_Enemy;
    public AEnemyTask(EnemyBehaviorComponent enemy) => m_Enemy = enemy;
    public virtual void Start() { }
    public abstract BT.Output Run();
    public virtual void End(BT.Output output) { }
    public virtual void OnInterrupted() { }
}

public class WaitForSecondsTask : AEnemyTask
{
    private readonly float m_Time;
    private float m_Timer = 0f;
    public WaitForSecondsTask(EnemyBehaviorComponent enemy, float time) : base(enemy) { m_Time = time; }
    public override void Start()
    {
        m_Enemy.LogAction($"Wait for {m_Time} seconds Started");
        m_Timer = m_Time;
    }
    public override BT.Output Run()
    {
        m_Timer -= Time.deltaTime;
        return m_Timer <= 0f ? BT.Output.Success : BT.Output.Running;
    }

    public override void End(BT.Output output)
    {
        m_Enemy.LogAction($"Wait for {m_Time} seconds Ended");
    }
}

public class MoveForwardTask : AEnemyTask
{
    public MoveForwardTask(EnemyBehaviorComponent enemy) : base(enemy) { }

    public override void Start()
    {
        m_Enemy.LogAction("MoveForward Started");
        m_Enemy.Animator.StartGroundedAnimation();
    }
    public override BT.Output Run()
    {
        m_Enemy.LogAction("MoveForward Run");
        m_Enemy.Movement.MoveForward();
        return BT.Output.Running;
    }

    public override void End(BT.Output output)
    {
        m_Enemy.LogAction("MoveForward Ended");
    }
    public override void OnInterrupted()
    {
        m_Enemy.LogAction("MoveForward Interrupted");
    }
}

public class AttackTargetTask : AEnemyTask
{
    public AttackTargetTask(EnemyBehaviorComponent enemy) : base(enemy) { }

    public override void Start()
    {
        m_Enemy.LogAction("Attack Started");
        m_Enemy.Animator.StartAttackAnimation(true);
    }
    public override BT.Output Run()
    {
        m_Enemy.LogAction("Attack Run");
        return m_Enemy.Animator.AttackAnimationPhaseCompletedThisFrame is AttackAnimationPhase.Withdrawing
            ? BT.Output.Success
            : BT.Output.Running;
    }

    public override void End(BT.Output output)
    {
        m_Enemy.LogAction("Attack Ended");
    }
    public override void OnInterrupted()
    {
        m_Enemy.LogAction("Attack Interrupted");
    }
}