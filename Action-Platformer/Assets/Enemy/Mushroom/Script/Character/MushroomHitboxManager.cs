using UnityEditor;
using UnityEngine;

public class MushroomHitboxManager : EnemyHitboxManagerBase
{
    [Header("Melee Attack")]
    [SerializeField] Transform _attack1_Center;
    [SerializeField] Vector2 _attack1_Size = new(0.6f, 0.8f);
    [SerializeField] AttackData _attack1_Data;
    [Space(10)]
    [SerializeField] Transform _attack2_Center;
    [SerializeField] Vector2 _attack2_Size = new(0.74f, 1f);
    [SerializeField] AttackData _attack2_Data;

    public void CheckAttack1()
    {
        CheckHit(_attack1_Center, _attack1_Size, _attack1_Data, out bool isSuccess);
    }

    public void CheckAttack2()
    {
        CheckHit(_attack2_Center, _attack2_Size, _attack2_Data, out bool isSuccess);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (Selection.activeTransform == null)
            return;

        Transform selected = Selection.activeTransform;

        if (selected == transform)
        {
            DrawHitbox(_attack1_Center, _attack1_Size);
            DrawHitbox(_attack2_Center, _attack2_Size);
            return;
        }

        if (_attack1_Center != null &&
            Selection.Contains(_attack1_Center.gameObject))
        {
            DrawHitbox(_attack1_Center, _attack1_Size);
        }

        if (_attack2_Center != null &&
            Selection.Contains(_attack2_Center.gameObject))
        {
            DrawHitbox(_attack2_Center, _attack2_Size);
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
