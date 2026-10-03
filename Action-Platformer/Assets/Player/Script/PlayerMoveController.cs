using System;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    [Header("Refrences")]
    PlayerInput _playerInput;
    Rigidbody2D _rigidbody;
    PlayerVisual _playerVisual;
    PlayerCombatController _playerCombatController;
    PlayerResourceManager _resourceManager;
    [SerializeField] EnvironmentChecker _environmentChecker;
    [SerializeField] UnitStateMachine _stateMachine;
    [SerializeField] PlayerHitboxManager _playerHitboxManager;

    [Header("Move Speed")]
    [SerializeField] float _runSpeed = 4f;
    [SerializeField] float _walkSpeedMultiplier = 0.5f;
    [Header("Move Thresholds")]
    [SerializeField, Range(0f, 1f)] float _idleThreshold = 0.01f;
    [SerializeField, Range(0f, 1f)] float _walkThreshold = 0.4f;
    float _moveSpeedMultiplier = 1f;

    [Header("Jump")]
    [SerializeField] float _jumpVelocity = 16f;
    [SerializeField, Range(0f, 1f)] float _jumpCutMultiplier = 0.4f;

    [Header("Jump Gravity")]
    [SerializeField] float _ascendGravity = 7f;
    [SerializeField] float _apexGravity = 6f;
    [SerializeField] float _descendGravity = 9f;
    [SerializeField] float _apexThreshold = 1f;
    [SerializeField] JumpPhase _jumpPhase = JumpPhase.Grounded;
    enum JumpPhase
    {
        Grounded,
        Ascend,
        Apex,
        Descend,
    }

    [Header("Dodge")]
    [SerializeField] float _dodgeSpeed = 8f;
    [SerializeField] float _dodgeSpeedMultiplier = 1f; // Animation Curve로 조정
    public bool IsDodging { get; private set; }

    float _originalGravityScale;
    Vector2 _moveDirection;
    bool _isMovable => _stateMachine?.Move?.CurrentState.Equals(Move.State.Movable) ?? false;

    void Awake()
    {
        TryGetComponent(out _playerInput);
        TryGetComponent(out _rigidbody);
        TryGetComponent(out _playerVisual);
        TryGetComponent(out _playerCombatController);
        TryGetComponent(out _resourceManager);
    }

    private void Start()
    {
        _originalGravityScale = _rigidbody.gravityScale;
    }

    void OnEnable()
    {
        if (_playerInput == null)
            return;

        _playerInput.JumpStarted += OnJumpStarted;
        _playerInput.JumpCanceled += OnJumpCanceled;

        _playerInput.DodgePerformed += OnDodgePerformed;
    }

    void OnDisable()
    {
        if (_playerInput == null)
            return;

        _playerInput.JumpStarted -= OnJumpStarted;
        _playerInput.JumpCanceled -= OnJumpCanceled;

        _playerInput.DodgePerformed -= OnDodgePerformed;
    }

    void Update()
    {
        var desiredDirection = _playerInput.DesiredDirection;
        _moveDirection = GetMoveVector(desiredDirection);

        // Move.State 확인
        if (!_isMovable)
            return;

        _playerVisual.SetFacingDirection(desiredDirection);
        _playerHitboxManager.SetFacingDirection(desiredDirection);

        float inputMagnitude = Mathf.Abs(_playerInput.RawInput.x);
        // Idle [0.0 ~ _idleThreshold]
        if (inputMagnitude <= _idleThreshold) 
        {
            _moveSpeedMultiplier = 0f;
        }
        // Walk [_idleThreshold ~ _walkThreshold]
        else if (inputMagnitude <= _walkThreshold)
        {
            _moveSpeedMultiplier = _walkSpeedMultiplier;
        }
        // Run [_walkThreshold ~ 1.0]
        else
        {
            _moveSpeedMultiplier = 1f;
        }
        _playerVisual.SetMoveSpeed(_moveSpeedMultiplier);
    }
    Vector2 GetMoveVector(PlayerInput.MoveDirection direction)
    {
        switch (direction)
        {
            case PlayerInput.MoveDirection.Right:
                return Vector2.right;
            case PlayerInput.MoveDirection.Left:
                return Vector2.left;
            default:
                return Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        Vector2 velocity = _rigidbody.linearVelocity;

        // dodge
        if (IsDodging)
        {
            velocity.x = _dodgeDirection.x * _dodgeSpeed * _dodgeSpeedMultiplier;
            _rigidbody.linearVelocity = velocity;
            return;
        }

        // y velocity
        UpdateJumpPhase();
        UpdateJumpGravity();

        // x velocity
        if (!_isMovable)
        {
            velocity.x = 0f;
            _rigidbody.linearVelocity = velocity;
            return;
        }
        velocity.x = _moveDirection.x * _runSpeed * _moveSpeedMultiplier;
        _rigidbody.linearVelocity = velocity;
    }

    #region Jump

    void OnJumpStarted()
    {
        // Move.State 확인
        if (!_isMovable)
            return;

        // 지면에서만 점프 가능
        if (_environmentChecker == null || !_environmentChecker.IsGrounded)
            return;

        Vector2 velocity = _rigidbody.linearVelocity;
        velocity.y = _jumpVelocity;

        _rigidbody.linearVelocity = velocity;
    }

    void OnJumpCanceled()
    {
        if (!_isMovable)
            return;

        // 상승 중일 때, Jump Cut 적용
        if (_rigidbody.linearVelocity.y > 0f)
        {
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.y *= _jumpCutMultiplier;

            _rigidbody.linearVelocity = velocity;
        }
    }

    void UpdateJumpPhase()
    {
        float yVelocity = _rigidbody.linearVelocity.y;

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
                _rigidbody.gravityScale = _originalGravityScale;
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
    #region Dodge

    void OnDodgePerformed()
    {
        if (IsDodging)
            return;

        if (_playerCombatController.IsAttacking)
        {
            _playerCombatController.EndCombo();
        }

        _playerVisual.PlayDodge();
    }

    void AnimEvent_StartDodge()
    {
        gameObject.layer = LayerMask.NameToLayer("Ghost");
        StartDodge();
    }
    Vector2 _dodgeDirection;
    public void StartDodge()
    {
        IsDodging = true;

        // 이동입력 있다면 해당 방향, 없다면 바라보는 방향으로 Dodge
        if (_moveDirection != Vector2.zero)
        {
            _dodgeDirection = _moveDirection;

            // Dodge 방향에 맞게 갱신
            var faceDirection = _dodgeDirection.x > 0f
                ? PlayerInput.MoveDirection.Right
                : PlayerInput.MoveDirection.Left;

            _playerVisual.SetFacingDirection(faceDirection);
            _playerHitboxManager.SetFacingDirection(faceDirection);
        }
        else
        {
            _dodgeDirection =
                _playerVisual.FaceDirection == PlayerInput.MoveDirection.Right
                ? Vector2.right
                : Vector2.left;
        }

        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.gravityScale = 0f;
    }

    void AnimEvent_SetDodgeState()
    {
        _stateMachine.SetState(new DodgeState());
    }

    void AnimEvent_SetNormalState()
    {
        _stateMachine.SetState(new NormalState());
    }

    void AnimEvent_EndDodge()
    {
        gameObject.layer = LayerMask.NameToLayer("Player");
        EndDodge();
    }
    public void EndDodge()
    {
        IsDodging = false;

        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.gravityScale = _originalGravityScale;
    }

    #endregion
}
