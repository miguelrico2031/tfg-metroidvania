using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private DataReference<IPersistence> m_Persistence;

    private static Checkpoint s_LevelCheckpoint;

    public static bool GetActiveLevelCheckpoint(out Checkpoint checkpoint)
    {
        checkpoint = s_LevelCheckpoint;
        return checkpoint != null &&
            checkpoint.m_Persistence.Value.TryGetEntry(PersistentData.ActiveCheckpointLevel, out string level) &&
            level == SceneManager.GetActiveScene().name;
    }

    public void SetAsActiveCheckpoint()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        m_Persistence.Value.SetEntry(PersistentData.ActiveCheckpointLevel, sceneName);
        m_Persistence.Value.SetEntry(PersistentData.CheckpointUnlocked, sceneName, "1");
        m_Persistence.Value.Save();
    }

    public bool IsCheckpointUnlocked()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return m_Persistence.Value.TryGetEntry(PersistentData.CheckpointUnlocked, sceneName, out _);
    }

    public bool IsCheckpointActive()
    {
        return GetActiveLevelCheckpoint(out var checkpoint) && checkpoint == this;
    }

    private void Awake()
    {
        s_LevelCheckpoint = this;
    }
}