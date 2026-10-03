using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("Input Action References")]
    [SerializeField] InputActionReference _move;
    [SerializeField] InputActionReference _jump;
    [SerializeField] InputActionReference _dodge;
    [SerializeField] InputActionReference _attack;
    [SerializeField] InputActionReference _fireBreath;
    [SerializeField] InputActionReference _fireBall;
    [SerializeField] InputActionReference _igniteSword;
    [SerializeField] InputActionReference _parry;

    #region Gamepad Shoulder Combination

    [Header("Gamepad Shoulder Combination")]
    [SerializeField] float _shoulderCombinationTime = 0.08f;
    bool _leftShoulderPressed;
    bool _rightShoulderPressed;
    CancellationTokenSource _shoulderCombinationCts;

    void HandleLeftShoulder()
    {
        _leftShoulderPressed = true;

        // RB가 이미 눌린 상태면 조합
        if (_rightShoulderPressed)
        {
            PerformShoulderCombination();
            return;
        }

        WaitForShoulderCombination().Forget();
    }

    void HandleRightShoulder()
    {
        _rightShoulderPressed = true;

        // LB가 이미 눌린 상태면 조합
        if (_leftShoulderPressed)
        {
            PerformShoulderCombination();
            return;
        }

        WaitForShoulderCombination().Forget();
    }

    void PerformShoulderCombination()
    {
        _shoulderCombinationCts?.Cancel();
        _shoulderCombinationCts?.Dispose();
        _shoulderCombinationCts = null;

        _leftShoulderPressed = false;
        _rightShoulderPressed = false;

        IgniteSwordPerformed?.Invoke();
    }

    async UniTaskVoid WaitForShoulderCombination()
    {
        _shoulderCombinationCts?.Cancel();
        _shoulderCombinationCts?.Dispose();
        _shoulderCombinationCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_shoulderCombinationTime), DelayType.Realtime, cancellationToken: _shoulderCombinationCts.Token);

            if (_leftShoulderPressed && !_rightShoulderPressed)
            {
                _leftShoulderPressed = false;
                FireBreathPerformed?.Invoke();
            }
            else if (_rightShoulderPressed && !_leftShoulderPressed)
            {
                _rightShoulderPressed = false;
                FireBallPerformed?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            // 조합 성공 시
        }
    }

    #endregion

    void OnEnable()
    {
        if (_move != null)
        {
            _move.action.performed += OnMove;
            _move.action.canceled += OnMove;
            _move.action.Enable();
        }

        if (_jump != null)
        {
            _jump.action.started += OnJumpStarted;
            _jump.action.canceled += OnJumpCanceled;
            _jump.action.Enable();
        }

        if (_dodge != null)
        {
            _dodge.action.performed += OnDodgePerformed;
            _dodge.action.Enable();
        }

        if (_attack != null)
        {
            _attack.action.performed += OnAttackPerformed;
            _attack.action.Enable();
        }

        if (_fireBreath != null)
        {
            _fireBreath.action.performed += OnFireBreathPerformed;
            _fireBreath.action.Enable();
        }

        if (_fireBall != null)
        {
            _fireBall.action.performed += OnFireBallPerformed;
            _fireBall.action.Enable();
        }

        if (_igniteSword != null)
        {
            _igniteSword.action.performed += OnIgniteSwordPerformed;
            _igniteSword.action.Enable();
        }

        if (_parry != null)
        {
            _parry.action.started += OnParryStarted;
            _parry.action.canceled += OnParryCanceled;
            _parry.action.Enable();
        }
    }

    void OnDisable()
    {
        RawInput = Vector2.zero;
        DesiredDirection = MoveDirection.None;

        _shoulderCombinationCts?.Cancel();
        _shoulderCombinationCts?.Dispose();
        _shoulderCombinationCts = null;
        _leftShoulderPressed = false;
        _rightShoulderPressed = false;

        if (_move != null)
        {
            _move.action.performed -= OnMove;
            _move.action.canceled -= OnMove;
            _move.action.Disable();
        }

        if (_jump != null)
        {
            _jump.action.performed -= OnJumpStarted;
            _jump.action.canceled -= OnJumpCanceled;
            _jump.action.Disable();
        }

        if (_dodge != null)
        {
            _dodge.action.performed -= OnDodgePerformed;
            _dodge.action.Disable();
        }

        if (_attack != null)
        {
            _attack.action.performed -= OnAttackPerformed;
            _attack.action.Disable();
        }

        if (_fireBreath != null)
        {
            _fireBreath.action.performed -= OnFireBreathPerformed;
            _fireBreath.action.Disable();
        }

        if (_fireBall != null)
        {
            _fireBall.action.performed -= OnFireBallPerformed;
            _fireBall.action.Disable();
        }

        if (_igniteSword != null)
        {
            _igniteSword.action.performed -= OnIgniteSwordPerformed;
            _igniteSword.action.Disable();
        }

        if (_parry != null)
        {
            _parry.action.started -= OnParryStarted;
            _parry.action.canceled -= OnParryCanceled;
            _parry.action.Disable();
        }
    }

    // Move
    public enum MoveDirection
    {
        None,
        Right,
        Left,
    }
    public Vector2 RawInput { get; private set; }
    public MoveDirection DesiredDirection { get; private set; }

    void OnMove(InputAction.CallbackContext context)
    {
        RawInput = context.ReadValue<Vector2>();
        DesiredDirection = GetDesiredDirection(RawInput);
    }

    MoveDirection GetDesiredDirection(Vector2 input)
    {
        // 입력이 없는 경우
        if (input.sqrMagnitude < 0.01f)
            return MoveDirection.None;

        if (input.x > 0.01f)
            return MoveDirection.Right;

        if (input.x < -0.01f)
            return MoveDirection.Left;

        return MoveDirection.None;
    }

    // Jump
    public event Action JumpStarted;
    public event Action JumpCanceled;
    void OnJumpStarted(InputAction.CallbackContext context)
    {
        JumpStarted?.Invoke();
    }
    void OnJumpCanceled(InputAction.CallbackContext context)
    {
        JumpCanceled?.Invoke();
    }

    // Attack (Combo, Dash, InAir)
    public event Action AttackPerformed;
    void OnAttackPerformed(InputAction.CallbackContext context)
    {
        AttackPerformed?.Invoke();
    }

    // FireBreath
    public event Action FireBreathPerformed;
    void OnFireBreathPerformed(InputAction.CallbackContext context)
    {
        // 키보드 입력이면 즉시 실행
        if (context.control.device is Keyboard)
        {
            FireBreathPerformed?.Invoke();
            return;
        }

        // Gamepad LB
        if (context.control.device is Gamepad)
        {
            HandleLeftShoulder();
        }
    }

    // FireBall
    public event Action FireBallPerformed;
    void OnFireBallPerformed(InputAction.CallbackContext context)
    {
        // 키보드 입력이면 즉시 실행
        if (context.control.device is Keyboard)
        {
            FireBallPerformed?.Invoke();
            return;
        }

        // Gamepad RB
        if (context.control.device is Gamepad)
        {
            HandleRightShoulder();
        }
    }

    // IgniteSword
    public event Action IgniteSwordPerformed;
    void OnIgniteSwordPerformed(InputAction.CallbackContext context)
    {
        IgniteSwordPerformed?.Invoke();
    }

    // Parry
    public event Action ParryStarted;
    public event Action ParryCanceled;
    void OnParryStarted(InputAction.CallbackContext context)
    {
        ParryStarted?.Invoke();
    }
    void OnParryCanceled(InputAction.CallbackContext context)
    {
        ParryCanceled?.Invoke();
    }

    // Dodge
    public event Action DodgePerformed;
    void OnDodgePerformed(InputAction.CallbackContext context)
    {
        DodgePerformed?.Invoke();
    }
}
