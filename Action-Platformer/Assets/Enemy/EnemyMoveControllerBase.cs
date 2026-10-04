using UnityEngine;

public class EnemyMoveControllerBase : MonoBehaviour
{
    [Header("Refrences")]
    protected Rigidbody2D _rigidbody;
    protected EnemyVisualBase _visual;
    protected EnemyResourceManager _resourceManager;
    protected EnemyCombatControllerBase _combatController;
    [SerializeField] protected EnvironmentChecker _environmentChecker;
    [SerializeField] protected UnitStateMachine _stateMachine;
    [SerializeField] protected EnemyHitboxManagerBase _hitboxManager;

    [Header("Chase")]
    [SerializeField] protected float _chaseSpeed = 4f;
    [field: SerializeField] public float StopDistance { get; private set; } = .5f;

    [Header("Retreat")]
    [SerializeField, Min(0f)] protected float _retreatSpeed = 4f;
    [SerializeField] protected Vector2 _retreatMaxDistanceRange = new(2f, 3f);
    [SerializeField, Min(0f)] protected float _retreatMaxDuration = 2f;
    protected float _currentRetreatMaxDistance;
    protected float _retreatDirectionX;
    protected float _retreatEndTime;

    protected float _originalGravityScale;
    protected bool _isMovable => _stateMachine?.Move?.CurrentState.Equals(Move.State.Movable) ?? false;

    protected virtual void Awake()
    {
        TryGetComponent(out _rigidbody);
        TryGetComponent(out _visual);
        TryGetComponent(out _combatController);
        TryGetComponent(out _resourceManager);
    }

    protected virtual void Start()
    {
        _originalGravityScale = _rigidbody.gravityScale;
    }

    public virtual void TickMove(Transform target)
    {
        // 내부에서 _isMovable을 확인하고 추적 / 후퇴 / 거리 유지 처리
    }

    public virtual void StopMove()
    {
        // 수평 이동 정지. 중력과 수직 속도는 필요에 따라 유지
    }

    public virtual bool TryBeginRetreat(Transform target)
    {
        // 후퇴 시작시 true
        return false;
    }

    public virtual bool TryTickRetreat(Transform target)
    {
        // 후퇴중이면 true, 후퇴완료면 false
        return false;
    }
}    
