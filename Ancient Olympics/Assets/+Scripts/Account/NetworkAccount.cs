using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkAccount : Singleton<NetworkAccount>
{
    public static string Username { get; private set; }

    [SerializeField] TMP_InputField if_Name;
    [SerializeField] Button nameButton;

    void Start()
    {
        nameButton.onClick.AddListener(SetUsername);
    }

    void SetUsername()
    {
        if (!string.IsNullOrEmpty(if_Name.text))
        {
            Username = if_Name.text;
            if_Name.transform.parent.gameObject.SetActive(false);
        }
    }
}
