using UnityEngine;

public class NecromancerCombatController : EnemyCombatControllerBase
{
    enum AttackType
    {
        None,
        MagicMissile,
    }

    [Header("RangedAttack")]
    [SerializeField, Min(0f)] float _attackRange = 3f;
    [SerializeField, Min(0f)] float _attackHeightTolerance = 3f;

    [Header("MagicMissile")]
    [SerializeField] Transform _spawnPosition;
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] AttackData _projectileData;
    [SerializeField] LayerMask _targetLayer;
    [SerializeField] float _flySpeed;

    [Space(10)]
    [SerializeField] AttackType _selectedAttack;

    public override bool TrySelectAttack(Transform target, out EnemyAttackType attackType)
    {
        // Mushroom은 근접 공격만 사용
        attackType = EnemyAttackType.Ranged;

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
        _selectedAttack = (AttackType)Random.Range(1, 2);
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
            case AttackType.MagicMissile:
                break;
        }
    }

    public override void BeginAttack()
    {
        base.BeginAttack();

        switch (_selectedAttack)
        {
            case AttackType.MagicMissile:
                StartMagicMissile();
                break;
        }
    }

    protected override void EndAttack()
    {
        base.EndAttack();

        switch (_selectedAttack)
        {
            case AttackType.MagicMissile:
                break;
        }
        _selectedAttack = AttackType.None;
    }

    public override void CancelAttack()
    {
        base.CancelAttack();

        switch (_selectedAttack)
        {
            case AttackType.MagicMissile:
                break;
        }
        _selectedAttack = AttackType.None;
    }

    #endregion

    #region MagicMissile

    void StartMagicMissile()
    {
        var visual = _visual as NecromancerVisual;
        visual.PlayMagicMissile();
    }

    void AnimEvent_SpawnMagicMissile()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        DamageReceiver receiver = player.GetComponentInChildren<DamageReceiver>();
        Vector2 targetPosition = receiver.AimPoint.position;
        Vector2 spawnPoint = _spawnPosition.position;

        GameObject projectileObj = Instantiate(_projectilePrefab, spawnPoint, Quaternion.identity);
        if (projectileObj.TryGetComponent(out ProjectileController controller))
        {
            controller.Initialize(
                caster: transform,
                targetPosition: targetPosition,
                targetLayerMask: _targetLayer,
                speed: _flySpeed,
                onHit: OnFireballHit,
                ignoreYOffset: false
            );
        }
    }

    void OnFireballHit(ProjectileController projectile, Collider2D hitbox)
    {
        if (!hitbox.TryGetComponent<IDamageable>(out var target))
            return;

        Vector2 hitPoint = hitbox.ClosestPoint(projectile.transform.position);
        var hitData = new HitData
        {
            Attacker = projectile.Owner.root.gameObject,
            Target = target,
            HitPoint = hitPoint,
            HitDirection = projectile.FlyDirection,
            AttackData = _projectileData
        };

        CombatManager.Instance.ProcessHit(hitData);
        Debug.Log($"{hitData}");
    }

    void AnimEvent_MagicMissileEnd()
    {
        BeginRecovery();
    }

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
