using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class Entity : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!string.IsNullOrEmpty(GameManager.CurrentScene))
        {
            var status = NetworkManager.SceneManager.LoadScene(GameManager.CurrentScene, LoadSceneMode.Single);

            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to load {GameManager.CurrentScene} " +
                      $"with a {nameof(SceneEventProgressStatus)}: {status}");
            }
        }
    }
}
