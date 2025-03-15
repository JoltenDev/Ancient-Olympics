using Unity.Netcode;
using UnityEngine;

public class GameManager : Singleton
{
    public void ApplicationQuit() => Application.Quit();

    void OnApplicationQuit()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
