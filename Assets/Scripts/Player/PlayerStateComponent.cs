using UnityEngine;
using System;

[RequireComponent(typeof(PlayerMovementComponent), typeof(PlayerInputComponent), typeof(GroundCheckComponent))]
[RequireComponent(typeof(ObstacleCheckComponent), typeof(AnimatorComponent), typeof(PlayerStaminaComponent))]
[RequireComponent(typeof(AttackTargetComponent), typeof(HealthComponent))]
public class PlayerStateComponent : MonoBehaviour
{
    public PlayerMovementComponent Movement { get; private set; }
    public PlayerInputComponent Input { get; private set; }
    public GroundCheckComponent GroundCheck { get; private set; }
    public ObstacleCheckComponent ObstacleCheck { get; private set; }
    public AnimatorComponent Animator { get; private set; }
    public PlayerStaminaComponent Stamina { get; private set; }
    public AttackComboComponent AttackCombo { get; private set; }
    public AttackTargetComponent AttackTarget { get; private set; }
    public HealthComponent Health { get; private set; }

    public Type CurrentState => m_StateMachine?.CurrentState;
    public event Action<Type> OnStateChanged;

    [SerializeField] private bool m_LogStateChanges;

    private string m_DebugStrStateMachine = "Current State: None";

    private FSM.StateMachine m_StateMachine;

    private void Awake()
    {
        Movement = GetComponent<PlayerMovementComponent>();
        Input = GetComponent<PlayerInputComponent>();
        GroundCheck = GetComponent<GroundCheckComponent>();
        ObstacleCheck = GetComponent<ObstacleCheckComponent>();
        Animator = GetComponent<AnimatorComponent>();
        Stamina = GetComponent<PlayerStaminaComponent>();
        AttackCombo = GetComponent<AttackComboComponent>();
        AttackTarget = GetComponent<AttackTargetComponent>();
        Health = GetComponent<HealthComponent>();

        m_StateMachine = SetUpStateMachine();

        m_StateMachine.OnStateChanged += HandleStateChanged;
    }
    private void Start()
    {
        m_StateMachine?.Start();
    }

    private void Update()
    {
        m_StateMachine?.Update();
    }

    private void FixedUpdate()
    {
        m_StateMachine?.FixedUpdate();
    }

    private void HandleStateChanged()
    {
        string stateName = CurrentState?.Name ?? "None";
        m_DebugStrStateMachine = $"Current State: {stateName}";
        if (m_LogStateChanges)
        {
            Debug.Log($"Player State Machine Current State changed to {stateName}");
        }

        OnStateChanged?.Invoke(CurrentState);
    }

    private FSM.StateMachine SetUpStateMachine()
    {
        return new FSM.StateMachineBuilder()

            .AddState(new PlayerGroundedState(this), isInitialState: true)
            .AddState(new PlayerJumpingState(this))
            .AddState(new PlayerFallingState(this))
            .AddState(new PlayerDashingState(this))
            .AddState(new PlayerKnockbackState(this))
            .AddState(new PlayerDyingState(this))
            .AddState(new PlayerAttackingState(this))

            .AddTransitionFromAnyState<PlayerDyingState>((state) =>
                Health.CurrentHealth == 0 &&
                !HasBeenHitWithKnockback() &&
                state != typeof(PlayerKnockbackState) &&
                state != typeof(PlayerDyingState))

            .AddTransition<PlayerGroundedState, PlayerKnockbackState>(HasBeenHitWithKnockback)
            .AddTransition<PlayerGroundedState, PlayerJumpingState>(IsJumpRequestedAndAllowed)
            .AddTransition<PlayerGroundedState, PlayerDashingState>(IsDashRequestedAndAllowed)
            .AddTransition<PlayerGroundedState, PlayerFallingState>(() => !GroundCheck.IsGrounded)
            .AddTransition<PlayerGroundedState, PlayerAttackingState>(IsAttack1RequestedAndAllowed)

            .AddTransition<PlayerJumpingState, PlayerFallingState>(() => Movement.VerticalVelocity <= 0f)
            .AddTransition<PlayerJumpingState, PlayerKnockbackState>(HasBeenHitWithKnockback)

            .AddTransition<PlayerFallingState, PlayerGroundedState>(() => GroundCheck.IsGrounded)
            .AddTransition<PlayerFallingState, PlayerJumpingState>(() => GroundCheck.CoyoteTimeBuffer.Check() && IsJumpRequestedAndAllowed())
            .AddTransition<PlayerFallingState, PlayerKnockbackState>(HasBeenHitWithKnockback)

            .AddTransition<PlayerDashingState, PlayerGroundedState>(() => GroundCheck.IsGrounded && IsDashFinished())
            .AddTransition<PlayerDashingState, PlayerFallingState>(() => !GroundCheck.IsGrounded && IsDashFinished())

            .AddTransition<PlayerKnockbackState, PlayerDyingState>(() => Health.CurrentHealth == 0 && IsKnockbackFinished())
            .AddTransition<PlayerKnockbackState, PlayerGroundedState>(() => GroundCheck.IsGrounded && IsKnockbackFinished())
            .AddTransition<PlayerKnockbackState, PlayerFallingState>(() => !GroundCheck.IsGrounded && IsKnockbackFinished())

            .AddTransition<PlayerAttackingState, PlayerDashingState>(IsDashRequestedAndAllowed)
            .AddTransition<PlayerAttackingState, PlayerAttackingState>(IsAttack2RequestedAndAllowed)
            .AddTransition<PlayerAttackingState, PlayerGroundedState>(HasAttackAnimationJustFinished)


            .Build();
    }

    private bool IsJumpRequestedAndAllowed() =>
        Input.JumpBuffer.Check() &&
        Stamina.CanPerformAction(StaminaAction.Jump);

    private bool IsDashRequestedAndAllowed()
    {
        if(!Input.DashBuffer.Check() || !Stamina.CanPerformAction(StaminaAction.Dash))
            return false;

        int forwardDirection = Mathf.CeilToInt(transform.right.x);
        int dashDirection = Input.Movement != 0 ? Input.Movement : forwardDirection;
        bool wantsToDashForward = dashDirection == forwardDirection;
        return wantsToDashForward ? !ObstacleCheck.IsObstructedForward : !ObstacleCheck.IsObstructedBehind;
    }

    private bool IsDashFinished() => 
        !Movement.IsDashing ||
        ObstacleCheck.IsObstructedForward;

    private bool HasBeenHitWithKnockback() => AttackTarget.ResolvedAttackThisFrame is
    {
        Result: AttackResult.Blocked or AttackResult.Hit,
        Attack: { Knockback: { Distance: > 0.01f, Duration: > 0.01f } }
    };

    private bool IsKnockbackFinished() =>
        !Movement.IsInKnockback ||
        ObstacleCheck.IsObstructedBehind;

    private bool IsAttack1RequestedAndAllowed() =>
        Input.AttackBuffer.Check() &&
        Stamina.CanPerformAction(StaminaAction.Attack);

    private bool IsAttack2RequestedAndAllowed() =>
        Input.AttackBuffer.Check() &&
        Animator.AttackAnimationPhase is AttackAnimationPhase.Withdrawing &&
        Stamina.CanPerformAction(StaminaAction.Attack) &&
        AttackCombo.ActiveAttack == 1;

    private bool HasAttackAnimationJustFinished() => Animator.AttackAnimationPhaseCompletedThisFrame is AttackAnimationPhase.Withdrawing;
}