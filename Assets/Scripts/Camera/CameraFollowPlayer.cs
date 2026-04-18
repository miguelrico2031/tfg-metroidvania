using DG.Tweening;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] private float m_FlipTime;
    [SerializeField] private PlayerMovementComponent m_PlayerMovement;

    private Transform m_Transform;
    private Transform m_PlayerTransform;

    private void OnEnable()
    {
        m_Transform = transform;
        m_PlayerTransform = m_PlayerMovement.transform;
        m_Transform.SetLocalPositionAndRotation(m_PlayerTransform.position, m_PlayerMovement.transform.rotation);
        m_PlayerMovement.OnDirectionChanged += Flip;
    }

    private void OnDisable()
    {
        m_PlayerMovement.OnDirectionChanged -= Flip;
    }

    private void FixedUpdate()
    {
        m_Transform.position = m_PlayerTransform.position;
    }

    private void Flip()
    {
        m_Transform.DORotate(new Vector3(0f, m_PlayerTransform.right.x > 0f ? 0f : 180f, 0f), m_FlipTime);
    }
}