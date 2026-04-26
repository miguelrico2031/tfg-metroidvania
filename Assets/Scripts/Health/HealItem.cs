using UnityEngine;

public class HealItem : MonoBehaviour
{
    public void PickUp()
    {
        Destroy(gameObject);
    }
}