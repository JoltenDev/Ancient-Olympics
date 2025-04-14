using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class NetworkLobbyManager : Singleton<NetworkLobbyManager>
{
    [Header("UI Elements")]
    [SerializeField] TMP_InputField ifCode;
    [SerializeField] Button hostButton;
    [SerializeField] Button connectButton;

    [SerializeField] GameObject playerPrefab;

    public void RegisterMenuItems(MenuItems menuItems)
    {
        ifCode = menuItems.ifCode;
        hostButton = menuItems.hostButton;
        connectButton = menuItems.connectButton;

        hostButton.onClick.AddListener(StartHost);
        connectButton.onClick.AddListener(StartClient);
    }

    void StartHost()
    {
        NetworkRelay.Instance.CreateRelay();
    }

    void StartClient()
    {
        NetworkRelay.Instance.JoinRelay(ifCode.text);
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    void OnClientConnected(ulong id)
    {
        if (!IsServer) return;

        // Spawn player only on the server
        GameObject player = Instantiate(playerPrefab, new Vector3(0, 2, 0), Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);
        DontDestroyOnLoad(player);

        UIManager.Instance.UpdateLobbyUI(NetworkManager.ConnectedClients.Count);

        StartCoroutine(SendUsernameUIUpdates());
    }

    void OnClientDisconnected(ulong id)
    {
        if (id == NetworkManager.Singleton.LocalClientId)
        {
            UIManager.Instance.DestroyLobbyMenu();

            if (hostButton != null)
                hostButton.onClick.RemoveAllListeners();

            if (connectButton != null)
                connectButton.onClick.RemoveAllListeners();

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

            SceneManager.LoadScene("Scene_MainMenu");
        }

        if (!IsServer) return;

        if (GameManager.Instance.CurrentState == GameManager.Instance.States.GameLobbyState())
            UIManager.Instance.UpdateLobbyUI(NetworkManager.ConnectedClients.Count);
    }

    public void LeaveServer()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("Scene_MainMenu");
    }

    IEnumerator SendUsernameUIUpdates()
    {
        yield return new WaitForSeconds(1);
        foreach (var id in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            var player_ = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponentInParent<Player>();
            player_.SetPlayerUIRpc(player_.Username);
        }
    }
}
