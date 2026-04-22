
public enum PersistentData
{
    None = 0,
    ActiveCheckpoint,
}

public interface ISerializable
{
    public string Serialize();
    public void Deserialize(string serializedData);
}

public interface IPersistence
{
    public void Save(PersistentData key, object value);
    public void Clear(PersistentData key);
    public void ClearAll();
    public bool Load<T>(PersistentData key, out T output, T fallback = default);
}