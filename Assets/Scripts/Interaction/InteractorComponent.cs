using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractorComponent : MonoBehaviour
{
    public Interactable ClosestInteractable => m_ClosestInteractableComputedThisFrame ? m_ClosestInteractable : ComputeClosestInteractable();

    private readonly List<Interactable> m_Interactables = new();
    private Interactable m_ClosestInteractable;
    private bool m_ClosestInteractableComputedThisFrame;

    public void AddInteractable(Interactable interactable) => m_Interactables.Add(interactable);
    
    public void RemoveInteractable(Interactable interactable) => m_Interactables.Remove(interactable);


    private void LateUpdate()
    {
        m_ClosestInteractableComputedThisFrame = false;
    }
    private Interactable ComputeClosestInteractable()
    {
        m_ClosestInteractableComputedThisFrame = true;
        if(m_Interactables.Count <= 1)
        {
            m_ClosestInteractable = m_Interactables.FirstOrDefault();
            return m_ClosestInteractable;
        }
        Vector2 position = transform.position;
        Func<Interactable, float> distance = i => Vector2.Distance(position, i.transform.position);
        m_ClosestInteractable = m_Interactables.Aggregate((min, current) => distance(current) < distance(min) ? current : min);
        return m_ClosestInteractable;
    }
}
