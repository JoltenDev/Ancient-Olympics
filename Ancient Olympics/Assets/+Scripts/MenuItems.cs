using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuItems : MonoBehaviour
{
    [Header("Items")]
    public TMP_InputField ifIpAddress;
    public TMP_InputField ifName;
    public Button hostButton;
    public Button connectButton;
    public Button nameButton;
    public Button quitButton;

    [Header("Managers")]
    [SerializeField] List<GameObject> managers = new List<GameObject>();

    void OnEnable()
    {
        foreach (GameObject manager in managers)
            Instantiate(manager);

        NetworkLobbyManager.Instance.RegisterMenuItems(this);
        NetworkAccount.Instance.RegisterMenuItems(this);
        GameManager.Instance.RegisterMenuItems(this);
    }
}
