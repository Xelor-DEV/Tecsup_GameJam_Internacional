using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Candy", menuName = "Game/Candy")]
public class Candy : ScriptableObject
{
    public int amount;

    public void AddCandy(int value)
    {
        amount += value;
        Debug.Log($"Caramelos actualizados: {amount}");
    }
}
