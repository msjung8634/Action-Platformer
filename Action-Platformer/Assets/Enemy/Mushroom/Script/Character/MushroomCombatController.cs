using UnityEngine;

public class MushroomCombatController : EnemyCombatControllerBase
{
    enum AttackType
    {
        None,
        Attack1,
        Attack2,
    }

    [Header("MeleeAttack")]
    [SerializeField, Min(0f)] float _attackRange = .6f;
    [SerializeField, Min(0f)] float _attackHeightTolerance = 1f;

    [Space(10)]
    [SerializeField] AttackType _selectedAttack;

    public override bool TrySelectAttack(Transform target, out EnemyAttackType attackType)
    {
        // Mushroom은 근접 공격만 사용
        attackType = EnemyAttackType.Melee;

        if (target == null || !CanAttack)
            return false;

        Vector2 offset = target.position - transform.position;

        // 공격 거리 밖이면 Move
        if (Mathf.Abs(offset.x) > _attackRange)
            return false;

        // 높이 차이가 크면 Move
        if (Mathf.Abs(offset.y) > _attackHeightTolerance)
            return false;

        // 공격 조건 충족 시, Brain에서 Coordinator에 Lock 요청
        _selectedAttack = (AttackType)Random.Range(1, 3);
        return true;
    }

    #region Common

    protected override void BeginRecovery()
    {
        base.BeginRecovery();
    }

    public override void BeginLock(Transform target)
    {
        base.BeginLock(target);

        switch (_selectedAttack)
        {
            case AttackType.Attack1:
                break;
            case AttackType.Attack2:
                break;
        }
    }

    public override void BeginAttack()
    {
        base.BeginAttack();

        switch (_selectedAttack)
        {
            case AttackType.Attack1:
                StartAttack1();
                break;
            case AttackType.Attack2:
                StartAttack2();
                break;
        }
    }

    protected override void EndAttack()
    {
        base.EndAttack();

        switch (_selectedAttack)
        {
            case AttackType.Attack1:
                break;
            case AttackType.Attack2:
                break;
        }
        _selectedAttack = AttackType.None;
    }

    public override void CancelAttack()
    {
        base.CancelAttack();

        switch (_selectedAttack)
        {
            case AttackType.Attack1:
                break;
            case AttackType.Attack2:
                break;
        }
        _selectedAttack = AttackType.None;
    }

    #endregion

    #region Attack1

    void StartAttack1()
    {
        var visual = _visual as MushroomVisual;
        visual.PlayAttack1();
        Debug.Log("Start Attack1");
    }

    void AnimEvent_CheckAttack1Hit()
    {
        var hitboxManager = _hitboxManager as MushroomHitboxManager;
        hitboxManager.CheckAttack1();
    }

    void AnimEvent_Attack1End()
    {
        BeginRecovery();
    }

    #endregion
    #region Attack2

    void StartAttack2()
    {
        var visual = _visual as MushroomVisual;
        visual.PlayAttack2();
        Debug.Log("Start Attack2");
    }

    void AnimEvent_CheckAttack2Hit()
    {
        var hitboxManager = _hitboxManager as MushroomHitboxManager;
        hitboxManager.CheckAttack2();
    }

    void AnimEvent_Attack2End()
    {
        BeginRecovery();
    }

    #endregion
    #region Die

    // 죽기전에 폭발하며 포자 날리기~

    #endregion

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!TryGetComponent<EnemyMoveControllerBase>(out var moveController))
            return;

        float stopDistance = moveController.StopDistance;

        if (stopDistance >= _attackRange)
        {
            Debug.LogError($"[{name}] moveController.StopDistance < combatController.AttackRange을 만족하게 설정하세요");
            _attackRange = stopDistance + 0.01f;
        }
    }
#endif
}
