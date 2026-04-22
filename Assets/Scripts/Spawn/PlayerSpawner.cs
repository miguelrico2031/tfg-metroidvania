using System;
using UnityEngine;
public interface IPlayerSpawner
{
    public event Action OnPlayerSpawned;
    public void SpawnPlayer();
}

[CreateAssetMenu(menuName = "ScriptableObjects/PlayerSpawner")]
public class PlayerSpawner : ScriptableObject, IPlayerSpawner
{
    [SerializeField] private string m_PlayerGameObjectName;
    [SerializeField] private GameObject m_PlayerPrefab;

    public event Action OnPlayerSpawned;

    public void SpawnPlayer()
    {
        var playerRoot = Instantiate(m_PlayerPrefab);
        var playerTransform = playerRoot.transform.Find(m_PlayerGameObjectName);
        var activeCheckpoint = Checkpoint.GetActiveCheckpoint();
        playerTransform.position = activeCheckpoint.transform.position;
        OnPlayerSpawned?.Invoke();
    }
}