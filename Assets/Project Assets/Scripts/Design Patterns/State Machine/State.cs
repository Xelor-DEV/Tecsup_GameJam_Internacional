using UnityEngine;

public abstract class State : MonoBehaviour
{
    [Header("State Configuration")]
    [SerializeField] private string stateName;

    protected StateMachine stateMachine;
    public string StateName => stateName;
    public StateMachine StateMachine => stateMachine;

    public virtual void Initialize(StateMachine machine)
    {
        stateMachine = machine;

        if (string.IsNullOrEmpty(stateName))
        {
            stateName = GetType().Name;
            Debug.LogWarning($"State on {gameObject.name} has no name assigned. Using: {stateName}");
        }
    }

    public virtual void Enter()
    {
        enabled = true;
    }

    public virtual void Execute()
    {

    }

    public virtual void Exit()
    {
        enabled = false;
    }

    public virtual bool ValidateState()
    {
        return !string.IsNullOrEmpty(stateName);
    }
}