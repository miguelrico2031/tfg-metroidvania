using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent (typeof(Collider2D))]
public class AIBounds : MonoBehaviour
{
    public event Action<Collider2D> OnIntruderEnterBounds;
    public event Action<Collider2D> OnIntruderLeaveBounds;
    public readonly HashSet<Collider2D> IntrudersInsideBounds = new();

    [SerializeField] private LayerMask m_IntruderLayers;

    private Collider2D m_Collider;

    private void Awake()
    {
        m_Collider = GetComponent<Collider2D>();
        m_Collider.isTrigger = true;
    }

    private void Start()
    {
        var results = new Collider2D[10];
        int count = m_Collider.Overlap(new ContactFilter2D() { layerMask = m_IntruderLayers }, results);
        for (int i = 0; i < count; i++)
        {
            IntrudersInsideBounds.Add(results[i]);
            OnIntruderEnterBounds?.Invoke(results[i]);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((m_IntruderLayers & (1 << other.gameObject.layer)) != 0)
        {
            IntrudersInsideBounds.Add(other);
            OnIntruderEnterBounds?.Invoke(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if ((m_IntruderLayers & (1 << other.gameObject.layer)) != 0)
        {
            IntrudersInsideBounds.Remove(other);
            OnIntruderLeaveBounds?.Invoke(other);
        }
    }
}