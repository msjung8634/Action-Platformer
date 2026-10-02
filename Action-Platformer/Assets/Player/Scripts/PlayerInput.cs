using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("Input Action References")]
    [SerializeField] InputActionReference _move;
    [SerializeField] InputActionReference _jump;
    [SerializeField] InputActionReference _attack;
    [SerializeField] InputActionReference _parry;
    [SerializeField] InputActionReference _dodge;

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

        if (_attack != null)
        {
            _attack.action.performed += OnAttackPerformed;
            _attack.action.Enable();
        }

        if (_parry != null)
        {
            _parry.action.started += OnParryStarted;
            _parry.action.canceled += OnParryCanceled;
            _parry.action.Enable();
        }

        if (_dodge != null)
        {
            _dodge.action.performed += OnDodgePerformed;
            _dodge.action.Enable();
        }
    }

    void OnDisable()
    {
        RawInput = Vector2.zero;
        DesiredDirection = MoveDirection.None;

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

        if (_attack != null)
        {
            _attack.action.performed -= OnAttackPerformed;
            _attack.action.Disable();
        }

        if (_parry != null)
        {
            _parry.action.started -= OnParryStarted;
            _parry.action.canceled -= OnParryCanceled;
            _parry.action.Disable();
        }

        if (_dodge != null)
        {
            _dodge.action.performed -= OnDodgePerformed;
            _dodge.action.Disable();
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

    // Attack
    public event Action AttackPerformed;

    void OnAttackPerformed(InputAction.CallbackContext context)
    {
        AttackPerformed?.Invoke();
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
