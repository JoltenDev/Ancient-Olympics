using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NetworkLobbyManager : Singleton<NetworkLobbyManager>
{
    [Header("UI Elements")]
    [SerializeField] TMP_InputField if_IPAddress;
    [SerializeField] Button hostButton;
    [SerializeField] Button connectButton;

    [SerializeField] GameObject playerPrefab;

    public Dictionary<ulong, GameObject> playersInServer = new Dictionary<ulong, GameObject>();
    public int PlayerCount { get => playersInServer.Count; }
    public static event Action<int> OnPlayerCountChanged;

    void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        connectButton.onClick.AddListener(StartClient);
    }

    void StartHost()
    {
        //SetHostIP();
        NetworkManager.Singleton.StartHost();

        if (NetworkManager.Singleton.IsServer)
        {
            GameManager.Instance.SwitchState(GameManager.Instance.States.GameLobbyState());
            OnPlayerCountChanged += UIManager.Instance.UpdateLobbyUI;
            UpdatePlayerCount();  // Initial update when host starts
        }
    }

    void StartClient()
    {
        //SetTransportIP();
        NetworkManager.Singleton.StartClient();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public override void OnDestroy()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        // Spawn player only on the server
        GameObject player = Instantiate(playerPrefab, new Vector3(0, 2, 0), Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        playersInServer[clientId] = player;
        DontDestroyOnLoad(player);

        UpdatePlayerCount(); // Update player count
    }

    void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        if (playersInServer.TryGetValue(clientId, out GameObject player))
        {
            Destroy(player);
            playersInServer.Remove(clientId);
        }

        UpdatePlayerCount();
    }
    
    void UpdatePlayerCount()
    {
        OnPlayerCountChanged?.Invoke(PlayerCount);
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
        if (transport != null && !string.IsNullOrEmpty(if_IPAddress.text))
        {
            string[] addressParts = if_IPAddress.text.Split(':');
            transport.ConnectionData.Address = addressParts[0];
            transport.ConnectionData.Port = (addressParts.Length > 1 && ushort.TryParse(addressParts[1], out ushort port)) ? port : (ushort)7777;
        }
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
}
