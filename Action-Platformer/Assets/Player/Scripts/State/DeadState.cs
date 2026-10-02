using UnityEngine;

public class DeadState : IUnitState
{
    public void OnEnter(UnitStateMachine _stateMachine)
    {
        _stateMachine.SetMoveState(Move.State.NonMovable);
        _stateMachine.SetAttackState(Attack.State.NonAttackable);
        _stateMachine.SetHitState(Hit.State.NonHittable);
    }

    public void OnExit(UnitStateMachine _stateMachine)
    {
        
    }

    public void OnUpdate(UnitStateMachine _stateMachine)
    {

    }
}
