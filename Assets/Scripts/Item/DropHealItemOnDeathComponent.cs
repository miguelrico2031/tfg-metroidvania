using UnityEngine;

[RequireComponent (typeof(HealthComponent))]
public class DropHealItemOnDeathComponent : MonoBehaviour
{
    [SerializeField] private bool m_DropOnTheGround;
    [SerializeField] private DataReference<IPerceptionStats> m_Stats;
    [SerializeField] private LevelServiceLocator m_LevelServiceLocator;

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

    private void Start()
    {
        m_LevelServiceLocator.TryGetService<IDistributeHealItemDropsService>(out var distributeService);
        distributeService.RegisterComponent(this);
    }

    private void OnHealthChanged(int _)
    {
        if (m_Health.CurrentHealth > 0)
            return;

        m_Health.OnHealthChanged -= OnHealthChanged;
        m_LevelServiceLocator.TryGetService<IObjectPoolService>(out var poolService);
        GameObject item = poolService.GetHealsPoolContainer().Get();
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