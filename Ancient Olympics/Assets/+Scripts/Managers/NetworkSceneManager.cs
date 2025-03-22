using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSceneManager : Singleton<NetworkSceneManager>
{
    /// <summary>
    /// Handles the scene management and waits for completion.
    /// Only the server should trigger scene changes.
    /// </summary>
    public void ChangeScene(string scene, Action onSceneLoaded = null)
    {
        if (!IsServer || string.IsNullOrEmpty(scene))
            return; // Ensure only the server executes scene changes

        var status = NetworkManager.Singleton.SceneManager.LoadScene(scene, LoadSceneMode.Single);

        if (status == SceneEventProgressStatus.Started)
        {
            Debug.Log($"Scene change started: {scene}");

            // Only the server listens for the load event
            NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleSceneEvent;

            void HandleSceneEvent(SceneEvent sceneEventArgs)
            {
                if (sceneEventArgs.SceneName == scene && sceneEventArgs.SceneEventType == SceneEventType.LoadComplete)
                {
                    Debug.Log($"Scene loaded: {scene}");
                    onSceneLoaded?.Invoke();  // Notify when fully loaded

                    // Unsubscribe to prevent multiple calls
                    NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleSceneEvent;
                }
            }
        }
        else
        {
            Debug.LogWarning($"Failed to load {scene} with status: {status}");
        }
    }
}
