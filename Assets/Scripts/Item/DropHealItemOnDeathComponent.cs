using UnityEngine;

[RequireComponent (typeof(HealthComponent))]
public class DropHealItemOnDeathComponent : MonoBehaviour
{
    [SerializeField] private bool m_DropOnTheGround;
    [SerializeField] private ObjectPoolContainer m_ObjectPool;
    [SerializeField] private DataReference<IPerceptionStats> m_Stats;

    private HealthComponent m_Health;

    private void OnEnable()
    {
        m_Health = GetComponent<HealthComponent>();
        m_Health.OnHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        m_Health.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int _)
    {
        if (m_Health.CurrentHealth > 0)
            return;

        m_Health.OnHealthChanged -= OnHealthChanged;
        GameObject item = m_ObjectPool.Get();
        item.transform.position = GetItemDropPosition();
    }

    private Vector3 GetItemDropPosition()
    {
        if (!m_DropOnTheGround)
            return transform.position;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, float.MaxValue, m_Stats.Value.GroundLayers.value);
        return hit.collider ? hit.point : transform.position;
    }
}