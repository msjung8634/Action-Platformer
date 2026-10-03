using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerHitboxManager : MonoBehaviour
{
    [Header("Attack Hitboxes")]
    [SerializeField] LayerMask _attackTargetLayer;
    [Space(10)]

    [Header("Combo Attack")]
    [SerializeField] Transform _comboAttack1_Center;
    [SerializeField] Vector2 _comboAttack1_Size = new(0.6f, 0.8f);
    [SerializeField] AttackData _comboAttack1_Data;
    [Space(10)]
    [SerializeField] Transform _comboAttack2_Center;
    [SerializeField] Vector2 _comboAttack2_Size = new(0.74f, 1f);
    [SerializeField] AttackData _comboAttack2_Data;
    [Space(10)]
    [SerializeField] Transform _comboAttack3_Center;
    [SerializeField] Vector2 _comboAttack3_Size = new(1.6f, 0.6f);
    [SerializeField] AttackData _comboAttack3_Data;

    [Header("InAir Attack")]
    [SerializeField] Transform _inAirAttack_Center;
    [SerializeField] Vector2 _inAirAttack_Size = new(0.1f, 0.8f);
    [SerializeField] AttackData _inAirAttack_Data;

    [Header("Dash Attack")]
    [SerializeField] Transform _dashAttack_Center;
    [SerializeField] Vector2 _dashAttack_Size = new(0.1f, 0.8f);
    [SerializeField] AttackData _dashAttack_Data;

    HashSet<IDamageable> _hitTargetCache = new();
    public void ClearCachedTarget()
    {
        _hitTargetCache.Clear();
    }

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

    void CheckAttackHit(Transform center, Vector2 size, AttackData attackData)
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
            if (!_hitTargetCache.Add(target))
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

    #region ComboAttack

    public void CheckComboAttack1()
    {
        CheckAttackHit(_comboAttack1_Center, _comboAttack1_Size, _comboAttack1_Data);
    }

    public void CheckComboAttack2()
    {
        CheckAttackHit(_comboAttack2_Center, _comboAttack2_Size, _comboAttack2_Data);
    }

    public void CheckComboAttack3()
    {
        CheckAttackHit(_comboAttack3_Center, _comboAttack3_Size, _comboAttack3_Data);
    }

    #endregion

    #region DashAttack

    public void CheckDashAttack()
    {
        CheckAttackHit(_dashAttack_Center, _dashAttack_Size, _dashAttack_Data);
    }

    #endregion

    #region InAirAttack

    public void CheckInAirAttack()
    {
        CheckAttackHit(_inAirAttack_Center, _inAirAttack_Size, _inAirAttack_Data);
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
            DrawHitbox(_comboAttack1_Center, _comboAttack1_Size);
            DrawHitbox(_comboAttack2_Center, _comboAttack2_Size);
            DrawHitbox(_comboAttack3_Center, _comboAttack3_Size);
            DrawHitbox(_inAirAttack_Center, _inAirAttack_Size);
            DrawHitbox(_dashAttack_Center, _dashAttack_Size);
            return;
        }

        if (_comboAttack1_Center != null &&
            Selection.Contains(_comboAttack1_Center.gameObject))
        {
            DrawHitbox(_comboAttack1_Center, _comboAttack1_Size);
        }

        if (_comboAttack2_Center != null &&
            Selection.Contains(_comboAttack2_Center.gameObject))
        {
            DrawHitbox(_comboAttack2_Center, _comboAttack2_Size);
        }

        if (_comboAttack3_Center != null &&
            Selection.Contains(_comboAttack3_Center.gameObject))
        {
            DrawHitbox(_comboAttack3_Center, _comboAttack3_Size);
        }

        if (_inAirAttack_Center != null &&
            Selection.Contains(_inAirAttack_Center.gameObject))
        {
            DrawHitbox(_inAirAttack_Center, _inAirAttack_Size);
        }

        if (_dashAttack_Center != null &&
            Selection.Contains(_dashAttack_Center.gameObject))
        {
            DrawHitbox(_dashAttack_Center, _dashAttack_Size);
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
