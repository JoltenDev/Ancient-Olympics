using System.Net.Sockets;
using System.Net;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkLobbyManager : Singleton<MonoBehaviour>
{
    [Header("Texts")]
    [SerializeField] TMP_Text t_Identifier;
    [SerializeField] TMP_Text t_IPAddress;

    [Header("Buttons")]
    [SerializeField] Button hostButton;
    [SerializeField] Button connectButton;

    [Header("Input Field")]
    [SerializeField] TMP_InputField if_IPAddress;

    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        hostButton.onClick.AddListener(delegate
        {
            SetHostIP();
            NetworkManager.Singleton.StartHost();
        });

        connectButton.onClick.AddListener(delegate 
        {
            SetTransportIP();
            NetworkManager.Singleton.StartClient();
        });

        Debug.Log("Creating instance of Network Lobby");
    }

    void SetHostIP()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            string hostIP = GetLocalIPAddress();

            transport.ConnectionData.Address = hostIP;
            transport.ConnectionData.Port = (ushort)Random.Range(7777, 7999);
        }
    }

    void SetTransportIP()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null) 
        {
            string ip = string.IsNullOrEmpty(if_IPAddress.text) ? "127.0.0.1" : if_IPAddress.text;
            transport.ConnectionData.Address = ip;
        }
    }

    string GetLocalIPAddress()
    {
        foreach (var netInterface in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (netInterface.AddressFamily == AddressFamily.InterNetwork)
                return netInterface.ToString();
        }

        return "127.0.0.1";
    }

    void OnClientConnected(ulong clientId)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (NetworkManager.Singleton.IsHost)
        {
            t_Identifier.text = "Host";
            t_IPAddress.text = $"{transport.ConnectionData.Address}:{transport.ConnectionData.Port}";
        }
        else if (NetworkManager.Singleton.IsClient)
            t_Identifier.text = "Client";
    }
}
