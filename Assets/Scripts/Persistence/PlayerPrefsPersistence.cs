using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/PlayerPrefsPersistence")]
public class PlayerPrefsPersistence : ScriptableObject, IPersistence
{
    public event Action OnSave;
    public event Action OnLoad;

    [SerializeField] private bool m_LogSaveLoad;

    private readonly Dictionary<PersistentData, string> m_Entries = new();

    public void Save()
    {
        OnSave?.Invoke();
        foreach(var (key, value) in m_Entries)
        {
            PlayerPrefs.SetString(key.ToString(), value);
            if(m_LogSaveLoad)
            {
                Debug.Log($"[PlayerPrefsPersistence] [Save] Key: {key.ToString()}, Value : [{value}]");
            }
        }
        PlayerPrefs.Save();
    }
    public void Load()
    {
        m_Entries.Clear();
        foreach(PersistentData key in Enum.GetValues(typeof(PersistentData)))
        {
            string value = PlayerPrefs.GetString(key.ToString());
            if (!String.IsNullOrEmpty(value))
            {
                m_Entries[key] = value;
                if (m_LogSaveLoad)
                {
                    Debug.Log($"[PlayerPrefsPersistence] [Load] Key: {key.ToString()}, Value : [{value}]");
                }
            }
        }
        OnLoad?.Invoke();
    }
    public void SetEntry(PersistentData key, string value)
    {
        m_Entries[key] = value;
    }
    public void ClearEntry(PersistentData key)
    {
        m_Entries.Remove(key);
    }
    public void ClearAllEntries()
    {
        m_Entries.Clear();
    }
    public bool TryGetEntry(PersistentData key, out string value)
    {
        return m_Entries.TryGetValue(key, out value);
    }
}