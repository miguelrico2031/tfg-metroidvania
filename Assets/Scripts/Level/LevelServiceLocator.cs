using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public interface ILevelService { }

[CreateAssetMenu(menuName ="ScriptableObjects/LevelServiceLocator")]
public class LevelServiceLocator : ScriptableObject
{

    private readonly Dictionary<Type, ILevelService> m_RegisteredServices = new();

    public void RegisterService<T>(ILevelService service) where T : class, ILevelService
    {
        m_RegisteredServices[typeof(T)] = service;
    }

    public void UnregisterService<T>(ILevelService service) where T : class, ILevelService
    {
        Assert.AreEqual(m_RegisteredServices[typeof(T)], service);
        m_RegisteredServices.Remove(typeof(T));
    }

    public bool TryGetService<T>(out T service) where T : class, ILevelService
    {
        service = null;
        if (!m_RegisteredServices.TryGetValue(typeof(T), out var iLevelService))
            return false;

        service = iLevelService as T;
        return service != null;
    }
}