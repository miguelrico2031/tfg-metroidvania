using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(menuName = "ScriptableObjects/PlayerPrefsPersistence")]
public class PlayerPrefsPersistence : ScriptableObject, IPersistence
{
    [SerializeField] private bool m_LogOperation;

    public void Save(PersistentData key, object value)
    {
        string keyStr = key.ToString();
        string valueStr = "";
        if(value is float f)
        {
            PlayerPrefs.SetFloat(keyStr, f);
            valueStr = f.ToString();
        }
        else if(value is int i)
        {
            PlayerPrefs.SetInt(keyStr, i);
            valueStr = i.ToString();

        }
        else if(value is string s)
        {
            PlayerPrefs.SetString(keyStr,s);
            valueStr = s;
        }
        else if(value is ISerializable sz)
        {
            valueStr = sz.Serialize();
            PlayerPrefs.SetString(keyStr, valueStr);
        }
        else
        {
            Assert.IsNotNull(null, "Unable to serialize data.");
        }
        PlayerPrefs.Save();
        Log(key, valueStr, true);
    }

    public void Clear(PersistentData key)
    {
        PlayerPrefs.DeleteKey(key.ToString());
        PlayerPrefs.Save();
        Log("Cleared", key.ToString());
    }

    public void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Log("Cleared", "All keys");
    }

    public bool Load<T>(PersistentData key, out T output, T fallback = default)
    {
        string keyStr = key.ToString();
        string valueStr = "";
        output = fallback;
        if (!PlayerPrefs.HasKey(keyStr))
            return false;

        if (typeof(T) == typeof(int))
        {
            output =  (T)(object)PlayerPrefs.GetInt(keyStr, (int)(object)fallback);
            valueStr = output.ToString();
        }
        else if (typeof(T) == typeof(float))
        {
            output = (T)(object)PlayerPrefs.GetFloat(keyStr, (float)(object)fallback);
            valueStr = output.ToString();
        }
        else if (typeof(T) == typeof(string))
        {
            output = (T)(object)PlayerPrefs.GetString(keyStr, (string)(object)fallback);
            valueStr = output.ToString();
        }
        else if (typeof(ISerializable).IsAssignableFrom(typeof(T)))
        {
            valueStr = PlayerPrefs.GetString(keyStr);
            (output as ISerializable).Deserialize(valueStr);
        }
        Log(key, valueStr, false);
        return true;
    }

    private void Log(PersistentData key, string value, bool save) =>
        Log(save ? "Saved" : "Loaded", $"Key: {key}, Value: {value}");

    private void Log(string operation, string message)
    {
        if(m_LogOperation)
        {
            Debug.Log($"[PlayerPrefsPersistence] [{operation}] {message}");
        }
    }
}