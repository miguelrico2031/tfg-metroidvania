using System;
using UnityEngine;

[Serializable]
public class DataReference<T> where T : class
{
    public T Value => m_Scriptable as T;

    [SerializeField] private ScriptableObject m_Scriptable;
}

