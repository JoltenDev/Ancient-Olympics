using System.Net.Sockets;
using System.Net;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkLobbyManager : Singleton<NetworkLobbyManager>
{
    [Header("Buttons")]
    [SerializeField] Button hostButton;
    [SerializeField] Button connectButton;

    [Header("Input Field")]
    [SerializeField] TMP_InputField if_IPAddress;

    void Start()
    {
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
    }

    /// <summary>
    /// Sets host's connection IP and Port to their local IP, auto generates Port
    /// </summary>
    void SetHostIP()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            string hostIP = GetLocalIPAddress();

            //transport.ConnectionData.Address = hostIP;
            //transport.ConnectionData.Port = (ushort)Random.Range(7777, 7999);
        }
    }

    /// <summary>
    /// Retrieves entered IP and Port from client
    /// </summary>
    void SetTransportIP()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null && if_IPAddress.text != "")
        {
            string[] addressParts = if_IPAddress.text.Split(':');
            string ip = addressParts[0];
            ushort port = 7777;

            if (addressParts.Length > 1)
            {
                if (ushort.TryParse(addressParts[1], out ushort parsedPort))
                {
                    port = parsedPort;
                }
                else
                {
                    Debug.LogWarning("Invalid port provided, using default 7777.");
                }
            }

            transport.ConnectionData.Address = ip;
            transport.ConnectionData.Port = port;
        }
    }

    /// <summary>
    /// Retrieves local IP Address of host
    /// </summary>
    /// <returns> "127.0.0.1" if not found </returns>
    string GetLocalIPAddress()
    {
        foreach (var netInterface in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (netInterface.AddressFamily == AddressFamily.InterNetwork)
                return netInterface.ToString();
        }

        return "127.0.0.1";
    }
}
