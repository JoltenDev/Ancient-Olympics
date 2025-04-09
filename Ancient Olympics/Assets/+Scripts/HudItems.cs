using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudItems : MonoBehaviour
{
    public GameObject health;
    public GameObject globalMessage;
    public GameObject pauseMenu;
    public Button settingsButton;
    public Button leaveButton;

    void OnEnable()
    {
        UIManager.Instance.RegisterHudItems(this);
    }
}
