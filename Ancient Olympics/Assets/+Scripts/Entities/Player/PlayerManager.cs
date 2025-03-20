using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] TMP_InputField if_Name;
    [SerializeField] Button nameButton;
    public static string username;

    [SerializeField] GameObject defaultPlayerPrefab;
    [SerializeField] Texture2D defaultTexture;
    [SerializeField] GameObject lobbyMenu;

    [SerializeField] List<GameObject> playersInServer = new List<GameObject>();

    void Start()
    {
        nameButton.onClick.AddListener(delegate
        {
            if (if_Name.text != "")
            {
                username = if_Name.text;
                if_Name.transform.parent.gameObject.SetActive(false);
            }
        });

        Cursor.SetCursor(defaultTexture, Vector2.zero, CursorMode.Auto);
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SpawnPlayersInScene;
    }

    /// <summary>
    /// Spawns clients, excluding host, into the game when connected to host
    /// </summary>
    /// <param name="id"> The id of the client </param>
    void SpawnPlayer(ulong id)
    {
        if (!IsOwner) return;

        GameObject player = Instantiate(defaultPlayerPrefab, new Vector3(0, 2, 0), Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
        
        if (IsServer)
            playersInServer.Add(player);
    }

    /// <summary>
    /// Spawns every client into the provided scene
    /// </summary>
    /// <param name="sceneName"> Name of the scene to spawn clients into </param>
    /// <param name="loadSceneMode"> Scene mode </param>
    /// <param name="clientsSuccessful"> List of clients that successfully connected to host </param>
    /// <param name="clientsTimedOut"> List of clients that failed to connect to host </param>
    void SpawnPlayersInScene(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsSuccessful, List<ulong> clientsTimedOut)
    {
        if (IsServer)
        {
            playersInServer?.Clear();

            foreach (ulong id in clientsSuccessful) 
            {
                GameObject player = Instantiate(defaultPlayerPrefab, new Vector3(0, 2, 0), Quaternion.identity);
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);

                playersInServer.Add(player);
                Instantiate(lobbyMenu);
            }
        }
    }
}
