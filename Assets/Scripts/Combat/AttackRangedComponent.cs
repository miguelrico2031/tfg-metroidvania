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
        targetPosition += m_Stats.Value.TargetPositionOffset;
        Projectile projectile = Instantiate(m_Stats.Value.ProjectilePrefab);
        projectile.Cast(m_ProjectileCastPosition.position, targetPosition, m_Stats.Value.ProjectileSpeed, m_Stats.Value.ProjectileMaxHeight);

        m_CooldownTimer = m_Stats.Value.AttackRangedCooldown;
    }

    public bool IsTargetFarEnough(Vector2 targetPosition)
    {
        float minDistance = m_Stats.Value.MinCastingDistance;
        if (minDistance < 0.01f)
            return true;
        return Vector2.Distance(targetPosition + m_Stats.Value.TargetPositionOffset, m_ProjectileCastPosition.position) > minDistance;
    }

    private void Update()
    {
        if(IsOnCooldown)
        {
            m_CooldownTimer -= Time.deltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.darkRed;
        Gizmos.DrawWireSphere(m_ProjectileCastPosition.position, m_Stats.Value.MinCastingDistance);
    }
}