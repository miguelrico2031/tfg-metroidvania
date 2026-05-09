using System;
public enum PersistentData
{
    None = 0,
    ActiveCheckpointLevel,
    PlayerHeals,
    CheckpointUnlocked,
}

public interface IPersistence
{
    public event Action OnSave;
    public event Action OnLoad;
    public void Save();
    public void Load();
    public void SetEntry(PersistentData key, string value);
    public void SetEntry(PersistentData key, string keySuffix, string value);
    public void ClearEntry(PersistentData key, string keySuffix = null);
    public void ClearAllEntries();
    public bool TryGetEntry(PersistentData key, out string value);
    public bool TryGetEntry(PersistentData key, string keySuffix, out string value);
}