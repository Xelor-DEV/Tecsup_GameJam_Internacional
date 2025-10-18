using UnityEngine;
using UnityEngine.Events;

public enum Entity
{
    Player,
    Enemy,
    Item,
    None
}

public class EntityIdentifier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform pivot;

    [Header("Settings")]
    [SerializeField] private Entity entity = Entity.None;
    [SerializeField] private bool isTargetable = true;

    [Header("Events")]
    public UnityEvent<bool> OnTargetableStatusChanged;

    public void SetTargetable(bool targetable)
    {
        if (isTargetable != targetable)
        {
            isTargetable = targetable;
            OnTargetableStatusChanged?.Invoke(isTargetable);
        }
    }

    public Entity Entity
    {
        get
        {
            return entity;
        }
        set
        {
            entity = value;
        }
    }
    public bool IsTargetable
    {
        get
        {
            return isTargetable;
        }
        set
        {
            isTargetable = value;
        }
    }

    public Transform Pivot => pivot;
}