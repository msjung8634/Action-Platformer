using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerHitboxManager : MonoBehaviour
{
    [Header("Attack Hitboxes")]
    [SerializeField] LayerMask _attackTargetLayer;
    [Space(10)]
    [SerializeField] Transform _gAttack0_Center;
    [SerializeField] Vector2 _gAttack0_Size = new(0.1f, 0.8f);
    [SerializeField] AttackData _gAttack0Data;
    [Space(10)]
    [SerializeField] Transform _gAttack1_Center;
    [SerializeField] Vector2 _gAttack1_Size = new(0.1f, 0.8f);
    [SerializeField] AttackData _gAttack1Data;
    [Space(10)]
    [SerializeField] Transform _gAttack2_Center;
    [SerializeField] Vector2 _gAttack2_Size = new(0.1f, 0.8f);
    [SerializeField] AttackData _gAttack2Data;

    public void SetFacingDirection(PlayerInput.MoveDirection direction)
    {
        switch (direction)
        {
            case PlayerInput.MoveDirection.Right:
                transform.localScale = Vector3.one;
                break;
            case PlayerInput.MoveDirection.Left:
                transform.localScale = new Vector3(-1, 1, 1);
                break;
        }
    }

    #region GroundAttack

    public void CheckGroundAttack0()
    {
        CheckGroundAttack(_gAttack0_Center, _gAttack0_Size, _gAttack0Data);
    }

    public void CheckGroundAttack1()
    {
        CheckGroundAttack(_gAttack1_Center, _gAttack1_Size, _gAttack1Data);
    }

    public void CheckGroundAttack2()
    {
        CheckGroundAttack(_gAttack2_Center, _gAttack2_Size, _gAttack2Data);
    }

    HashSet<IDamageable> _hitTargets = new();

    public void BeginAttack()
    {
        _hitTargets.Clear();
    }

    void CheckGroundAttack(Transform center, Vector2 size, AttackData attackData)
    {
        Collider2D[] enemyHitboxes = Physics2D.OverlapBoxAll(center.position, size, 0f, _attackTargetLayer);
        if (enemyHitboxes.Length == 0)
            return;

        foreach (Collider2D hitbox in enemyHitboxes)
        {
            // IDamagable만 검출
            if (!hitbox.TryGetComponent<IDamageable>(out var target))
                continue;

            // 이미 적중한 상황이면 skip
            if (!_hitTargets.Add(target))
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
            Debug.Log($"{hitData}");
        }
    }

    #endregion

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Selection.activeTransform == null)
            return;

        Transform selected = Selection.activeTransform;

        if (selected == transform)
        {
            DrawHitbox(_gAttack0_Center, _gAttack0_Size);
            DrawHitbox(_gAttack1_Center, _gAttack1_Size);
            DrawHitbox(_gAttack2_Center, _gAttack2_Size);
            return;
        }

        if (_gAttack0_Center != null &&
            Selection.Contains(_gAttack0_Center.gameObject))
        {
            DrawHitbox(_gAttack0_Center, _gAttack0_Size);
        }

        if (_gAttack1_Center != null &&
            Selection.Contains(_gAttack1_Center.gameObject))
        {
            DrawHitbox(_gAttack1_Center, _gAttack1_Size);
        }

        if (_gAttack2_Center != null &&
            Selection.Contains(_gAttack2_Center.gameObject))
        {
            DrawHitbox(_gAttack2_Center, _gAttack2_Size);
        }
    }

    void DrawHitbox(Transform center, Vector2 size)
    {
        if (center == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center.position, size);
    }
#endif
}
