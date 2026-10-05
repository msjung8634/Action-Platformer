using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public enum UnitMoveDirection
{
    None,
    Right,
    Left,
}

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] SpriteRenderer _renderer;
    [SerializeField] Animator _animator;

    public UnitMoveDirection FaceDirection { get; private set; } = UnitMoveDirection.Right;

    public void SetFacingDirection(UnitMoveDirection newDirection)
    {
        if (FaceDirection == newDirection)
            return;

        switch (newDirection)
        {
            case UnitMoveDirection.Right:
                FaceDirection = UnitMoveDirection.Right;
                _renderer.flipX = false;
                break;
            case UnitMoveDirection.Left:
                FaceDirection = UnitMoveDirection.Left;
                _renderer.flipX = true;
                break;
            default:
                break;
        }
    }

    void OnEnable()
    {
        if (WaveManager.Instance == null) return;
        WaveManager.Instance.OnRestartGame += OnRestartGame;
    }

    void OnDisable()
    {
        if (WaveManager.Instance == null) return;
        WaveManager.Instance.OnRestartGame -= OnRestartGame;
    }

    static readonly int _locomotionStateHash = Animator.StringToHash("Base Layer.Locomotion");
    public void OnRestartGame()
    {
        SetFacingDirection(UnitMoveDirection.Right);

        SetMoveSpeed(0f);
        _animator.ResetTrigger(_hitHash);
        _animator.ResetTrigger(_dieHash);

        _animator.ResetTrigger(_ascendHash);
        _animator.ResetTrigger(_descendHash);
        _animator.ResetTrigger(_landHash);

        _animator.ResetTrigger(_comboAttackHash);
        _animator.SetInteger(_comboIndexHash, 0);

        _animator.SetBool(_dashAttackHash, false);

        _animator.ResetTrigger(_inAirAttackHash);
        _animator.ResetTrigger(_inAirAttackHash);

        _animator.ResetTrigger(_dodgeHash);

        _animator.ResetTrigger(_fireBreathHash);

        _animator.ResetTrigger(_fireBallHash);

        _animator.ResetTrigger(_igniteSwordHash);

        _animator.Play(_locomotionStateHash, 0, 0f);
    }

    static readonly int _moveSpeedHash = Animator.StringToHash("moveSpeed");
    public void SetMoveSpeed(float speed)
    {
        _animator.SetFloat(_moveSpeedHash, speed);
    }

    static readonly int _hitHash = Animator.StringToHash("hit");
    public void PlayHit()
    {
        _animator.SetTrigger(_hitHash);
    }

    static readonly int _dieHash = Animator.StringToHash("die");
    public void PlayDie()
    {
        _animator.SetTrigger(_dieHash);
    }

    static readonly int _ascendHash = Animator.StringToHash("ascend");
    static readonly int _descendHash = Animator.StringToHash("descend");
    static readonly int _landHash = Animator.StringToHash("land");
    public void PlayAscend()
    {
        _animator.SetTrigger(_ascendHash);
    }
    public void PlayDescend()
    {
        _animator.SetTrigger(_descendHash);
    }
    public void PlayLand()
    {
        _animator.SetTrigger(_landHash);
    }

    static readonly int _comboAttackHash = Animator.StringToHash("comboAttack");
    static readonly int _comboIndexHash = Animator.StringToHash("comboIndex");
    public void PlayComboAttack(int comboIndex)
    {
        _animator.SetTrigger(_comboAttackHash);
        _animator.SetInteger(_comboIndexHash, comboIndex);
    }

    static readonly int _dashAttackHash = Animator.StringToHash("dashAttack");
    public void SetDashAttack(bool value)
    {
        _animator.SetBool(_dashAttackHash, value);
    }

    static readonly int _inAirAttackHash = Animator.StringToHash("inAirAttack");
    public void PlayInAirAttack()
    {
        _animator.SetTrigger(_inAirAttackHash);
    }
    public void ResetInAirAttack()
    {
        _animator.ResetTrigger(_inAirAttackHash);
    }

    static readonly int _dodgeHash = Animator.StringToHash("dodge");
    public void PlayDodge()
    {
        _animator.SetTrigger(_dodgeHash);
    }

    static readonly int _fireBreathHash = Animator.StringToHash("fireBreath");
    public void PlayFireBreath()
    {
        _animator.SetTrigger(_fireBreathHash);
    }

    static readonly int _fireBallHash = Animator.StringToHash("fireBall");
    public void PlayFireBall()
    {
        _animator.SetTrigger(_fireBallHash);
    }

    static readonly int _igniteSwordHash = Animator.StringToHash("igniteSword");
    public void PlayIgniteSword()
    {
        _animator.SetTrigger(_igniteSwordHash);
    }
}
