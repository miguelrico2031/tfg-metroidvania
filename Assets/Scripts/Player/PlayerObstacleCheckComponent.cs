using UnityEngine;

public class PlayerObstacleCheckComponent : MonoBehaviour
{
    public bool IsObstructed => m_CheckedThisFrame ? m_IsObstructed : CheckObstacles();

    [SerializeField] private PlayerStats m_Stats;

    private BoxCollider2D m_BoxCollider;
    private PlayerDirectionComponent m_Direction;

    private bool m_CheckedThisFrame = false;
    private bool m_IsObstructed = false;

    private void Awake()
    {
        m_BoxCollider = GetComponent<BoxCollider2D>();
        m_Direction = GetComponent<PlayerDirectionComponent>();
    }

    private void LateUpdate()
    {
        m_CheckedThisFrame = false;
    }
    private void OnDrawGizmosSelected()
    {
        if (m_BoxCollider == null)
            return;

        Gizmos.color = m_IsObstructed ? Color.green : Color.red;
        var (origin, size) = ComputeObstacleCheckBox();
        Gizmos.DrawWireCube(origin, size);
    }

    private bool CheckObstacles()
    {
        var (origin, size) = ComputeObstacleCheckBox();
        m_IsObstructed = Physics2D.OverlapBox(origin, size, angle: 0f, m_Stats.ObstacleLayers);
        m_CheckedThisFrame = true;
        return m_IsObstructed;
    }

    private (Vector3 origin, Vector3 size) ComputeObstacleCheckBox()
    {
        Vector3 origin = m_BoxCollider.bounds.center;
        float displacement = m_BoxCollider.bounds.extents.x + m_Stats.ObstacleCheckSize.x * 0.5f + m_Stats.ObstacleCheckOffset;
        origin.x += m_Direction.Direction * displacement;
        return (origin, m_Stats.ObstacleCheckSize);
    }
}