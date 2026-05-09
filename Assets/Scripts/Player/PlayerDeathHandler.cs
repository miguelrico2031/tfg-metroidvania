using System;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/PlayerDeathHandler")]
public class PlayerDeathHandler : ScriptableObject
{
    [SerializeField] private float m_RespawnDelay;
    [SerializeField] private LevelLoader m_LevelLoader;
    [SerializeField] private DataReference<IPersistence> m_Persistence;
    [SerializeField] private string m_RespawnLevelFallback;

    public async void HandleDeadPlayer(GameObject player)
    {
        await Task.Delay(Mathf.CeilToInt(1000 * m_RespawnDelay));
        Entrypoint.ClearActiveEntrypoint();
        if (!m_Persistence.Value.TryGetEntry(PersistentData.ActiveCheckpointLevel, out string respawnLevel))
        {
            respawnLevel = m_RespawnLevelFallback;
        }
        m_LevelLoader.LoadLevel(respawnLevel);
    }
}

