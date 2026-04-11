using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerObstacleCheckComponent : MonoBehaviour
{
    public bool IsObstructed => m_CheckedThisFrame ? m_IsObstructed : CheckObstacles();

    [SerializeField] private PlayerStats m_Stats;

    private Collider2D m_Collider;
    private Rigidbody2D m_Rigidbody;
    private Transform m_Transform;

    private bool m_CheckedThisFrame = false;
    private bool m_IsObstructed = false;

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
        Vector3 origin = m_Collider.bounds.center;
        float displacement = m_Collider.bounds.extents.x + m_Stats.ObstacleCheckSize.x * 0.5f + m_Stats.ObstacleCheckOffset;
        float direction =  Mathf.Abs(m_Rigidbody.linearVelocityX) < 0.01f
            ? transform.right.x
            : Mathf.Sign(m_Rigidbody.linearVelocityX);
        origin.x += direction * displacement;
        return (origin, m_Stats.ObstacleCheckSize);
    }
}