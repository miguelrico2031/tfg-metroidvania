using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class ObstacleCheckComponent : MonoBehaviour
{
    public bool IsObstructedForward => ObstacleForward != null;
    public bool IsObstructedBehind => ObstacleBehind != null;
    public Collider2D ObstacleForward => m_CheckedThisFrame ? m_ObstacleForward : CheckObstacles(true);
    public Collider2D ObstacleBehind => m_CheckedThisFrame ? m_ObstacleBehind : CheckObstacles(true);

    [SerializeField] private StatsReference<IPerceptionStats> m_Stats;

    private bool m_CheckedThisFrame = false;
    private Collider2D m_ObstacleForward;
    private Collider2D m_ObstacleBehind;
    private Collider2D m_Collider;
    private Transform m_Transform;

    private void Awake()
    {
        m_Collider = GetComponent<Collider2D>();
        m_Transform = transform;
    }

    private void LateUpdate()
    {
        m_CheckedThisFrame = false;
    }
    private void OnDrawGizmosSelected()
    {
        if (m_Collider == null || m_Transform == null)
        {
            m_Collider = GetComponent<Collider2D>();
            m_Transform = transform;
        }
        var (originForward, originBehind, size) = ComputeObstacleCheckBox();
        Gizmos.color = m_ObstacleForward ? Color.green : Color.red;
        Gizmos.DrawWireCube(originForward, size);
        Gizmos.color = m_ObstacleBehind ? Color.green : Color.red;
        Gizmos.DrawWireCube(originBehind, size);
    }

    private Collider2D CheckObstacles(bool returnForward)
    {
        var (originForward, originBehind, size) = ComputeObstacleCheckBox();
        m_ObstacleForward = Physics2D.OverlapBox(originForward, size, angle: 0f, m_Stats.Value.ObstacleLayers);
        m_ObstacleBehind = Physics2D.OverlapBox(originBehind, size, angle: 0f, m_Stats.Value.ObstacleLayers);
        m_CheckedThisFrame = true;
        return returnForward ? m_ObstacleForward : m_ObstacleBehind;
    }

    private (Vector3 originForward, Vector3 originBehind, Vector3 size) ComputeObstacleCheckBox()
    {
        Vector3 origin = m_Collider.bounds.center;
        float displacement = m_Collider.bounds.extents.x + m_Stats.Value.ObstacleCheckSize.x * 0.5f + m_Stats.Value.ObstacleCheckOffset;
        return (origin + transform.right * displacement, origin - transform.right * displacement, m_Stats.Value.ObstacleCheckSize);
    }
}