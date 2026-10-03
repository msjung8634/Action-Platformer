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
    [SerializeField] float _runSpeed = 5f;
    [SerializeField] float _walkSpeedMultiplier = 0.5f;
    [Header("Move Thresholds")]
    [SerializeField, Range(0f, 1f)] float _idleThreshold = 0.01f;
    [SerializeField, Range(0f, 1f)] float _walkThreshold = 0.4f;
    float _moveSpeedMultiplier = 1f;

    [Header("Jump")]
    [SerializeField] float _jumpVelocity = 12f;
    [SerializeField, Range(0f, 1f)] float _jumpCutMultiplier = 0.5f;
    [SerializeField] float _ascendGravity = 6f;
    [SerializeField] float _apexGravity = 4f;
    [SerializeField] float _descendGravity = 9f;
    [SerializeField] float _apexThreshold = 2f;

    [Header("Dodge")]
    [SerializeField] float _dodgeSpeed = 15f;
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

        // Dodge는 별도 처리
        if (IsDodging)
        {
            velocity.x = _dodgeDirection.x * _dodgeSpeed * _dodgeSpeedMultiplier;
            _rigidbody.linearVelocity = velocity;
            return;
        }

        // Move.State 확인
        if (!_isMovable)
        {
            velocity.x = 0f;
            _rigidbody.linearVelocity = velocity;
            return;
        }

        // X축 제어
        velocity.x = _moveDirection.x * _runSpeed * _moveSpeedMultiplier;
        _rigidbody.linearVelocity = velocity;

        // y축 제어
        UpdateJumpGravity();
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

    void UpdateJumpGravity()
    {
        // 지상
        if (_environmentChecker.IsGrounded)
        {
            _rigidbody.gravityScale = _originalGravityScale;
            return;
        }

        float yVelocity = _rigidbody.linearVelocity.y;

        // apex 근처
        if (Mathf.Abs(yVelocity) <= _apexThreshold)
        {
            _rigidbody.gravityScale = _apexGravity;
        }
        // ascend
        else if (yVelocity > 0f)
        {
            _rigidbody.gravityScale = _ascendGravity;
        }
        // descend
        else
        {
            _rigidbody.gravityScale = _descendGravity;
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
