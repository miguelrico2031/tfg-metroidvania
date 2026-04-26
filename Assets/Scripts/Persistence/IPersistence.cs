using System;
public enum PersistentData
{
    None = 0,
    ActiveCheckpoint,
    PlayerHeals,
}

public interface IPersistence
{
    public event Action OnSave;
    public event Action OnLoad;
    public void Save();
    public void Load();
    public void SetEntry(PersistentData key, string value);
    public void ClearEntry(PersistentData key);
    public void ClearAllEntries();
    public bool TryGetEntry(PersistentData key, out string value);
}