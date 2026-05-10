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

    public async Task HandleDeadPlayer(GameObject player)
    {
        await Task.Delay(TimeSpan.FromSeconds(m_RespawnDelay));
        Entrypoint.ClearActiveEntrypoint();
        if (!m_Persistence.Value.TryGetEntry(PersistentData.ActiveCheckpointLevel, out string respawnLevel))
        {
            respawnLevel = m_RespawnLevelFallback;
        }
        await m_LevelLoader.LoadLevel(respawnLevel);
    }
}

