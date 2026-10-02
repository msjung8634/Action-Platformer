using System;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] Rigidbody2D _rigidbody;
    [SerializeField] EnvironmentChecker _environmentChecker;
    [SerializeField] UnitStateMachine _stateMachine;
    [SerializeField] PlayerVisual _playerVisual;
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

    Vector2 _moveDirection;
    bool _isMovable => _stateMachine?.Move?.CurrentState.Equals(Move.State.Movable) ?? false;

    void OnEnable()
    {
        if (_playerInput != null)
        {
            _playerInput.JumpStarted += OnJumpStarted;
            _playerInput.JumpCanceled += OnJumpCanceled;
        }
    }

    void OnDisable()
    {
        if (_playerInput != null)
        {
            _playerInput.JumpStarted -= OnJumpStarted;
            _playerInput.JumpCanceled -= OnJumpCanceled;
        }
    }

    void Update()
    {
        // Move.State 확인
        if (!_isMovable)
            return;

        var desiredDirection = _playerInput.DesiredDirection;
        _moveDirection = GetMoveVector(desiredDirection);

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

        // Move.State 확인
        if (!_isMovable)
        {
            velocity.x = 0f;
            _rigidbody.linearVelocity = velocity;
            return;
        }

        // X축만 제어
        velocity.x = _moveDirection.x * _runSpeed * _moveSpeedMultiplier;
        _rigidbody.linearVelocity = velocity;
    }

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
}
