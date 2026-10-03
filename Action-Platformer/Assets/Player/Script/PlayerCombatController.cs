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
    public bool IsAttacking { get; private set; }

    [Header("ComboAttack")]
    [SerializeField] int _maxCombo = 3;
    [SerializeField] float _comboAttackInputBufferTime = 0.4f;
    int _comboIndex = -1;
    bool _isComboWindowOpen;
    bool _isComboBuffered;
    float _comboAttackInputBufferTimer;

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
        _playerInput.FireBreathPerformed += OnFireBreathPerformed;
        _playerInput.FireBallPerformed += OnFireBallPerformed;
        _playerInput.IgniteSwordPerformed += OnIgniteSwordPerformed;
    }

    void OnDisable()
    {
        if (_playerInput == null)
            return;

        _playerInput.AttackPerformed -= OnAttackPerformed;
        _playerInput.FireBallPerformed -= OnFireBallPerformed;
        _playerInput.FireBreathPerformed -= OnFireBreathPerformed;
        _playerInput.IgniteSwordPerformed -= OnIgniteSwordPerformed;
    }

    void Update()
    {
        UpdateAttackInputBuffer();
    }

    #region Attack(Sword)

    #region HandleAttackInput

    void OnAttackPerformed()
    {
        // 공격 중이면 콤보저장 시도
        if (IsAttacking)
        {
            // TODO : DashAttack, InAirAttack은 콤보가 없는 상태이므로 수정필요
            TryBufferComboInput();
            return;
        }

        if (!_isAttackable)
            return;

        if (_playerMoveController.IsDodging)
        {
            StartDashAttack();
            return;
        }

        if (_environmentChecker.IsGrounded)
        {
            StartComboAttack();
            return;
        }
        
        StartInAirAttack();
    }

    void UpdateAttackInputBuffer()
    {
        if (_comboAttackInputBufferTimer <= 0f)
            return;

        _comboAttackInputBufferTimer = Mathf.Max(_comboAttackInputBufferTimer - Time.deltaTime, 0f);
    }

    #endregion
    #region Attack Common 

    void BeginAttack()
    {
        IsAttacking = true;
        _stateMachine.SetState(new AttackState());
        _playerHitboxManager.ClearCachedTarget();
    }

    void EndAttack()
    {
        IsAttacking = false;
        _stateMachine.SetState(new NormalState());
    }

    public void CancelAttack()
    {
        IsAttacking = false;
        ResetCombo();
        _playerVisual.SetDashAttack(false);
        _playerMoveController.EndAirAttackTime();
    }

    #endregion

    #region ComboAttack

    void StartComboAttack()
    {
        if (!IsAttacking)
        {
            BeginAttack();
        }

        ResetComboInput();
        _playerHitboxManager.ClearCachedTarget();
        _playerVisual.PlayComboAttack(++_comboIndex);
    }

    void ResetComboInput()
    {
        _isComboWindowOpen = false;
        _isComboBuffered = false;
        _comboAttackInputBufferTimer = 0f;
    }

    void TryBufferComboInput()
    {
        // ComboAttack 아니거나, 마지막 콤보면 무시
        if (_comboIndex < 0 || _comboIndex >= _maxCombo - 1)
            return;

        // Combo Window 열려있으면 예약
        if (_isComboWindowOpen)
        {
            _isComboBuffered = true;
            return;
        }

        // Combo Window 닫혀있다면 일정 시간 입력 기억
        _comboAttackInputBufferTimer = _comboAttackInputBufferTime;
    }

    void AnimEvent_OpenComboWindow()
    {
        _isComboWindowOpen = true;

        if (_comboAttackInputBufferTimer > 0f)
        {
            _isComboBuffered = true;
            _comboAttackInputBufferTimer = 0f;
        }
    }

    void AnimEvent_CloseComboWindow()
    {
        _isComboWindowOpen = false;
    }

    void AnimEvent_ComboAttackEnd()
    {
        if (_isComboBuffered && _comboIndex < _maxCombo - 1)
        {
            StartComboAttack();
            return;
        }

        ResetCombo();
        EndAttack();
    }

    public void ResetCombo()
    {
        _comboIndex = -1;
        ResetComboInput();
    }

    void AnimEvent_CheckComboAttack1Hit()
    {
        _playerHitboxManager.CheckComboAttack1();
    }

    void AnimEvent_CheckComboAttack2Hit()
    {
        _playerHitboxManager.CheckComboAttack2();
    }

    void AnimEvent_CheckComboAttack3Hit()
    {
        _playerHitboxManager.CheckComboAttack3();
    }

    #endregion
    #region DashAttack

    void StartDashAttack()
    {
        _playerVisual.SetDashAttack(true);
    }

    void AnimEvent_DashAttackStart()
    {
        BeginAttack();
    }

    void AnimEvent_CheckDashAttackHit()
    {
        _playerHitboxManager.CheckDashAttack();
    }

    void AnimEvent_DashAttackEnd()
    {
        EndAttack();
        _playerVisual.SetDashAttack(false);
    }

    #endregion
    #region InAirAttack

    void StartInAirAttack()
    {
        _playerVisual.PlayInAirAttack();
    }

    void AnimEvent_InAirAttackStart()
    {
        BeginAttack();
    }

    void AnimEvent_CheckInAirAttackHit()
    {
        bool isHit = _playerHitboxManager.CheckInAirAttack();
        if (isHit)
        {
            _playerMoveController.StartAirAttackTime();
        }
    }

    void AnimEvent_InAirAttackEnd()
    {
        EndAttack();
    }

    #endregion

    #endregion

    #region FireBreath

    void OnFireBreathPerformed()
    {
        if (!_isAttackable)
            return;

        StartFireBreath();
    }

    void StartFireBreath()
    {
        _playerVisual.PlayFireBreath();
    }

    void AnimEvent_FireBreathStart()
    {
        BeginAttack();
    }

    void AnimEvent_CheckFireBreathHit()
    {
        _playerHitboxManager.CheckFireBreath();
    }

    void AnimEvent_FireBreathEnd()
    {
        EndAttack();
    }

    #endregion

    #region FireBall

    void OnFireBallPerformed()
    {
        if (!_isAttackable)
            return;

        StartFireBall();
    }

    void StartFireBall()
    {
        _playerVisual.PlayFireBall();
    }

    void AnimEvent_FireBallStart()
    {
        BeginAttack();
    }

    void AnimEvent_SpawnFireBall()
    {
        // TODO : Prefab 생성해 날리기
    }

    void AnimEvent_FireBallEnd()
    {
        EndAttack();
    }

    #endregion

    #region IgniteSword

    void OnIgniteSwordPerformed()
    {
        if (!_isAttackable)
            return;

        StartIgniteSword();
    }

    void StartIgniteSword()
    {
        _playerVisual.PlayIgniteSword();
    }

    void AnimEvent_IgniteSwordStart()
    {
        BeginAttack();
    }

    void AnimEvent_IgniteSwordEnd()
    {
        EndAttack();
    }

    #endregion

    #region Parry

    #endregion
}
