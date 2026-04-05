using UnityEngine;

public class PlayerMovementComponent : MonoBehaviour
{
    public float VerticalVelocity => m_Rigidbody.linearVelocityY;
    public bool IsDashing => m_DashTimer > 0f;

    [SerializeField] private PlayerStats m_Stats;

    private float m_DashTimer;
    private Rigidbody2D m_Rigidbody;
    private PlayerDirectionComponent m_Direction;

    public void MoveGrounded(int movementDirection)
    {
        m_Rigidbody.linearVelocityX = movementDirection * m_Stats.GroundSpeed;
    }

    public void MoveAirborne(int movementDirection)
    {
        m_Rigidbody.linearVelocityX = movementDirection * m_Stats.AirSpeed;
    }
    public void Jump()
    {
        m_Rigidbody.linearVelocityY = m_Stats.JumpForce;
    }

    public void CutJump()
    {
        m_Rigidbody.linearVelocityY *= m_Stats.JumpCutMultiplier;
    }

    public void Dash()
    {
        m_DashTimer = m_Stats.DashDuration;
        float dashSpeedX = m_Stats.DashDistance / m_Stats.DashDuration;
        m_Rigidbody.linearVelocityX = m_Direction.Direction * dashSpeedX;
    }

    public void FinishDash()
    {
        m_DashTimer = 0f;
        m_Rigidbody.linearVelocityX = 0f;
    }

    public void SetRisingGravity() => m_Rigidbody.gravityScale = m_Stats.RisingGravity;
    public void SetFallingGravity() => m_Rigidbody.gravityScale = m_Stats.FallingGravity;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Direction = GetComponent<PlayerDirectionComponent>();
        SetRisingGravity();
    }

    private void Update()
    {
        if (m_DashTimer > 0f)
        {
            m_DashTimer -= Time.deltaTime;
            if (m_DashTimer <= 0)
            {
                FinishDash();
            }
        }
    }
}
