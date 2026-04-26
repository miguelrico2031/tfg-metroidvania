using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/PlayerSpawner")]
public class PlayerSpawner : ScriptableObject
{
    [SerializeField] private string m_PlayerGameObjectName;
    [SerializeField] private GameObject m_PlayerPrefab;

    public event Action OnPlayerSpawned;

    public void SpawnPlayer(Transform existingPlayerTransform = null)
    {
        if (existingPlayerTransform == null)
        {
            var playerRoot = Instantiate(m_PlayerPrefab);
            existingPlayerTransform = playerRoot.transform.Find(m_PlayerGameObjectName);
        }
        if (Checkpoint.TryGetActiveCheckpoint(out var activeCheckpoint))
        {
            existingPlayerTransform.position = activeCheckpoint.transform.position;
        }
        OnPlayerSpawned?.Invoke();
    }
}