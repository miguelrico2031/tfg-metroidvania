using UnityEngine;

public class ScenePersistenceLoader : MonoBehaviour
{
    [SerializeField] private DataReference<IPersistence> m_Persistence;

    //This script has negative execution order so it runs before all other scripts in frame
    //This allows all scripts to initialize their internal data using loaded data in their Awake methods
    private void Awake()
    {
        m_Persistence.Value.Load();
    }
}