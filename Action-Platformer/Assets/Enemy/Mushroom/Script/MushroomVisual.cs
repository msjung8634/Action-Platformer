using UnityEngine;

public class MushroomVisual : EnemyVisualBase
{
    static readonly int _moveSpeedHash = Animator.StringToHash("moveSpeed");
    public void SetMoveSpeed(float value)
    {
        _animator.SetFloat(_moveSpeedHash, value);
    }

    static readonly int _attack1Hash = Animator.StringToHash("attack1");
    public void PlayAttack1()
    {
        _animator.SetTrigger(_attack1Hash);
    }

    static readonly int _attack2Hash = Animator.StringToHash("attack2");
    public void PlayAttack2()
    {
        _animator.SetTrigger(_attack2Hash);
    }

    static readonly int _attack3Hash = Animator.StringToHash("attack3");
    public void PlayAttack3()
    {
        _animator.SetTrigger(_attack3Hash);
    }
}
