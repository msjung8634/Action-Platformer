using System.Resources;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [Header("Refrences")]
    PlayerInput _playerInput;
    Rigidbody2D _rigidbody;
    PlayerVisual _playerVisual;
    PlayerMoveController _playerMoveController;
    [SerializeField] PlayerResourceManager _resourceManager;
    [SerializeField] EnvironmentChecker _environmentChecker;
    [SerializeField] UnitStateMachine _stateMachine;
    [SerializeField] PlayerHitboxManager _playerHitboxManager;

    bool _isAttackable => _stateMachine?.Attack?.CurrentState.Equals(Attack.State.Attackable) ?? false;
    public bool IsAttacking { get; private set; }

    [Header("ComboAttack")]
    [SerializeField] AttackData _comboAttack1;
    [SerializeField] AttackData _comboAttack2;
    [SerializeField] AttackData _comboAttack3;
    [SerializeField] int _maxCombo = 3;
    [SerializeField] float _comboAttackInputBufferTime = 0.4f;
    int _comboIndex = -1;
    bool _isComboWindowOpen;
    bool _isComboBuffered;
    float _comboAttackInputBufferTimer;

    [Header("DashAttack")]
    [SerializeField] AttackData _dashAttack;

    [Header("InAir - UpSlash")]
    [SerializeField] AttackData _upSlashData;
    [Header("InAir - DownSlash")]
    [SerializeField] AttackData _downSlashData;
    [SerializeField, Min(0f)] float _downSlashDiveSpeed = 18f;
    bool _isAirAttacking;
    bool _airAttackAlreadyPerformed;

    [Header("Fireball")]
    [SerializeField] AttackData _fireball;
    [SerializeField] Transform _spawnPosition;
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] LayerMask _targetLayer;
    [SerializeField] float _flySpeed;

    [Header("Firebreath")]
    [SerializeField] AttackData _firebreath;

    bool b;

    void Awake()
    {
        TryGetComponent(out _playerInput);
        TryGetComponent(out _rigidbody);
        TryGetComponent(out _playerVisual);
        TryGetComponent(out _playerMoveController);
    }

    void OnEnable()
    {
        _resourceManager.OnHit += OnHit;
        _resourceManager.OnDead += OnDead;

        if (_playerInput == null)
            return;

        _playerInput.AttackPerformed += OnAttackPerformed;
        _playerInput.FireBreathPerformed += OnFireBreathPerformed;
        _playerInput.FireBallPerformed += OnFireBallPerformed;

        if (WaveManager.Instance == null) return;
        WaveManager.Instance.OnRestartGame += OnRestartGame;
    }

    void OnDisable()
    {
        _resourceManager.OnHit -= OnHit;
        _resourceManager.OnDead -= OnDead;

        if (_playerInput == null)
            return;

        _playerInput.AttackPerformed -= OnAttackPerformed;
        _playerInput.FireBallPerformed -= OnFireBallPerformed;
        _playerInput.FireBreathPerformed -= OnFireBreathPerformed;

        if (WaveManager.Instance == null) return;
        WaveManager.Instance.OnRestartGame -= OnRestartGame;
    }

    void Update()
    {
        UpdateAttackInputBuffer();
    }

    #region Common

    bool TryRequestAttack(AttackData attackData)
    {
        if (!_isAttackable || IsAttacking)
            return false;

        if (!_resourceManager.TryConsumeResource(attackData))
            return false;

        // 애니메이션 시작 이벤트가 오기 전까지
        // 연속 입력으로 SP가 중복 소비되는 것을 방지
        IsAttacking = true;
        return true;
    }

    void BeginAttack()
    {
        IsAttacking = true;
        _stateMachine.SetState(new AttackState());
        _playerHitboxManager.ClearCachedTarget();
    }

    void EndAttack()
    {
        IsAttacking = false;
        _isAirAttacking = false;
        _stateMachine.SetState(new NormalState());
    }

    public void CancelAttack()
    {
        IsAttacking = false;
        _isAirAttacking = false;
        ResetCombo();

        _playerMoveController.EndUpSlashMove();
        _playerMoveController.EndDownSlashMove();
        _playerVisual.SetDashAttack(false);
    }

    #endregion
    #region HandleAttackInput

    void OnAttackPerformed()
    {
        // 지면에서 공격 중이면 콤보저장 시도
        if (IsAttacking && !_isAirAttacking)
        {
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
            TryStartNextComboAttack();
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

    #region MeleeAttack

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

    AttackData GetComboAttackData(int comboIndex)
    {
        return comboIndex switch
        {
            0 => _comboAttack1,
            1 => _comboAttack2,
            2 => _comboAttack3,
            _ => null
        };
    }

    bool TryStartNextComboAttack()
    {
        int nextComboIndex = _comboIndex + 1;

        if (nextComboIndex >= _maxCombo)
            return false;

        // 실패하면 콤보 번호, 공격 상태를 유지
        AttackData attackData = GetComboAttackData(nextComboIndex);
        if (!_resourceManager.TryConsumeResource(attackData))
            return false;

        if (!IsAttacking)
            BeginAttack();

        ResetComboInput();
        _playerHitboxManager.ClearCachedTarget();

        _comboIndex = nextComboIndex;
        _playerVisual.PlayComboAttack(_comboIndex);

        return true;
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
        if (_isComboBuffered && _comboIndex < _maxCombo - 1 && TryStartNextComboAttack())
        {
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
        if (!TryRequestAttack(_dashAttack))
            return;

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
        if (_isAirAttacking && _airAttackAlreadyPerformed)
            return;

        bool isDownSlash = _playerMoveController.CurrentJumpPhase == PlayerMoveController.JumpPhase.Descend;
        AttackData airAttackData = isDownSlash ? _downSlashData : _upSlashData;

        if (!TryRequestAttack(airAttackData))
            return;

        BeginAttack();
        _isAirAttacking = true;
        _airAttackAlreadyPerformed = true;

        if (isDownSlash)
        {
            _playerVisual.PlayDownSlashAttack();
        }
        else
        {
            _playerVisual.PlayUpSlashAttack();
        }
    }
    public void ResetAirAttackOnLanding()
    {
        // airAttack을 실행한 후 한번만 실행
        if (!_airAttackAlreadyPerformed)
            return;

        EndAttack();
        _isAirAttacking = false;
        _airAttackAlreadyPerformed = false;
    }

    #region UpSlash

    void AnimEvent_UpSlashStopMoveStart()
    {
        _playerMoveController.StartUpSlashMove();
    }

    void AnimEvent_UpSlashCheckAttackHit()
    {
        _playerHitboxManager.CheckUpSlashAttack();
    }

    void AnimEvent_UpSlashStopMoveEnd()
    {
        _playerMoveController.EndUpSlashMove();
        EndAttack();
    }

    #endregion
    #region DownSlash

    void AnimEvent_DownSlashDiveMoveStart()
    {
        _playerMoveController.StartDownSlashMove(_downSlashDiveSpeed);
    }
    void AnimEvent_DownSlashCheckAttackHit()
    {
        _playerHitboxManager.CheckDownSlashAttack();
    }
    void AnimEvent_DownSlashDiveMoveEnd()
    {
        _playerMoveController.EndDownSlashMove();
        EndAttack();
    }

    #endregion

    #endregion

    #endregion
    #region FireBreath

    void OnFireBreathPerformed()
    {
        StartFireBreath();
    }

    void StartFireBreath()
    {
        if (!TryRequestAttack(_firebreath))
            return;

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
        StartFireBall();
    }

    void StartFireBall()
    {
        if (!TryRequestAttack(_fireball))
            return;

        _playerVisual.PlayFireBall();
    }

    void AnimEvent_FireBallStart()
    {
        BeginAttack();
    }

    void AnimEvent_SpawnFireBall()
    {
        Vector2 dir = _playerVisual.FaceDirection == UnitMoveDirection.Right
            ? Vector2.right
            : Vector2.left;

        Vector2 spawnPoint = _spawnPosition.position;

        GameObject projectileObj = Instantiate(_projectilePrefab, spawnPoint, Quaternion.identity);
        if (projectileObj.TryGetComponent(out ProjectileController controller))
        {
            Vector2 targetPosition = spawnPoint + dir;

            controller.Initialize(
                caster: transform,
                targetPosition: targetPosition,
                targetLayerMask: _targetLayer,
                speed: _flySpeed,
                onHit: OnFireballHit,
                ignoreYOffset: true
            );
        }
    }

    void OnFireballHit(ProjectileController projectile, Collider2D hitbox)
    {
        if (!hitbox.TryGetComponent<IDamageable>(out var target))
            return;

        Vector2 hitPoint = hitbox.ClosestPoint(projectile.transform.position);
        var hitData = new HitData
        {
            Attacker = projectile.Owner.root.gameObject,
            Target = target,
            HitPoint = hitPoint,
            HitDirection = projectile.FlyDirection,
            AttackData = _fireball
        };

        CombatManager.Instance.ProcessHit(hitData);
    }

    void AnimEvent_FireBallEnd()
    {
        EndAttack();
    }

    #endregion

    #region OnHit

    void OnHit()
    {
        CancelAttack();
        _stateMachine.SetState(new StunState());
        _playerVisual.PlayHit();
    }

    void AnimEvent_HitEnd()
    {
        _stateMachine.SetState(new NormalState());
    }

    #endregion
    #region OnDead

    void OnDead()
    {
        CancelAttack();
        _stateMachine.SetState(new DeadState());
        _playerVisual.PlayDie();
    }

    #endregion
    #region OnRestartGame

    void OnRestartGame()
    {
        IsAttacking = false;
        _isAirAttacking = false;
        _airAttackAlreadyPerformed = false;
        ResetCombo();
    }

    #endregion
}
