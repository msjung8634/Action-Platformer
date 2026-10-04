using UnityEngine;

public class DodgeState : IUnitState
{
    public void OnEnter(UnitStateMachine _stateMachine)
    {
        _stateMachine.SetMoveState(Move.State.NonMovable);
        _stateMachine.SetAttackState(Attack.State.Attackable);
        _stateMachine.SetHitState(Hit.State.NonHittable);
    }

    public void OnExit(UnitStateMachine _stateMachine)
    {

    }

    public void OnUpdate(UnitStateMachine _stateMachine)
    {

    }
}
