using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class PlayerInputComponent : MonoBehaviour
{
    public int Movement { get; private set; }
    public bool PressingBlock { get; private set; }
    public BufferingTimer JumpBuffer { get; private set; }
    public BufferingTimer ReleaseJumpBuffer { get; private set; }
    public BufferingTimer DashBuffer { get; private set; }
    public BufferingTimer AttackBuffer { get; private set; }

    [SerializeField] private PlayerStats m_Stats;
    [SerializeField] private InputActionAsset m_InputActionAsset;

    private InputAction m_MoveAction;
    private InputAction m_JumpAction;
    private InputAction m_DashAction;
    private InputAction m_AttackAction;
    private InputAction m_BlockAction;
    private IEnumerable<BufferingTimer> m_Buffers => 
        new[] { JumpBuffer, ReleaseJumpBuffer, DashBuffer, AttackBuffer };

    private const float c_BufferAliveForAFewFramesTime = 0.05f;

    private void OnEnable()
    {
        m_MoveAction ??= m_InputActionAsset.FindAction("Player/Move", throwIfNotFound: true);
        m_MoveAction.canceled += OnMoveAction;
        m_MoveAction.performed += OnMoveAction;
        m_MoveAction.started += OnMoveAction;

        m_JumpAction ??= m_InputActionAsset.FindAction("Player/Jump", throwIfNotFound: true);
        m_JumpAction.canceled += OnJumpAction;
        m_JumpAction.started += OnJumpAction;

        m_DashAction ??= m_InputActionAsset.FindAction("Player/Dash", throwIfNotFound: true);
        m_DashAction.started += OnDashAction;

        m_AttackAction ??= m_InputActionAsset.FindAction("Player/Attack", throwIfNotFound: true);
        m_AttackAction.started += OnAttackAction;

        m_BlockAction ??= m_InputActionAsset.FindAction("Player/Block", throwIfNotFound: true);
        m_BlockAction.canceled += OnBlockAction;
        m_BlockAction.started += OnBlockAction;
    }

    private void OnDisable()
    {
        if (m_MoveAction is not null)
        {
            m_MoveAction.canceled -= OnMoveAction;
            m_MoveAction.performed -= OnMoveAction;
            m_MoveAction.started -= OnMoveAction;
        }
        if (m_JumpAction is not null)
        {
            m_JumpAction.canceled -= OnJumpAction;
            m_JumpAction.started -= OnJumpAction;
        }
        if (m_DashAction is not null)
        {
            m_DashAction.started -= OnDashAction;
        }
        if(m_AttackAction is not null)
        {
            m_AttackAction.started -= OnAttackAction;
        }
        if (m_BlockAction is not null)
        {
            m_BlockAction.canceled -= OnBlockAction;
            m_BlockAction.started -= OnBlockAction;
        }
    }

    private void Awake()
    {
        JumpBuffer = new(() => m_Stats.JumpBufferTime);
        ReleaseJumpBuffer = new(() => c_BufferAliveForAFewFramesTime);
        DashBuffer = new(() => m_Stats.DashBufferTime);
        AttackBuffer = new(() => m_Stats.AttackBufferTime);
    }

    private void Update()
    {
        foreach(var buffer in m_Buffers)
        {
            buffer.Tick(Time.deltaTime);
        }
    }

    private void OnMoveAction(InputAction.CallbackContext context)
    {
        Movement = Mathf.CeilToInt(context.ReadValue<Vector2>().x);
    }

    private void OnJumpAction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            JumpBuffer.Register();
        }
        if (context.canceled)
        {
            ReleaseJumpBuffer.Register();
        }
    }

    private void OnDashAction(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            DashBuffer.Register();
        }
    }

    private void OnAttackAction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            AttackBuffer.Register();
        }
    }

    private void OnBlockAction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            PressingBlock = true;
        }
        if (context.canceled)
        {
            PressingBlock = false;
        }
    }
}