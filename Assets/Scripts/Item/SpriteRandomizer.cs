using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]
public class SpriteRandomizer : MonoBehaviour
{
    [SerializeField] private Sprite[] m_Sprites;

    private void OnEnable()
    {
        if (m_Sprites is { Length: > 0 })
        {
            GetComponent<SpriteRenderer>().sprite = m_Sprites[Random.Range(0, m_Sprites.Length)];
        }
    }
}