using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    enum State
    {
        Move,
        Lock,
        Attack
    }

    [Header("Search Target")]
    [SerializeField, Min(0f)] float _searchInterval = 0.5f;
    float _nextSearchTime;

    [Header("Lock")]
    [SerializeField, Min(0f)] float _lockDuration = 0.4f;
    float _lockEndTime;
    bool _hasLock;

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
            case State.Move:
                UpdateMove();
                break;

            case State.Lock:
                UpdateLock();
                break;

            case State.Attack:
                UpdateAttack();
                break;
        }
    }

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
        _currentState = State.Move;
    }

    void FixedUpdate()
    {
        if (_currentTarget != null && _currentState == State.Move)
            _moveController.TickMove(_currentTarget);
    }

    void OnDisable()
    {
        CancelCurrentAction();
    }

    public void CancelCurrentAction()
    {
        if (_currentState != State.Move)
            _combatController.CancelAttack();

        _moveController.StopMove();
        ReleaseLock();
        _currentState = State.Move;
    }

    void ReleaseLock()
    {
        if (!_hasLock)
            return;

        EnemyCoordinator.Instance.ReleaseLock(this);
        _hasLock = false;
    }
}
