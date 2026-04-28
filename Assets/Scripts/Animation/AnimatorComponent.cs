using UnityEngine;
using UnityEngine.Assertions;

public enum AttackAnimationPhase
{
    NotAttacking,
    Drawing,
    Striking,
    Withdrawing
}

public enum BlockAnimationPhase
{
    NotBlocking,
    Drawing,
    Blocking,
    Withdrawing
}

[RequireComponent(typeof(Rigidbody2D))]
public class AnimatorComponent : MonoBehaviour
{
    public AttackAnimationPhase AttackAnimationPhase { get; private set; } = AttackAnimationPhase.NotAttacking;
    public AttackAnimationPhase? AttackAnimationPhaseCompletedThisFrame { get; private set; } = null;

    public BlockAnimationPhase BlockAnimationPhase { get; private set; } = BlockAnimationPhase.NotBlocking;
    public AnimationEventType? AnimationCompleted { get; private set; } = null;

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
    private static readonly int s_Block = Animator.StringToHash("Block");
    private static readonly int s_StopBlock = Animator.StringToHash("StopBlock");
    private static readonly int s_PickUpHeal = Animator.StringToHash("PickUpHeal");
    private static readonly int s_Heal = Animator.StringToHash("Heal");

    private bool m_IsGrounded;
    private int m_TriggerThisFrame = -1;
    private Rigidbody2D m_Rigidbody;

    public void StartGroundedAnimation()
    {
        SetAnimation(s_Grounded);
        m_IsGrounded = true;
    }

    public void StartJumpAnimation()
    {
        SetAnimation(s_Jump);
        m_IsGrounded = false;
    }

    public void StartFallAnimation(bool transitionFromJump)
    {
        SetAnimation(transitionFromJump ? s_FallFromJump : s_Fall);
        m_IsGrounded = false;
    }

    public void StartDashAnimation()
    {
        SetAnimation(s_Dash);
        m_IsGrounded = true;
    }

    public void StartStandAnimation(bool transitionFromKnockback)
    {
        SetAnimation(transitionFromKnockback ? s_StandFromKnockback : s_Stand);
        m_IsGrounded = true;
    }

    public void StartKnockbackAnimation(bool isAirborne)
    {
        SetAnimation(isAirborne ? s_KnockbackAirborne : s_Knockback);
        BlockAnimationPhase = BlockAnimationPhase.NotBlocking;
        m_IsGrounded = !isAirborne;
    }

    public void StartDeathAnimation()
    {
        SetAnimation(s_Death);
    }
    public void StartAttackAnimation(bool isFirstAttack) //Refers to melee default attack
    {
        SetAnimation(isFirstAttack ? s_Attack : s_AttackContinue);
        AttackAnimationPhase = AttackAnimationPhase.Drawing;
    }

    public void StartBlockAnimation()
    {
        SetAnimation(s_Block);
        AdvanceBlockAnimationPhase(BlockAnimationPhase.Drawing);
    }

    public void StartStopBlockAnimation()
    {
        SetAnimation(s_StopBlock);
        AdvanceBlockAnimationPhase(BlockAnimationPhase.Withdrawing);
    }

    public void StartPickUpHealAnimation()
    {
        SetAnimation(s_PickUpHeal);
    }

    public void StartHealAnimation()
    {
        SetAnimation(s_Heal);
    }

    private void OnEnable()
    {
        if (m_AnimationEvents != null)
        {
            m_AnimationEvents.OnEventReceived += OnAnimationEventReceived;
        }
    }

    private void OnDisable()
    {
        if (m_AnimationEvents != null)
        {
            m_AnimationEvents.OnEventReceived -= OnAnimationEventReceived;
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

    private void SetAnimation(int animationTrigger)
    {
        Assert.IsTrue(m_TriggerThisFrame == -1, "Already set animation trigger this frame, cannot set it again.");
        m_TriggerThisFrame = animationTrigger;
        AnimationCompleted = null;
    }

    private void OnAnimationEventReceived(AnimationEventType type)
    {
        switch (type)
        {
            case AnimationEventType.AttackDrawCompleted:
                AdvanceAttackAnimationPhase(AttackAnimationPhase.Striking);
                break;
            case AnimationEventType.AttackStrikeCompleted:
                AdvanceAttackAnimationPhase(AttackAnimationPhase.Withdrawing);
                break;
            case AnimationEventType.AttackWithdrawCompleted:
                AdvanceAttackAnimationPhase(AttackAnimationPhase.NotAttacking);
                break;
            case AnimationEventType.StartBlockCompleted:
                AdvanceBlockAnimationPhase(BlockAnimationPhase.Blocking);
                break;
            case AnimationEventType.StopBlockCompleted:
                AdvanceBlockAnimationPhase(BlockAnimationPhase.NotBlocking);
                break;   
            case AnimationEventType.StandCompleted:
            case AnimationEventType.PickUpHealCompleted:
            case AnimationEventType.HealCompleted:
                AnimationCompleted = type;
                break;
        }
    }
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

    private void AdvanceBlockAnimationPhase(BlockAnimationPhase newPhase)
    {
        var expectedCurrentPhase = newPhase switch
        {
            BlockAnimationPhase.NotBlocking => BlockAnimationPhase.Withdrawing,
            BlockAnimationPhase.Drawing => BlockAnimationPhase.NotBlocking,
            BlockAnimationPhase.Blocking => BlockAnimationPhase.Drawing,
            BlockAnimationPhase.Withdrawing => BlockAnimationPhase.Blocking,
            _ => throw new System.NotImplementedException()
        };
        Assert.AreEqual(BlockAnimationPhase, expectedCurrentPhase,
            $"Wrong Block animation phase progression: transitioning from {BlockAnimationPhase} to {newPhase}.");

        BlockAnimationPhase = newPhase;
    }
}