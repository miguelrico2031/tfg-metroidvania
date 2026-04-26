using UnityEngine;

[RequireComponent(typeof(AttackTargetComponent))]
public class DamageFlashComponent : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_SpriteRenderer;
    [SerializeField] private DataReference<IAnimationStats> m_Stats;

    private float m_DamageFlashTimer;
    private AttackTargetComponent m_AttackTarget;
    private Material m_DefaultMaterial;

    private void OnEnable()
    {
        m_AttackTarget = GetComponent<AttackTargetComponent>();
        m_AttackTarget.OnAttackReceived += OnAttackReceived;
    }

    private void OnDisable()
    {
        m_AttackTarget.OnAttackReceived -= OnAttackReceived;
    }

    private void Start()
    {
        m_DefaultMaterial = m_SpriteRenderer.sharedMaterial;
    }

    private void Update()
    {
        if (m_DamageFlashTimer > 0f)
        {
            m_DamageFlashTimer -= Time.deltaTime;
            if (m_DamageFlashTimer <= 0f)
            {
                m_SpriteRenderer.material = m_DefaultMaterial;
            }
        }
    }

    private void OnAttackReceived()
    {
        if (!m_AttackTarget.IsAlive ||
            m_AttackTarget.ResolvedAttackThisFrame.Result is not AttackResult.Hit ||
            m_Stats.Value.DamageFlashAnimationDuration < 0.01f)
            return;

        m_SpriteRenderer.material = m_Stats.Value.DamageFlashMaterial;
        m_DamageFlashTimer = m_Stats.Value.DamageFlashAnimationDuration;
    }
}