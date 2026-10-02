using UnityEngine;

public class NormalState : IUnitState
{
    public void OnEnter(UnitStateMachine _stateMachine)
    {
        _stateMachine.SetMoveState(Move.State.Movable);
        _stateMachine.SetAttackState(Attack.State.Attackable);
        _stateMachine.SetHitState(Hit.State.Hittable);
    }

    public void OnExit(UnitStateMachine _stateMachine)
    {
        
    }

    public void OnUpdate(UnitStateMachine _stateMachine)
    {
        
    }
}
