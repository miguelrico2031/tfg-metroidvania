using BT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(AIMovementComponent), typeof(ObstacleCheckComponent), typeof(EdgeCheckComponent))]
[RequireComponent(typeof(EnemyTargetterComponent), typeof(AnimatorComponent))]
public class EnemyBehaviorComponent : MonoBehaviour
{
    public AIMovementComponent Movement { get; private set; }
    public ObstacleCheckComponent ObstacleCheck { get; private set; }
    public EdgeCheckComponent EdgeCheck { get; private set; }
    public EnemyTargetterComponent Targetter { get; private set; }
    public AnimatorComponent Animator { get; private set; }

    [SerializeField] private bool m_LogBehavior;

    private BT.BehaviorTree m_BehaviorTree;
    private BT.Output m_BehaviorTreeOutput = BT.Output.Running;


    private void Awake()
    {
        Movement = GetComponent<AIMovementComponent>();
        ObstacleCheck = GetComponent<ObstacleCheckComponent>();
        EdgeCheck = GetComponent<EdgeCheckComponent>();
        Targetter = GetComponent<EnemyTargetterComponent>();
        Animator = GetComponent<AnimatorComponent>();

        SetUpBehaviorTree();
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

    private void SetUpBehaviorTree()
    {
        BT.INode patrolBT = new BT.ReactiveSequencer(
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

        BT.INode chaseTargetBT = new BT.ReactiveSequencer(
            new BT.Selector(
                new BT.Condition(IsFacingTarget),
                new BT.InstantAction(Turn)
            ),
            new BT.TaskAction(new MoveForwardTask(this))
        );

        BT.INode rootBT = new BT.Loop(
            new BT.ReactiveSelector(
                new BT.ReactiveSequencer(
                    new BT.Condition(HasAliveTarget),
                    new BT.ReactiveSelector(
                        new BT.Sequencer(
                            new BT.Condition(IsTargetMelee),
                            new BT.TaskAction(new AttackTargetTask(this))
                        ),
                        chaseTargetBT
                    )
                ),
                patrolBT
            ),
            BT.Loop.Type.UntilFailure
        );

        m_BehaviorTree = new BT.BehaviorTree(rootBT);
    }

    #region ACTIONS
    private void Turn()
    {
        LogAction("Turn");
        Movement.Turn();
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

        private BT.Output m_Output;
        public override void Start()
        {
            m_Enemy.LogAction("Attack Started");
            m_Enemy.Animator.StartAttackAnimation(true);
            m_Output = BT.Output.Running;
        }
        public override BT.Output Run()
        {
            m_Enemy.LogAction("Attack Run");
            if(m_Enemy.Animator.AttackAnimationPhase is AttackAnimationPhase.JustWithdrawn)
            {
                m_Enemy.Animator.StartGroundedAnimation();
                m_Enemy.StartCoroutine(Finish());
            }
            return m_Output;
        }

        public override void End(BT.Output output)
        {
            m_Enemy.LogAction("Attack Ended");
        }
        public override void OnInterrupted()
        {
            m_Enemy.LogAction("Attack Interrupted");
        }

        private IEnumerator Finish()
        {
            yield return new WaitForSeconds(2f);
            m_Output = BT.Output.Success;
        }
    }

    #endregion

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
        LogCondition($"HasTarget: {hasAliveTarget}");
        return hasAliveTarget;
    }

    private bool IsFacingTarget()
    {
        Assert.IsNotNull(Targetter.ActiveTarget, "Wrong BT desgn, IsFacingTarget should not be called if there is no target.");
        float directionToTarget = Targetter.ActiveTarget.transform.position.x- transform.position.x;
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

    private void Log(string message)
    {
        if (m_LogBehavior)
        {
            Debug.Log($"[BT] [{gameObject.name}] {message}");
        }
    }
    public void LogAction(string message) => Log($"[ACTION] {message}");
    private void LogCondition(string message) => Log($"[CONDITION] {message}");
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
