using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkRelay : Singleton<NetworkRelay>
{
    string code = "";
    public string Code { get => code; }

    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string jcode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            code = jcode;

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

            NetworkManager.Singleton.StartHost();
            if (NetworkManager.Singleton.IsServer && GameManager.Instance != null)
            {
                GameManager.Instance.SwitchState(GameManager.Instance.States.GameLobbyState());
            }
        } 
        catch (RelayServiceException e) 
        {
            Debug.Log(e);
        }
    }

    public async void JoinRelay(string code)
    {
        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(code);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
            NetworkManager.Singleton.StartClient();
        } 
        catch (RelayServiceException e) 
        { 
            Debug.Log(e);
        }
    }
}
