using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hurtbox : MonoBehaviour
{
    public bool IsActive { get; private set; } = true;
    public IAttackTarget AttackTarget { get; private set; }

    private Collider2D m_Collider;

    public void SetAttackTarget(IAttackTarget attackTarget) => AttackTarget = attackTarget;
    public void SetActive(bool active)
    {
        IsActive = active;
        m_Collider.enabled = active;
    }

    private void Awake()
    {
        m_Collider = GetComponent<Collider2D>();
        m_Collider.isTrigger = true;
    }

    private void OnDrawGizmos()
    {
        if (m_Collider == null)
        {
            m_Collider = GetComponent<Collider2D>();
        }
        Gizmos.color = IsActive ? new Color32(255, 251, 20, 200) : new Color32(255, 251, 20, 100);
        DrawColliderGizmo();
    }
    private void DrawColliderGizmo()
    {
        if (m_Collider is BoxCollider2D box)
        {
            Matrix4x4 old = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.offset, box.size);
            Gizmos.matrix = old;
        }
        else if (m_Collider is CircleCollider2D circle)
        {
            Gizmos.DrawSphere(transform.TransformPoint(circle.offset), circle.radius * transform.lossyScale.x);
        }
    }
}