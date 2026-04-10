using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]

public class PlayerAnimatorComponent : MonoBehaviour
{
    [SerializeField] private Animator m_Animator;

    private static readonly int s_MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int s_Grounded = Animator.StringToHash("Grounded");
    private static readonly int s_Jump = Animator.StringToHash("Jump");
    private static readonly int s_Fall = Animator.StringToHash("Fall");
    private static readonly int s_Dash = Animator.StringToHash("Dash");
    private static readonly int s_Knockback = Animator.StringToHash("Knockback");

    private bool m_IsGrounded;
    private int m_TriggerThisFrame = -1;
    private Rigidbody2D m_Rigidbody;

    public void StartGroundedAnimation()
    {
        m_IsGrounded = true;
        m_TriggerThisFrame = s_Grounded;
    }

    public void StartJumpAnimation()
    {
        m_IsGrounded = false;
        m_TriggerThisFrame = s_Jump;
    }

    public void StartFallAnimation()
    {
        m_IsGrounded = false;
        m_TriggerThisFrame = s_Fall;
    }

    public void StartDashAnimation()
    {
        m_IsGrounded = true;
        m_TriggerThisFrame = s_Dash;
    }

    public void StartKnockbackAnimation()
    {
        m_TriggerThisFrame = s_Knockback;
    }

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (m_IsGrounded)
        {
            float speed = Mathf.Abs(m_Rigidbody.linearVelocityX);
            m_Animator.SetFloat(s_MoveSpeed, speed);
        }
    }

    private void LateUpdate()
    {
        if (m_TriggerThisFrame != -1)
        {
            m_Animator.SetTrigger(m_TriggerThisFrame);
            m_TriggerThisFrame = -1;
        }
    }
}