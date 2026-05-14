using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Hitbox))]
public class Projectile : MonoBehaviour, IPoolable
{
    public ObjectPool<GameObject> ObjectPool { get; set; }

    [SerializeField] private bool m_FaceMovement;

    private Rigidbody2D m_Rigidbody;
    private Collider2D m_Collider;
    private Hitbox m_Hitbox;

    private Vector2 m_StartPosition;
    private Vector2 m_TargetPosition;
    private float m_FlightDurationInv;
    private float m_ElapsedTime;
    private bool m_IsLaunched;
    private float m_ArcHeight;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Rigidbody.gravityScale = 0f;
        m_Rigidbody.freezeRotation = true;

        m_Collider = GetComponent<Collider2D>();
        m_Collider.isTrigger = true;

        m_Hitbox = GetComponent<Hitbox>();
    }

    public void Cast(Vector2 targetPosition, float speed, float arcHeight = 0f)
    {
        m_StartPosition = transform.position;
        m_TargetPosition = targetPosition;
        m_ArcHeight = arcHeight;

        float distance = Vector2.Distance(m_StartPosition, targetPosition);
        m_FlightDurationInv =  speed / distance; // 1 / flight duration

        m_ElapsedTime = 0f;
        m_IsLaunched = true;

        m_Rigidbody.position = m_StartPosition;
        m_Rigidbody.linearVelocity = ComputeVelocity(0f);
    }
    private void FixedUpdate()
    {
        if (!m_IsLaunched) return;

        m_ElapsedTime += Time.fixedDeltaTime;
        float t = m_ElapsedTime * m_FlightDurationInv;
        m_Rigidbody.linearVelocity = ComputeVelocity(t);

        if(m_FaceMovement)
        {
            if (m_Rigidbody.linearVelocity.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(m_Rigidbody.linearVelocityY, m_Rigidbody.linearVelocityX) * Mathf.Rad2Deg;
                m_Rigidbody.MoveRotation(angle);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("KillPlane"))
        {
            ObjectPool.Release(gameObject);
        }
    }

    private Vector2 ComputeVelocity(float t)
    {
        Vector2 baseVelocity = (m_TargetPosition - m_StartPosition) * m_FlightDurationInv;

        // d/dt[-4h(t²-t)] = -4h(2t-1), then multiplied by flightDurationInv (chain rule)
        float arcVerticalVelocity = -4f * m_ArcHeight * (2f * t - 1f) * m_FlightDurationInv;

        return new Vector2(baseVelocity.x, baseVelocity.y + arcVerticalVelocity);
    }
}