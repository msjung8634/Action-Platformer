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

    protected static readonly int _hitHash = Animator.StringToHash("hit");
    public void PlayHit()
    {
        _animator.SetTrigger(_hitHash);
    }

    protected static readonly int _dieHash = Animator.StringToHash("die");
    public void PlayDie()
    {
        _animator.SetTrigger(_dieHash);
    }
}
