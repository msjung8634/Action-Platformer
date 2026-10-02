using UnityEngine;

/// <summary>
/// UnitState 전환 시 Move,Attack,Hit 상태를 제어하기 위한 인터페이스
/// </summary>
public interface IUnitState
{
    public void OnEnter(UnitStateMachine _stateMachine);
    public void OnExit(UnitStateMachine _stateMachine);
    public void OnUpdate(UnitStateMachine _stateMachine);
}
