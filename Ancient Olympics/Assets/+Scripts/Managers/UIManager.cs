using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] GameObject lobbyMenuPrefab;
    [SerializeField] GameObject leaveMenuPrefab;

    public GameObject LobbyMenu { get; private set; }
    public GameObject LeaveMenu { get; private set; }
    public GameObject Hud { get; set; }

    public void RegisterHudItems(HudItems hud)
    {
        hud.leaveButton.onClick.AddListener(() =>
        {
            NetworkLobbyManager.Instance.LeaveServer();

            if (IsHost && LobbyMenu != null)
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
            CreateLobbyMenu();

            string canStart = playerCount > 1 ? "#59B359" : "#F85A5D";
            TMP_Text[] texts = LobbyMenu.GetComponentsInChildren<TMP_Text>();

            texts[0].text = NetworkRelay.Instance.Code;
            texts[1].text = $"Start <color={canStart}>[{playerCount}/4]</color>";
        }
    }

    [Rpc(SendTo.Everyone)]
    public void DisplayLeaveUIRpc()
    {
        if (!IsHost) return;
        
        if (LeaveMenu == null)
        {
            LeaveMenu = Instantiate(leaveMenuPrefab);
            LeaveMenu?.SetActive(true);  // Set the clone active, not the original one
            DontDestroyOnLoad(LeaveMenu);

            LeaveMenu.GetComponentInChildren<Button>().onClick.AddListener(delegate
            {
                GameManager.Instance.SwitchState(GameManager.Instance.States.GameLobbyState());
                Destroy(LeaveMenu);

                UpdateLobbyUI(NetworkManager.ConnectedClients.Count);
            });
        }
    }

    void CreateLobbyMenu()
    {
        if (LobbyMenu == null)
        {
            LobbyMenu = Instantiate(lobbyMenuPrefab);
            LobbyMenu?.SetActive(true);  // Set the clone active, not the original one
            DontDestroyOnLoad(LobbyMenu);

            LobbyMenu.GetComponentsInChildren<Button>()[0].onClick.AddListener(delegate
            {
                CopyCode();
            });

            LobbyMenu.GetComponentsInChildren<Button>()[1].onClick.AddListener(delegate
            {
                if (NetworkManager.Singleton.ConnectedClients.Count < 2) return;

                GameManager.Instance.SwitchState(GameManager.Instance.States.GameTransitionState(false));
                Destroy(LobbyMenu);
            });
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

    public void CopyCode() => GUIUtility.systemCopyBuffer = NetworkRelay.Instance.Code;
}
