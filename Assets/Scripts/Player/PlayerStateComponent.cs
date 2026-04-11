using UnityEngine;
using System;

[RequireComponent(typeof(PlayerMovementComponent), typeof(PlayerInputComponent), typeof(PlayerGroundCheckComponent))]
[RequireComponent(typeof(PlayerObstacleCheckComponent), typeof(PlayerAnimatorComponent), typeof(PlayerStaminaComponent))]
[RequireComponent(typeof(AttackTargetComponent), typeof(HealthComponent))]
public class PlayerStateComponent : MonoBehaviour
{
    public PlayerMovementComponent Movement { get; private set; }
    public PlayerInputComponent Input { get; private set; }
    public PlayerGroundCheckComponent GroundCheck { get; private set; }
    public PlayerObstacleCheckComponent ObstacleCheck { get; private set; }
    public PlayerAnimatorComponent Animator { get; private set; }
    public PlayerStaminaComponent Stamina { get; private set; }
    public AttackTargetComponent AttackTarget { get; private set; }
    public HealthComponent Health { get; private set; }

    public Type CurrentState => m_StateMachine?.CurrentState;
    public event Action<Type> OnStateChanged;

    [SerializeField] private bool m_LogStateChanges;

    private string m_DebugStrStateMachine = "Current State: None";

    private StateMachine m_StateMachine;

    private void Awake()
    {
        Movement = GetComponent<PlayerMovementComponent>();
        Input = GetComponent<PlayerInputComponent>();
        GroundCheck = GetComponent<PlayerGroundCheckComponent>();
        ObstacleCheck = GetComponent<PlayerObstacleCheckComponent>();
        Animator = GetComponent<PlayerAnimatorComponent>();
        Stamina = GetComponent<PlayerStaminaComponent>();
        AttackTarget = GetComponent<AttackTargetComponent>();
        Health = GetComponent<HealthComponent>();

        m_StateMachine = new StateMachineBuilder()

            .AddState(new PlayerGroundedState(this), isInitialState: true)
            .AddState(new PlayerJumpingState(this))
            .AddState(new PlayerFallingState(this))
            .AddState(new PlayerDashingState(this))
            .AddState(new PlayerKnockbackState(this))
            .AddState(new PlayerDyingState(this))

            .AddTransitionFromAnyState<PlayerDyingState>((state) =>
                Health.CurrentHealth == 0 &&
                !HasBeenHitWithKnockback() &&
                state != typeof(PlayerKnockbackState))

            .AddTransition<PlayerGroundedState, PlayerJumpingState>(() => IsJumpRequestedAndAllowed())
            .AddTransition<PlayerGroundedState, PlayerFallingState>(() => !GroundCheck.IsGrounded)
            .AddTransition<PlayerGroundedState, PlayerDashingState>(() => IsDashRequestedAndAllowed())
            .AddTransition<PlayerGroundedState, PlayerKnockbackState>(() => HasBeenHitWithKnockback())

            .AddTransition<PlayerJumpingState, PlayerFallingState>(() => Movement.VerticalVelocity <= 0f)
            .AddTransition<PlayerJumpingState, PlayerKnockbackState>(() => HasBeenHitWithKnockback())

            .AddTransition<PlayerFallingState, PlayerGroundedState>(() => GroundCheck.IsGrounded)
            .AddTransition<PlayerFallingState, PlayerJumpingState>(() => GroundCheck.CheckCoyoteTime() && IsJumpRequestedAndAllowed())
            .AddTransition<PlayerFallingState, PlayerKnockbackState>(() => HasBeenHitWithKnockback())

            .AddTransition<PlayerDashingState, PlayerGroundedState>(() => GroundCheck.IsGrounded && IsDashFinished())
            .AddTransition<PlayerDashingState, PlayerFallingState>(() => !GroundCheck.IsGrounded && IsDashFinished())

            .AddTransition<PlayerKnockbackState, PlayerDyingState>(() => Health.CurrentHealth == 0 && IsKnockbackFinished())
            .AddTransition<PlayerKnockbackState, PlayerGroundedState>(() => GroundCheck.IsGrounded && IsKnockbackFinished())
            .AddTransition<PlayerKnockbackState, PlayerFallingState>(() => !GroundCheck.IsGrounded && IsKnockbackFinished())
            
            .Build();

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

    private bool IsJumpRequestedAndAllowed() =>
        Input.CheckJumpBuffer() &&
        Stamina.CanPerformAction(StaminaAction.Jump);

    private bool IsDashRequestedAndAllowed() =>
        Input.CheckDashBuffer() &&
        Stamina.CanPerformAction(StaminaAction.Dash) &&
        !ObstacleCheck.IsObstructed;

    private bool IsDashFinished() => 
        !Movement.IsDashing ||
        ObstacleCheck.IsObstructed;

    private bool HasBeenHitWithKnockback() => AttackTarget.ResolvedAttackThisFrame is
    {
        Result: AttackResult.Blocked or AttackResult.Hit,
        Attack:
        {
            Knockback:
            {
                Distance: > 0.01f,
                Duration: > 0.01f
            }
        }
    };

    private bool IsKnockbackFinished() =>
        !Movement.IsInKnockback ||
        ObstacleCheck.IsObstructed;
}