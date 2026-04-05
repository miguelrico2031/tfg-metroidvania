using UnityEngine;

public class PlayerDirectionComponent : MonoBehaviour
{
    public bool Right { get; private set; } = true;
    public int Direction => Right ? 1 : -1;

    [SerializeField] private SpriteRenderer m_SpriteRenderer;

    private PlayerInputComponent m_Input;

    private void OnEnable()
    {
        m_Input = GetComponent<PlayerInputComponent>();
        m_Input.OnMovementChanged += OnMovementInputChanged;
    }

    private void OnDisable()
    {
        if(m_Input != null)
        {
            m_Input.OnMovementChanged -= OnMovementInputChanged;
        }
    }

    private void OnMovementInputChanged(int movementInput)
    {
        if (movementInput == 0 || movementInput == Direction)
            return;

        Right = movementInput > 0;
        m_SpriteRenderer.flipX = !Right;
    }
}
