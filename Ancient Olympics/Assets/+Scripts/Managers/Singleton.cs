using Unity.Netcode;
using UnityEngine;

public class Singleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    public static Singleton<T> Instance { get; private set; }

    protected virtual void Awake()
    {
        Debug.Log($"Awake called on {typeof(T)}: {gameObject.name}");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Destroying duplicate instance of {typeof(T)} on {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
