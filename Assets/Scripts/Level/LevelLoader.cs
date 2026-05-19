using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "ScriptableObjects/LevelLoader")]
public class LevelLoader : ScriptableObject
{
    [field: SerializeField] public string[] Scenes { get; private set; }
    [field: SerializeField] public float ExitLevelDelay { get; private set; }

    public event Action<string, Reason> OnLevelLoadStarted;
    public event Action OnLevelLoadCompleted;

    public async UniTask LoadLevel(string name, Reason reason)
    {
        OnLevelLoadStarted?.Invoke(name, reason);
        await UniTask.Delay(TimeSpan.FromSeconds(ExitLevelDelay), ignoreTimeScale: false);
        await SceneManager.LoadSceneAsync(name);
        OnLevelLoadCompleted?.Invoke();
    }

    public enum Reason
    {
        PlayerDied,
        DoorTriggered,
    }
}
