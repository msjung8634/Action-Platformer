using UnityEngine;

public class MushroomMoveController : EnemyMoveControllerBase
{
    [Header("Movement")]
    [SerializeField] float _runSpeed = 3f;
    [field: SerializeField] public float StopDistance { get; private set; } = .8f;

    public override void TickMove(Transform target)
    {
        if (target == null || !_isMovable)
        {
            StopMove();
            return;
        }

        float xDistance = target.position.x - transform.position.x;
        if (Mathf.Abs(xDistance) <= StopDistance)
        {
            StopMove();
            return;
        }

        float directionX = Mathf.Sign(xDistance);

        _rigidbody.linearVelocityX = directionX * _runSpeed;

        var visual = _visual as MushroomVisual;
        visual.SetFacingDirection(directionX > 0f
            ? UnitMoveDirection.Right 
            : UnitMoveDirection.Left);
        visual.SetMoveSpeed(1f);
    }

    public override void StopMove()
    {
        _rigidbody.linearVelocityX = 0f;

        var visual = _visual as MushroomVisual;
        visual.SetMoveSpeed(0f);
    }
}
