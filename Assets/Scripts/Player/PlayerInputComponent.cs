using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class PlayerInputComponent : MonoBehaviour
{
    public int Movement { get; private set; }
    public event Action<int> OnMovementChanged;
    public event Action OnJumpPressed;
    public event Action OnJumpReleased;
    public event Action OnDashPressed;
    public event Action OnAttackPressed;

    public InputBuffer JumpBuffer { get; private set; }
    public InputBuffer DashBuffer { get; private set; }
    public InputBuffer Attack1Buffer { get; private set; }
    public InputBuffer Attack2Buffer { get; private set; }

    [SerializeField] private PlayerStats m_Stats;
    [SerializeField] private InputActionAsset m_InputActionAsset;

    private InputAction m_MoveAction;
    private InputAction m_JumpAction;
    private InputAction m_DashAction;
    private InputAction m_AttackAction;
    private IEnumerable<InputBuffer> m_Buffers => new[] { JumpBuffer, DashBuffer, Attack1Buffer, Attack2Buffer };

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
    }

    private void Awake()
    {
        JumpBuffer = new(m_Stats.JumpBufferTime);
        DashBuffer = new(m_Stats.DashBufferTime);
        Attack1Buffer = new(m_Stats.Attack1BufferTime);
        Attack2Buffer = new(m_Stats.Attack2BufferTime);
    }

    private void Update()
    {
        foreach(var buffer in m_Buffers)
        {
            buffer.Tick();
        }
    }

    private void OnMoveAction(InputAction.CallbackContext context)
    {
        int newMovement = Mathf.CeilToInt(context.ReadValue<Vector2>().x);
        if (newMovement != Movement)
        {
            Movement = newMovement;
            OnMovementChanged?.Invoke(Movement);
        }
    }

    private void OnJumpAction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            JumpBuffer.Register();
            OnJumpPressed?.Invoke();
        }
        if (context.canceled)
        {
            OnJumpReleased?.Invoke();
        }
    }

    private void OnDashAction(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            DashBuffer.Register();
            OnDashPressed?.Invoke();
        }
    }

    private void OnAttackAction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Attack1Buffer.Register();
            Attack2Buffer.Register();
            OnAttackPressed?.Invoke();
        }
    }

    public class InputBuffer
    {
        private float m_Timer = 0f;
        private readonly float m_Time;
        public InputBuffer(float time) {  m_Time = time; }
        public void Register() => m_Timer = m_Time;
        public bool Check() => m_Timer > 0f;
        public void Clear() => m_Timer = 0f;
        public void Tick()
        {
            if (m_Timer > 0)
            {
                m_Timer -= Time.deltaTime;
            }
        }
    }
}