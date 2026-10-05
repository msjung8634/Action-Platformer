using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

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
    //[Space(10)]
    //[SerializeField] GameObject _parry;
    //[SerializeField] AttackData _parryData;
    [Space(10)]
    [SerializeField] GameObject _fireBreath;
    [SerializeField] AttackData _fireBreathData;
    [Space(10)]
    [SerializeField] GameObject _fireBall;
    [SerializeField] AttackData _fireBallData;
    //[Space(10)]
    //[SerializeField] GameObject _igniteSword;
    //[SerializeField] AttackData _igniteSwordData;

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

        _playerResourceManager.OnSpChanged += OnSpChanged;
        _playerResourceManager.OnFpChanged += OnFpChanged;
    }

    void OnDisable()
    {
        _buttonSubscription?.Dispose();
        _buttonSubscription = null;
        InputSystem.onActionChange -= OnActionChange;

        _playerResourceManager.OnSpChanged -= OnSpChanged;
        _playerResourceManager.OnFpChanged -= OnFpChanged;
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

        OnSpChanged(_playerResourceManager.SP.Current);
        OnFpChanged(_playerResourceManager.FP.Current);
    }

    static void SetObjectsActive(GameObject[] objects, bool active)
    {
        foreach (GameObject target in objects)
        {
            if (target != null && target.activeSelf != active)
                target.SetActive(active);
        }
    }

    void OnSpChanged(int current, int _ = 0)
    {
        SetSpActionColor(_jump, current >= _jumpData.SpConsumption);
        SetSpActionColor(_dodge, current >= _dodgeData.SpConsumption);
        SetSpActionColor(_meleeAttack, current >= _meleeAttackData.SpConsumption);
        //SetSpActionColor(_parry, current >= _parryData.SpConsumption);
    }
    void SetSpActionColor(GameObject root, bool isEnough)
    {
        Color color = isEnough ? _spEnough : _spNotEnough;
        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            image.color = color;
        }
    }

    void OnFpChanged(int current, int _ = 0)
    {
        SetFpActionColor(_fireBreath, current >= _fireBreathData.FpConsumption);
        SetFpActionColor(_fireBall, current >= _fireBreathData.FpConsumption);
        //SetFpActionColor(_igniteSword, current >= _igniteSwordData.FpConsumption);
    }
    void SetFpActionColor(GameObject root, bool isEnough)
    {
        Color color = isEnough ? _fpEnough : _fpNotEnough;
        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            image.color = color;
        }
    }

}

