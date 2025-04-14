using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuItems : MonoBehaviour
{
    [Header("Items")]
    public TMP_InputField ifCode;
    public TMP_InputField ifName;
    public Button hostButton;
    public Button connectButton;
    public Button nameButton;
    public Button quitButton;

    [Header("Managers")]
    [SerializeField] List<GameObject> managers = new List<GameObject>();
    List<GameObject> managerClones = new List<GameObject>();

    void OnEnable()
    {
        if (managerClones.Count > 0)
        {
            foreach (GameObject clone in managerClones)
            {
                Destroy(clone);
            }
        }
        managerClones.Clear();

        foreach (GameObject manager in managers)
        {
            var clone = Instantiate(manager);
            managerClones.Add(clone);
        }

        NetworkLobbyManager.Instance.RegisterMenuItems(this);
        NetworkAccount.Instance.RegisterMenuItems(this);
        GameManager.Instance.RegisterMenuItems(this);
    }
}
