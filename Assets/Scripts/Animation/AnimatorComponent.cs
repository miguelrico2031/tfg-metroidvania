using UnityEngine;
using UnityEngine.Assertions;

public enum AttackAnimationPhase
{
    None,
    Attacking,
    JustCompleted,
    Withdrawing,
    JustWithdrawn
}

[RequireComponent(typeof(Rigidbody2D))]
public class AnimatorComponent : MonoBehaviour
{
    public AttackAnimationPhase AttackAnimationPhase { get; private set; } = AttackAnimationPhase.None;

    [SerializeField] private Animator m_Animator;
    [SerializeField] private AnimationEvents m_AnimationEvents;

    private static readonly int s_MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int s_Grounded = Animator.StringToHash("Grounded");
    private static readonly int s_Jump = Animator.StringToHash("Jump");
    private static readonly int s_Fall = Animator.StringToHash("Fall");
    private static readonly int s_FallFromJump = Animator.StringToHash("FallFromJump");
    private static readonly int s_Dash = Animator.StringToHash("Dash");
    private static readonly int s_Knockback = Animator.StringToHash("Knockback");
    private static readonly int s_KnockbackAirborne = Animator.StringToHash("KnockbackAirborne");
    private static readonly int s_Death = Animator.StringToHash("Death");
    private static readonly int s_Attack = Animator.StringToHash("Attack");
    private static readonly int s_AttackContinue = Animator.StringToHash("AttackContinue");

    private bool m_IsGrounded;
    private int m_TriggerThisFrame = -1;
    private Rigidbody2D m_Rigidbody;

    public void StartGroundedAnimation()
    {
        Assert.IsTrue(m_TriggerThisFrame == -1, "Already set animation trigger this frame, cannot set it again.");
        m_IsGrounded = true;
        m_TriggerThisFrame = s_Grounded;
    }

    public void StartJumpAnimation()
    {
        Assert.IsTrue(m_TriggerThisFrame == -1, "Already set animation trigger this frame, cannot set it again.");
        m_IsGrounded = false;
        m_TriggerThisFrame = s_Jump;
    }

    public void StartFallAnimation(bool transitionFromJump)
    {
        Assert.IsTrue(m_TriggerThisFrame == -1, "Already set animation trigger this frame, cannot set it again.");
        m_IsGrounded = false;
        m_TriggerThisFrame = transitionFromJump ? s_FallFromJump : s_Fall;
    }

    public void StartDashAnimation()
    {
        Assert.IsTrue(m_TriggerThisFrame == -1, "Already set animation trigger this frame, cannot set it again.");
        m_IsGrounded = true;
        m_TriggerThisFrame = s_Dash;
    }

    public void StartKnockbackAnimation(bool isAirborne)
    {
        Assert.IsTrue(m_TriggerThisFrame == -1, "Already set animation trigger this frame, cannot set it again.");
        m_TriggerThisFrame = isAirborne ? s_KnockbackAirborne : s_Knockback;
    }

    public void StartDeathAnimation()
    {
        Assert.IsTrue(m_TriggerThisFrame == -1, "Already set animation trigger this frame, cannot set it again.");
        m_TriggerThisFrame = s_Death;
    }
    public void StartAttackAnimation(bool isFirstAttack)
    {
        Assert.IsTrue(m_TriggerThisFrame == -1, "Already set animation trigger this frame, cannot set it again.");
        m_TriggerThisFrame = isFirstAttack ? s_Attack : s_AttackContinue;
        AttackAnimationPhase = AttackAnimationPhase.Attacking;
    }

    private void OnEnable()
    {
        if (m_AnimationEvents != null)
        {
            m_AnimationEvents.OnPlayerAttack1Completed += OnAttackCompleted;
            m_AnimationEvents.OnPlayerAttack1Withdrawn += OnAttackWithdrawn;
            m_AnimationEvents.OnPlayerAttack2Completed += OnAttackCompleted;
            m_AnimationEvents.OnPlayerAttack2Withdrawn += OnAttackWithdrawn;
        }
    }

    private void OnDisable()
    {
        if (m_AnimationEvents != null)
        {
            m_AnimationEvents.OnPlayerAttack1Completed -= OnAttackCompleted;
            m_AnimationEvents.OnPlayerAttack1Withdrawn -= OnAttackWithdrawn;
            m_AnimationEvents.OnPlayerAttack2Completed -= OnAttackCompleted;
            m_AnimationEvents.OnPlayerAttack2Withdrawn -= OnAttackWithdrawn;
        }
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

        //This is not in late update because this script execution order is set to +10, so it will run at the end of the frame anyway

        AttackAnimationPhase = AttackAnimationPhase is AttackAnimationPhase.JustCompleted
            ? AttackAnimationPhase.Withdrawing
            : AttackAnimationPhase is AttackAnimationPhase.JustWithdrawn
            ? AttackAnimationPhase.None
            : AttackAnimationPhase;
    }

    private void LateUpdate()
    {
        if (m_TriggerThisFrame != -1)
        {
            m_Animator.SetTrigger(m_TriggerThisFrame);
            m_TriggerThisFrame = -1;
        }
    }

    private void OnAttackCompleted()
    {
        AttackAnimationPhase = AttackAnimationPhase.JustCompleted;
    }
    private void OnAttackWithdrawn()
    {
        AttackAnimationPhase = AttackAnimationPhase.JustWithdrawn;
    }
}