// NightMapConfig.cs
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Config/NightMap Config")]
public class NightMapConfig : ScriptableObject
{
    [Header("Current Day")]
    public int currentDay = 0;

    [Header("Day-based Zones")]
    public DayZoneConfiguration[] dayZoneConfigurations;

    [Header("Timer")]
    public float timerDuration = 180;
}

[System.Serializable]
public class DayZoneConfiguration
{
    public int day;
    public ZoneState[] zoneStates;
}

[System.Serializable]
public class ZoneDefinition
{
    public string zoneName;
    public GameObject[] zoneObjects;
}

[System.Serializable]
public class ZoneState
{
    public string zoneName;
    public bool state;
}