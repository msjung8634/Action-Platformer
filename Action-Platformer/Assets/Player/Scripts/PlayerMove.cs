using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerVisual))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    PlayerInput _playerInput;
    PlayerVisual _playerVisual;
    Rigidbody2D _rigidbody;

    [Header("CollisionChecker")]
    [SerializeField] CollisionChecker _collisionChecker;

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

    void Awake()
    {
        TryGetComponent(out _playerInput);
        TryGetComponent(out _playerVisual);
        TryGetComponent(out _rigidbody);
    }

    void OnEnable()
    {
        _playerInput.JumpStarted += OnJumpStarted;
        _playerInput.JumpCanceled += OnJumpCanceled;
    }

    void OnDisable()
    {
        _playerInput.JumpStarted -= OnJumpStarted;
        _playerInput.JumpCanceled -= OnJumpCanceled;
    }

    void Update()
    {
        var desiredDirection = _playerInput.DesiredDirection;
        _moveDirection = GetMoveVector(desiredDirection);
        _playerVisual.SetFacingDirection(desiredDirection);

        // Idle [0.0 ~ _idleThreshold] / Walk [_idleThreshold ~ _walkThreshold] / Run [_walkThreshold ~ 1.0]
        float inputMagnitude = Mathf.Abs(_playerInput.RawInput.x);
        if (inputMagnitude <= _idleThreshold)
        {
            _moveSpeedMultiplier = 0f;
        }
        else if (inputMagnitude <= _walkThreshold)
        {
            _moveSpeedMultiplier = _walkSpeedMultiplier;
        }
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
        // X축만 제어
        Vector2 velocity = _rigidbody.linearVelocity;
        velocity.x = _moveDirection.x * _runSpeed * _moveSpeedMultiplier;

        _rigidbody.linearVelocity = velocity;
    }

    void OnJumpStarted()
    {
        if (_collisionChecker == null || !_collisionChecker.IsGrounded) return;

        Vector2 velocity = _rigidbody.linearVelocity;
        velocity.y = _jumpVelocity;

        _rigidbody.linearVelocity = velocity;
    }

    void OnJumpCanceled()
    {
        // 상승 중일 때, Jump Cut 적용
        if (_rigidbody.linearVelocity.y > 0f)
        {
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.y *= _jumpCutMultiplier;

            _rigidbody.linearVelocity = velocity;
        }
    }
}
