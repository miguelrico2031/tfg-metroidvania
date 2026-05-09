using UnityEngine;

[RequireComponent (typeof(Interactable))]
public class HealItem : MonoBehaviour
{
    private Interactable m_Interactable;

    private void OnEnable()
    {
        m_Interactable = GetComponent<Interactable>();
        m_Interactable.OnInteract += PickUp;
    }

    private void PickUp()
    {
        m_Interactable.OnInteract -= PickUp;
        Destroy(gameObject, Time.deltaTime);
    }
}