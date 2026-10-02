using UnityEngine;

public class Attack : MonoBehaviour
{
    public enum State
    {
        None,
        Attackable,
        NonAttackable,
    }

    [field:SerializeField] public State CurrentState { get; private set; }

    public void SetState(State newState)
    {
        CurrentState = newState;
    }
}
