using UnityEngine;

public class PlayerAnimatorComponent : MonoBehaviour
{
    [SerializeField] private Animator m_Animator;
    [SerializeField] private SpriteRenderer m_SpriteRenderer;

    private static readonly int s_Jump = Animator.StringToHash("Jump");
    private static readonly int s_Fall = Animator.StringToHash("Fall");
    private static readonly int s_Grounded = Animator.StringToHash("Grounded");
    private static readonly int s_MoveSpeed = Animator.StringToHash("MoveSpeed");

    private bool m_IsGrounded;
    private PlayerInputComponent m_Input;
    private Rigidbody2D m_Rigidbody;

    private void Awake()
    {
        m_Input = GetComponent<PlayerInputComponent>();
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    public void StartGroundedAnimation()
    {
        m_IsGrounded = true;
        m_Animator.SetTrigger(s_Grounded);
    }

    public void StartJumpAnimation()
    {
        m_IsGrounded = false;
        m_Animator.SetTrigger(s_Jump);
    }

    public void StartFallAnimation()
    {
        m_IsGrounded = false;
        m_Animator.SetTrigger(s_Fall);
    }

    private void Update()
    {
        if (m_IsGrounded)
        {
            float speed = Mathf.Abs(m_Rigidbody.linearVelocityX);
            m_Animator.SetFloat(s_MoveSpeed, speed);
        }
        if (m_Input.Movement != 0 && (m_Input.Movement < 0) != m_SpriteRenderer.flipX)
        {
            m_SpriteRenderer.flipX = !m_SpriteRenderer.flipX;
        }
    }
}