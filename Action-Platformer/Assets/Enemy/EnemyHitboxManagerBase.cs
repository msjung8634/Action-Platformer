using System.Collections.Generic;
using UnityEngine;

public class EnemyHitboxManagerBase : MonoBehaviour
{
    [Header("Attack Hitboxes")]
    [SerializeField] LayerMask _attackTargetLayer;
    [Space(10)]

    HashSet<IDamageable> _hitTargetCache = new();
    public void ClearCachedTarget()
    {
        _hitTargetCache.Clear();
    }

    public void SetFacingDirection(UnitMoveDirection direction)
    {
        switch (direction)
        {
            case UnitMoveDirection.Right:
                transform.localScale = Vector3.one;
                break;
            case UnitMoveDirection.Left:
                transform.localScale = new Vector3(-1, 1, 1);
                break;
        }
    }

    protected void CheckHit(Transform center, Vector2 size, AttackData attackData, out bool isSuccess)
    {
        Collider2D[] enemyHitboxes = Physics2D.OverlapBoxAll(center.position, size, 0f, _attackTargetLayer);
        if (enemyHitboxes.Length == 0)
        {
            isSuccess = false;
            return;
        }
        isSuccess = true;

        foreach (Collider2D hitbox in enemyHitboxes)
        {
            // IDamagable만 검출
            if (!hitbox.TryGetComponent<IDamageable>(out var target))
                continue;

            // SingleHit는 이미 적중한 상황이면 skip
            if (!attackData.AllowMultiHit && !_hitTargetCache.Add(target))
                continue;

            Vector2 hitPoint = hitbox.ClosestPoint(center.position);
            Vector2 hitDirection = (hitbox.transform.position - transform.position).normalized;

            var hitData = new HitData
            {
                Attacker = transform.root.gameObject,
                Target = target,
                HitPoint = hitPoint,
                HitDirection = hitDirection,
                AttackData = attackData
            };

            CombatManager.Instance.ProcessHit(hitData);
        }
    }
}
