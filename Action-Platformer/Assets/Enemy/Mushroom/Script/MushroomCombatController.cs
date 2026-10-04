using UnityEngine;

public class MushroomCombatController : EnemyCombatControllerBase
{
    enum MushroomAttackType
    {
        None,
        Attack1,
        Attack2,
        Attack3,
    }

    [Header("MeleeAttack")]
    [SerializeField, Min(0f)] float _attackRange = 1f;
    [SerializeField, Min(0f)] float _attackHeightTolerance = 1f;
    [SerializeField] MushroomAttackType _selectedAttack;

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

        // 공격 조건 충족 → Brain에서 Coordinator에 Lock 요청
        _selectedAttack = (MushroomAttackType)Random.Range(1, 4);
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
            case MushroomAttackType.Attack1:
                break;
            case MushroomAttackType.Attack2:
                break;
            case MushroomAttackType.Attack3:
                break;
        }
    }

    public override void BeginAttack()
    {
        base.BeginAttack();

        switch (_selectedAttack)
        {
            case MushroomAttackType.Attack1:
                StartAttack1();
                break;
            case MushroomAttackType.Attack2:
                StartAttack2();
                break;
            case MushroomAttackType.Attack3:
                StartAttack3();
                break;
        }
    }

    protected override void EndAttack()
    {
        base.EndAttack();

        switch (_selectedAttack)
        {
            case MushroomAttackType.Attack1:
                break;
            case MushroomAttackType.Attack2:
                break;
            case MushroomAttackType.Attack3:
                break;
        }
        _selectedAttack = MushroomAttackType.None;
    }

    public override void CancelAttack()
    {
        base.CancelAttack();

        switch (_selectedAttack)
        {
            case MushroomAttackType.Attack1:
                break;
            case MushroomAttackType.Attack2:
                break;
            case MushroomAttackType.Attack3:
                break;
        }
        _selectedAttack = MushroomAttackType.None;
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
    #region Attack3

    void StartAttack3()
    {
        var visual = _visual as MushroomVisual;
        visual.PlayAttack3();
        Debug.Log("Start Attack3");
    }

    void AnimEvent_CheckAttack3Hit()
    {
        var hitboxManager = _hitboxManager as MushroomHitboxManager;
        hitboxManager.CheckAttack3();
    }

    void AnimEvent_Attack3End()
    {
        BeginRecovery();
    }

    #endregion

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!TryGetComponent<MushroomMoveController>(out var moveController))
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
