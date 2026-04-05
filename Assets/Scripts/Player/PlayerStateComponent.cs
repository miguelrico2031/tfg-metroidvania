using UnityEngine;
using System;


public class PlayerStateComponent : MonoBehaviour
{
    public PlayerMovementComponent Movement {  get; private set; }
    public PlayerInputComponent Input {  get; private set; }
    public PlayerGroundCheckComponent GroundCheck {  get; private set; }
    public PlayerAnimatorComponent Animator { get; private set; }
    public PlayerStaminaComponent Stamina { get; private set; }
    public Type CurrentState => m_StateMachine?.CurrentState;

    [SerializeField] private bool m_LogStateChanges;

    private string m_DebugStrStateMachine = "Current State: None";

    private StateMachine m_StateMachine;

    private void Awake()
    {
        Movement = GetComponent<PlayerMovementComponent>();
        Input = GetComponent<PlayerInputComponent>();
        GroundCheck = GetComponent<PlayerGroundCheckComponent>();
        Animator = GetComponent<PlayerAnimatorComponent>();
        Stamina = GetComponent<PlayerStaminaComponent>();

        m_StateMachine = new StateMachineBuilder()
            .AddState(new PlayerGroundedState(this), isInitialState: true)
            .AddState(new PlayerJumpingState(this))
            .AddState(new PlayerFallingState(this))
            .AddTransition<PlayerGroundedState, PlayerJumpingState>(() => PlayerTransitions.TransitionGroundedToJumping(this))
            .AddTransition<PlayerJumpingState, PlayerFallingState>(() => PlayerTransitions.TransitionJumpingToFalling(this))
            .AddTransition<PlayerFallingState, PlayerGroundedState>(() => PlayerTransitions.TransitionFallingToGrounded(this))
            .AddTransition<PlayerGroundedState, PlayerFallingState>(() => PlayerTransitions.TransitionGroundedToFalling(this))
            .AddTransition<PlayerFallingState, PlayerJumpingState>(() => PlayerTransitions.TransitionFallingToJumping(this))
            .Build();
        m_StateMachine.OnStateChanged += OnStateChanged;
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

    private void OnStateChanged()
    {
        string stateName = CurrentState?.Name ?? "None";
        m_DebugStrStateMachine = $"Current State: {stateName}";
        if (m_LogStateChanges)
        {
        Debug.Log($"Player State Machine Current State changed to {stateName}");
        }
    }
}