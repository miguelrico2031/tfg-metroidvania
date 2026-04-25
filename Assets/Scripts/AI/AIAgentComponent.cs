using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(AnimatorComponent), typeof(ObstacleCheckComponent), typeof(EdgeCheckComponent))]
[RequireComponent(typeof(AITargetterComponent), typeof(AIMovementComponent), typeof(HealthComponent))]
[RequireComponent(typeof(AttackTargetComponent))]
public class AIAgentComponent : MonoBehaviour
{
    [field: SerializeField] public EnemyStats Stats { get; private set; }

    public AIMovementComponent Movement { get; private set; }
    public ObstacleCheckComponent ObstacleCheck { get; private set; }
    public EdgeCheckComponent EdgeCheck { get; private set; }
    public AITargetterComponent Targetter { get; private set; }
    public AnimatorComponent Animator { get; private set; }
    public HealthComponent Health { get; private set; }
    public AttackTargetComponent AttackTarget { get; private set; }

    [SerializeField] private BehaviorTree m_SelectedBehaviorTree;
    [SerializeField] private bool m_LogBehavior;

    private Vector3 m_PostPosition;
    private FSM.StateMachine m_StateMachine;

    private void Awake()
    {
        Movement = GetComponent<AIMovementComponent>();
        ObstacleCheck = GetComponent<ObstacleCheckComponent>();
        EdgeCheck = GetComponent<EdgeCheckComponent>();
        Targetter = GetComponent<AITargetterComponent>();
        Animator = GetComponent<AnimatorComponent>();
        Health = GetComponent<HealthComponent>();
        AttackTarget = GetComponent<AttackTargetComponent>();

        m_StateMachine = SetUpStateMachine();
        m_PostPosition = transform.position;
    }

    private void Start()
    {
        m_StateMachine.Start();
    }

    private void Update()
    {
        m_StateMachine.Update();
    }

    private void FixedUpdate()
    {
        m_StateMachine.FixedUpdate();
    }

    private FSM.StateMachine SetUpStateMachine()
    {
        return new FSM.StateMachineBuilder()

            .AddState(new AIBehaviorTreeState(this, SetUpSelectedBehaviorTree()), isInitialState: true)
            .AddState(new AIDyingState(this))

            .AddTransition<AIBehaviorTreeState, AIDyingState>(() => Health.CurrentHealth == 0)

            .Build();
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

    public void BTLog(string message)
    {
        if (m_LogBehavior)
        {
            Debug.Log($"[BT] [{gameObject.name}] {message}");
        }
    }
    public void BTLogAction(string message) => BTLog($"[ACTION] {message}");
    public void BTLogCondition(string message) => BTLog($"[CONDITION] {message}");

    #region CONDITIONS
    private bool IsFacingObstacle()
    {
        BTLogCondition($"IsFacingObstacle: {ObstacleCheck.IsObstructedForward}");
        return ObstacleCheck.IsObstructedForward;
    }

    private bool IsFacingEdge()
    {
        BTLogCondition($"IsFacingEdge: {EdgeCheck.HasEdgeForward}");
        return EdgeCheck.HasEdgeForward;
    }

    private bool HasAliveTarget()
    {
        bool hasAliveTarget = Targetter.ActiveTarget is { IsAlive: true };
        BTLogCondition($"HasAliveTarget: {hasAliveTarget}");
        return hasAliveTarget;
    }

    private bool IsFacingTarget()
    {
        Assert.IsNotNull(Targetter.ActiveTarget, "Wrong BT desgn, IsFacingTarget should not be called if there is no target.");
        float directionToTarget = Targetter.ActiveTarget.transform.position.x - transform.position.x;
        bool isFacingTarget = Mathf.Abs(directionToTarget) < 0.01f ||
            Mathf.CeilToInt(Mathf.Sign(directionToTarget)) == Mathf.CeilToInt(Mathf.Sign(transform.right.x));
        BTLogCondition($"IsFacingTarget: {isFacingTarget}");
        return isFacingTarget;
    }

    private bool IsTargetMelee()
    {
        Assert.IsNotNull(Targetter.ActiveTarget, "Wrong BT desgn, IsTargetMelee should not be called if there is no target.");
        bool isTargetMelee = ObstacleCheck.ObstacleForward != null &&
            ObstacleCheck.ObstacleForward.TryGetComponent<AttackTargetComponent>(out var target) &&
            target == Targetter.ActiveTarget;
        BTLogCondition($"IsTargetMelee: {isTargetMelee}");
        return isTargetMelee;
    }

    private bool IsFacingPost()
    {
        float directionToPost = m_PostPosition.x - transform.position.x;
        bool isFacingPost = Mathf.Abs(directionToPost) < 0.1f ||
            Mathf.CeilToInt(Mathf.Sign(directionToPost)) == Mathf.CeilToInt(Mathf.Sign(transform.right.x));
        BTLogCondition($"IsFacingPost: {isFacingPost}");
        return isFacingPost;
    }

    private bool IsInPost()
    {
        bool isInPost = Vector2.Distance(transform.position, m_PostPosition) < 0.1f;
        BTLogCondition($"IsInPost: {isInPost}");
        return isInPost;
    }
    #endregion

    #region ACTIONS
    private void Turn()
    {
        BTLogAction("Turn");
        Movement.Turn();
    }

    private void Stop()
    {
        BTLogAction("Stop");
        Movement.Stop();
    }

    public void DisableAllHitboxes()
    {
        BTLogAction("DisableAllHitboxes");
        foreach (var hitbox in GetComponentsInChildren<Hitbox>(true))
        {
            hitbox.enabled = false;
        }
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

    private BT.INode m_SeekPostBT => new BT.Sequencer(
        new BT.Succeeder(new BT.TaskAction(new IdleTask(this))),
        new BT.TaskAction(new WaitForSecondsTask(this, Stats.DelayBeforeSeekingPost)),
        new BT.Selector(
            new BT.Condition(IsFacingPost),
            new BT.InstantAction(Turn)
        ),
        new BT.ReactiveSelector(
            new BT.Condition(IsInPost),
            new BT.TaskAction(new MoveForwardTask(this))
        ),
        new BT.TaskAction(new IdleTask(this))
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
        new BT.TaskAction(new WaitForSecondsTask(this, Stats.DelayBeforeAttacking)),
        new BT.TaskAction(new AttackTargetTask(this)),
        new BT.TaskAction(new WaitForSecondsTask(this, Stats.DelayAfterAttacking))
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
            m_SeekPostBT
        ),
        BT.Loop.Type.UntilFailure
    );
    #endregion
}