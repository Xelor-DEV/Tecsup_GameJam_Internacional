using UnityEngine;
using System.Collections.Generic;

public class StateMachine : MonoBehaviour
{
    [Header("State Configuration")]
    [SerializeField] private string initialStateName;

    [Header("Debug")]
    [SerializeField] protected State currentState;
    [SerializeField] private bool logStateTransitions = true;

    protected Dictionary<string, State> states = new Dictionary<string, State>();

    public State CurrentState => currentState;
    public string CurrentStateName => currentState?.StateName ?? "No State";

    protected virtual void Start()
    {
        RegisterAllStates();

        if (!string.IsNullOrEmpty(initialStateName))
        {
            ChangeState(initialStateName);
        }
        else
        {
            Debug.LogError("No initial state name assigned!");
        }
    }

    protected virtual void Update()
    {
        currentState?.Execute();
    }

    public virtual void ChangeState(string stateName)
    {
        if (!states.ContainsKey(stateName))
        {
            Debug.LogError($"State '{stateName}' not found! Available states: {string.Join(", ", states.Keys)}");
            return;
        }

        State newState = states[stateName];

        // Si ya estamos en este estado, no hacer nada
        if (currentState == newState) return;

        // Salir del estado actual
        currentState?.Exit();

        // Cambiar al nuevo estado
        currentState = newState;
        currentState.Enter();

        if (logStateTransitions)
        {
            Debug.Log($"State changed: {currentState.StateName} on {gameObject.name}");
        }
    }

    public virtual bool RegisterState(State state)
    {
        if (state == null || !state.ValidateState())
        {
            Debug.LogError($"Invalid state component on {gameObject.name}");
            return false;
        }

        string stateName = state.StateName;

        if (states.ContainsKey(stateName))
        {
            Debug.LogError($"State '{stateName}' is already registered on {gameObject.name}!");
            return false;
        }

        states.Add(stateName, state);
        state.Initialize(this);

        // Desactivar el estado inicialmente
        state.enabled = false;

        return true;
    }

    protected virtual void RegisterAllStates()
    {
        State[] foundStates = GetComponents<State>();

        if (foundStates.Length == 0)
        {
            Debug.LogWarning($"No states found on {gameObject.name}!");
            return;
        }

        foreach (State state in foundStates)
        {
            RegisterState(state);
        }

        Debug.Log($"Registered {states.Count} states on {gameObject.name}: {string.Join(", ", states.Keys)}");
    }

    public bool IsCurrentState(string stateName)
    {
        return currentState?.StateName == stateName;
    }

    public bool HasState(string stateName)
    {
        return states.ContainsKey(stateName);
    }

    public IReadOnlyCollection<string> GetAvailableStates()
    {
        return states.Keys;
    }
}