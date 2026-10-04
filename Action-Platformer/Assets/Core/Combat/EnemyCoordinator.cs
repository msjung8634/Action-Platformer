using System.Collections.Generic;
using UnityEngine;

public enum EnemyAttackType
{
    Melee,
    Ranged
}

public class EnemyCoordinator : Singleton<EnemyCoordinator>
{
    [Header("Lock")]
    [SerializeField, Min(1)] int _maxMeleeLocks = 1;
    [SerializeField, Min(1)] int _maxRangedLocks = 1;
    [SerializeField, Min(0.1f)] float _lockInterval = 0.4f;

    readonly HashSet<EnemyBrain> _meleeEnemySet = new();
    readonly HashSet<EnemyBrain> _rangedEnemySet = new();
    float _nextLockTime;

    public bool TryAcquireLock(EnemyBrain requester, EnemyAttackType attackType)
    {
        if (requester == null)
            return false;

        // 반환하지 못하고 파괴된 객체 정리
        _meleeEnemySet.RemoveWhere(owner => owner == null);
        _rangedEnemySet.RemoveWhere(owner => owner == null);

        // 같은 적이 중복으로 Lock을 획득하지 못하도록 처리
        if (HasLock(requester))
            return false;

        if (Time.time < _nextLockTime)
            return false;

        HashSet<EnemyBrain> owners;
        int maxLocks;

        switch (attackType)
        {
            case EnemyAttackType.Melee:
                owners = _meleeEnemySet;
                maxLocks = _maxMeleeLocks;
                break;

            case EnemyAttackType.Ranged:
                owners = _rangedEnemySet;
                maxLocks = _maxRangedLocks;
                break;

            default:
                return false;
        }

        if (owners.Count >= maxLocks)
            return false;

        owners.Add(requester);
        _nextLockTime = Time.time + _lockInterval;
        return true;
    }

    public bool HasLock(EnemyBrain requester)
    {
        return requester != null &&
               (_meleeEnemySet.Contains(requester) ||
                _rangedEnemySet.Contains(requester));
    }

    public void ReleaseLock(EnemyBrain requester)
    {
        // 이미 반환한 요청도 안전하게 무시
        _meleeEnemySet.Remove(requester);
        _rangedEnemySet.Remove(requester);
    }
}
