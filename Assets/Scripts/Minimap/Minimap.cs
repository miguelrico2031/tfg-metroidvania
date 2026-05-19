using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class Minimap : MonoBehaviour
{
    [SerializeField] private Grid m_MinimapGrid;
    [SerializeField] private InputActionReference m_ToggleMinimapAction;
    [SerializeField] private DataReference<IPersistence> m_Persistence;

    private GameObject[] m_Levels;

    private void OnEnable()
    {
        m_ToggleMinimapAction.action.performed += ToggleMinimap;
    }

    private void OnDisable()
    {
        m_ToggleMinimapAction.action.performed -= ToggleMinimap;
    }

    private void Start()
    {
        var tilemaps = m_MinimapGrid.GetComponentsInChildren<Tilemap>(includeInactive: true);
        m_Levels = new GameObject[tilemaps.Length];
        for (int i = 0; i < tilemaps.Length; i++)
        {
            m_Levels[i] = tilemaps[i].gameObject;

        }
        m_MinimapGrid.gameObject.SetActive(false);
    }

    private void ToggleMinimap(InputAction.CallbackContext context)
    {
        m_MinimapGrid.gameObject.SetActive(!m_MinimapGrid.gameObject.activeSelf);
        if (m_MinimapGrid.gameObject.activeSelf)
        {
            foreach (GameObject level in m_Levels)
            {
                level.SetActive(m_Persistence.Value.TryGetEntry(PersistentData.LevelVisited, level.name, out _));
            }
        }
    }
}