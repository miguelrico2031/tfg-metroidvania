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

[RequireComponent(typeof(Rigidbody2D), typeof(AttackTargetComponent))]
public class PlayerAnimatorComponent : MonoBehaviour
{
    public AttackAnimationPhase AttackAnimationPhase {  get; private set; } = AttackAnimationPhase.None;

    [SerializeField] private Material m_DefaultMaterial;
    [SerializeField] private Material m_DamageFlashMaterial;
    [SerializeField] private SpriteRenderer m_SpriteRenderer;
    [SerializeField] private Animator m_Animator;
    [SerializeField] private AnimationEvents m_AnimationEvents;
    [SerializeField] private PlayerStats m_Stats;

    private static readonly int s_MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int s_Grounded = Animator.StringToHash("Grounded");
    private static readonly int s_Jump = Animator.StringToHash("Jump");
    private static readonly int s_Fall = Animator.StringToHash("Fall");
    private static readonly int s_Dash = Animator.StringToHash("Dash");
    private static readonly int s_Knockback = Animator.StringToHash("Knockback");
    private static readonly int s_KnockbackAirborne = Animator.StringToHash("KnockbackAirborne");
    private static readonly int s_Death = Animator.StringToHash("Death");
    private static readonly int s_Attack = Animator.StringToHash("Attack");
    private static readonly int s_AttackContinue = Animator.StringToHash("AttackContinue");

    private bool m_IsGrounded;
    private int m_TriggerThisFrame = -1;
    private float m_DamageFlashTimer;
    private Rigidbody2D m_Rigidbody;
    private AttackTargetComponent m_AttackTarget;

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

    public void StartFallAnimation()
    {
        Assert.IsTrue(m_TriggerThisFrame == -1, "Already set animation trigger this frame, cannot set it again.");
        m_IsGrounded = false;
        m_TriggerThisFrame = s_Fall;
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
        m_AttackTarget = GetComponent<AttackTargetComponent>();
        m_AttackTarget.OnAttackReceived += OnAttackReceived;

        m_AnimationEvents.OnPlayerAttack1Completed += OnPlayerAttackCompleted;
        m_AnimationEvents.OnPlayerAttack1Withdrawn += OnPlayerAttackWithdrawn;
        m_AnimationEvents.OnPlayerAttack2Completed += OnPlayerAttackCompleted;
        m_AnimationEvents.OnPlayerAttack2Withdrawn += OnPlayerAttackWithdrawn;
    }

    private void OnDisable()
    {
        m_AttackTarget.OnAttackReceived -= OnAttackReceived;

        m_AnimationEvents.OnPlayerAttack1Completed -= OnPlayerAttackCompleted;
        m_AnimationEvents.OnPlayerAttack1Withdrawn -= OnPlayerAttackWithdrawn;
        m_AnimationEvents.OnPlayerAttack2Completed -= OnPlayerAttackCompleted;
        m_AnimationEvents.OnPlayerAttack2Withdrawn -= OnPlayerAttackWithdrawn;
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

        if (m_DamageFlashTimer > 0f)
        {
            m_DamageFlashTimer -= Time.deltaTime;
            if (m_DamageFlashTimer <= 0f)
            {
                m_SpriteRenderer.material = m_DefaultMaterial;
            }
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

    private void OnAttackReceived()
    {
        if (!m_AttackTarget.IsAlive ||
            m_AttackTarget.ResolvedAttackThisFrame.Result is not AttackResult.Hit ||
            m_Stats.DamageFlashAnimationDuration < 0.01f)
            return;

        m_SpriteRenderer.material = m_DamageFlashMaterial;
        m_DamageFlashTimer = m_Stats.DamageFlashAnimationDuration;
    }

    private void OnPlayerAttackCompleted()
    {
        AttackAnimationPhase = AttackAnimationPhase.JustCompleted;       
    }
    private void OnPlayerAttackWithdrawn()
    {
        AttackAnimationPhase = AttackAnimationPhase.JustWithdrawn;
    }
}