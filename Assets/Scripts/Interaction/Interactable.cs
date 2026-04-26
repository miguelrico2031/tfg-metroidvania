using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    private Collider2D m_Collider;
    private InteractorComponent m_Interactor;

    private void Awake()
    {
        m_Collider = GetComponent<Collider2D>();
        m_Collider.isTrigger = true;
    }

    private void OnEnable()
    {
        List<Collider2D> colliders = new();
        m_Collider.Overlap(colliders);        
        foreach(var collider in colliders)
        {
            if(collider != null && collider.TryGetComponent<InteractorComponent>(out var interactor))
            {
                AddInteractor(interactor);
            }
        }  
    }

    private void OnDisable()
    {
        if(m_Interactor != null)
        {
            RemoveInteractor(m_Interactor);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent<InteractorComponent>(out var interactor))
        {
            AddInteractor(interactor);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.TryGetComponent<InteractorComponent>(out var interactor))
        {
            RemoveInteractor(interactor);
        }
    }

    private void AddInteractor(InteractorComponent interactor)
    {
        if (m_Interactor != null)
            return;
        interactor.AddInteractable(this);
        m_Interactor = interactor;
    }
    
    private void RemoveInteractor(InteractorComponent interactor)
    {
        interactor.RemoveInteractable(this);
        m_Interactor = null;
    }
}
