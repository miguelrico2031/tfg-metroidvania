using UnityEngine;

public class GroundCheckComponent : MonoBehaviour
{
    public bool JustLanded { get; private set; }
    public bool JustLeftGround { get; private set; }
    public bool IsGrounded { get; private set; }

    [SerializeField] private Vector2 m_GroundCheckSize = new Vector2(0.9f, 0.1f);
    [SerializeField] private float m_GroundCheckDistance = 0.05f;
    [SerializeField] private LayerMask m_GroundLayer;
    [SerializeField] private Transform m_GroundCheckOrigin;
    [SerializeField] private float m_CoyoteTime;
    
    private bool m_WasGrounded;
    private float m_CoyoteTimer;

    public bool CheckCoyoteTime() => m_CoyoteTimer > 0f;
    public void ClearCoyoteTime() => m_CoyoteTimer = 0f;

    private void Update()
    {
        if (JustLeftGround)
        {
            m_CoyoteTimer = m_CoyoteTime;
        }
        if(m_CoyoteTimer > 0f)
        {
            m_CoyoteTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        m_WasGrounded = IsGrounded;

        IsGrounded = Physics2D.OverlapBox(
            point: m_GroundCheckOrigin.position + Vector3.down * m_GroundCheckDistance,
            size: m_GroundCheckSize,
            angle: 0f,
            layerMask: m_GroundLayer
        );

        JustLanded = IsGrounded && !m_WasGrounded;
        JustLeftGround = !IsGrounded && m_WasGrounded;
    }

    private void OnDrawGizmosSelected()
    {
        if (m_GroundCheckOrigin == null)
            return;

        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(
            center: m_GroundCheckOrigin.position + Vector3.down * m_GroundCheckDistance,
            size: m_GroundCheckSize
        );
    }
}