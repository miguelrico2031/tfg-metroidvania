using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputComponent : MonoBehaviour
{
    public int Movement { get; private set; }
    public event Action<int> OnMovementChanged;
    public event Action OnJumpPressed;
    public event Action OnJumpReleased;
    public event Action OnDashPressed;

    [SerializeField] private PlayerStats m_Stats;
    [SerializeField] private InputActionAsset m_InputActionAsset;

    private float m_JumpBufferTimer;
    private float m_DashBufferTimer;
    private InputAction m_MoveAction;
    private InputAction m_JumpAction;
    private InputAction m_DashAction;

    public bool CheckJumpBuffer() => m_JumpBufferTimer > 0f;
    public void ClearJumpBuffer() => m_JumpBufferTimer = 0f;

    public bool CheckDashBuffer() => m_DashBufferTimer > 0f;
    public void ClearDashBuffer() => m_DashBufferTimer = 0f;

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
    }

    private void Update()
    {
        if (m_JumpBufferTimer > 0f)
        {
            m_JumpBufferTimer -= Time.deltaTime;
        }
        if (m_DashBufferTimer > 0f)
        {
            m_DashBufferTimer -= Time.deltaTime;
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
            m_JumpBufferTimer = m_Stats.JumpBufferTime; //Register jump in buffer
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
            m_DashBufferTimer = m_Stats.DashBufferTime;
            OnDashPressed?.Invoke();
        }
    }
}