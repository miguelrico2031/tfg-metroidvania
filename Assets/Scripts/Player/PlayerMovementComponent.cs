using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementComponent : MonoBehaviour
{
    public float VerticalVelocity => m_Rigidbody.linearVelocityY;
    public bool IsDashing => m_DashTimer > 0f;
    public bool IsInKnockback => m_KnockbackTimer > 0f;
    public event Action OnDirectionChanged;

    [SerializeField] private PlayerStats m_Stats;

    private float m_DashTimer;
    private float m_KnockbackTimer;
    private Transform m_Transform;
    private Rigidbody2D m_Rigidbody;

    public void MoveGrounded(int movementDirection)
    {
        Move(movementDirection, m_Stats.GroundSpeed);
    }

    public void MoveAirborne(int movementDirection)
    {
        Move(movementDirection, m_Stats.AirSpeed);
    }
    public void Jump()
    {
        m_Rigidbody.linearVelocityY = m_Stats.JumpForce;
    }

    public void CutJump()
    {
        m_Rigidbody.linearVelocityY *= m_Stats.JumpCutMultiplier;
    }

    public void ApplyDash()
    {
        m_DashTimer = m_Stats.DashDuration;
        float dashSpeedX = m_Stats.DashDistance / m_Stats.DashDuration;
        m_Rigidbody.linearVelocityX = m_Transform.right.x * dashSpeedX;
    }

    public void FinishDash()
    {
        m_DashTimer = 0f;
        m_Rigidbody.linearVelocityX = 0f;
    }

    public bool ApplyAttackKnockback(Attack attack)
    {
        if (attack.KnockbackDistance < 0.01f || attack.KnockbackDuration < 0.01f)
            return false;

        m_KnockbackTimer = attack.KnockbackDuration;
        float knockbackSpeedX = attack.KnockbackDistance / attack.KnockbackDuration;
        float directionToAttackX = Mathf.Sign(m_Transform.position.x - attack.Source.Position.x);
        if(Mathf.Sign(m_Transform.right.x) != directionToAttackX) //Ensure facing attack source
        {
            Flip(right: directionToAttackX < 0f); 
        }
        m_Rigidbody.linearVelocityX = -m_Transform.right.x * knockbackSpeedX;
        return true;
    }

    public void FinishKnockback()
    {
        m_KnockbackTimer = 0f;
        m_Rigidbody.linearVelocityX = 0f;
    }

    public void SetRisingGravity() => m_Rigidbody.gravityScale = m_Stats.RisingGravity;
    public void SetFallingGravity() => m_Rigidbody.gravityScale = m_Stats.FallingGravity;

    private void Awake()
    {
        m_Transform = transform;
        m_Rigidbody = GetComponent<Rigidbody2D>();
        SetRisingGravity();
    }

    private void Update()
    {
        if (m_DashTimer > 0f)
        {
            m_DashTimer -= Time.deltaTime;
            if (m_DashTimer <= 0f)
            {
                FinishDash();
            }
        }
        if(m_KnockbackTimer > 0f)
        {
            m_KnockbackTimer -= Time.deltaTime;
            if(m_KnockbackTimer <= 0f)
            {
                FinishKnockback();
            }
        }
    }

    private void Move(int movementDirection, float speed)
    {
        bool directionChanged = movementDirection != 0 && movementDirection != Mathf.CeilToInt(m_Transform.right.x);
        if (directionChanged)
        {
            Flip(right: movementDirection > 0f);
        }

        if (directionChanged && !m_Stats.MoveOnChangeDirection)
        {
            m_Rigidbody.linearVelocityX = 0f;
        }
        else
        {
            m_Rigidbody.linearVelocityX = movementDirection * speed;
        }
    }

    private void Flip(bool right)
    {
        m_Transform.rotation = Quaternion.Euler(0f, right ? 0f : 180f, 0f);
        OnDirectionChanged?.Invoke();
    }
}
