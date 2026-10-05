using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    [Header("Refrences")]
    PlayerInput _playerInput;
    Rigidbody2D _rigidbody;
    PlayerVisual _playerVisual;
    PlayerCombatController _playerCombatController;
    [SerializeField] PlayerResourceManager _resourceManager;
    [SerializeField] EnvironmentChecker _environmentChecker;
    [SerializeField] UnitStateMachine _stateMachine;
    [SerializeField] PlayerHitboxManager _playerHitboxManager;

    [Header("Move(Walk/Run)")]
    [SerializeField] float _runSpeed = 4f;
    // Locomotion Blend Tree의 Threshold와 일치시킬 것
    [SerializeField] float _walkSpeedMultiplier = 0.5f;
    [Space(10)]
    [SerializeField, Range(0f, 1f)] float _idleThreshold = 0.01f;
    [SerializeField, Range(0f, 1f)] float _walkThreshold = 0.4f;
    float _moveSpeedMultiplier = 1f;

    [Header("Jump")]
    [SerializeField] MoveData _jumpData;
    [SerializeField] float _jumpVelocity = 16f;
    [SerializeField, Range(0f, 1f)] float _jumpCutMultiplier = 0.4f;
    [Space(10)]
    [SerializeField] float _ascendGravity = 7f;
    [SerializeField] float _apexGravity = 6f;
    [SerializeField] float _descendGravity = 4f;
    [SerializeField] float _apexThreshold = 1f;
    [SerializeField] JumpPhase _jumpPhase = JumpPhase.None;
    enum JumpPhase
    {
        None,
        Grounded,
        Ascend,
        Apex,
        Descend,
    }

    [Header("Dodge")]
    [SerializeField] MoveData _dashData;
    [SerializeField] float _dashSpeed = 8f;
    [SerializeField] float _dashSpeedMultiplier = 1f; // Animation Curve로 조정
    public bool IsDodging { get; private set; }

    [Header("InAirAttackTime")]
    [SerializeField] float _airAttackStopDuration = 0.1f;
    public bool IsAirAttacking { get; private set; }

    Vector2 _moveDirection;
    bool _isMovable => _stateMachine?.Move?.CurrentState.Equals(Move.State.Movable) ?? false;

    float _initialGravityScale;
    int _initialLayer;
    Vector3 _initialPosition;

    void Awake()
    {
        TryGetComponent(out _playerInput);
        TryGetComponent(out _rigidbody);
        TryGetComponent(out _playerVisual);
        TryGetComponent(out _playerCombatController);
    }

    void Start()
    {
        _initialGravityScale = _rigidbody.gravityScale;
        _initialLayer = gameObject.layer;
        _initialPosition = transform.position;
    }

    void OnEnable()
    {
        if (_playerInput == null)
            return;

        _playerInput.JumpStarted += OnJumpStarted;
        _playerInput.JumpCanceled += OnJumpCanceled;

        _playerInput.DodgePerformed += OnDodgePerformed;

        if (WaveManager.Instance == null) return;
        WaveManager.Instance.OnRestartGame += OnRestartGame;
    }

    void OnDisable()
    {
        if (_playerInput == null)
            return;

        _playerInput.JumpStarted -= OnJumpStarted;
        _playerInput.JumpCanceled -= OnJumpCanceled;

        _playerInput.DodgePerformed -= OnDodgePerformed;

        if (WaveManager.Instance == null) return;
        WaveManager.Instance.OnRestartGame -= OnRestartGame;
    }

    void Update()
    {
        float horizontalInput = _playerInput.RawInput.x;
        float inputMagnitude = Mathf.Abs(horizontalInput);

        if (inputMagnitude <= _idleThreshold)
        {
            _moveDirection = Vector2.zero;
            _moveSpeedMultiplier = 0f;
        }
        else
        {
            _moveDirection = horizontalInput > 0f
                ? Vector2.right
                : Vector2.left;

            _moveSpeedMultiplier = inputMagnitude <= _walkThreshold
                ? _walkSpeedMultiplier
                : 1f;
        }

        if (!_isMovable)
            return;

        _playerVisual.SetFacingDirection(_playerInput.DesiredDirection);
        _playerHitboxManager.SetFacingDirection(_playerInput.DesiredDirection);
        _playerVisual.SetMoveSpeed(_moveSpeedMultiplier);
    }

    void FixedUpdate()
    {
        Vector2 velocity = _rigidbody.linearVelocity;

        // dodge
        if (IsDodging)
        {
            _rigidbody.linearVelocityX = _dodgeDirection.x * _dashSpeed * _dashSpeedMultiplier;
            return;
        }

        // inAirAttack
        if (IsAirAttacking)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            return;
        }

        // y velocity
        UpdateJumpPhase();
        UpdateJumpGravity();

        // x velocity
        if (!_isMovable)
        {
            _rigidbody.linearVelocityX = 0f;
            return;
        }
        _rigidbody.linearVelocityX = _moveDirection.x * _runSpeed * _moveSpeedMultiplier;
    }

    #region Common

    bool TryRequestMove(MoveData moveData)
    {
        if (IsDodging || _resourceManager.HP.Current == 0)
            return false;

        return _resourceManager.TryConsumeResource(moveData);
    }

    #endregion
    #region Jump

    void OnJumpStarted()
    {
        if (!_isMovable)
            return;

        if (_environmentChecker == null || !_environmentChecker.IsGrounded)
            return;

        if (!TryRequestMove(_jumpData))
            return;

        _rigidbody.linearVelocityY = _jumpVelocity;
    }

    void OnJumpCanceled()
    {
        if (!_isMovable)
            return;

        // 상승 중일 때, Jump Cut
        if (_rigidbody.linearVelocity.y > 0f)
        {
            _rigidbody.linearVelocityY *= _jumpCutMultiplier;
        }
    }

    void UpdateJumpPhase()
    {
        float yVelocity = _rigidbody.linearVelocityY;

        // Ascend [_jumpVelocity ~ _apexThreshold]
        if (yVelocity > _apexThreshold)
        {
            SetJumpPhase(JumpPhase.Ascend);
            return;
        }

        // Ascend처리한 후, Grounded확인
        if (_environmentChecker.IsGrounded)
        {
            SetJumpPhase(JumpPhase.Grounded);
            return;
        }

        // Apex [_apexThreshold ~ -_apexThreshold]
        if (yVelocity >= -_apexThreshold)
        {
            SetJumpPhase(JumpPhase.Apex);
        }
        // Descend [-_apexThreshold ~]
        else
        {
            SetJumpPhase(JumpPhase.Descend);
        }
    }
    void SetJumpPhase(JumpPhase newPhase)
    {
        if (_jumpPhase == newPhase)
            return;

        switch (newPhase)
        {
            case JumpPhase.Ascend:
                _playerVisual.PlayAscend();
                break;

            case JumpPhase.Descend:
                _playerVisual.PlayDescend();
                break;

            case JumpPhase.Grounded:
                _playerVisual.ResetInAirAttack();
                _playerVisual.PlayLand();
                break;
        }

        _jumpPhase = newPhase;
    }

    void UpdateJumpGravity()
    {
        switch (_jumpPhase)
        {
            case JumpPhase.Grounded:
                _rigidbody.gravityScale = _initialGravityScale;
                break;

            case JumpPhase.Ascend:
                _rigidbody.gravityScale = _ascendGravity;
                break;

            case JumpPhase.Apex:
                _rigidbody.gravityScale = _apexGravity;
                break;

            case JumpPhase.Descend:
                _rigidbody.gravityScale = _descendGravity;
                break;
        }
    }

    #endregion
    #region Dash

    void OnDodgePerformed()
    {
        if (!TryRequestMove(_dashData))
            return;

        if (_playerCombatController.IsAttacking)
        {
            _playerCombatController.CancelAttack();
        }

        gameObject.layer = LayerMask.NameToLayer("Ghost");
        _stateMachine.SetState(new DodgeState());
        _playerVisual.PlayDodge();
        StartDashMove();
    }

    Vector2 _dodgeDirection;
    public void StartDashMove()
    {
        IsDodging = true;

        // 이동입력 있다면 해당 방향, 없다면 바라보는 방향으로 Dodge
        if (_moveDirection != Vector2.zero)
        {
            _dodgeDirection = _moveDirection;

            // Dodge 방향에 맞게 갱신
            var faceDirection = _dodgeDirection.x > 0f
                ? UnitMoveDirection.Right
                : UnitMoveDirection.Left;

            _playerVisual.SetFacingDirection(faceDirection);
            _playerHitboxManager.SetFacingDirection(faceDirection);
        }
        else
        {
            _dodgeDirection = _playerVisual.FaceDirection == UnitMoveDirection.Right
                ? Vector2.right
                : Vector2.left;
        }

        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.gravityScale = 0f;
    }

    void AnimEvent_EndDodge()
    {
        _stateMachine.SetState(new NormalState());
        gameObject.layer = LayerMask.NameToLayer("Player");
        EndDodge();
    }
    public void EndDodge()
    {
        IsDodging = false;

        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.gravityScale = _initialGravityScale;
    }

    #endregion
    #region InAirAttackTime

    CancellationTokenSource _airAttackTimeCts;
    public void StartAirAttackTime()
    {
        if (_environmentChecker.IsGrounded)
            return;

        _airAttackTimeCts?.Cancel();
        _airAttackTimeCts?.Dispose();
        _airAttackTimeCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        StartAirAttakTimeAsync().Forget();
    }
    async UniTask StartAirAttakTimeAsync()
    {
        IsAirAttacking = true;

        // TODO : 일시정지 말고 좋은 방법으로 변경?
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.gravityScale = 0f;

        await UniTask.Delay(
            TimeSpan.FromSeconds(_airAttackStopDuration),
            DelayType.Realtime,
            cancellationToken: destroyCancellationToken
        );

        EndAirAttackTime();
    }
    public void EndAirAttackTime()
    {
        _airAttackTimeCts?.Cancel();
        _airAttackTimeCts?.Dispose();
        _airAttackTimeCts = null;

        IsAirAttacking = false;

        // 이후 JumpPhase에 따라 중력이 다시 결정됨
        UpdateJumpPhase();
        UpdateJumpGravity();
    }

    #endregion
    #region OnRestartGame

    void OnRestartGame()
    {
        // 이전 비동기 작업부터 취소
        EndAirAttackTime();
        IsDodging = false;
        _dodgeDirection = Vector2.zero;
        _moveDirection = Vector2.zero;
        _jumpPhase = JumpPhase.None;

        // layer
        gameObject.layer = _initialLayer;

        // 
        transform.position = _initialPosition;

        // rigidbody
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;
        _rigidbody.gravityScale = _initialGravityScale;
    }

    #endregion
}
