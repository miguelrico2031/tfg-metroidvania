using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(Collider2D))]
public class Hitbox : MonoBehaviour
{
    [field: SerializeField] public Mode HitboxMode { get; private set; }
    [field: SerializeField, FormerlySerializedAs("m_AttackData")] public AttackData AttackData { get; set; }

    public event Action<Hurtbox, AttackResult> OnAttackPerformed;

    [SerializeField] private float m_PersistentAttackCooldown;
    [SerializeField] private FactionsData m_FactionsData;

    [Header("Attack Sparks (leave fields empty if no sparks")]
    [SerializeField] private LevelServiceLocator m_LevelServiceLocator;
    [SerializeField] private Transform m_SparksPosition;

    private readonly HashSet<IAttackTarget> m_AttackedThisActivation = new();
    private readonly Dictionary<IAttackTarget, float> m_TargetsOnCooldown = new();
    private readonly Dictionary<IAttackTarget, float> m_UpdatedTargetsOnCooldown = new();
    private readonly Collider2D[] m_Overlaps = new Collider2D[10];
    private Collider2D m_Collider;
    private Rigidbody2D m_Rigidbody;

    public bool CanAttack(Faction targetFaction) => m_FactionsData.IsHostileTo(AttackData.Faction, targetFaction);
    private void Awake()
    {
        m_Collider = GetComponent<Collider2D>();
        m_Collider.isTrigger = true;
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        m_Collider.enabled = true;
        if (HitboxMode is Mode.OneShot)
        {
            CheckTriggerOverlaps();
        }
    }

    private void OnDisable()
    {
        m_Collider.enabled = false;
        m_AttackedThisActivation.Clear();
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

    private void OnDrawGizmosSelected()
    {
        if (m_Collider == null)
        {
            m_Collider = GetComponent<Collider2D>();
        }
        Gizmos.color = new Color32(5, 10, 255, 150);
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
        if (!enabled ||
            other.attachedRigidbody == m_Collider.attachedRigidbody ||
            !other.TryGetComponent<Hurtbox>(out var hurtbox) ||
            !hurtbox.IsActive ||
            !hurtbox.AttackTarget.IsAlive ||
            HasJustAttackedTarget(hurtbox.AttackTarget) ||
            !CanAttack(hurtbox.AttackTarget.Faction))
            return;

        AttackData attackData = AttackData;
        attackData.Position = transform.position;
        attackData.Velocity = m_Rigidbody != null ? m_Rigidbody.linearVelocity : Vector2.zero;
        AttackResult result = hurtbox.AttackTarget.ResolveAttack(attackData);
        OnAttackPerformed?.Invoke(hurtbox, result);

        if (HitboxMode is Mode.OneShot)
        {
            m_AttackedThisActivation.Add(hurtbox.AttackTarget);
        }
        else if (HitboxMode is Mode.Persistent)
        {
            m_TargetsOnCooldown.Add(hurtbox.AttackTarget, m_PersistentAttackCooldown);
        }

        if(m_LevelServiceLocator != null && m_SparksPosition != null)
        {
            m_LevelServiceLocator.TryGetService<IObjectPoolService>(out var poolService);
            GameObject sparks = poolService.GetAttackHitSparksPoolContainer().Get();
            sparks.transform.SetPositionAndRotation(m_SparksPosition.position, m_SparksPosition.rotation);
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