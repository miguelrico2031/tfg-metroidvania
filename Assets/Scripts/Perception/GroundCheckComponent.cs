using UnityEngine;

public class GroundCheckComponent : MonoBehaviour
{
    public bool JustLanded { get; private set; }
    public bool JustLeftGround { get; private set; }
    public bool IsGrounded { get; private set; }
    public BufferingTimer CoyoteTimeBuffer { get; private set; }

    [SerializeField] private DataReference<IPerceptionStats> m_Stats;
    [SerializeField] private Transform m_GroundCheckOrigin;

    private bool m_WasGrounded;

    private void Awake()
    {
        CoyoteTimeBuffer = new(() => m_Stats.Value.CoyoteTime);
    }

    private void Update()
    {
        CoyoteTimeBuffer.Tick(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        m_WasGrounded = IsGrounded;
        var (origin, size) = ComputeGroundCheckBox();
        IsGrounded = Physics2D.OverlapBox( origin, size, angle: 0f, m_Stats.Value.GroundLayers );
        JustLanded = IsGrounded && !m_WasGrounded;
        JustLeftGround = !IsGrounded && m_WasGrounded;
        if(JustLeftGround)
        {
            CoyoteTimeBuffer.Register();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (m_GroundCheckOrigin == null)
            return;

        Gizmos.color = IsGrounded ? Color.green : Color.red;
        var (origin, size) = ComputeGroundCheckBox();
        Gizmos.DrawWireCube(origin, size);
    }

    private (Vector3 origin, Vector3 size) ComputeGroundCheckBox()
    {
        float originOffsetY = m_Stats.Value.GroundCheckSize.y * 0.5f + m_Stats.Value.GroundCheckOffset;
        return (m_GroundCheckOrigin.position + Vector3.down * originOffsetY, m_Stats.Value.GroundCheckSize);
    }
}