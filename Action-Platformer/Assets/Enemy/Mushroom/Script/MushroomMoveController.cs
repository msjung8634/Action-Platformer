using UnityEngine;

public class MushroomMoveController : EnemyMoveControllerBase
{
    [Header("Chase")]
    [SerializeField] float _chaseSpeed = 4f;
    [field: SerializeField] public float StopDistance { get; private set; } = .8f;

    [Header("Retreat")]
    [SerializeField, Min(0f)] float _retreatMaxDistance = 4f;
    [SerializeField, Min(0f)] float _retreatSpeed = 4f;
    [SerializeField, Min(0f)] float _retreatMaxDuration = 1.5f;
    float _retreatDirectionX;
    float _retreatEndTime;

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

        _rigidbody.linearVelocityX = directionX * _chaseSpeed;

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

    public override bool TryBeginRetreat(Transform target)
    {
        if (target == null || !_isMovable)
            return false;

        if (_retreatMaxDistance <= 0f ||
            _retreatSpeed <= 0f ||
            _retreatMaxDuration <= 0f)
            return false;

        float offsetX = target.position.x - transform.position.x;

        // 충분히 멀면 후퇴 중단
        if (Mathf.Abs(offsetX) >= _retreatMaxDistance)
            return false;

        _retreatDirectionX = offsetX == 0f
            ? (_visual.FaceDirection == UnitMoveDirection.Right ? -1f : 1f)
            : -Mathf.Sign(offsetX);

        _retreatEndTime = Time.time + _retreatMaxDuration;

        return true;
    }

    public override bool TryTickRetreat(Transform target)
    {
        if (target == null || !_isMovable)
        {
            StopMove();
            return false;
        }

        float offsetX = target.position.x - transform.position.x;
        float distanceX = Mathf.Abs(offsetX);

        if (distanceX >= _retreatMaxDistance ||    // 충분히 이격됨
            Time.time >= _retreatEndTime)       // 제한시간 초과
        {
            StopMove();
            return false;
        }

        // 아주 가까우면 후퇴방향 유지
        if (distanceX > 0.1f)
        {
            _retreatDirectionX = -Mathf.Sign(offsetX);
        }

        _rigidbody.linearVelocityX = _retreatDirectionX * _retreatSpeed;

        var visual = _visual as MushroomVisual;
        visual.SetFacingDirection(_retreatDirectionX > 0f
            ? UnitMoveDirection.Right
            : UnitMoveDirection.Left);
        visual.SetMoveSpeed(1f);

        return true;
    }
}
