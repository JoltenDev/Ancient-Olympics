using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class NetworkLobbyManager : Singleton<NetworkLobbyManager>
{
    [Header("UI Elements")]
    [SerializeField] TMP_InputField ifIpAddress;
    [SerializeField] Button hostButton;
    [SerializeField] Button connectButton;

    [SerializeField] GameObject playerPrefab;

    public void RegisterMenuItems(MenuItems menuItems)
    {
        ifIpAddress = menuItems.ifIpAddress;
        hostButton = menuItems.hostButton;
        connectButton = menuItems.connectButton;

        hostButton.onClick.AddListener(StartHost);
        connectButton.onClick.AddListener(StartClient);
    }

    void StartHost()
    {
        //SetHostIP();
        NetworkManager.Singleton.StartHost();

        if (NetworkManager.Singleton.IsServer && GameManager.Instance != null)
        {
            GameManager.Instance.SwitchState(GameManager.Instance.States.GameLobbyState());
        }
    }

    void StartClient()
    {
        //SetTransportIP();
        NetworkManager.Singleton.StartClient();
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

            hostButton.onClick.RemoveAllListeners();
            connectButton.onClick.RemoveAllListeners();
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

            SceneManager.LoadScene("Scene_MainMenu");
        }

        if (!IsServer) return;

        UIManager.Instance.UpdateLobbyUI(NetworkManager.ConnectedClients.Count);
    }

    void SetHostIP()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.ConnectionData.Address = GetLocalIPAddress();
            transport.ConnectionData.Port = (ushort)UnityEngine.Random.Range(7777, 7999);
        }
    }

    void SetTransportIP()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null && !string.IsNullOrEmpty(ifIpAddress.text))
        {
            string[] addressParts = ifIpAddress.text.Split(':');
            transport.ConnectionData.Address = addressParts[0];
            transport.ConnectionData.Port = (addressParts.Length > 1 && ushort.TryParse(addressParts[1], out ushort port)) ? port : (ushort)7777;
        }
    }

    public void LeaveServer()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("Scene_MainMenu");
    }

    string GetLocalIPAddress()
    {
        foreach (var netInterface in System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList)
        {
            if (netInterface.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                return netInterface.ToString();
        }
        return "127.0.0.1";
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
