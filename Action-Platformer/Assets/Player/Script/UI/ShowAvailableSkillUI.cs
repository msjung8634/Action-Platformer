using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class ShowAvailableSkillUI : MonoBehaviour
{
    public enum DeviceType
    {
        Keyboard,
        Gamepad
    }

    [Header("Reference")]
    [SerializeField] PlayerResourceManager _playerResourceManager;

    [Header("Settings")]
    [SerializeField] DeviceType _defaultDevice = DeviceType.Keyboard;

    [Header("UI")]
    [SerializeField] GameObject[] _keyboardUiGroup;
    [SerializeField] GameObject[] _gamepadUiGroup;
    [Space(10)]
    [SerializeField] GameObject _jump;
    [SerializeField] MoveData _jumpData;
    [Space(10)]
    [SerializeField] GameObject _dodge;
    [SerializeField] MoveData _dodgeData;
    [Space(10)]
    [SerializeField] GameObject _meleeAttack;
    [SerializeField] AttackData _meleeAttackData;
    [Space(10)]
    [SerializeField] GameObject _parry;
    [SerializeField] AttackData _parryData;
    [Space(10)]
    [SerializeField] GameObject _fireBreath;
    [SerializeField] AttackData _fireBreathData;
    [Space(10)]
    [SerializeField] GameObject _fireBall;
    [SerializeField] AttackData _fireBallData;
    [Space(10)]
    [SerializeField] GameObject _igniteSword;
    [SerializeField] AttackData _igniteSwordData;

    [Header("Color")]
    [SerializeField] Color _spNotEnough;
    [SerializeField] Color _spEnough;
    [SerializeField] Color _fpNotEnough;
    [SerializeField] Color _fpEnough;


    public DeviceType CurrentDevice { get; private set; }
    IDisposable _buttonSubscription;

    void OnEnable()
    {
        CurrentDevice = _defaultDevice;
        RefreshUI();

        _buttonSubscription = InputSystem.onAnyButtonPress.Call(OnButtonPressed);
        InputSystem.onActionChange += OnActionChange;
    }

    void OnDisable()
    {
        _buttonSubscription?.Dispose();
        _buttonSubscription = null;
        InputSystem.onActionChange -= OnActionChange;
    }

    void OnButtonPressed(InputControl control)
    {
        if (control.device is Keyboard)
        {
            SetDevice(DeviceType.Keyboard);
        }
        else if (control.device is Gamepad)
        {
            SetDevice(DeviceType.Gamepad);
        }
    }

    void OnActionChange(object source, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed)
            return;

        if (source is not InputAction action)
            return;

        InputControl control = action.activeControl;

        if (control == null || control.device is not Gamepad)
            return;

        SetDevice(DeviceType.Gamepad);
    }

    void SetDevice(DeviceType device)
    {
        if (CurrentDevice == device)
            return;

        CurrentDevice = device;
        RefreshUI();
    }

    void RefreshUI()
    {
        SetObjectsActive(_keyboardUiGroup, CurrentDevice == DeviceType.Keyboard);
        SetObjectsActive(_gamepadUiGroup, CurrentDevice == DeviceType.Gamepad);
    }

    static void SetObjectsActive(GameObject[] objects, bool active)
    {
        foreach (GameObject target in objects)
        {
            if (target != null && target.activeSelf != active)
                target.SetActive(active);
        }
    }


}

