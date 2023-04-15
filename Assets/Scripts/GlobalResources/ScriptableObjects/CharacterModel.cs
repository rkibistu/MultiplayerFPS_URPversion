using UnityEngine;

/// <summary>
/// ScriptableObject
/// 
/// Ar trebui sa exista un scriptableObject pentru fiecare harta diferita:
/// 
/// Folosit: in Resources exista un vector de toate obiectele de genul acesta
/// </summary>

[CreateAssetMenu(fileName = "New Character", menuName = "Scriptable Object/Character Model")]
public class CharacterModel : ScriptableObject
{
    public string characterName; 
    public Sprite characterIcon;
    public GameObject characterPrefab;
}
