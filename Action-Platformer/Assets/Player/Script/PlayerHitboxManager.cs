using UnityEditor;
using UnityEngine;

public class PlayerHitboxManager : MonoBehaviour
{
    [Header("Attack Hitboxes")]
    [SerializeField] LayerMask _attackTargetLayer;
    [Space(10)]
    [SerializeField] Transform _gAttack0_Center;
    [SerializeField] Vector2 _gAttack0_Size = new(0.1f, 0.8f);
    [SerializeField] CombatFeedbackSO _gAttack0_Feedback;
    [Space(10)]
    [SerializeField] Transform _gAttack1_Center;
    [SerializeField] Vector2 _gAttack1_Size = new(0.1f, 0.8f);
    [SerializeField] CombatFeedbackSO _gAttack1_Feedback;
    [Space(10)]
    [SerializeField] Transform _gAttack2_Center;
    [SerializeField] Vector2 _gAttack2_Size = new(0.1f, 0.8f);
    [SerializeField] CombatFeedbackSO _gAttack2_Feedback;

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
        if (!IsGroundAttackHit(_gAttack0_Center, _gAttack0_Size))
            return;

        // TODO : 좋은 Feedback을 주도록 노력할 것
        // 이걸 구조화해서 정리하면 최고!

        // TimeManager.Instance를 통해 Timescale 조정 (슬로우 or 정지)
        // 타격 VFX 출력
        // SFX 출력
        // 캐릭터 약간 Knockback?

        // Camera Shake
        CombatFeedbackManager.Instance.ApplyFeedback(_gAttack0_Feedback);
    }

    public void CheckGroundAttack1()
    {
        if (!IsGroundAttackHit(_gAttack1_Center, _gAttack1_Size))
            return;

        CombatFeedbackManager.Instance.ApplyFeedback(_gAttack1_Feedback);
    }

    public void CheckGroundAttack2()
    {
        if (!IsGroundAttackHit(_gAttack2_Center, _gAttack2_Size))
            return;

        CombatFeedbackManager.Instance.ApplyFeedback(_gAttack2_Feedback);
    }

    bool IsGroundAttackHit(Transform center, Vector2 size)
    {
        Collider2D[] enemyHitboxes = Physics2D.OverlapBoxAll(
            center.position,
            size,
            0f,
            _attackTargetLayer
        );

        if (enemyHitboxes.Length == 0)
            return false;

        foreach (Collider2D hitbox in enemyHitboxes)
        {
            Debug.Log($"Hit : {hitbox.name}");
        }

        return true;
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
