using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/PlayerDeathHandler")]
public class PlayerDeathHandler : ScriptableObject
{
    public event Action OnPlayerDead;

    [SerializeField] private float m_RespawnDelay;
    [SerializeField] private Timeout m_TimeoutPrefab;
    [SerializeField] private PlayerSpawner m_PlayerSpawner;

    public void HandleDeadPlayer(GameObject player)
    {
        OnPlayerDead?.Invoke();
        Timeout timeout = Instantiate(m_TimeoutPrefab);
        timeout.StartTimeout(m_RespawnDelay, () =>
        {
            Destroy(player);
            m_PlayerSpawner.SpawnPlayer();
        });
    }
}

