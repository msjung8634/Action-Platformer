using UnityEngine;

public class Move : MonoBehaviour
{
    public enum State
    {
        None,
        Movable,
        NonMovable,
    }

    [field: SerializeField] public State CurrentState { get; private set; }

    public void SetState(State newState)
    {
        CurrentState = newState;
    }
}
