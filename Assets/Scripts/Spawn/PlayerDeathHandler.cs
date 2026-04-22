using System;
using UnityEngine;

public interface IPlayerDeathHandler
{
    public event Action OnPlayerDead;
    public void HandleDeadPlayer(GameObject player);
}

[CreateAssetMenu(menuName = "ScriptableObjects/PlayerDeathHandler")]
public class PlayerDeathHandler : ScriptableObject, IPlayerDeathHandler
{
    public event Action OnPlayerDead;

    [SerializeField] private float m_RespawnDelay;
    [SerializeField] private Timeout m_TimeoutPrefab;
    [SerializeField] private DataReference<IPlayerSpawner> m_PlayerSpawner;

    public void HandleDeadPlayer(GameObject player)
    {
        OnPlayerDead?.Invoke();
        Timeout timeout = Instantiate(m_TimeoutPrefab);
        timeout.StartTimeout(m_RespawnDelay, () =>
        {
            Destroy(player);
            m_PlayerSpawner.Value.SpawnPlayer();
        });
    }
}

