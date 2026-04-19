using UnityEngine;
using UnityEngine.Assertions;

public enum AttackAnimationPhase
{
    NotAttacking,
    Drawing,
    Striking,
    Withdrawing
}

[RequireComponent(typeof(Rigidbody2D))]
public class AnimatorComponent : MonoBehaviour
{
    public AttackAnimationPhase AttackAnimationPhase { get; private set; } = AttackAnimationPhase.NotAttacking;
    public AttackAnimationPhase? AttackAnimationPhaseCompletedThisFrame { get; private set; } = null;
    public bool IsStandingFinished { get; private set; } = true;

    [SerializeField] private Animator m_Animator;
    [SerializeField] private AnimationEvents m_AnimationEvents;

    private static readonly int s_MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int s_Grounded = Animator.StringToHash("Grounded");
    private static readonly int s_Jump = Animator.StringToHash("Jump");
    private static readonly int s_Fall = Animator.StringToHash("Fall");
    private static readonly int s_FallFromJump = Animator.StringToHash("FallFromJump");
    private static readonly int s_Dash = Animator.StringToHash("Dash");
    private static readonly int s_Stand = Animator.StringToHash("Stand");
    private static readonly int s_StandFromKnockback = Animator.StringToHash("StandFromKnockback");
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

    public void StartStandAnimation(bool transitionFromKnockback)
    {
        Assert.IsTrue(m_TriggerThisFrame == -1, "Already set animation trigger this frame, cannot set it again.");
        m_IsGrounded = true;
        m_TriggerThisFrame = transitionFromKnockback ? s_StandFromKnockback : s_Stand;
        IsStandingFinished = false;
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
        AttackAnimationPhase = AttackAnimationPhase.Drawing;
    }

    private void OnEnable()
    {
        if (m_AnimationEvents != null)
        {
            m_AnimationEvents.OnAttackDrawCompleted += OnAttackDrawCompleted;
            m_AnimationEvents.OnAttackStrikeCompleted += OnAttackStrikeCompleted;
            m_AnimationEvents.OnAttackWithdrawCompleted += OnAttackWithdrawCompleted;
            m_AnimationEvents.OnStandCompleted += OnStandCompleted;
            m_AnimationEvents.OnStandCompleted += OnStandCompleted;
        }
    }

    private void OnDisable()
    {
        if (m_AnimationEvents != null)
        {
            m_AnimationEvents.OnAttackDrawCompleted -= OnAttackDrawCompleted;
            m_AnimationEvents.OnAttackStrikeCompleted -= OnAttackStrikeCompleted;
            m_AnimationEvents.OnAttackWithdrawCompleted -= OnAttackWithdrawCompleted;
            m_AnimationEvents.OnStandCompleted -= OnStandCompleted;
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

        AttackAnimationPhaseCompletedThisFrame = null;
    }

    private void LateUpdate()
    {
        if (m_TriggerThisFrame != -1)
        {
            m_Animator.SetTrigger(m_TriggerThisFrame);
            m_TriggerThisFrame = -1;
        }
    }

    private void OnAttackDrawCompleted() => AdvanceAttackAnimationPhase(AttackAnimationPhase.Striking);
    private void OnAttackStrikeCompleted() => AdvanceAttackAnimationPhase(AttackAnimationPhase.Withdrawing);
    private void OnAttackWithdrawCompleted() => AdvanceAttackAnimationPhase(AttackAnimationPhase.NotAttacking);
    private void OnStandCompleted() => IsStandingFinished = true;

    private void AdvanceAttackAnimationPhase(AttackAnimationPhase newPhase)
    {
        var expectedCurrentPhase = newPhase switch
        {
            AttackAnimationPhase.NotAttacking => AttackAnimationPhase.Withdrawing,
            AttackAnimationPhase.Drawing => AttackAnimationPhase.NotAttacking,
            AttackAnimationPhase.Striking => AttackAnimationPhase.Drawing,
            AttackAnimationPhase.Withdrawing => AttackAnimationPhase.Striking,
            _ => throw new System.NotImplementedException()
        };
        Assert.AreEqual(AttackAnimationPhase, expectedCurrentPhase,
            $"Wrong attack animation phase progression: transitioning from {AttackAnimationPhase} to {newPhase}.");

        Assert.IsTrue(AttackAnimationPhaseCompletedThisFrame is null, "More than one phase completed this frame, unallowed behavior.");

        AttackAnimationPhaseCompletedThisFrame = AttackAnimationPhase;
        AttackAnimationPhase = newPhase;
    }
}