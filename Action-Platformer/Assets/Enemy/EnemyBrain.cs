using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    enum State
    {
        Chase,
        Lock,
        Attack,
        Retreat,
        Rest,
    }

    [Header("Search Target")]
    [SerializeField, Min(0f)] float _searchInterval = 0.5f;
    float _nextSearchTime;

    [Header("Lock")]
    [SerializeField, Min(0f)] float _lockDuration = 0.4f;
    float _lockEndTime;
    bool _hasLock;

    [Header("Rest")]
    [SerializeField, Min(0f)] float _restDuration = .6f;
    float _restEndTime;

    [SerializeField] State _currentState;
    EnemyMoveControllerBase _moveController;
    EnemyCombatControllerBase _combatController;
    [SerializeField] Transform _currentTarget;

    void Awake()
    {
        TryGetComponent(out _moveController);
        TryGetComponent(out _combatController);
    }

    void Update()
    {
        SearchTarget();

        if (_currentTarget == null)
        {
            CancelCurrentAction();
            return;
        }

        switch (_currentState)
        {
            case State.Chase:
                UpdateMove();
                break;

            case State.Lock:
                UpdateLock();
                break;

            case State.Attack:
                UpdateAttack();
                break;

            case State.Retreat:
                // FixedUpdate에서 처리
                break;

            case State.Rest:
                UpdateRest();
                break;
        }
    }

    #region Update

    void SearchTarget()
    {
        if (_currentTarget != null)
            return;

        if (Time.time < _nextSearchTime)
            return;

        _nextSearchTime = Time.time + _searchInterval;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _currentTarget = player != null ? player.transform : null;
    }

    void UpdateMove()
    {
        if (!_combatController.CanAttack)
            return;

        if (!_combatController.TrySelectAttack(_currentTarget, out var attackType))
            return;

        if (!EnemyCoordinator.Instance.TryAcquireLock(this, attackType))
            return;

        _hasLock = true;
        _currentState = State.Lock;
        _lockEndTime = Time.time + _lockDuration;

        _moveController.StopMove();
        _combatController.BeginLock(_currentTarget);
    }

    void UpdateLock()
    {
        if (Time.time < _lockEndTime)
            return;

        // Lock 이후에는 일단 공격
        _currentState = State.Attack;
        _combatController.BeginAttack();
    }

    void UpdateAttack()
    {
        if (_combatController.IsAttacking)
            return;

        ReleaseLock();

        _currentState = _moveController.TryBeginRetreat(_currentTarget)
            ? State.Retreat
            : State.Chase;
    }

    void UpdateRest()
    {
        if (Time.time < _restEndTime)
            return;

        _currentState = State.Chase;
    }

    #endregion

    void FixedUpdate()
    {
        if (_currentTarget == null)
            return;

        switch (_currentState)
        {
            case State.Chase:
                _moveController.TickMove(_currentTarget);
                break;
            case State.Retreat:
                if (!_moveController.TryTickRetreat(_currentTarget))
                {
                    BeginRest();
                }
                break;
        }
    }

    #region FixedUpdate

    void BeginRest()
    {
        _moveController.StopMove();
        _restEndTime = Time.time + _restDuration;
        _currentState = State.Rest;
    }

    #endregion

    void OnDisable()
    {
        CancelCurrentAction();
    }

    #region OnDisable

    public void CancelCurrentAction()
    {
        if (_currentState == State.Lock ||
            _currentState == State.Attack)
        {
            _combatController.CancelAttack();
        }

        _moveController.StopMove();
        ReleaseLock();
        _currentState = State.Chase;
    }

    void ReleaseLock()
    {
        if (!_hasLock)
            return;

        EnemyCoordinator.Instance.ReleaseLock(this);
        _hasLock = false;
    }

    #endregion
}
