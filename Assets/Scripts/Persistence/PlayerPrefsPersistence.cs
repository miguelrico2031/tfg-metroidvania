using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(menuName = "ScriptableObjects/PlayerPrefsPersistence")]
public class PlayerPrefsPersistence : ScriptableObject, IPersistence
{
    public event Action OnSave;
    public event Action OnLoad;

    [SerializeField] private bool m_LogSaveLoad;

    private readonly Dictionary<PersistentData, string> m_Entries = new();
    private readonly Dictionary<string, string> m_EntriesWithSuffixes = new();

    private const string c_SuffixEntry = "SuffixKeys";

    public void Save()
    {
        OnSave?.Invoke();
        PlayerPrefs.DeleteAll();
        foreach (var (key, value) in m_Entries)
        {
            PlayerPrefs.SetString(key.ToString(), value);
            if (m_LogSaveLoad)
            {
                Debug.Log($"[PlayerPrefsPersistence] [Save] Key: {key.ToString()}, Value : [{value}]");
            }
        }
        foreach (var (key, value) in m_EntriesWithSuffixes)
        {
            PlayerPrefs.SetString(key, value);
            if (m_LogSaveLoad)
            {
                Debug.Log($"[PlayerPrefsPersistence] [Save] Key: {key.ToString()}, Value : [{value}]");
            }
        }
        if(m_EntriesWithSuffixes.Count > 0)
        {
            string suffixKeys = string.Join(";", m_EntriesWithSuffixes.Keys);
            PlayerPrefs.SetString(c_SuffixEntry, suffixKeys);
            if (m_LogSaveLoad)
            {
                Debug.Log($"[PlayerPrefsPersistence] [Save] Key: {c_SuffixEntry}, Value : [{suffixKeys}]");
            }
        }
        PlayerPrefs.Save();
    }
    public void Load()
    {
        m_Entries.Clear();
        foreach (PersistentData key in Enum.GetValues(typeof(PersistentData)))
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

        if(PlayerPrefs.HasKey(c_SuffixEntry))
        {
            string suffixKeys = PlayerPrefs.GetString(c_SuffixEntry);
            foreach(string key in suffixKeys.Split(';'))
            {
                string value = PlayerPrefs.GetString(key);
                if (!String.IsNullOrEmpty(value))
                {
                    m_EntriesWithSuffixes[key] = value;
                    if (m_LogSaveLoad)
                    {
                        Debug.Log($"[PlayerPrefsPersistence] [Load] Key: {key}, Value : [{value}]");
                    }
                }
            }
        }

        OnLoad?.Invoke();
    }
    public void SetEntry(PersistentData key, string value)
    {
        m_Entries[key] = value;
    }

    public void SetEntry(PersistentData key, string keySuffix, string value)
    {
        m_EntriesWithSuffixes[$"{key}_{keySuffix}"] = value;
    }

    public void ClearEntry(PersistentData key, string keySuffix = null)
    {
        if(String.IsNullOrEmpty(keySuffix))
        {
            m_Entries.Remove(key);
        }
        else
        {
            m_EntriesWithSuffixes.Remove($"{key}_{keySuffix}");
        }
    }

    public void ClearAllEntries()
    {
        m_Entries.Clear();
        m_EntriesWithSuffixes.Clear();
    }
    public bool TryGetEntry(PersistentData key, out string value)
    {
        return m_Entries.TryGetValue(key, out value);
    }

    public bool TryGetEntry(PersistentData key, string keySuffix, out string value)
    {
        return m_EntriesWithSuffixes.TryGetValue($"{key}_{keySuffix}", out value);
    }
}