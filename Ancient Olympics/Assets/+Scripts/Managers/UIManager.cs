using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] GameObject lobbyMenuPrefab;

    public GameObject LobbyMenu { get; private set; }
    public GameObject Hud { get; set; }

    public void RegisterHudItems(HudItems hud)
    {
        hud.leaveButton.onClick.AddListener(() =>
        {
            NetworkLobbyManager.Instance.LeaveServer();

            if (IsHost)
            {
                LobbyMenu.SetActive(false);
            }
        });
    }

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

                LobbyMenu.GetComponentInChildren<Button>().onClick.AddListener(delegate
                {
                    if (NetworkManager.Singleton.ConnectedClients.Count < 2) return;

                    GameManager.Instance.SwitchState(GameManager.Instance.States.GameTransitionState(false));
                    LobbyMenu.SetActive(false);
                });
            }

            string canStart = playerCount > 1 ? "#59B359" : "#F85A5D";
            LobbyMenu.GetComponentInChildren<TMP_Text>().text = $"Start <color={canStart}>[{playerCount}/4]</color>";
        }
    }

    public void DestroyLobbyMenu()
    {
        Destroy(LobbyMenu);
    }

    [Rpc(SendTo.Everyone)]
    public void UpdatePlayerTimersRpc(string text)
    {
        if (Hud != null)
        {
            Hud.GetComponent<HudItems>().globalMessage.GetComponentInChildren<TMP_Text>().text = text;
        }
    }
    
    [Rpc(SendTo.Everyone)]
    public void HudUIRpc(bool status) => Hud.SetActive(status);

    [Rpc(SendTo.Everyone)]
    public void GlobalMessageRpc(bool status)
    {
        Hud.GetComponent<HudItems>().globalMessage.SetActive(status);
    }

    [Rpc(SendTo.Everyone)]
    public void HealthUIRpc(bool status)
    {
        Hud.GetComponent<HudItems>().health.SetActive(status);
    }

    public void ActivatePauseMenuRpc()
    {
        if (!Hud.GetComponent<HudItems>().pauseMenu.activeSelf)
            Hud.GetComponent<HudItems>().pauseMenu.SetActive(true);
        else
            Hud.GetComponent<HudItems>().pauseMenu.SetActive(false);
    }
}
