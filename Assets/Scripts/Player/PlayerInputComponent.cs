using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputComponent : MonoBehaviour
{
    public int Movement { get; private set; }
    public event Action<int> OnMovementChanged;
    public event Action OnJumpPressed;
    public event Action OnJumpReleased;

    [SerializeField] private float m_JumpBufferTime;
    [SerializeField] private InputActionAsset m_InputActionAsset;

    private float m_JumpBufferTimer;
    private InputAction m_MoveAction;
    private InputAction m_JumpAction;

    public bool CheckJumpBuffer() => m_JumpBufferTimer > 0f;
    public void ClearJumpBuffer() => m_JumpBufferTimer = 0f;

    private void OnEnable()
    {
        m_MoveAction ??= m_InputActionAsset.FindAction("Player/Move", throwIfNotFound: true);
        m_MoveAction.canceled += OnMoveAction;
        m_MoveAction.performed += OnMoveAction;
        m_MoveAction.started += OnMoveAction;

        m_JumpAction ??= m_InputActionAsset.FindAction("Player/Jump", throwIfNotFound: true);
        m_JumpAction.canceled += OnJumpAction;
        m_JumpAction.started += OnJumpAction;
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
    }

    private void Update()
    {
        if(m_JumpBufferTimer > 0f)
        {
            m_JumpBufferTimer -= Time.deltaTime;
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
            m_JumpBufferTimer = m_JumpBufferTime; //Register jump in buffer
            OnJumpPressed?.Invoke();
        }
        if (context.canceled)
        {
            OnJumpReleased?.Invoke();
        }
    }
}