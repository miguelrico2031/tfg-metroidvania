using UnityEngine;

public class EdgeCheckComponent : MonoBehaviour
{
    public bool HasEdgeForward => m_CheckedThisFrame ? m_HasEdgeForward : CheckEdges(true);
    public bool HasEdgeBehind => m_CheckedThisFrame ? m_HasEdgeBehind : CheckEdges(false);

    [SerializeField] private DataReference<IPerceptionStats> m_Stats;
    [SerializeField] private Transform m_GroundCheckOrigin;


    private bool m_CheckedThisFrame = false;
    private bool m_HasEdgeForward = false;
    private bool m_HasEdgeBehind = false;
    private Transform m_Transform;

    private void Awake()
    {
        m_Transform = transform;
    }

    private void LateUpdate()
    {
        m_CheckedThisFrame = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (m_GroundCheckOrigin == null)
            return;

        if(m_Transform == null)
        {
            m_Transform = transform;
        }

        var (originForward, originBehind) = ComputeEdgeCheckOrigins();

        Gizmos.color = m_HasEdgeForward ? Color.red : Color.green;
        Gizmos.DrawLine(originForward, originForward + Vector3.down * m_Stats.Value.EdgeCheckDepth);

        Gizmos.color = m_HasEdgeBehind ? Color.red : Color.green;
        Gizmos.DrawLine(originBehind, originBehind + Vector3.down * m_Stats.Value.EdgeCheckDepth);
    }

    private bool CheckEdges(bool returnForward)
    {
        var (originForward, originBehind) = ComputeEdgeCheckOrigins();
        RaycastHit2D hit = Physics2D.Raycast(originForward, Vector2.down, m_Stats.Value.EdgeCheckDepth, m_Stats.Value.GroundLayers);
        m_HasEdgeForward = !hit || !hit.collider;
        hit = Physics2D.Raycast(originBehind, Vector2.down, m_Stats.Value.EdgeCheckDepth, m_Stats.Value.GroundLayers);
        m_HasEdgeBehind = !hit || !hit.collider;
        m_CheckedThisFrame = true;
        return returnForward ? m_HasEdgeForward : m_HasEdgeBehind;
    }

    private (Vector3 originForward, Vector3 originBehind) ComputeEdgeCheckOrigins()
    {
        Vector3 offsetForward = new(Mathf.Sign(m_Transform.right.x) * Mathf.Abs(m_Stats.Value.EdgeCheckOffset.x), m_Stats.Value.EdgeCheckOffset.y, 0f);
        Vector3 offsetBehind = new(-Mathf.Sign(m_Transform.right.x) * Mathf.Abs(m_Stats.Value.EdgeCheckOffset.x), m_Stats.Value.EdgeCheckOffset.y, 0f);
        return (m_GroundCheckOrigin.position + offsetForward, m_GroundCheckOrigin.position + offsetBehind);
    }
}