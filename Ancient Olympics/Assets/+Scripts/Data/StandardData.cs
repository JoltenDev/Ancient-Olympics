using UnityEngine;

[CreateAssetMenu(fileName = "StandardData", menuName = "Scriptable Objects/StandardData")]
public class StandardData : ScriptableObject
{
    [Header("Player Fields")]
    public GameObject defaultPlayerHud;

    [Header("Cursor Textures")]
    public Texture2D defaultTexture;
    public Texture2D deathTexture;
}
