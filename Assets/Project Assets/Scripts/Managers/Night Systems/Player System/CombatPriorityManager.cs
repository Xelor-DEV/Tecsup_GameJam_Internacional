using UnityEngine;

[System.Serializable]
public class AttackPriority
{
    [Header("Priority Settings")]
    public int attackPriority = 0; 
    public bool hasHyperArmor = false;
    public bool canBeInterrupted = true;
}

public class CombatPriorityManager : NonPersistentSingleton<CombatPriorityManager>
{
    public bool CheckAttackPriority(AttackPriority attacker, AttackPriority defender, out bool attackerWins)
    {
        attackerWins = false;

        if (defender.hasHyperArmor)
        {
            attackerWins = false;
            return true;
        }

        if (attacker.hasHyperArmor)
        {
            attackerWins = true;
            return true;
        }

        if (attacker.attackPriority > defender.attackPriority)
        {
            attackerWins = true;
            return true;
        }
        else if (attacker.attackPriority < defender.attackPriority)
        {
            attackerWins = false;
            return true;
        }

        attackerWins = false;
        return false;
    }
}