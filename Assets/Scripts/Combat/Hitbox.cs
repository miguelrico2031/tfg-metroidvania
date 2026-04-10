using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class Hitbox : MonoBehaviour, IAttackSource
{
    public Vector2 Position => transform.position;
    public event Action<Attack, AttackResult> OnAttack;
    public bool IsActive { get; private set; } = true;
    [field: SerializeField] public Mode HitboxMode { get; private set; }
    [field: SerializeField] public Faction Faction { get; private set; }

    [SerializeField] private float m_PersistentAttackCooldown;
    [SerializeField] private FactionsData m_FactionsData;
    [SerializeField] private Attack m_Attack;

    private Collider2D m_Collider;
    private readonly HashSet<IAttackTarget> m_AttackedThisActivation = new();
    private readonly Dictionary<IAttackTarget, float> m_TargetsOnCooldown = new();
    private readonly Dictionary<IAttackTarget, float> m_UpdatedTargetsOnCooldown = new();
    private readonly Collider2D[] m_Overlaps = new Collider2D[10];

    public void SetActive(bool active)
    {
        IsActive = active;
        m_Collider.enabled = active;
        if (!active)
        {
            m_AttackedThisActivation.Clear();
        }
        else if (HitboxMode is Mode.OneShot)
        {
            CheckTriggerOverlaps();
        }
    }

    private void Awake()
    {
        m_Attack.Source = this;

        m_Collider = GetComponent<Collider2D>();
        m_Collider.isTrigger = true;
        if (HitboxMode is Mode.OneShot)
        {
            SetActive(false);
        }
    }

    private void Update()
    {
        if (m_TargetsOnCooldown.Any())
        {
            foreach (var (key, value) in m_TargetsOnCooldown)
            {
                m_UpdatedTargetsOnCooldown[key] = value - Time.deltaTime;
            }
            foreach (var (key, value) in m_UpdatedTargetsOnCooldown)
            {
                if (value > 0)
                {
                    m_TargetsOnCooldown[key] = value;
                }
                else
                {
                    m_TargetsOnCooldown.Remove(key);
                }
            }
            m_UpdatedTargetsOnCooldown.Clear();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (HitboxMode is Mode.OneShot)
        {
            TryAttack(other);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (HitboxMode is Mode.Persistent)
        {
            TryAttack(other);
        }
    }

    private void OnDrawGizmos()
    {
        if (m_Collider == null)
        {
            m_Collider = GetComponent<Collider2D>();
        }
        Gizmos.color = IsActive ? new Color32(5, 10, 255, 200) : new Color32(5, 10, 255, 100);
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

    private void CheckTriggerOverlaps()
    {
        ContactFilter2D filter = new() { layerMask = LayerMask.GetMask("Hit") };
        int size = m_Collider.Overlap(filter, m_Overlaps);
        for (int i = 0; i < size; i++)
        {
            TryAttack(m_Overlaps[i]);
        }
    }

    private void TryAttack(Collider2D other)
    {
        if (!IsActive ||
            other.transform.root == transform.root ||
            !other.TryGetComponent<Hurtbox>(out var hurtbox) ||
            !hurtbox.IsActive ||
            HasJustAttackedTarget(hurtbox.AttackTarget) ||
            !m_FactionsData.CanDamage(Faction, hurtbox.AttackTarget.Faction))
            return;

        AttackResult result = hurtbox.AttackTarget.ResolveAttack(m_Attack);
        OnAttack?.Invoke(m_Attack, result);

        if (HitboxMode is Mode.OneShot)
        {
            m_AttackedThisActivation.Add(hurtbox.AttackTarget);
        }
        else if (HitboxMode is Mode.Persistent)
        {
            m_TargetsOnCooldown.Add(hurtbox.AttackTarget, m_PersistentAttackCooldown);
        }
    }

    private bool HasJustAttackedTarget(IAttackTarget target) => HitboxMode switch
    {
        Mode.OneShot => m_AttackedThisActivation.Contains(target),
        Mode.Persistent => m_TargetsOnCooldown.ContainsKey(target),
        _ => false
    };

    public enum Mode
    {
        OneShot,
        Persistent
    }
}