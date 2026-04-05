using UnityEngine;

public class PlayerGroundCheckComponent : MonoBehaviour
{
    public bool JustLanded { get; private set; }
    public bool JustLeftGround { get; private set; }
    public bool IsGrounded { get; private set; }

    [SerializeField] private PlayerStats m_Stats;
    [SerializeField] private Transform m_GroundCheckOrigin;

    private bool m_WasGrounded;
    private float m_CoyoteTimer;

    public bool CheckCoyoteTime() => m_CoyoteTimer > 0f;
    public void ClearCoyoteTime() => m_CoyoteTimer = 0f;

    private void Update()
    {
        if (JustLeftGround)
        {
            m_CoyoteTimer = m_Stats.CoyoteTime;
        }
        if (m_CoyoteTimer > 0f)
        {
            m_CoyoteTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        m_WasGrounded = IsGrounded;
        var (origin, size) = ComputeGroundCheckBox();
        IsGrounded = Physics2D.OverlapBox( origin, size, angle: 0f, m_Stats.GroundLayer );
        JustLanded = IsGrounded && !m_WasGrounded;
        JustLeftGround = !IsGrounded && m_WasGrounded;
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
        float originOffsetY = m_Stats.GroundCheckSize.y * 0.5f + m_Stats.GroundCheckOffset;
        return (m_GroundCheckOrigin.position + Vector3.down * originOffsetY, m_Stats.GroundCheckSize);
    }
}