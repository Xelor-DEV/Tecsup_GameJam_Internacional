using UnityEngine;

[CreateAssetMenu(fileName = "New Waves Combat", menuName = "Combat/Waves Combat Data")]
public class WavesCombatData : ScriptableObject
{
    public WaveCombat[] waves;
}

[System.Serializable]
public class WaveCombat
{
    public string waveName;
    public bool completed;
    public ItemType requiredItem;
}