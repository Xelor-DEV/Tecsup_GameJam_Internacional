using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactersDatabase", menuName = "Game/Characters Database")]
public class CharactersDatabase : ScriptableObject
{
    public List<CharacterData> characters;
}
