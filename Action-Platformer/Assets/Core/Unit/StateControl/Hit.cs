using UnityEngine;

public class Hit : MonoBehaviour
{
    public enum State
    {
        None,
        Hittable,
        NonHittable,
    }

    [field: SerializeField] public State CurrentState { get; private set; }

    public void SetState(State newState)
    {
        CurrentState = newState;
    }
}
