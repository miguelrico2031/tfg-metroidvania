using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerObstacleCheckComponent : MonoBehaviour
{
    public bool IsObstructedForward => m_CheckedThisFrame ? m_IsObstructedForward : CheckObstacles(true);
    public bool IsObstructedBehind => m_CheckedThisFrame ? m_IsObstructedBehind : CheckObstacles(false);

    [SerializeField] private PlayerStats m_Stats;

    private Collider2D m_Collider;
    private Rigidbody2D m_Rigidbody;
    private Transform m_Transform;

    private bool m_CheckedThisFrame = false;
    private bool m_IsObstructedForward = false;
    private bool m_IsObstructedBehind = false;

    private void Awake()
    {
        m_Collider = GetComponent<Collider2D>();
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Transform = transform;
    }

    private void LateUpdate()
    {
        m_CheckedThisFrame = false;
    }
    private void OnDrawGizmosSelected()
    {
        if (m_Collider == null || m_Transform == null || m_Rigidbody == null)
        {
            m_Collider = GetComponent<Collider2D>();
            m_Rigidbody = GetComponent<Rigidbody2D>();
            m_Transform = transform;
        }
        var (originForward, originBehind, size) = ComputeObstacleCheckBox();
        Gizmos.color = m_IsObstructedForward ? Color.green : Color.red;
        Gizmos.DrawWireCube(originForward, size);
        Gizmos.color = m_IsObstructedBehind ? Color.green : Color.red;
        Gizmos.DrawWireCube(originBehind, size);
    }

    private bool CheckObstacles(bool returnForward)
    {
        var (originForward, originBehind, size) = ComputeObstacleCheckBox();
        m_IsObstructedForward = Physics2D.OverlapBox(originForward, size, angle: 0f, m_Stats.ObstacleLayers);
        m_IsObstructedBehind = Physics2D.OverlapBox(originBehind, size, angle: 0f, m_Stats.ObstacleLayers);
        m_CheckedThisFrame = true;
        return returnForward ? m_IsObstructedForward : m_IsObstructedBehind;
    }

    private (Vector3 originForward, Vector3 originBehind, Vector3 size) ComputeObstacleCheckBox()
    {
        Vector3 origin = m_Collider.bounds.center;
        float displacement = m_Collider.bounds.extents.x + m_Stats.ObstacleCheckSize.x * 0.5f + m_Stats.ObstacleCheckOffset;
        return (origin + transform.right * displacement, origin - transform.right * displacement, m_Stats.ObstacleCheckSize);
    }
}