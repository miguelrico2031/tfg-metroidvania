using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class PlayerMovementComponent : MonoBehaviour
{
    public float VerticalVelocity => m_Rigidbody.linearVelocityY;

    [Header("Movement")]
    [SerializeField] private float m_GroundSpeed;
    [SerializeField] private float m_AirSpeed;

    [Header("Jump")]
    [SerializeField] private float m_JumpForce;
    [SerializeField][Range(0f, 1f)] private float m_JumpCutMultiplier;

    [Header("Gravity")]
    [SerializeField] private float m_RisingGravity;
    [SerializeField] private float m_FallingGravity;

    private Rigidbody2D m_Rigidbody;

    public void MoveGrounded(int movementDirection)
    {
        m_Rigidbody.linearVelocityX = movementDirection * m_GroundSpeed;
    }

    public void MoveAirborne(int movementDirection)
    {
        m_Rigidbody.linearVelocityX = movementDirection * m_AirSpeed;
    }
    public void Jump()
    {
        m_Rigidbody.linearVelocityY = m_JumpForce;
    }

    public void CutJump()
    {
        m_Rigidbody.linearVelocityY *= m_JumpCutMultiplier;
    }

    public void SetRisingGravity() => m_Rigidbody.gravityScale = m_RisingGravity;
    public void SetFallingGravity() => m_Rigidbody.gravityScale = m_FallingGravity;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        SetRisingGravity();
    }
}
