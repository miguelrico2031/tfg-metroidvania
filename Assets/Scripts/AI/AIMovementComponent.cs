using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AIMovementComponent : MonoBehaviour
{
    [SerializeField] private float m_MoveSpeed;

    private Transform m_Transform;
    private Rigidbody2D m_Rigidbody;

    private bool m_MoveRequested;

    public void MoveForward()
    {
        m_MoveRequested = true;
    }

    public void Turn()
    {
        m_Transform.Rotate(Vector3.up, 180f);
        m_Rigidbody.linearVelocityX = 0f;
    }

    private void Awake()
    {
        m_Transform = transform;
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        m_Rigidbody.linearVelocityX = m_MoveRequested
            ? Mathf.Sign(m_Transform.right.x) * m_MoveSpeed
            : 0f;
        m_MoveRequested = false;
    }
}