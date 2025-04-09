using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkAccount : Singleton<NetworkAccount>
{
    public static string Username { get; private set; }

    [SerializeField] TMP_InputField ifName;
    [SerializeField] Button nameButton;

    public void RegisterMenuItems(MenuItems menuItems)
    {
        ifName = menuItems.ifName;
        nameButton = menuItems.nameButton;

        nameButton.onClick.AddListener(SetUsername);
    }

    void SetUsername()
    {
        if (!string.IsNullOrEmpty(ifName.text))
        {
            Username = ifName.text;
            ifName.transform.parent.gameObject.SetActive(false);
        }
    }
}
