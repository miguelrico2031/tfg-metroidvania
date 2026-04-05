using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class PlayerMovementComponent : MonoBehaviour
{
    public float VerticalVelocity => m_Rigidbody.linearVelocityY;

    [SerializeField] private PlayerStats m_Stats;

    private Rigidbody2D m_Rigidbody;

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

    public void SetRisingGravity() => m_Rigidbody.gravityScale = m_Stats.RisingGravity;
    public void SetFallingGravity() => m_Rigidbody.gravityScale = m_Stats.FallingGravity;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        SetRisingGravity();
    }
}
