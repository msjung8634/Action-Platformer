using UnityEngine;

public class EnemyCombatControllerBase : MonoBehaviour
{
    [Header("Refrences")]
    protected Rigidbody2D _rigidbody;
    protected EnemyVisualBase _visual;
    protected EnemyMoveControllerBase _moveController;
    [SerializeField] protected EnemyResourceManager _resourceManager;
    [SerializeField] protected EnvironmentChecker _environmentChecker;
    [SerializeField] protected UnitStateMachine _stateMachine;
    [SerializeField] protected EnemyHitboxManagerBase _hitboxManager;
    [SerializeField] protected EnemyBrain _brain;

    [Header("Recovery")]
    [SerializeField, Min(0f)] protected float _recoveryDuration = .5f;
    protected bool _isRecovering;
    protected float _recoveryEndTime;

    public bool CanAttack => _isAttackable && !IsAttacking;
    protected bool _isAttackable => _stateMachine?.Attack?.CurrentState.Equals(Attack.State.Attackable) ?? false;
    public bool IsAttacking { get; protected set; }

    protected virtual void Awake()
    {
        TryGetComponent(out _rigidbody);
        TryGetComponent(out _visual);
        TryGetComponent(out _moveController);
    }

    protected virtual void OnEnable()
    {
        _resourceManager.OnHit += OnHit;
        _resourceManager.OnDead += OnDead;
    }

    protected virtual void OnDisable()
    {
        _resourceManager.OnHit -= OnHit;
        _resourceManager.OnDead -= OnDead;
    }

    void Update()
    {
        if (!_isRecovering)
            return;

        if (Time.time < _recoveryEndTime)
            return;

        EndAttack();
    }

    public virtual bool TrySelectAttack(Transform target, out EnemyAttackType attackType)
    {
        attackType = EnemyAttackType.Melee;
        return false;
    }

    #region Common

    protected virtual void BeginRecovery()
    {
        // 중복 종료 이벤트 방지
        if (!IsAttacking || _isRecovering)
            return;

        _isRecovering = true;
        _recoveryEndTime = Time.time + _recoveryDuration;
    }

    public virtual void BeginLock(Transform target)
    {
        if (target == null)
            return;

        float enemyToPlayer = target.position.x - transform.position.x;
        var faceDirection = enemyToPlayer > 0f
            ? UnitMoveDirection.Right
            : UnitMoveDirection.Left;

        _visual.SetFacingDirection(faceDirection);
        _hitboxManager.SetFacingDirection(faceDirection);
    }

    public virtual void BeginAttack()
    {
        IsAttacking = true;
        _stateMachine.SetState(new AttackState());
        _hitboxManager.ClearCachedTarget();
    }

    protected virtual void EndAttack()
    {
        IsAttacking = false;
        _isRecovering = false;
        _stateMachine.SetState(new NormalState());
    }

    public virtual void CancelAttack()
    {
        IsAttacking = false;
        _isRecovering = false;
    }

    #endregion

    #region OnHit

    void OnHit()
    {
        _stateMachine.SetState(new StunState());
        _brain.BeginStun();
        _visual.PlayHit();
    }

    void AnimEvent_HitEnd()
    {
        _stateMachine.SetState(new NormalState());
        _brain.EndStun();
    }

    #endregion

    #region OnDead

    void OnDead()
    {
        _stateMachine.SetState(new DeadState());
        _brain.BeginDead();
        _visual.PlayDie();
    }

    void AnimEvent_DieEnd()
    {
        // TODO : Object Pool에 반환하도록 수정
        gameObject.SetActive(false);
    }

    #endregion
}
