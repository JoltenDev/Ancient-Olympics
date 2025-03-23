using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] GameObject lobbyMenuPrefab;

    public GameObject LobbyMenu { get; private set; }
    public GameObject Hud { get; set; }

    public void UpdateLobbyUI(int playerCount)
    {
        // Only update the UI if it's the host
        if (NetworkManager.Singleton.IsHost)
        {
            if (LobbyMenu == null)
            {
                LobbyMenu = Instantiate(lobbyMenuPrefab);
                LobbyMenu?.SetActive(true);  // Set the clone active, not the original one
                DontDestroyOnLoad(LobbyMenu);
            }

            string canStart = playerCount > 1 ? "#59B359" : "#F85A5D";
            LobbyMenu.GetComponentInChildren<TMP_Text>().text = $"Start <color={canStart}>[{playerCount}/4]</color>";

            LobbyMenu.GetComponentInChildren<Button>().onClick.AddListener(delegate 
            {
                if (NetworkLobbyManager.Instance.PlayerCount < 2) return;

                GameManager.Instance.SwitchState(GameManager.Instance.States.GameTransitionState());
                LobbyMenu.SetActive(false);
            });
        }
    }

    [Rpc(SendTo.Everyone)]
    public void UpdatePlayerTimersRpc(float time)
    {
        if (Hud != null)
        {
            Hud.GetComponent<HudItems>().globalMessage.GetComponentInChildren<TMP_Text>().text = $"Time Left: {time:F1}s";
        }
    }

    [Rpc(SendTo.Everyone)]
    public void ActivateHudUIRpc()
    {
        Hud.SetActive(true);
    }

    [Rpc(SendTo.Everyone)]
    public void ActivateGlobalMessageRpc()
    {
        Hud.GetComponent<HudItems>().globalMessage.SetActive(true);
    }

    [Rpc(SendTo.Everyone)]
    public void DeactivateGlobalMessageRpc()
    {
        Hud.GetComponent<HudItems>().globalMessage.SetActive(false);
    }
}
