using UnityEngine;
using UnityEngine.Assertions;

public class AttackRangedComponent : MonoBehaviour
{
    public bool IsOnCooldown => m_CooldownTimer > 0f;

    [SerializeField] private Transform m_ProjectileCastPosition;
    [SerializeField] private DataReference<IAttackRangedStats> m_Stats;

    private float m_CooldownTimer = 0f;

    public void CastProjectile(Vector2 targetPosition)
    {
        Assert.IsFalse(IsOnCooldown, "Cannot cast projectile while on cooldown.");

        Projectile projectile = Instantiate(m_Stats.Value.ProjectilePrefab);
        projectile.Cast(m_ProjectileCastPosition.position, targetPosition, m_Stats.Value.ProjectileSpeed, m_Stats.Value.ProjectileMaxHeight);

        m_CooldownTimer = m_Stats.Value.AttackRangedCooldown;
    }

    private void Update()
    {
        if(IsOnCooldown)
        {
            m_CooldownTimer -= Time.deltaTime;
        }
    }
}