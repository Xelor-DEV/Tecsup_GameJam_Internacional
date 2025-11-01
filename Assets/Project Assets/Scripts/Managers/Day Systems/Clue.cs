using UnityEngine;

[CreateAssetMenu(fileName = "Clue", menuName = "Game/Clue")]
public class Clue : ScriptableObject
{
    public string clueName;
    public bool isFound;
    public string description;

    public void MarkAsFound()
    {
        isFound = true;
        Debug.Log($"Pista encontrada: {clueName}");
    }
}