using UnityEngine;

public class EnemyVisualBase : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer _renderer;
    [SerializeField] protected Animator _animator;

    public UnitMoveDirection FaceDirection { get; private set; } = UnitMoveDirection.Right;

    public void SetFacingDirection(UnitMoveDirection newDirection)
    {
        if (FaceDirection == newDirection)
            return;

        switch (newDirection)
        {
            case UnitMoveDirection.Right:
                FaceDirection = UnitMoveDirection.Right;
                _renderer.flipX = false;
                break;
            case UnitMoveDirection.Left:
                FaceDirection = UnitMoveDirection.Left;
                _renderer.flipX = true;
                break;
            default:
                break;
        }
    }
}
