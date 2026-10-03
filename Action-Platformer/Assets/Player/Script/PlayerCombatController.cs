using System.Resources;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [Header("Refrences")]
    PlayerInput _playerInput;
    Rigidbody2D _rigidbody;
    PlayerVisual _playerVisual;
    PlayerMoveController _playerMoveController;
    PlayerResourceManager _resourceManager;
    [SerializeField] EnvironmentChecker _environmentChecker;
    [SerializeField] UnitStateMachine _stateMachine;
    [SerializeField] PlayerHitboxManager _playerHitboxManager;

    bool _isAttackable => _stateMachine?.Attack?.CurrentState.Equals(Attack.State.Attackable) ?? false;

    [Header("GroundAttack")]
    int _maxCombo = 3;
    int _comboIndex = -1;
    public bool IsAttacking { get; private set; }

    bool _isComboWindowOpen;
    bool _isComboBuffered;
    [SerializeField] float _attackInputBufferTime = 0.4f;
    float _attackInputBufferTimer;

    [Header("Parry")]
    bool b;

    void Awake()
    {
        TryGetComponent(out _playerInput);
        TryGetComponent(out _rigidbody);
        TryGetComponent(out _playerVisual);
        TryGetComponent(out _playerMoveController);
        TryGetComponent(out _resourceManager);
    }

    void OnEnable()
    {
        if (_playerInput == null)
            return;

        _playerInput.AttackPerformed += OnAttackPerformed;
    }

    void OnDisable()
    {
        if (_playerInput == null)
            return;

        _playerInput.AttackPerformed -= OnAttackPerformed;
    }

    void Update()
    {
        if (_attackInputBufferTimer > 0f)
        {
            _attackInputBufferTimer = Mathf.Max(_attackInputBufferTimer - Time.deltaTime, 0f);
        }
    }

    #region Attack

    void OnAttackPerformed()
    {
        // 공격 중이 아니라면 1타 시작
        if (!IsAttacking)
        {
            if (!_isAttackable)
                return;

            if (_environmentChecker.IsGrounded)
            {
                StartGroundAttack();
            }

            return;
        }

        // 마지막 콤보라면 무시
        if (_comboIndex >= _maxCombo - 1)
            return;

        // 콤보 입력 가능 구간이라면 즉시 예약
        if (_isComboWindowOpen)
        {
            _isComboBuffered = true;
            return;
        }

        // 입력을 일정 시간 기억
        _attackInputBufferTimer = _attackInputBufferTime;
    }

    #region GroundAttack

    void StartGroundAttack()
    {
        if (!IsAttacking)
        {
            IsAttacking = true;
            _stateMachine.SetState(new AttackState());
        }
        
        _isComboWindowOpen = false;
        _isComboBuffered = false;

        _attackInputBufferTimer = 0f;

        _comboIndex++;
        _playerHitboxManager.ClearCachedTarget();

        _playerVisual.PlayGroundAttack(_comboIndex);
    }

    void AnimEvent_OpenComboWindow()
    {
        _isComboWindowOpen = true;

        if (_attackInputBufferTimer > 0f)
        {
            _isComboBuffered = true;
            _attackInputBufferTimer = 0f;
        }
    }

    void AnimEvent_CloseComboWindow()
    {
        _isComboWindowOpen = false;
    }

    void AnimEvent_GroundAttackEnd()
    {
        if (_isComboBuffered && _comboIndex < _maxCombo - 1)
        {
            StartGroundAttack();
            return;
        }

        EndCombo();
        _stateMachine.SetState(new NormalState());
    }

    public void EndCombo()
    {
        IsAttacking = false;

        _comboIndex = -1;

        _isComboWindowOpen = false;
        _isComboBuffered = false;

        _attackInputBufferTimer = 0f;
    }

    void AnimEvent_CheckGroundAttack0Hit()
    {
        _playerHitboxManager.CheckGroundAttack0();
    }

    void AnimEvent_CheckGroundAttack1Hit()
    {
        _playerHitboxManager.CheckGroundAttack1();
    }

    void AnimEvent_CheckGroundAttack2Hit()
    {
        _playerHitboxManager.CheckGroundAttack2();
    }

    #endregion

    #endregion

    #region Parry

    #endregion
}
