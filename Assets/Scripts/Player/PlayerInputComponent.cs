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
    public BufferingTimer InteractBuffer { get; private set; }
    public BufferingTimer HealBuffer { get; private set; }

    [SerializeField] private PlayerStats m_Stats;
    [SerializeField] private InputActionAsset m_InputActionAsset;

    [SerializeField] private InputActionReference m_MoveAction;
    [SerializeField] private InputActionReference m_JumpAction;
    [SerializeField] private InputActionReference m_DashAction;
    [SerializeField] private InputActionReference m_AttackAction;
    [SerializeField] private InputActionReference m_BlockAction;
    [SerializeField] private InputActionReference m_InteractAction;
    [SerializeField] private InputActionReference m_HealAction;
    private IEnumerable<BufferingTimer> m_Buffers =>
        new[] { JumpBuffer, ReleaseJumpBuffer, DashBuffer, AttackBuffer, InteractBuffer, HealBuffer };

    //This allows using the input buffer approach for instant actions without relying on update execution order
    private const float c_BufferAliveForAFewFramesTime = 0.05f;

    private void OnEnable()
    {
        m_MoveAction.action.canceled += OnMoveAction;
        m_MoveAction.action.performed += OnMoveAction;
        m_MoveAction.action.started += OnMoveAction;

        m_JumpAction.action.canceled += OnJumpAction;
        m_JumpAction.action.started += OnJumpAction;

        m_DashAction.action.started += OnDashAction;

        m_AttackAction.action.started += OnAttackAction;

        m_BlockAction.action.canceled += OnBlockAction;
        m_BlockAction.action.started += OnBlockAction;

        m_InteractAction.action.started += OnInteractAction;

        m_HealAction.action.started += OnHealAction;
    }

    private void OnDisable()
    {
        m_MoveAction.action.canceled -= OnMoveAction;
        m_MoveAction.action.performed -= OnMoveAction;
        m_MoveAction.action.started -= OnMoveAction;

        m_JumpAction.action.canceled -= OnJumpAction;
        m_JumpAction.action.started -= OnJumpAction;

        m_DashAction.action.started -= OnDashAction;

        m_AttackAction.action.started -= OnAttackAction;

        m_BlockAction.action.canceled -= OnBlockAction;
        m_BlockAction.action.started -= OnBlockAction;

        m_InteractAction.action.started -= OnInteractAction;

        m_HealAction.action.started -= OnHealAction;
    }

    private void Awake()
    {
        JumpBuffer = new(() => m_Stats.JumpBufferTime);
        ReleaseJumpBuffer = new(() => c_BufferAliveForAFewFramesTime);
        DashBuffer = new(() => m_Stats.DashBufferTime);
        AttackBuffer = new(() => m_Stats.AttackBufferTime);
        InteractBuffer = new(() => c_BufferAliveForAFewFramesTime);
        HealBuffer = new(() => c_BufferAliveForAFewFramesTime);
    }

    private void Update()
    {
        foreach (var buffer in m_Buffers)
        {
            buffer.Update(Time.deltaTime);
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
        if (context.started)
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

    private void OnInteractAction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InteractBuffer.Register();
        }
    }

    private void OnHealAction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            HealBuffer.Register();
        }
    }
}