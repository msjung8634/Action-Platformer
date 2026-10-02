using UnityEngine;

[RequireComponent(typeof(Move))]
[RequireComponent(typeof(Attack))]
[RequireComponent(typeof(Hit))]
public class UnitStateMachine : MonoBehaviour
{
    public Move Move { get; private set; }
    public Attack Attack { get; private set; }
    public Hit Hit { get; private set; }

    public IUnitState CurrentState { get; private set; }

    void Awake()
    {
        Move = GetComponent<Move>();
        Attack = GetComponent<Attack>();
        Hit = GetComponent<Hit>();
    }

    void Start()
    {
        SetState(new NormalState());
    }

    void Update()
    {
        if (CurrentState == null)
            return;

        CurrentState.OnUpdate(this);
    }

    public void SetState(IUnitState newState)
    {
        // 초기화
        if (CurrentState == null)
        {
            CurrentState = newState;
            CurrentState.OnEnter(this);
            return;
        }

        // 중복
        if (CurrentState.GetType() == newState.GetType())
            return;

        CurrentState.OnExit(this);
        CurrentState = newState;
        CurrentState.OnEnter(this);
    }

    public void SetMoveState(Move.State newState)
    {
        Move.SetState(newState);
    }

    public void SetAttackState(Attack.State newState)
    {
        Attack.SetState(newState);
    }

    public void SetHitState(Hit.State newState)
    {
        Hit.SetState(newState);
    }
}
