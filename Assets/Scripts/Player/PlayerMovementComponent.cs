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

    public void Stop()
    {
        m_Rigidbody.linearVelocity = Vector2.zero;
        m_DashTimer = 0f;
        m_KnockbackTimer = 0f;
    }
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

    public void ApplyDash(int dashDirection)
    {
        m_DashTimer = m_Stats.DashDuration;
        if(dashDirection != Mathf.CeilToInt(m_Transform.right.x))
        {
            Turn(right: dashDirection > 0f);
        }
        float dashSpeedX = m_Stats.DashDistance / m_Stats.DashDuration;
        m_Rigidbody.linearVelocityX = m_Transform.right.x * dashSpeedX;
    }

    public void FinishDash()
    {
        m_DashTimer = 0f;
        m_Rigidbody.linearVelocityX = 0f;
    }

    public bool ApplyAttackKnockback(AttackData attack)
    {
        KnockbackData knockback = attack.Knockback;
        if (knockback.Distance < 0.01f || knockback.Duration < 0.01f)
            return false;

        float knockbackDirection = Mathf.Sign(m_Transform.position.x - attack.Source.Position.x);
        if (Mathf.Approximately(knockbackDirection, 0f))
        {
            knockbackDirection = -m_Transform.right.x;
        }

        bool sourceIsToTheRight = attack.Source.Position.x > m_Transform.position.x;
        bool currentlyFacingRight = m_Transform.right.x > 0f;
        if (currentlyFacingRight != sourceIsToTheRight) //Ensure facing attack source
        {
            Turn(right: sourceIsToTheRight);
        }

        if (knockback.Height > 0.1f)
        {
            ApplyParabolaKnockback(knockback, knockbackDirection);
        }
        else
        {
            ApplyHorizontalKnockback(knockback, knockbackDirection);
        }

        m_KnockbackTimer = knockback.Duration;
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
        if (m_KnockbackTimer > 0f)
        {
            m_KnockbackTimer -= Time.deltaTime;
            if (m_KnockbackTimer <= 0f)
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
            Turn(right: movementDirection > 0f);
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

    private void Turn(bool right)
    {
        m_Transform.rotation = Quaternion.Euler(0f, right ? 0f : 180f, 0f);
        OnDirectionChanged?.Invoke();
    }

    private void ApplyHorizontalKnockback(KnockbackData knockback, float direction)
    {
        float speed = knockback.Distance / knockback.Duration;
        m_Rigidbody.linearVelocityX = direction * speed;
        m_Rigidbody.linearVelocityY = 0f;
    }

    private void ApplyParabolaKnockback(KnockbackData knockback, float direction)
    {
        float knockbackGravity = 8f * knockback.Height / (knockback.Duration * knockback.Duration);
        m_Rigidbody.gravityScale = -knockbackGravity / Physics2D.gravity.y;

        float speedX = knockback.Distance / knockback.Duration;
        m_Rigidbody.linearVelocityX = direction * speedX;
        m_Rigidbody.linearVelocityY = 4f * knockback.Height / knockback.Duration;
    }
}
