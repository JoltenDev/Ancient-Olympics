using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] GameObject lobbyMenu;
    GameObject lobbyMenuClone;

    public void UpdateLobbyUI(int playerCount)
    {
        // Only update the UI if it's the host
        if (NetworkManager.Singleton.IsHost)
        {
            if (lobbyMenuClone == null)
            {
                lobbyMenuClone = Instantiate(lobbyMenu);
                lobbyMenuClone?.SetActive(true);  // Set the clone active, not the original one
                DontDestroyOnLoad(lobbyMenuClone);
            }

            string canStart = playerCount > 1 ? "#59B359" : "#F85A5D";
            lobbyMenuClone.GetComponentInChildren<TMP_Text>().text = $"Start <color={canStart}>[{playerCount}/4]</color>";
        }
    }
}
