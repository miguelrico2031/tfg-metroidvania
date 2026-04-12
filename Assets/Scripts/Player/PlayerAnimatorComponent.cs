using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(AttackTargetComponent))]

public class PlayerAnimatorComponent : MonoBehaviour
{
    public bool Attack1JustCompleted {  get; private set; }
    public bool Attack1JustWithdrawn {  get; private set; }
    public bool Attack2JustCompleted { get; private set; }
    public bool Attack2JustWithdrawn { get; private set; }

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
    private static readonly int s_AttackWithdraw = Animator.StringToHash("AttackWithdraw");
    private static readonly int s_AttackContinue = Animator.StringToHash("AttackContinue");

    private bool m_IsGrounded;
    private int m_TriggerThisFrame = -1;
    private float m_DamageFlashTimer;
    private Rigidbody2D m_Rigidbody;
    private AttackTargetComponent m_AttackTarget;

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

    public void StartKnockbackAnimation(bool isAirborne)
    {
        m_TriggerThisFrame = isAirborne ? s_KnockbackAirborne : s_Knockback;
    }

    public void StartDeathAnimation()
    {
        m_TriggerThisFrame = s_Death;
    }

    public void StartAttackAnimation()
    {
        m_TriggerThisFrame = s_Attack;
    }

    public void ResolveAttackAnimation(bool continueAttack)
    {
        m_TriggerThisFrame = continueAttack ? s_AttackContinue : s_AttackWithdraw;
    }

    private void OnEnable()
    {
        m_AttackTarget = GetComponent<AttackTargetComponent>();
        m_AttackTarget.OnAttackReceived += OnAttackReceived;

        m_AnimationEvents.OnPlayerAttack1Completed += OnPlayerAttack1Completed;
        m_AnimationEvents.OnPlayerAttack1Withdrawn += OnPlayerAttack1Withdrawn;
        m_AnimationEvents.OnPlayerAttack2Completed += OnPlayerAttack2Completed;
        m_AnimationEvents.OnPlayerAttack2Withdrawn += OnPlayerAttack2Withdrawn;
    }

    private void OnDisable()
    {
        m_AttackTarget.OnAttackReceived -= OnAttackReceived;

        m_AnimationEvents.OnPlayerAttack1Completed -= OnPlayerAttack1Completed;
        m_AnimationEvents.OnPlayerAttack1Withdrawn -= OnPlayerAttack1Withdrawn;
        m_AnimationEvents.OnPlayerAttack2Completed -= OnPlayerAttack2Completed;
        m_AnimationEvents.OnPlayerAttack2Withdrawn -= OnPlayerAttack2Withdrawn;
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

        //This is not in late update because this script execution order is set to +10, so it will run 
        Attack1JustCompleted = false;
        Attack1JustWithdrawn = false;
        Attack2JustCompleted = false;
        Attack2JustWithdrawn = false;
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

    private void OnPlayerAttack1Completed()
    {
        Attack1JustCompleted = true;        
    }
    private void OnPlayerAttack1Withdrawn()
    {
        Attack1JustWithdrawn = true;
    }
    private void OnPlayerAttack2Completed()
    {
        Attack2JustCompleted = true;
    }
    private void OnPlayerAttack2Withdrawn()
    {
        Attack2JustWithdrawn = true;
    }
}