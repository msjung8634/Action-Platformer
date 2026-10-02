using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerHitboxManager : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] LayerMask _attackTargetLayer;
    [Space(10)]
    [SerializeField] Transform _gAttack0_center;
    [SerializeField] Vector2 _gAttack0_Size = new(0.1f, 0.8f);
    [Space(10)]
    [SerializeField] Transform _gAttack1_center;
    [SerializeField] Vector2 _gAttack1_Size = new(0.1f, 0.8f);
    [Space(10)]
    [SerializeField] Transform _gAttack2_center;
    [SerializeField] Vector2 _gAttack2_Size = new(0.1f, 0.8f);

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
        CheckGroundAttackHit(_gAttack0_center, _gAttack0_Size);
    }

    public void CheckGroundAttack1()
    {
        CheckGroundAttackHit(_gAttack1_center, _gAttack1_Size);
    }

    public void CheckGroundAttack2()
    {
        CheckGroundAttackHit(_gAttack2_center, _gAttack2_Size);
    }

    void CheckGroundAttackHit(Transform center, Vector2 size)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            center.position,
            size,
            0f,
            _attackTargetLayer
        );

        if (hits.Length == 0)
            return;

        foreach (Collider2D hit in hits)
        {
            Debug.Log($"Hit : {hit.name}");
        }

        CameraManager.Instance.Shake();
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
            DrawHitbox(_gAttack0_center, _gAttack0_Size);
            DrawHitbox(_gAttack1_center, _gAttack1_Size);
            DrawHitbox(_gAttack2_center, _gAttack2_Size);
            return;
        }

        if (_gAttack0_center != null &&
            Selection.Contains(_gAttack0_center.gameObject))
        {
            DrawHitbox(_gAttack0_center, _gAttack0_Size);
        }

        if (_gAttack1_center != null &&
            Selection.Contains(_gAttack1_center.gameObject))
        {
            DrawHitbox(_gAttack1_center, _gAttack1_Size);
        }

        if (_gAttack2_center != null &&
            Selection.Contains(_gAttack2_center.gameObject))
        {
            DrawHitbox(_gAttack2_center, _gAttack2_Size);
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
