using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private bool m_FaceMovement;

    private Rigidbody2D m_Rigidbody;

    private Vector2 m_StartPosition;
    private Vector2 m_TargetPosition;
    private float m_Speed = 10f;
    private float m_FlightDurationInv;
    private float m_ElapsedTime;
    private bool m_IsLaunched;
    private float m_ArcHeight = 3f;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();

        m_Rigidbody.gravityScale = 0f;
        m_Rigidbody.freezeRotation = true;
    }

    public void Cast(Vector2 startPosition, Vector2 targetPosition, float speed, float archeight = 0f)
    {
        m_StartPosition = startPosition;
        m_TargetPosition = targetPosition;

        float distance = Vector2.Distance(startPosition, targetPosition);
        m_FlightDurationInv =  m_Speed / distance; // 1 / flight duration

        m_ElapsedTime = 0f;
        m_IsLaunched = true;

        m_Rigidbody.position = startPosition;

        m_Speed = speed;
        m_ArcHeight = archeight;
    }


    private void FixedUpdate()
    {
        if (!m_IsLaunched) return;

        m_ElapsedTime += Time.fixedDeltaTime;
        float t = m_ElapsedTime * m_FlightDurationInv;
        Vector2 nextPosition = Vector2.LerpUnclamped(m_StartPosition, m_TargetPosition, t);

        if(m_ArcHeight > 0f)
        {
            float arcOffset = -4f * m_ArcHeight * (t * t - t);
            nextPosition = new Vector2(nextPosition.x, nextPosition.y + arcOffset);
        }

        if(m_FaceMovement)
        {
            Vector2 delta = nextPosition - m_Rigidbody.position;
            if (delta.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                m_Rigidbody.MoveRotation(angle);
            }
        }

        m_Rigidbody.MovePosition(nextPosition);
    }
}