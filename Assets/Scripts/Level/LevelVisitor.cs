using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelVisitor : MonoBehaviour
{
    [SerializeField] private DataReference<IPersistence> m_Persistence;

    private void Start()
    {
        string levelName = SceneManager.GetActiveScene().name;
        if(!m_Persistence.Value.TryGetEntry(PersistentData.LevelVisited, levelName, out _))
        {
            m_Persistence.Value.SetEntry(PersistentData.LevelVisited, levelName, "1");
            m_Persistence.Value.Save();
        }
    }
}