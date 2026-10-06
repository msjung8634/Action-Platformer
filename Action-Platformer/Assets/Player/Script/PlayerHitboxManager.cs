using System;
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

    [Header("InAir - UpSlash Attack")]
    [SerializeField] Transform _upSlashAttack_Center;
    [SerializeField] Vector2 _upSlashAttack_Size = new(0.1f, 0.8f);
    [SerializeField] AttackData _upSlashAttack_Data;

    [Header("InAir - DownSlash Attack")]
    [SerializeField] Transform _downSlashAttack_Center;
    [SerializeField] Vector2 _downSlashAttack_Size = new(0.1f, 0.8f);
    [SerializeField] AttackData _downSlashAttack_Data;

    [Header("Dash Attack")]
    [SerializeField] Transform _dashAttack_Center;
    [SerializeField] Vector2 _dashAttack_Size = new(0.1f, 0.8f);
    [SerializeField] AttackData _dashAttack_Data;

    [Header("FireBreath Attack")]
    [SerializeField] Transform _fireBreath_Center;
    [SerializeField] Vector2 _fireBreath_Size = new(0.1f, 0.8f);
    [SerializeField] AttackData _fireBreath_Data;

    void OnEnable()
    {
        if (WaveManager.Instance == null) return;
        WaveManager.Instance.OnRestartGame += OnRestartGame;
    }

    void OnDisable()
    {
        if (WaveManager.Instance == null) return;
        WaveManager.Instance.OnRestartGame -= OnRestartGame;
    }

    void OnRestartGame()
    {
        ClearCachedTarget();
        SetFacingDirection(UnitMoveDirection.Right);
    }

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

    void CheckHit(Transform center, Vector2 size, AttackData attackData, out bool isSuccess)
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

    #region ComboAttack

    public void CheckComboAttack1()
    {
        CheckHit(_comboAttack1_Center, _comboAttack1_Size, _comboAttack1_Data, out bool isSuccess);
    }

    public void CheckComboAttack2()
    {
        CheckHit(_comboAttack2_Center, _comboAttack2_Size, _comboAttack2_Data, out bool isSuccess);
    }

    public void CheckComboAttack3()
    {
        CheckHit(_comboAttack3_Center, _comboAttack3_Size, _comboAttack3_Data, out bool isSuccess);
    }

    #endregion
    #region DashAttack

    public void CheckDashAttack()
    {
        CheckHit(_dashAttack_Center, _dashAttack_Size, _dashAttack_Data, out bool isSuccess);
    }

    #endregion
    #region InAirAttack (UpSlash/DownSlash)

    public bool CheckUpSlashAttack()
    {
        CheckHit(_upSlashAttack_Center, _upSlashAttack_Size, _upSlashAttack_Data, out bool isSuccess);
        return isSuccess;
    }

    public bool CheckDownSlashAttack()
    {
        CheckHit(_downSlashAttack_Center, _downSlashAttack_Size, _downSlashAttack_Data, out bool isSuccess);
        return isSuccess;
    }
    #endregion

    #region FireBreath

    public void CheckFireBreath()
    {
        CheckHit(_fireBreath_Center, _fireBreath_Size, _fireBreath_Data, out bool isSuccess);
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
            DrawHitbox(_upSlashAttack_Center, _upSlashAttack_Size);
            DrawHitbox(_downSlashAttack_Center, _downSlashAttack_Size);
            DrawHitbox(_dashAttack_Center, _dashAttack_Size);
            DrawHitbox(_fireBreath_Center, _fireBreath_Size);
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

        if (_upSlashAttack_Center != null &&
            Selection.Contains(_upSlashAttack_Center.gameObject))
        {
            DrawHitbox(_upSlashAttack_Center, _upSlashAttack_Size);
        }

        if (_downSlashAttack_Center != null &&
            Selection.Contains(_downSlashAttack_Center.gameObject))
        {
            DrawHitbox(_downSlashAttack_Center, _downSlashAttack_Size);
        }

        if (_dashAttack_Center != null &&
            Selection.Contains(_dashAttack_Center.gameObject))
        {
            DrawHitbox(_dashAttack_Center, _dashAttack_Size);
        }

        if (_fireBreath_Center != null &&
            Selection.Contains(_fireBreath_Center.gameObject))
        {
            DrawHitbox(_fireBreath_Center, _fireBreath_Size);
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
